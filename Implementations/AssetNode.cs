using FedoraDev.NodeEditor.Abstract;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	public class AssetNode : ANode, IAssetNode
	{
		public override string Name => Asset.name;
		public ScriptableObject Asset { get => _asset; set => _asset = value; }

		[SerializeField] ScriptableObject _asset;

		public override INode Produce(IFactory _factory) => new AssetNode();
	}
}
