namespace AEServer.Game
{
    public interface IGameObject
    {
        string name
        {
            get;
        }

        dynamic dynamicData
        {
            get;
        }

        bool syncFrom(object orgData);
    }

    public interface IPlayerGameObject : IGameObject
    {
        IPlayer player
        {
            get;
        }


    }
}