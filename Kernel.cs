using System.Diagnostics;
using System.Text;

namespace SuperAdminBot;

internal static class Kernel
{
    private static Setting? _settings;
    private static TGSubsystem? _telegram;
    private static VKSubsystem? _vkontakte;
    private static CacheSubsystem? _cache;
    private static CallbackQueryOrder? _callbackQueryOrder;
    private static DateTime _startTime;
    
    [Obsolete("Obsolete")]
    public static void Main()
    {
        Debug.Message("SuperAdmin Bot version 0.1 release");
        Debug.NewLine();
        Debug.Message("Copyright © 2023 Edward Gushchin");
        Debug.Message("Licensed under the Apache License, Version 2.0");
        Debug.NewLine();
        Debug.Message("Need a bot? For you here: https://t.me/eduardgushchin");
        Debug.NewLine();

        if (!Path.Exists("cache")) Directory.CreateDirectory("cache");
        if (!Path.Exists("logs")) Directory.CreateDirectory("logs");

        _settings = new Setting();

        if (!_settings.IsInitialized) return;

        _telegram = new TGSubsystem();
        _vkontakte = new VKSubsystem();
        _callbackQueryOrder = new CallbackQueryOrder();
        _cache = new CacheSubsystem();
        
        _vkontakte.SubscribeEvents();

        _startTime = DateTime.Now;
    }

    public static async Task<bool> SendForModerationAsync(string path, string username)
    {
        return await _telegram?.SendForModerationAsync(path, username)!;
    }
    
