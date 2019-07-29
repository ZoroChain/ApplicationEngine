namespace AEServer
{
    public interface IDBObject
    {
        string id { get; }

        bool isPersistObj { get; }

        object getData(string key);

        bool modifyData(string key, object val);

        bool flush(bool persist);

    }
}