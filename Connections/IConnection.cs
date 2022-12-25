using MochaMoth.NodeEditor.Nodes;

namespace MochaMoth.NodeEditor.Connections
{
	public interface IConnection
	{
        INode NodeA { get; set; }
		INode NodeB { get; set; }
        int Distance { get; set; }

		INode GetOtherNode(INode node);
		IConnection Produce();
	}
}
