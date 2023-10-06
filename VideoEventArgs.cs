namespace SuperAdminBot;

public class VideoEventArgs : EventArgs
{
    private readonly List<VKVideo> _videoList;

    public VideoEventArgs(string subscriptionName, List<VKVideo> videoList)
    {
        SubscriptionName = subscriptionName;
        _videoList = videoList;
    }

    public VKVideo[] Videos => _videoList.ToArray();

    public string SubscriptionName { get; }
}