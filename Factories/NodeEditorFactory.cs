using MochaMoth.NodeEditor.Connections;
using MochaMoth.NodeEditor.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MochaMoth.NodeEditor.Implementations
{
	[CreateAssetMenu(fileName = "Node Editor Factory", menuName = "Node Editor/Factory")]
	public class NodeEditorFactory : SerializedScriptableObject, INodeFactory, IAssetNodeFactory, IConnectionFactory
	{
		[SerializeField] IConnection _connectionFab;
		[SerializeField] INode _nodeFab;
		[SerializeField] IAssetNode _assetNodeFab;

		public IConnection ProduceConnection() => _connectionFab.Produce();
		public INode ProduceNode() => _nodeFab.Produce();
		public IAssetNode ProduceAssetNode() => _assetNodeFab.Produce() as IAssetNode;
	}
}
