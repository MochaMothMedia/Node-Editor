using UnityEngine;

namespace FedoraDev.NodeEditor
{
    public interface IAssetNode : INode
    {
        ScriptableObject Asset { get; set; }
    }
}
