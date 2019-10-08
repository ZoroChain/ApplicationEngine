using System.Collections.Generic;
using System.Dynamic;

namespace AEServer
{
    public class ConfigManager : IConfigManager
    {
        protected dynamic _serverConf = new ExpandoObject();

        public bool init()
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Load Configs...");
            // for Debug...

            dynamic conf = _serverConf;

            conf.tickTime = 1000; // 500 millisecond tick proc

            //conf.DBManagerConf = new ExpandoObject();

            //// mysql db
            //conf.DBManagerConf.mysql = new ExpandoObject();
            //ArrayList mysqlDB = new ArrayList();

            //dynamic mysqlGlobalDB = new ExpandoObject();
            //mysqlGlobalDB.name = "global";
            //mysqlGlobalDB.source = "server=127.0.0.1;";
            //mysqlGlobalDB.options = "uid=root;pwd=123456;database=global";
            //mysqlDB.Add(mysqlGlobalDB);

            //for (int i=0; i< 10; ++i)
            //{
            //    dynamic mysqlUserDB = new ExpandoObject();
            //    mysqlUserDB.name = "user"+i;
            //    mysqlUserDB.source = "server=127.0.0.1;";
            //    mysqlUserDB.options = "uid=root;pwd=123456;database=user"+i;
            //    mysqlDB.Add(mysqlUserDB);
            //}

            //dynamic mysqlGameDB = new ExpandoObject();
            //mysqlGameDB.name = "game";
            //mysqlGameDB.source = "server=127.0.0.1;";
            //mysqlGameDB.options = "uid=root;pwd=123456;database=game";
            //mysqlDB.Add(mysqlGameDB);

            //conf.DBManagerConf.mysql.db = mysqlDB;

            //// redis cache
            //conf.DBManagerConf.redis = new ExpandoObject();
            //ArrayList redisDB = new ArrayList();

            //dynamic redisGlobalCache = new ExpandoObject();
            //redisGlobalCache.name = "global";
            //redisGlobalCache.source = "127.0.0.1:6379,127.0.0.1:6381";
            //redisGlobalCache.options = "allowAdmin=true,password=123456";
            //redisGlobalCache.keyExpireTime = 600; // persist db cache with expire time; // TO DO : handle by aeserver, not redis
            //redisDB.Add(redisGlobalCache);

            //dynamic redisUserCache = new ExpandoObject();
            //redisUserCache.name = "user";
            //redisUserCache.source = "127.0.0.1:6379,127.0.0.1:6383";
            //redisUserCache.options = "allowAdmin=true,password=123456";
            //redisUserCache.keyExpireTime = 600; // persist db cache with expire time;
            //redisDB.Add(redisUserCache);

            //dynamic redisGameCache = new ExpandoObject();
            //redisGameCache.name = "game.land";
            //redisGameCache.source = "127.0.0.1:6379,127.0.0.1:6385";
            //redisGameCache.options = "allowAdmin=true,password=123456";
            //redisGameCache.keyExpireTime = 600; // persist db cache with expire time;
            //redisDB.Add(redisGameCache);

            //redisGameCache.name = "game.player";
            //redisGameCache.source = "127.0.0.1:6379,127.0.0.1:6387";
            //redisGameCache.options = "allowAdmin=true,password=123456";
            //redisGameCache.keyExpireTime = 600; // persist db cache with expire time;
            //redisDB.Add(redisGameCache);


            //redisGameCache.name = "mem.rankboard";
            //redisGameCache.source = "127.0.0.1:6379,127.0.0.1:6389";
            //redisGameCache.options = "allowAdmin=true,password=123456";
            //redisGameCache.keyExpireTime = -1; // mem cache won't expire, data must store persist in cache until server restart;
            //redisDB.Add(redisGameCache);

            //conf.DBManagerConf.redis.db = redisDB;


            //conf.DBManagerConf.memtables = new ArrayList();

            //dynamic rankboardTable = new ExpandoObject();
            //rankboardTable.name = "rankboard";
            //rankboardTable.keyName = "name";
            //rankboardTable.colum = new ExpandoObject();
            //rankboardTable.colum.name = "string";
            //rankboardTable.colum.updatetime = "uint";
            //rankboardTable.colum.board = "binary";
            //rankboardTable.memDBName = "mem.rankboard";
            //conf.DBManagerConf.memtables.Add(rankboardTable);


