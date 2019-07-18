namespace AEServer
{
    public interface IDBObject
    {
        string id { get; }

        bool isPersistObj { get; }

        dynamic getData(string key);

        bool modifyData(string key, dynamic val);

        bool flush(bool persist);

    }
}