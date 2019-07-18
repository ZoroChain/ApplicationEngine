
using AEServer.Session;

namespace AEServer.Game
{
    public class AEPlayer : IPlayer
    {
        protected ISession _session = null;

        public ISession session
        {
            get
            {
                return _session;
            } 
        }

        virtual public IGameObject getGameObject(string name)
        {
            return null;
        }

        virtual public IDBObject getMemDBObj(string name)
        {
            return null;
        }
        virtual public IDBObject getPersistDBObj(string name)
        {
            return null;
        }

        virtual public bool init(ISession s)
        {
            _session = s;

            return true;
        }

        virtual public bool fin(bool clearData)
        {
            _session = null;

            return true;
        }

        virtual public bool flushAll(bool persit)
        {
            return true;
        }
        
        virtual public void onTick(ulong timestamp)
        {

        }
    }
}