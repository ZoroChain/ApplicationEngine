namespace AEServer
{
    public interface IDBObject
    {
        ulong id {get;}

        bool isDirty {get;}
        
        dynamic data {get;}

        bool modifyObject(object newData);

        bool modifyObject(string route, object newData);

        void markDirty();

        bool flush();

    }

    public interface IDBPersistObject : IDBObject
    {
        bool persist();
    }
}