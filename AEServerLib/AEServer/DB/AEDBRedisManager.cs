namespace AEServer.DB
{
    public class AEDBRedisManager : IDBManager
    {
        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager redis impl...");

            // TO DO : initialize DB

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize DBManager redis impl Finished");
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