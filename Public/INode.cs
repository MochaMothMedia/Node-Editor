using UnityEngine;

namespace FedoraDev.NodeEditor
{
    public interface INode : IProduce<INode>
    {
        string Name { get; }
        IConnection[] Connections { get; set; }
        Vector2 Position { get; set; }
        Vector2 Size { get; }
        Vector2 ConnectPosition { get; }
        Vector2 ConnectSize { get; }

        void AddConnection(IConnection connection);
        void RemoveConnection(INode otherNode);
        void Move(Vector2 delta);
    }
}
