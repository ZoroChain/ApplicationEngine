namespace AEServer
{
    public interface IDBTable<IDBObjectT> where IDBObjectT: IDBObject
    {
        string name {get;}

        ulong dataCount {get;}

        bool init(object config);

        IDBObjectT insertData(object Data);

        IDBObjectT getData(ulong id);

        IDBObjectT queryDBObject(string query);
        
        object queryData(string query);
    }
}