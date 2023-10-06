namespace SuperAdminBot;

public class VKVideo
{
    public VKVideo(long? id, string link, VideoQuality quality)
    {
        ID = id;
        Link = link;
        Quality = quality;
    }

    public string Link { get; }
    
    public long? ID { get; }
    
    public VideoQuality Quality { get; }
}

internal class VKVideoEqualityComparer : IEqualityComparer<VKVideo>
{
    public bool Equals(VKVideo? x, VKVideo? y)
    {
        return x!.ID == y!.ID;
    }

    public int GetHashCode(VKVideo obj)
    {
        unchecked
        {
            if (obj == null)
                return 0;
            var hashCode = obj.ID.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.ID.GetHashCode();
            return hashCode;
        }
    }
}