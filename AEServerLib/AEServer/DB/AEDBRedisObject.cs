using System.Dynamic;

namespace AEServer.DB
{
    public class AEDBRedisObject : IDBObject
    {
        protected ulong _id = 0;
        protected bool _isDirty = false;

        protected dynamic _data = null;

        public ulong id 
        {
            get
            {
                return _id;
            }
        }

        public bool isDirty 
        {
            get
            {
                return _isDirty;
            }
        }
        
        public dynamic data
        {
            get
            {
                return _data;
            }
        }

        public bool modifyObject(object newData)
        {
            if(_data == null)
            {
                _data = AEHelper.CloneDynamicObject(newData);
                return true;
            }

            // TO DO : compare and modify data object

            _isDirty = true;

            return true;
        }

        public bool modifyObject(string route, object newData)
        {
            // TO DO : modify by route
            
            _isDirty = true;
            return true;
        }

        public void markDirty()
        {
            _isDirty = true;
        }

        public bool flush()
        {
            return true;
        }
    }
}