namespace AEServer.DB
{
    public class AEDBManager : IDBManager
    {
        DBMySqlManager _mySqlManager = new DBMySqlManager();
        DBRedisManager _redisManager = new DBRedisManager();

        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager ...");

            dynamic conf = config;

            // TO DO : initialize DB
            _mySqlManager.init(conf.mysql);
            _redisManager.init(conf.redis);

            // TO DO : initialize tables

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize DBManager redis OK!");
            return true;
        }

        public bool fin()
        {
            return true;
        }

        public IDBTable<IDBObject> getMemDBTalbe(string name)
        {
            return null;
        }

        public IDBTable<IDBPersistObject> getPersistDBTable(string name)
        {
            return null;
        }
    }
}