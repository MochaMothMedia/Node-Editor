namespace MochaMoth.NodeEditor.Connections
{
    public interface IConnectionFactory
    {
        IConnection ProduceConnection();
    }
}
