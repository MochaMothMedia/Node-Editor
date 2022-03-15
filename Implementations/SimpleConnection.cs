using Sirenix.OdinInspector;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	public class SimpleConnection : IConnection
	{
		public INode NodeA { get => _nodeA; set => _nodeA = value; }
		public INode NodeB { get => _nodeB; set => _nodeB = value; }
		public float Distance { get => _distance; set => _distance = value; }

		[SerializeField, ReadOnly] INode _nodeA;
		[SerializeField, ReadOnly] INode _nodeB;
		[SerializeField, ReadOnly] float _distance = 1;

		public INode GetOtherNode(INode node) => NodeA == node ? NodeB : NodeA;
		public IConnection Produce(IFactory _factory) => new SimpleConnection();
	}
}
