namespace SuperAdminBot;

public enum TelegramStatus
{
    Initialization,
    SendingVideo,
    WaitingRequests,
    ProcessingRequest
}

public enum VKStatus
{
    Initialization,
    DownloadingNewVideo,
    GlueWatermark,
    SubmitForModeration,
    UpdateVideoCache,
    WaitingForCommand,
    GetVideoInfo
}

public enum CacheStatus
{
    Initialization,
    Wait,
    FindNewVideo,
    Restarting
}

public enum QueryOrderStatus
{
    Initialization,
    Ready
}