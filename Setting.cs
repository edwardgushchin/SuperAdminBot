using Newtonsoft.Json;

namespace SuperAdminBot;

public class Setting
{
    private long _adminId;
    private string[]? _moderators;
    private int _checkTimeoutInMinutes, _sleepForRateLimit, _cashSize;

    public Setting()
    {
        Debug.Log("Reading the settings file...", Debug.Sender.Kernel, Debug.MessageStatus.INFO);
        
        try
        {
            var settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));

            if (settings != null)
            {
                SettingInit(settings);
                
                Debug.Log("The settings file was successfully read.", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

                IsInitialized = true;
            }
            else
            {
                Debug.Log("An error has occurred with the settings file. Data not received.", Debug.Sender.Settings,
                    Debug.MessageStatus.FAIL);
                IsInitialized = false;
            }
        }
        catch (FileNotFoundException)
        {
            Debug.Log("The settings.json file was not found.", Debug.Sender.Settings, Debug.MessageStatus.FAIL);
            IsInitialized = false;
        }
        catch (Exception e)
        {
            Debug.Log($"[Init] {e.Message}", Debug.Sender.Settings, Debug.MessageStatus.FAIL);
            IsInitialized = false;
        }
    }

    public bool IsChatContains(long id)
    {
        return ChatList != null && ChatList.Contains(id);
    }

    public void AddChat(long id)
    {
        if (ChatList != null && ChatList.Contains(id)) return;
        ChatList?.Add(id);
        UpdateFile();
    }

    public void RemoveChat(long id)
    {
        if (ChatList != null && !ChatList.Contains(id)) return;
        ChatList?.Remove(id);
        UpdateFile();
    }

    private void SettingInit(dynamic settings)
    {
        _checkTimeoutInMinutes = Convert.ToInt32(settings["CheckTimeoutInMinutes"]);
        TgAccessToken = settings["TgAccessToken"].ToString();
        VkAccessToken = settings["VkAccessToken"].ToString();
        VkPublicAccessToken = settings["VkPublicAccessToken"].ToString();
        JoinMessage = settings["JoinMessage"].ToString();
        StartMessage = settings["StartMessage"].ToString();
        ChatList = settings["ChatList"].ToObject<List<long>>();
        MainChannel = Convert.ToInt64(settings["MainChannel"]);
        _cashSize = Convert.ToInt32(settings["CacheSize"]);
        _sleepForRateLimit = Convert.ToInt32(settings["SleepForRateLimit"]);
        _adminId = Convert.ToInt64(settings["AdminId"]);
        _moderators = settings["Moderators"].ToObject<string[]>();
        Subscriptions = settings["Subscriptions"].ToObject<List<dynamic>>();
        SubscribeButtonUrl = settings["SubscribeButtonUrl"].ToString();
        VideoCaption = settings["VideoCaption"].ToString();
        PublicName = settings["PublicName"].ToString();
        VKPublicId = Convert.ToInt64(settings["VKPublicId"]);
        Watermark = bool.Parse(settings["Watermark"].ToString());
    }
    
    private void UpdateFile()
    {
        File.WriteAllText("settings.json", JsonConvert.SerializeObject(this));
    }

    public bool AddModerator(string username)
    {
        if (_moderators!.Contains(username)) return false;
        var newModerators = new List<string>(_moderators!) {username};
        _moderators = newModerators.ToArray();
        UpdateFile();
        return true;
    }

    public bool DeleteModerator(string username)
    {
        if (!_moderators!.Contains(username)) return false;
        var newModerators = new List<string>(_moderators!);
        newModerators.Remove(username);
        _moderators = newModerators.ToArray();
        UpdateFile();
        return true;
    }
    
    public List<dynamic>? Subscriptions { get; private set; }

    public int CheckTimeoutInMinutes
    {
        get => _checkTimeoutInMinutes;
        set
        {
            Debug.Log($"The \"CheckTimeoutInMinutes\" parameter is set to {value}.", Debug.Sender.Settings, Debug.MessageStatus.WARN);
            _checkTimeoutInMinutes = value;
            UpdateFile();
            Kernel.Cache!.Restart();
        }
    }

    public string? TgAccessToken { get; private set;}

    public string? VkAccessToken { get; private set;}
    
    public string? VkPublicAccessToken { get; private set;}

    public string? JoinMessage { get; private set;}
    
    public string? StartMessage { get; private set; }

    public long MainChannel { get; private set; }
    
    public string? VideoCaption { get; private set; }
    public string? PublicName { get; private set; }
    
    public long VKPublicId { get; private set; }
    
    public bool Watermark { get; private set; }

    public int CacheSize
    {
        get => _cashSize;
        set
        {
            Debug.Log($"The \"CacheSize\" parameter is set to {value}.", Debug.Sender.Settings, Debug.MessageStatus.WARN);
            _cashSize = value;
            UpdateFile();
            Kernel.Cache!.Restart();
        }
    }

    public int SleepForRateLimit
    {
        get => _sleepForRateLimit;
        set
        {
            Debug.Log($"The \"SleepForRateLimit\" parameter is set to {value}.", Debug.Sender.Settings, Debug.MessageStatus.WARN);
            _sleepForRateLimit = value;
            UpdateFile();
        }
    }
    
    public string? SubscribeButtonUrl { get; private set; }

    public long AdminId
    {
        get => _adminId;
        set
        {
            Debug.Log($"The \"AdminId\" parameter is set to {value}.", Debug.Sender.Settings, Debug.MessageStatus.WARN);
            _adminId = value;
            UpdateFile();
        }
    }
    
    public string[]? Moderators => _moderators;

    public List<long>? ChatList { get; private set;}

    [JsonIgnore]
    public bool IsInitialized { get; }
}