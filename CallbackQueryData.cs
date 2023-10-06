namespace SuperAdminBot;

public class CallbackQueryData
{
    public CallbackType Type { get; set; }
    
    public string? Payload { get; set; }
}

public enum CallbackType
{
    Suggestion
}



