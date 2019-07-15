namespace AEServer
{
    public interface IConfigManager
    {
        bool init();

        object serverConfig
        {
            get;
        }

        int serverConfigVersion
        {
            get;
        }

        object refreshConfig(string configName);

        object getConfig(string configName);

        int getConfigVersion(string configName);
    }
}