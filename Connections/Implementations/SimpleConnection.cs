using MochaMoth.NodeEditor.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MochaMoth.NodeEditor.Connections.Implementations
{
	public class SimpleConnection : IConnection
	{
		public INode NodeA { get => _nodeA; set => _nodeA = value; }
		public INode NodeB { get => _nodeB; set => _nodeB = value; }
		public int Distance { get => _distance; set => _distance = value; }

		[SerializeField, ReadOnly] INode _nodeA;
		[SerializeField, ReadOnly] INode _nodeB;
		[SerializeField, ReadOnly] int _distance = 1;

		public INode GetOtherNode(INode node) => NodeA == node ? NodeB : NodeA;
		public IConnection Produce() => new SimpleConnection();
	}
}
