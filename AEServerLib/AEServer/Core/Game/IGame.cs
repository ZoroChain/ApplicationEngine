using AEServer.Session;

namespace AEServer.Game
{
    public interface IGame
    {
        string name
        {
            get;
        }

        int serviceThreadIndex
        {
            get;
        }

        IPlayer getPlayerByID(ulong id);

        IGameModule getGameModule(string name);

        bool onStartup(object conf, int index);

        bool onShutdown();

        void onAddSession(ISession s, string cls, string method, object par);
        void onRemoveSession(ISession s, string cls, string method, object par);
        void onSessionClose(ISession s, string cls, string method, object par);

        bool onSessionCmd(ISession s, string cls, string method, object par);

        void onTick(ulong timeStamp);
    }
}