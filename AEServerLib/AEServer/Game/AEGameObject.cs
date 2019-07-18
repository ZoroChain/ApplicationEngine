using System.Dynamic;
using System.ComponentModel;

using AEServer.Session;

namespace AEServer.Game
{
    public class AEGameObject : IGameObject
    {
        // use struct provide runtime game object data instead of dynamic object to improve performance
        protected dynamic   _dynamicData = new ExpandoObject();

        public dynamic dynamicData
        {
            get
            {
                return _dynamicData;
            }
        }

        virtual public string name
        {
            get
            {
                return "base";
            }
        }

        virtual protected bool _syncData(object orgData)
        {
            return true;
        }

        public bool syncFrom(object orgData)
        {
            return _syncData(orgData);
        }
    }

    public class AEPlayerGameObject: AEGameObject, IPlayerGameObject
    {
        protected IPlayer _player = null;

        public AEPlayerGameObject(IPlayer p)
        {
            _player = p;
        }

        public IPlayer player
        {
            get
            {
                return _player;
            }
        }
    }
}