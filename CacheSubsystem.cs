namespace SuperAdminBot;

public class CacheSubsystem
{
    private readonly List<Subscription> _subscribers;
    private Thread? _updateThread;
    private bool _threadAlive;
    private CacheStatus _status;

    public delegate void VideoEventHandler(object? sender, VideoEventArgs e);
    public event VideoEventHandler? NewVideoEvent; 

    public CacheSubsystem()
    {
        _status = CacheStatus.Initialization;
        
        Debug.Log("Initializing the cache subsystem...", Debug.Sender.CacheSubsystem, Debug.MessageStatus.INFO);
        
        _subscribers = new List<Subscription>();

        var subscriptions = Kernel.Settings?.Subscriptions;

        if (subscriptions == null) return;
        
        foreach (var subscription in subscriptions.Where(subscription => subscription["Enabled"] == true))
        {
            _subscribers.Add(new Subscription(Convert.ToInt64(subscription["ID"]), subscription["Name"].ToString()));
        }

        _updateThread = new Thread(UpdateThread);
        _threadAlive = true;
        _updateThread.Start();
        
        Debug.Log("Cache subsystem initialized successfully.", Debug.Sender.CacheSubsystem, Debug.MessageStatus.INFO);
    }

    private void UpdateThread()
    {
        var videoCount = 0;
        
        foreach (var subscriber in _subscribers)
        { 
            subscriber.InitializingAsync().Wait();
            videoCount += subscriber.Count;
        }
        
        Debug.Log($"The cache is ready to use. Uploaded {videoCount} videos from {_subscribers.Count} sources.", Debug.Sender.CacheSubsystem, Debug.MessageStatus.WARN);
        
        Debug.NewLine();

        _status = CacheStatus.Wait;
        
        Thread.Sleep(Kernel.Settings!.CheckTimeoutInMinutes * 60000);

        while (_threadAlive)
        {
            _status = CacheStatus.FindNewVideo;
            
            var updated = false;
            foreach (var sub in _subscribers)
            {
                try
                {
                    var newVideoLinks = sub.UpdateAsync().Result?.Where(video => video.Link != null);
                
                    if (newVideoLinks == null) continue;

                    NewVideoEvent?.Invoke(this, new VideoEventArgs(sub.Name, newVideoLinks.ToList()));

                    updated = true;
                }
                catch (Exception)
                {
                    Debug.Log("An error occurred while downloading the video.", Debug.Sender.CacheSubsystem, Debug.MessageStatus.FAIL);
                }
                
            }
            
            if(!updated) Debug.Log("The cache has been updated. No new content found.", Debug.Sender.CacheSubsystem, Debug.MessageStatus.INFO);

            _status = CacheStatus.Wait;
            
            Thread.Sleep(Kernel.Settings.CheckTimeoutInMinutes * 60000);
        }
    }

    public void Restart()
    {
        Debug.Log("Restarting the cache subsystem...", Debug.Sender.CacheSubsystem, Debug.MessageStatus.INFO);
        _status = CacheStatus.Restarting;
        _threadAlive = false;
        _updateThread = null;
        _updateThread = new Thread(UpdateThread);
        _threadAlive = true;
        _updateThread.Start();
        Debug.Log("Cache subsystem restart successfully.", Debug.Sender.CacheSubsystem, Debug.MessageStatus.INFO);
    }

    public string GetSubscriptionNameFromId(long id)
    {
        return _subscribers.Find(subscription => subscription.ID == id)!.Name;
    }
    
    
    public CacheStatus Status => _status;

    public int CashCount
    {
        get
        {
            return _subscribers.Sum(sub => sub.Count);
        }
    }
}