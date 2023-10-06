namespace SuperAdminBot;

public class Subscription
{
    private List<VKVideo>? _cache;

    public Subscription(long id, string name)
    {
        if (Kernel.Settings != null) 
            _cache = new List<VKVideo>(Kernel.Settings.CacheSize);
        ID = id;
        Name = name;
    }

    public async Task InitializingAsync()
    {
        var video = await Kernel.GetVideoListAsync(ID);
        _cache = video;
    }

    public async Task<List<VKVideo>?> UpdateAsync()
    {
        var video = await Kernel.GetVideoListAsync(ID);

        if (_cache == null) return null;
        var newVideo = video.Except(_cache, new VKVideoEqualityComparer()).ToList();

        if (newVideo.Count == 0) return null;
        
        _cache = video;
        return newVideo;
    }

    public long ID { get; }
    
    public string Name { get; }

    public int Count => _cache!.Count;
}