using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.NodeEditor.Abstract
{
    public abstract class ANode : INode
    {
		public virtual string Name => "Node";
		public virtual Vector2 Size => new Vector2(80, 80);
		public virtual IConnection[] Connections { get => _connections; set => _connections = value; }
		public virtual Vector2 Position { get => _position; set => _position = value; }
		public virtual Vector2 ConnectPosition => new Vector2(15, (Size.y / 2));
		public virtual Vector2 ConnectSize => new Vector2(Size.x - 30, (Size.y / 2) - 15);

		[SerializeField, ReadOnly] IConnection[] _connections = new IConnection[0];
		[SerializeField, ReadOnly] Vector2 _position = new Vector2(0, 0);

		public virtual void AddConnection(IConnection connection)
		{
			IConnection[] newConnections = new IConnection[Connections.Length + 1];

			for (int i = 0; i < Connections.Length; i++)
				newConnections[i] = Connections[i];

			newConnections[Connections.Length] = connection;
			Connections = newConnections;
		}

		public virtual void RemoveConnection(INode otherNode)
		{
			List<IConnection> newConnections = new List<IConnection>();
			for (int i = 0; i < Connections.Length; i++)
				if (Connections[i].GetOtherNode(this) != otherNode)
					newConnections.Add(Connections[i]);
			Connections = newConnections.ToArray();
		}

		public virtual void Move(Vector2 delta) => Position += delta;

		public virtual INode Produce(IFactory _factory) => null;
	}
}