            //conf.DBManagerConf.persisttables = new ArrayList();

            //dynamic globalTable = new ExpandoObject();
            //globalTable.name = "global";
            //globalTable.keyName = "name";
            //globalTable.colum = new ExpandoObject();
            //globalTable.colum.name = "string";
            //globalTable.colum.data = "binary";
            //globalTable.memDBName = "global";
            //globalTable.persistDBName = "global";
            //globalTable.distDBCount = 1;
            //conf.DBManagerConf.persisttables.Add(globalTable);

            //dynamic userTable = new ExpandoObject();
            //userTable.name = "user";
            //userTable.keyName = "uid";
            //userTable.colum = new ExpandoObject();
            //userTable.colum.uid = "ulong";
            //userTable.colum.uname = "string";
            //userTable.memDBName = "user";
            //userTable.persistDBName = "user";
            //userTable.distDBCount = 10;
            //conf.DBManagerConf.persisttables.Add(userTable);

            //dynamic gameTable = new ExpandoObject();
            //gameTable.name = "game.land";
            //gameTable.keyName = "landid";
            //gameTable.colum = new ExpandoObject();
            //gameTable.colum.landid = "string";
            //gameTable.colum.owneruid = "ulong";
            //gameTable.colum.name = "string";
            //gameTable.colum.lvl = "uint";
            //gameTable.colum.data = "binary";
            //gameTable.memDBName = "game.land";
            //gameTable.persistDBName = "game";
            //gameTable.distDBCount = 1;
            //conf.DBManagerConf.persisttables.Add(gameTable);

            //gameTable.name = "game.player";
            //gameTable.keyName = "uid";
            //gameTable.colum = new ExpandoObject();
            //gameTable.colum.uid = "ulong";
            //gameTable.colum.name = "string";
            //gameTable.colum.lvl = "uint";
            //gameTable.colum.zoro = "uint";
            //gameTable.colum.magicCrystal = "uint";
            //gameTable.colum.data = "binary";
            //gameTable.memDBName = "game.player";
            //gameTable.persistDBName = "game";
            //gameTable.distDBCount = 1;
            //conf.DBManagerConf.persisttables.Add(gameTable);



            conf.SessionManagerConf = new ExpandoObject();
            conf.SessionManagerConf.sessionExpireTime = 3000000; // 3000 seconds
            conf.SessionManagerConf.checkSessionExipreInterval = 600000; // 10 min per check
            conf.SessionManagerConf.checkSessionExipreCount = 100000;

            conf.SessionManagerConf.Listener = new ExpandoObject();
            conf.SessionManagerConf.Listener.HTTP = new ExpandoObject();
            //conf.SessionManagerConf.Listener.SignalR = new ExpandoObject();
            //conf.SessionManagerConf.Listener.WebSocket = new ExpandoObject();

            conf.ServiceManagerConf = new ExpandoObject();
            conf.ServiceManagerConf.defaultService = new ExpandoObject();

            dynamic defSvrConf = conf.ServiceManagerConf.defaultService;
            defSvrConf.name = "loginService";
            defSvrConf.Game = new ExpandoObject();
            defSvrConf.Game.name = "outGame";
            defSvrConf.Threading = new ExpandoObject();
            defSvrConf.Threading.threads = new ExpandoObject[4];

            var serviceList = new List<ExpandoObject>();
            
            dynamic webGameSvrConf = new ExpandoObject();
            webGameSvrConf.name = "webGameService";
            webGameSvrConf.Game = new ExpandoObject();
            webGameSvrConf.Game.name = "webGame";
            webGameSvrConf.Threading = new ExpandoObject();
            webGameSvrConf.Threading.threads = new ExpandoObject[4];

            serviceList.Add(webGameSvrConf);
            
            conf.ServiceManagerConf.Service = serviceList;


            // TO DO : initialize configs

            
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Load Configs Finished");

            return true;
        }

        public object serverConfig
        {
            get
            {
                return _serverConf;
            }
        }

        public int serverConfigVersion
        {
            get
            {
                return _serverConf.version;
            }
        }

        public object refreshConfig(string configName)
        {
            return null;
        }

        public object getConfig(string configName)
        {
            return null;
        }

        public int getConfigVersion(string configName)
        {
            return 0x00000000;
        }
    }
}