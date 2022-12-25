using MochaMoth.NodeEditor.Nodes;
using MochaMoth.NodeEditor.NodeWebs;

namespace MochaMoth.NodeEditor.PathingAlgorithms
{
    public interface IPathingAlgorithm
    {
        INode[] GetPath(INodeWeb nodeWeb, INode startNode, INode endNode, INode[] mustVisitNodes = null);
    }
}
