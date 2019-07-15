namespace AEServer
{
    public class Debug
    {
        protected static ILogger _logger = null;
        
        public static ILogger logger
        {
            get
            {
                return _logger;
            }
        }

        public static void regLogger(ILogger l)
        {
            _logger = l;
        }

        // TO DO : profiler / dumpstack / footpriter etc,...
    }
}