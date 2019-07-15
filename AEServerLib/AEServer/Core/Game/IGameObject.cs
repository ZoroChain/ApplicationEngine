namespace AEServer.Game
{
    public interface IGameObject
    {
        string name
        {
            get;
        }

        dynamic dynamicData
        {
            get;
        }

        bool syncFrom(object orgData);
    }

    public interface IGameDBObject : IGameObject
    {
        IDBObject dbObj
        {
            get;
        }

        bool initDBObject(IDBObject dbObj);

        bool flushData(bool persist);
    }
}