using FedoraDev.NodeEditor.Abstract;

namespace FedoraDev.NodeEditor.Implementations
{
	public class SimpleNode : ANode
	{
		public override INode Produce(IFactory _factory) => new SimpleNode();
	}
}
