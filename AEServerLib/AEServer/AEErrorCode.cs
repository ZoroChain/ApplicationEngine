namespace AEServer
{
    public class AEErrorCode
    {
        // system error
        public static int ERR_SYS_SERVER_INTERNAL_ERROR = -1;
        public static int ERR_SYS_SERVER_LOGIN_VERIFSIGN_ERROR = -101;
        public static int ERR_SYS_SERVER_LOGINPARAMETER_ISNULL_ERROR = -102;

        // session error
        public static int ERR_SESSION_NEED_LOGIN        = -1001;
        public static int ERR_SESSION_LOGIN_WRONG_TOKEN = -1002;
        public static int ERR_SESSOIN_PLAYER_NOT_EXIST  = -1003;
        public static int ERR_SESSION_QUEUE_TASK_ERROR  = -1004;
        

        // service error
        public static int ERR_SERVICE_NOT_FOUND         = -2001;

        public static int ERR_PARAMETER_IS_NULL = -2021;

    }
}