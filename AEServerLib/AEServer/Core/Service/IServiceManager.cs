namespace AEServer.Service
{
    public interface IServiceManager
    {
        bool init(object config);

        bool fin();

        IService defaultService
        {
            get;
        }

        IService getService(string name);

        // unix timestamp in ms
        void tickProcess(ulong timestamp);
    }
}