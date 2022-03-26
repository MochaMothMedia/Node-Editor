using Sirenix.OdinInspector;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	[CreateAssetMenu(fileName = "Node Editor Factory", menuName = "Node Editor/Factory")]
	public class Factory : SerializedScriptableObject, IFactory
	{
		[SerializeField] IConnection _connectionFab;
		[SerializeField] INode _nodeFab;
		[SerializeField] IAssetNode _assetNodeFab;

		public IConnection ProduceConnection() => _connectionFab.Produce(this);
		public INode ProduceNode() => _nodeFab.Produce(this);
		public IAssetNode ProduceAssetNode() => _assetNodeFab.Produce(this) as IAssetNode;
	}
}
