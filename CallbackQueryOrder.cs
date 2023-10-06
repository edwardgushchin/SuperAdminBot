using Newtonsoft.Json;

namespace SuperAdminBot;

public class CallbackQueryOrder
{
    private readonly List<CallbackQuery> _order;
    private readonly QueryOrderStatus _status;

    public CallbackQueryOrder()
    {
        _status = QueryOrderStatus.Initialization;
        
        Debug.Log("The CallbackOrder subsystem is being initialized...", Debug.Sender.CallbackOrder, Debug.MessageStatus.INFO);
        
        if (!File.Exists("queryorder.json")) File.Create("queryorder.json").Close();

        var queryFile = File.ReadAllText("queryorder.json");

        var queryList = JsonConvert.DeserializeObject<List<CallbackQuery>>(queryFile);

        if (queryList == null)
        {
            _order = new List<CallbackQuery>();

            File.WriteAllText("queryorder.json",JsonConvert.SerializeObject(_order));
        }
        else
        {
            _order = queryList;
        }

        Debug.Log("The CallbackOrder subsystem was successfully initialized.", Debug.Sender.CallbackOrder, Debug.MessageStatus.INFO);

        _status = QueryOrderStatus.Ready;
    }

    public CallbackQuery AddQuery(string query)
    {
        var guid = Guid.NewGuid().ToString();
        var q = new CallbackQuery(guid, query);
        _order.Add(q);
        File.WriteAllText("queryorder.json",JsonConvert.SerializeObject(_order));
        return q;
    }

    public CallbackQuery? GetQuery(string guid)
    {
        var get = _order.Find(pair => pair.Guid == guid);
        return get;
    }

    public void RemoveQuery(CallbackQuery query)
    {
        _order.Remove(query);
        
        File.WriteAllText("queryorder.json",JsonConvert.SerializeObject(_order));
    }

    public void Clear()
    {
        _order.Clear();
        File.WriteAllText("queryorder.json",JsonConvert.SerializeObject(_order));
    }

    public int Count => _order.Count;

    public QueryOrderStatus Status => _status;
}