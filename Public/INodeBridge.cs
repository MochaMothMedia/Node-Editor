using UnityEngine;

namespace FedoraDev.NodeEditor
{
	public interface INodeBridge
    {
        string Name { get; }
        Vector2 Size { get; }
        IConnection[] Connections { get; }
        Rect Position { get; set; }

        void AddConnection(IConnection connection);
        void RemoveConnection(INode node);
        bool ProcessEvents(Event currentEvent);
        INodeBridge CreateCopy();
        void Move(Vector2 delta);
        void Place();
    }
}
