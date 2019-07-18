using AEServer.Session;

namespace AEServer.Game
{
    public interface IPlayer
    {
        ISession session
        {
            get;
        }


        bool init(ISession s);
        bool fin(bool clearData);

        bool flushAll(bool persit);

        void onTick(ulong timestamp);

        IGameObject getGameObject(string name);
        IDBObject getMemDBObj(string name);
        IDBObject getPersistDBObj(string name);
    }
}