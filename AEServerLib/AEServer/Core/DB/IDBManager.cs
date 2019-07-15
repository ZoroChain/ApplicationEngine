namespace AEServer
{
    public interface IDBManager
    {
        bool init(object config);

        bool fin();

        IDBTable<IDBObject> getMemDBTalbe(string name);

        IDBTable<IDBPersistObject> getPersistDBTable(string name);
    }
}