using UnityEngine;

namespace MochaMoth.NodeEditor.Nodes
{
    public interface IAssetNode : INode
    {
        ScriptableObject Asset { get; set; }
    }
}
