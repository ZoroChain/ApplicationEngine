namespace AEServer
{
    public interface IDBTable
    {
        string name { get; }

        string keyName { get; }

        ulong dataCount {get;}

        bool init(object config);

        IDBObject insertData(object Data);

        IDBObject getDataObject(ulong id);

        IDBObject queryDBObject(string query);
        
        object queryData(string query);
    }
}