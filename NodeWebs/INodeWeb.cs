using MochaMoth.NodeEditor.Connections;
using MochaMoth.NodeEditor.Nodes;
using UnityEngine;

namespace MochaMoth.NodeEditor.NodeWebs
{
    public interface INodeWeb
    {
        string Name { get; }
        INode[] Nodes { get; set; }
        IConnection[] Connections { get; set; }
        Vector2 Offset { get; set; }

        void AddNode(INode node);
        void RemoveNode(INode node);
        void AddConnection(IConnection connection);
        void RemoveConnection(IConnection connection);
        INode[] GetPath(INode startNode, INode endNode, INode[] requiredNodes = null);
    }
}
