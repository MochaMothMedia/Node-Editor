using MochaMoth.NodeEditor.Nodes;

namespace MochaMoth.NodeEditor
{
    public interface INodeFactory
    {
        INode ProduceNode();
    }
}
