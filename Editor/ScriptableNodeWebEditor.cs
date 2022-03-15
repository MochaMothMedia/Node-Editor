using UnityEditor;
using UnityEditor.Callbacks;

namespace FedoraDev.NodeEditor.Editor
{
    public class ScriptableNodeWebEditor : PropertyDrawer
    {
		[OnOpenAsset]
		public static bool OpenNodeWeb(int instanceID, int line)
		{
			INodeWeb nodeWeb = EditorUtility.InstanceIDToObject(instanceID) as INodeWeb;
			if (nodeWeb == null)
				return false;

			NodeEditor.OpenWindow(nodeWeb);
			return true;
		}
	}
}
