namespace FedoraDev.NodeEditor
{
	public interface IConnection : IProduce<IConnection>
	{
        INode NodeA { get; set; }
		INode NodeB { get; set; }
        float Distance { get; set; }

		INode GetOtherNode(INode node);
	}
}
