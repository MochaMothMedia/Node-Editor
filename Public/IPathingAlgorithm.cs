using UnityEngine;

namespace FedoraDev.NodeEditor
{
    public interface IPathingAlgorithm
    {
        INode[] GetPath(INodeWeb nodeWeb, INode startNode, INode endNode, INode[] mustVisitNodes = null);
    }
}
