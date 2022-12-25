using MochaMoth.NodeEditor.Connections;
using MochaMoth.NodeEditor.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MochaMoth.NodeEditor.Nodes.Implementations
{
	public class AssetNode : IAssetNode
	{
		public ScriptableObject Asset { get => _asset; set => _asset = value; }
		public string Name => Asset.name;
		public Vector2 Size => new Vector2(80, 80);
		public IConnection[] Connections { get => _connections; set => _connections = value; }
		public Vector2 Position { get => _position; set => _position = value; }
		public Vector2 ConnectPosition => new Vector2(15, (Size.y / 2));
		public Vector2 ConnectSize => new Vector2(Size.x - 30, (Size.y / 2) - 15);

		[SerializeField, ReadOnly] IConnection[] _connections = new IConnection[0];
		[SerializeField, ReadOnly] Vector2 _position = new Vector2(0, 0);

		public void Move(Vector2 delta) => Position = NodeUtils.Move(Position, delta);
		public void AddConnection(IConnection connection) => Connections = NodeUtils.AddConnection(connection, Connections);
		public void RemoveConnection(INode otherNode) => Connections = NodeUtils.RemoveConnection(this, otherNode, Connections);

		[SerializeField] ScriptableObject _asset;

		public INode Produce() => new AssetNode();
	}
}
