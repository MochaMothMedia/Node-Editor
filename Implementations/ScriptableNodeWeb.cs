using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	[CreateAssetMenu(fileName = "New Node Web", menuName = "Node Editor/Node Web")]
	public class ScriptableNodeWeb : SerializedScriptableObject, INodeWeb
	{
		public string Name => base.name;

		public INode[] Nodes
		{
			get
			{
				SetSelfDirty();
				if (_nodes == null)
					_nodes = new INode[0];
				return _nodes;
			}
			set
			{
				_nodes = value;
				SetSelfDirty();
			}
		}

		public Vector2 Offset
		{
			get
			{
				SetSelfDirty();
				return _offset;
			}
			set
			{
				_offset = value;
				SetSelfDirty();
			}
		}

		public IConnection[] Connections
		{
			get
			{
				SetSelfDirty();
				if (_connections == null)
					_connections = new IConnection[0];
				return _connections;
			}

			set
			{
				_connections = value;
				SetSelfDirty();
			}
		}

		[SerializeField, ReadOnly] Vector2 _offset;
		[SerializeField, ReadOnly] INode[] _nodes = new INode[0];
		[SerializeField, ReadOnly] IConnection[] _connections = new IConnection[0];

		public void AddNode(INode node)
		{
			INode[] newNodes = new INode[_nodes.Length + 1];

			for (int i = 0; i < _nodes.Length; i++)
				newNodes[i] = _nodes[i];
			newNodes[_nodes.Length] = node;

			_nodes = newNodes;
			SetSelfDirty();
		}

		public void RemoveNode(INode node)
		{
			for (int i = 0; i < node.Connections.Length; i++)
				node.Connections[i].GetOtherNode(node).RemoveConnection(node);

			List<INode> newNodes = new List<INode>();

			for (int i = 0; i < _nodes.Length; i++)
				if (_nodes[i] != node)
					newNodes.Add(_nodes[i]);

			_nodes = newNodes.ToArray();
			SetSelfDirty();
		}

		public void AddConnection(IConnection connection)
		{
			IConnection[] newConnections = new IConnection[_connections.Length + 1];

			for (int i = 0; i < _connections.Length; i++)
				newConnections[i] = _connections[i];
			newConnections[_connections.Length] = connection;

			_connections = newConnections;
			SetSelfDirty();
		}

		public void RemoveConnection(IConnection connection)
		{
			connection.NodeA.RemoveConnection(connection.NodeB);
			connection.NodeB.RemoveConnection(connection.NodeA);

			List<IConnection> newConnections = new List<IConnection>();

			for (int i = 0; i < _connections.Length; i++)
				if (_connections[i] != connection)
					newConnections.Add(_connections[i]);

			_connections = newConnections.ToArray();
			SetSelfDirty();
		}

		void SetSelfDirty()
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		#region Produce In Editor
#if UNITY_EDITOR
		//public static INodeWeb ProduceInEditor(string location)
		//{
		//	string[] locationArray = location.Split('/');
		//	string folder = "";
		//	string filename = locationArray[locationArray.Length - 1];


		//	for (int i = 0; i < locationArray.Length - 1; i++)
		//	{
		//		if (locationArray[i] == string.Empty)
		//			continue;
		//		folder += $"{locationArray[i]}/";
		//	}

		//	ScriptableNodeWeb nodeWeb = CreateInstance<ScriptableNodeWeb>();

		//	if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
		//		System.IO.Directory.CreateDirectory(folder);
		//	if (System.IO.File.Exists($"{folder}/{filename}"))
		//	{
		//		Debug.Log($"There's already a '{filename}' at this location!");
		//		return null;
		//	}

		//	UnityEditor.AssetDatabase.CreateAsset(nodeWeb, $"{folder}/{filename}");
		//	UnityEditor.AssetDatabase.SaveAssets();

		//	return nodeWeb;
		//}
#endif
		#endregion
	}
}