    public static string WatermarkAttach(string? name, VideoQuality quality)
    {
        Debug.Log($"Putting a watermark on the video \"{name}\"...", Debug.Sender.Kernel, Debug.MessageStatus.INFO);
        
        var watermark = quality switch
        {
            VideoQuality.Q240 => Path.Combine("water", "96.png"),
            VideoQuality.Q360 => Path.Combine("water", "144.png"),
            VideoQuality.Q480 => Path.Combine("water", "192.png"),
            VideoQuality.Q720 => Path.Combine("water", "192.png"),
            VideoQuality.Q1080 => Path.Combine("water", "288.png"),
            VideoQuality.Q1440 => Path.Combine("water", "432.png"),
            VideoQuality.Q2160 => Path.Combine("water", "576.png"),
            _ => ""
        };

        var extension = Path.GetExtension(name);
        
        if (extension == ".webm")
        {
            name = ConvertWebmToMp4(name!);
        }

        var newName = $"{Guid.NewGuid().ToString()}.mp4";
        var newFullPath = Path.Combine("cache", newName);
        var arguments = $"-i {Path.Combine("cache", name!)} -i {watermark} -filter_complex \"overlay=15:H-h-15\" -codec:a copy {newFullPath} -loglevel panic";
        
        ProcessStartInfo startInfo = new()
        {
            FileName = "ffmpeg.exe",
            UseShellExecute = false,
            Arguments = arguments
        };
        
        using var exeProcess = Process.Start(startInfo);
        
        exeProcess?.WaitForExit();
        exeProcess?.Close();
        
        File.Delete(Path.Combine("cache", name!));

        Debug.Log($"The watermark has been applied successfully. Output file: \"{newName}\"", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

        return newName;
    }

    public static string ConvertWebmToMp4(string mp4file)
    {
        Debug.Log("Starting the process of converting a video file from webm to mp4...", Debug.Sender.Kernel, Debug.MessageStatus.INFO);
        
        var newFile = $"convert_{mp4file}.mp4";
        var newFilePath = Path.Combine("cache", newFile);

        var arguments = $"-fflags +genpts -i {Path.Combine("cache", mp4file)} -r 25 {newFilePath} -loglevel panic";

        ProcessStartInfo startInfo = new()
        {
            FileName = "ffmpeg.exe",
            UseShellExecute = false,
            Arguments = arguments
        };
        
        using var exeProcess = Process.Start(startInfo);
        
        exeProcess?.WaitForExit();
        exeProcess?.Close();

        File.Delete(Path.Combine("cache", mp4file));
        
        Debug.Log("The process of converting a video file from webm to mp4 was successful.", Debug.Sender.Kernel, Debug.MessageStatus.INFO);
        return newFile;
    }

    public static VideoQuality GetQualityFromVideo(string name)
    {
        var filePath = Path.Combine("cache", name);
        var arguments = $"-v error -select_streams v:0 -show_entries stream=width -of csv=p=0 {filePath}";
        
        ProcessStartInfo startInfo = new()
        {
            FileName = "ffprobe.exe",
            UseShellExecute = false,
            Arguments = arguments,
            RedirectStandardOutput = true
        };

        using var exeProcess = Process.Start(startInfo);
        var data = string.Empty;
        
        if (exeProcess != null)
        {
            data = exeProcess.StandardOutput.ReadToEnd();
            
            exeProcess.WaitForExit();
            exeProcess.Close();
        }

        if (data == null) return VideoQuality.None;
        
        var width = Convert.ToInt32(data);

        return width switch
        {
            >= 0 and < 360 => VideoQuality.Q240,
            >= 360 and < 480 => VideoQuality.Q360,
            >= 480 and < 720 => VideoQuality.Q480,
            >= 720 and < 1080 => VideoQuality.Q720,
            >= 1080 and < 1440 => VideoQuality.Q1080,
            >= 1440 and < 2160 => VideoQuality.Q1440,
            >= 2160 => VideoQuality.Q2160,
            _ => VideoQuality.None
        };

    }

    public static string GetSubsystemsStatus()
    {
        var uptime = DateTime.Now - _startTime;
        var header =     "*Информация о состоянии бота:*";
        var uptimestr = $"*⏱ Uptime:* {uptime.Days} дней {uptime.Hours} часов {uptime.Minutes} минут";
        var telegram =  $"*⚙️ Telegram Subsystem:* {_telegram!.Status}";
        var vk =        $"*⚙️ VK Subsystem:* {_vkontakte!.Status}";
        var query =     $"*⚙️ QueryOrder Subsystem:* {_callbackQueryOrder!.Status}";
        var cash =      $"*⚙️ Cash Subsystem:* {_cache!.Status}";
        
        var subscribers = $"*📺 Подписок ВК:* {_settings!.Subscriptions!.Count}";
        var cashObjects = $"*📊 Объектов в кешэ:* {_cache!.CashCount}";
        var videos = $"*⚖️ Очередь запросов:* {_callbackQueryOrder!.Count}";
        
        var timeout = $"*⌛️ Таймаут обновления:* {_settings.CheckTimeoutInMinutes} минут";
        var sleep = $"*⌛️ Таймаут при блокировке:* {_settings.SleepForRateLimit} минут";
        var size = $"*💾 Размер кэша:* {_settings.CacheSize}";
        var folder = $"*🎥 Видео в папке кэша:* {Directory.GetFiles("cache").Length}";
        var builder = new StringBuilder();

        builder.AppendLine(header);
        builder.AppendLine();
        builder.AppendLine(uptimestr);
        builder.AppendLine(vk);
        builder.AppendLine(cash);
        builder.AppendLine(telegram);
        builder.AppendLine(query);
        builder.AppendLine();
        builder.AppendLine(subscribers);
        builder.AppendLine(cashObjects);
        builder.AppendLine(videos);
        builder.AppendLine(folder);
        builder.AppendLine();
        builder.AppendLine(timeout);
        builder.AppendLine(sleep);
        builder.AppendLine(size);
        
        builder.AppendLine();

        return builder.ToString();
    }

    [Obsolete("Obsolete")]
    public static async Task PublicVideoAsync(string path)
    {
        var newPath = Path.Combine("cache", _settings!.Watermark ? WatermarkAttach(path, GetQualityFromVideo(path)) : path);
        await _telegram!.PublicVideoAsync(newPath);
        await _vkontakte!.PublicVideoAsync(newPath);
    }

    [Obsolete("Obsolete")]
    public static void RestartTelegramApi()
    {
        _telegram!.RestartApi();
    }

    public static async Task<List<VKVideo>> GetVideoListAsync(long id)
    {
        return await _vkontakte?.GetVideoListAsync(id)!;
    }

    public static Setting? Settings => _settings;

    public static CacheSubsystem? Cache => _cache;

    public static CallbackQueryOrder? CallbackOrder => _callbackQueryOrder;
}
