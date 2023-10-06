using System.Net;
using System.Text;
using VkNet;
using VkNet.Model;
using VkNet.Exception;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace SuperAdminBot;

public class VKSubsystem
{
    private readonly VkApi _vkApi, _vkClient;
    private VKStatus _status;
    
    public VKSubsystem()
    {
        _status = VKStatus.Initialization;
        
        Debug.Log("Initializing an instance of the VK API...", Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);

        _vkApi = new VkApi();

        _vkClient = new VkApi();
    
        _vkApi.Authorize(new ApiAuthParams
        {
            AccessToken = Kernel.Settings?.VkAccessToken
        });
        
        _vkClient.Authorize(new ApiAuthParams
        {
            AccessToken = Kernel.Settings?.VkPublicAccessToken
        });

        Debug.Log("The VK API instance has been initialized and authorized on the server.", Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);
    }

    public void SubscribeEvents()
    {
        Kernel.Cache!.NewVideoEvent += OnNewVideoEvent;
    }

    private async void OnNewVideoEvent(object? sender, VideoEventArgs e)
    {
        Debug.Log($"Found {e.Videos.Length} new videos in the source \"{e.SubscriptionName}\".", Debug.Sender.VKSubsystem, Debug.MessageStatus.WARN);

        foreach (var video in e.Videos)
        {
            _status = VKStatus.DownloadingNewVideo;

            var newVideo = await DownloadVideoAsync(video.ID, video.Link, e.SubscriptionName);

            if (newVideo == null) continue;
            _status = VKStatus.GlueWatermark;

            //var videoWaterMark = Kernel.WatermarkAttach(newVideo, video.Quality);
            _status = VKStatus.SubmitForModeration;
            await Kernel.SendForModerationAsync(newVideo, e.SubscriptionName);
        }

        _status = VKStatus.WaitingForCommand;
    }


    public async Task<List<VKVideo>> GetVideoListAsync(long subscriber)
    {
        while (true)
        {
            try
            {
                if (Kernel.Settings == null) continue;

                _status = VKStatus.UpdateVideoCache;
                VkCollection<Video> videoList;
                
                try
                {
                    videoList = await _vkApi.Video.GetAsync(new VideoGetParams
                    {
                        OwnerId = subscriber,
                        Count = Kernel.Settings.CacheSize,
                    })!;
                }
                catch (CannotBlacklistYourselfException)
                {
                    Debug.Log($"Public {subscriber} has deactivated the video section.", Debug.Sender.VKSubsystem, Debug.MessageStatus.WARN);
                    return new List<VKVideo>();
                }
                
                var returnedVideoList = new List<VKVideo>();

                foreach (var video in videoList)
                {
                    if (video.Files == null) continue;

                    _status = VKStatus.GetVideoInfo;

                    GetVideoInfo(video, out var quality, out var link, out var id);

                    returnedVideoList.Add(new VKVideo(id, link, quality));
                }

                _status = VKStatus.WaitingForCommand;

                return returnedVideoList;
            }
            catch (RateLimitReachedException)
            {
                if (Kernel.Settings != null)
                {
                    Debug.Log(
                        $"Limits on requests to the VK API have been exhausted. The thread sleeps for {Kernel.Settings.SleepForRateLimit} minutes.",
                        Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);
                    Thread.Sleep(Kernel.Settings.SleepForRateLimit * 60000);
                }
            }
        }
    }

    private void GetVideoInfo(Video video, out VideoQuality quality, out string link, out long? id)
    {
        if (video.Files != null)
        {
            if (video.Files.Mp4_2160 != null)
            {
                link = video.Files.Mp4_2160.ToString();
                quality = VideoQuality.Q2160;
                id = video.Id;
                return;
            }

            if (video.Files.Mp4_1440 != null)
            {
                link = video.Files.Mp4_1440.ToString();
                quality = VideoQuality.Q1440;
                id = video.Id;
                return;
            }

            if (video.Files.Mp4_1080 != null)
            {
                link = video.Files.Mp4_1080.ToString();
                quality = VideoQuality.Q1080;
                id = video.Id;
                return;
            }

            if (video.Files.Mp4_720 != null)
            {
                link = video.Files.Mp4_720.ToString();
                quality = VideoQuality.Q720;
                id = video.Id;
                return;
            }

            if (video.Files.Mp4_480 != null)
            {
                link = video.Files.Mp4_480.ToString();
                quality = VideoQuality.Q480;
                id = video.Id;
                return;
            }

            if (video.Files.Mp4_360 != null)
            {
                link = video.Files.Mp4_360.ToString();
                quality = VideoQuality.Q360;
                id = video.Id;
                return;
            }

            if (video.Files.Mp4_240 != null)
            {
                link = video.Files.Mp4_240.ToString();
                quality = VideoQuality.Q240;
                id = video.Id;
                return;
            }
        }

        quality = VideoQuality.Q240;
        link = string.Empty;
        id = video.Id;
    }

    private async Task<string?> DownloadVideoAsync(long? id, string link, string subscriptionName)
    {
        try
        {
            Debug.Log($"Downloading video {id} from source \"{subscriptionName}\"...", Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);
            
            var name = $"{Guid.NewGuid().ToString()}.mp4";

            var fullPath = Path.Combine("cache", name);

            using var client = new HttpClient();
            await using (var s = await client.GetStreamAsync(link))
            {
                await using (var fs = new FileStream(fullPath, FileMode.CreateNew))
                {
                    await s.CopyToAsync(fs);
                }
            }
                    
            Debug.Log($"Video {id} from source \"{subscriptionName}\" was successfully downloaded and saved to file \"{name}\".", Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);
            return name;
        }
        catch (Exception e)
        {
            Debug.Log($"[DownloadVideoAsync] {e.Message}", Debug.Sender.VKSubsystem, Debug.MessageStatus.WARN);
            return null;
        }
        
    }


    [Obsolete("Obsolete")]
    public async Task PublicVideoAsync(string path)
    {
        try
        {
            //var newPath = Path.Combine("cache", Kernel.WatermarkAttach(path, Kernel.GetQualityFromVideo(path)));

            Debug.Log($"Sending video \"{path}\" to vk...", Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);
        
            var video = await _vkApi.Video.SaveAsync(new VideoSaveParams
            {
                IsPrivate = false,
                Repeat = false,
                GroupId = Kernel.Settings!.VKPublicId,
                //Description = "",
                Name = Kernel.Settings!.PublicName
            });
            var wc = new WebClient();
            wc.UploadFile(video.UploadUrl, path);
            //Console.WriteLine(responseFile);  //{"size":15966484,"owner_id":330464853,"video_id":456239204,"video_hash":"d9e969f3020a227db9"}
            await _vkApi.Wall.PostAsync(new WallPostParams
            {
                //Message = postBody,
                FromGroup = true,
                OwnerId = -Kernel.Settings!.VKPublicId,
                Attachments = new List<MediaAttachment> { video }
            });
        
            Debug.Log($"Video \"{path}\" has been successfully uploaded to the Vkontakte server and published in the main channel.",
                Debug.Sender.VKSubsystem, Debug.MessageStatus.INFO);
        }
        catch (Exception e)
        {
            Debug.Log($"Error: {e.Message}", Debug.Sender.VKSubsystem, Debug.MessageStatus.WARN);
        }
        
    }

    public VKStatus Status => _status;
}