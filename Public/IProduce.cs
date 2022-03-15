namespace FedoraDev.NodeEditor
{
    public interface IProduce<T>
    {
        T Produce(IFactory _factory);
    }
}
