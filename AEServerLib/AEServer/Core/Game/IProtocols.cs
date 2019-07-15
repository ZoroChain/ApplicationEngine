namespace AEServer.Game
{
    public interface IRpcProtocol
    {
        bool doCommand(IPlayer p, IGameModule m, dynamic par);
    }

    // TO DO : add protocol manager
}