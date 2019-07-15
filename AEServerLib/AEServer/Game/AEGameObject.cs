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

    public class AEGameDBObject: AEGameObject, IGameDBObject
    {
        protected IDBObject _dbObj = null;

        public IDBObject dbObj
        {
            get
            {
                return _dbObj;
            }
        }

        virtual protected void _updateData()
        {
            
        }

        public bool initDBObject(IDBObject dbObj)
        {
            _dbObj = dbObj;

            _syncData(_dbObj.data);

            return true;
        }

        public bool flushData(bool persist)
        {
            if(_dbObj == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEGameObject ["+name+"] flushData without _dbObject");
                return false;
            }
            
            _updateData();

            //_dbObj.modifyObject(orgData);

            bool ret = _dbObj.flush();

            if(persist && _dbObj is IDBPersistObject)
            {
                ret = ((IDBPersistObject)_dbObj).persist();
            }

            return ret;
        }
    }
}