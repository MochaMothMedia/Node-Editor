using UnityEngine;

namespace FedoraDev.NodeEditor
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
        //IPointOfInterest[] GetShortestPath(IPointOfInterest start, IPointOfInterest end, params IPointOfInterest[] mustVisitPOIs);
    }
}
