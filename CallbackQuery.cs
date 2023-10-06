namespace SuperAdminBot;

public class CallbackQuery
{
    public CallbackQuery(string guid, string query)
    {
        Guid = guid;
        Query = query;
    }
    
    public string Guid { get; }
    
    public string Query { get; }
}