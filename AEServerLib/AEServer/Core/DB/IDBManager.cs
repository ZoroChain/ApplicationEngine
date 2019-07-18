namespace AEServer
{
    public interface IDBManager
    {
        bool init(object config);

        bool fin();

        IDBTable getMemDBTalbe(string name);

        IDBTable getPersistDBTable(string name);
    }
}