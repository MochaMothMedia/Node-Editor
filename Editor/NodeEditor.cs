using FedoraDev.NodeEditor.Implementations;
using System;
using UnityEditor;
using UnityEngine;

namespace FedoraDev.NodeEditor.Editor
{
    public class NodeEditor : EditorWindow
    {
		#region Properties
		const string EDITOR_PREF_NODEWEB = "FedoraDev_NodeEditor_NodeWeb";
		const string FACTORY_PATH = "Assets/Fedora Dev/";
		const string FACTORY_NAME = "Node Editor Factory.asset";
		static string FactoryAsset => $"{FACTORY_PATH}{FACTORY_NAME}";
		const int SNAP_VALUE = 20;
		const int SNAP_OFFSET = 0;

		INodeWeb NodeWeb { get; set; }

		IFactory Factory
		{
			get
			{
				if (_factory == null)
					_factory = (IFactory)AssetDatabase.LoadAssetAtPath(FactoryAsset, typeof(IFactory));

				if (_factory == null)
					throw new NullReferenceException($"No factory reference found at {FactoryAsset}. Please create one!");

				return _factory;
			}
		}

		IFactory _factory;
		INode _draggedNode;
		string _targetSaveLocation = "Example";
		bool _isDraggingNode = false;
		bool _isDraggingConnector = false;
		#endregion

		#region Initialization
		[MenuItem("Tools/Node Editor")]
        public static void OpenWindow() => OpenWindow(null);
        public static void OpenWindow(INodeWeb nodeWeb)
		{
            NodeEditor nodeEditor = GetWindow<NodeEditor>();
            nodeEditor.LoadSettings();
            if (nodeWeb != null)
            {
                nodeEditor.NodeWeb = nodeWeb;
                nodeEditor.SaveSettings();
            }
            nodeEditor.Initialize();
            nodeEditor.Show();
		}

        void OnEnable() { LoadSettings(); Initialize(); }

        void LoadSettings()
		{
            NodeWeb = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString(EDITOR_PREF_NODEWEB), typeof(ScriptableObject)) as INodeWeb;
		}

        void SaveSettings()
		{
            EditorPrefs.SetString(EDITOR_PREF_NODEWEB, AssetDatabase.GetAssetPath(NodeWeb as ScriptableObject));
		}

        public void Initialize()
		{
			NodeEditorUtility.CacheResults<INodeWeb>();
			NodeEditorUtility.CacheResults<INode>();
			string title = NodeWeb == null ? "New" : NodeWeb.Name;
            titleContent = new GUIContent($"Node Editor - {title}");
		}
		#endregion

		#region ON GUI
		void OnGUI()
		{
			try
			{
				if (NodeWeb == null)
					DrawEmpty();
				else
					DrawNodeEditor();
			}
			catch (ClearGUIException) { }

            if (GUI.changed) Repaint();
		}

		#region Draw Empty
		void DrawEmpty()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(GUILayout.MaxWidth(125));
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Create a new Node Web:", EditorStyles.HeaderStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Assets/");
			_targetSaveLocation = GUILayout.TextField(_targetSaveLocation, GUILayout.MinWidth(100f));
			GUILayout.Label("/Node Web.asset");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical();
			for (int i = 0; i < NodeEditorUtility.GetAllThatImplement<INodeWeb>().Length; i++)
			{
				Type nodeWebType = NodeEditorUtility.GetAllThatImplement<INodeWeb>()[i];
				string className = ObjectNames.NicifyVariableName(nodeWebType.Name);
				if (GUILayout.Button($"Create: {className}"))
					CreateNodeWebAsset(nodeWebType);
			}
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Open a Node Web:", EditorStyles.HeaderStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			for (int i = 0; i < NodeEditorUtility.GetAllScriptableObjectsThatImplement<INodeWeb>().Length; i++)
			{
				INodeWeb nodeWeb = NodeEditorUtility.GetAllScriptableObjectsThatImplement<INodeWeb>()[i] as INodeWeb;
				if (GUILayout.Button(nodeWeb.Name))
					LoadNodeWebAsset(nodeWeb);
			}
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void LoadNodeWebAsset(INodeWeb nodeWeb)
		{
			NodeWeb = nodeWeb;
			SaveSettings();
			Initialize();
			GUI.changed = true;
		}

		private void CreateNodeWebAsset(Type nodeWebType)
		{
			NodeWeb = Activator.CreateInstance(nodeWebType) as INodeWeb;
			NodeWeb.Offset = new Vector2(position.width / 2, position.height / 2);
			AssetDatabase.CreateFolder("Assets", _targetSaveLocation);
			AssetDatabase.CreateAsset(NodeWeb as ScriptableObject, $"Assets/{_targetSaveLocation}/Node Web.asset");
			SaveSettings();
			Initialize();
			GUI.changed = true;
		}
		#endregion

		#region Draw Node Editor
		void DrawNodeEditor()
		{
			DrawGrid(20, 0.2f, Color.gray);
			DrawGrid(80, 0.4f, Color.gray);
			DrawCenterLines(0.5f, Color.blue);
			DrawConnections();
			DrawNodes();
			DrawUI();

			ProcessEvent(Event.current);
		}

		void DrawGrid(float spacing, float opacity, Color color)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			int widthLineCount = Mathf.CeilToInt(position.width / spacing);
			int heightLineCount = Mathf.CeilToInt(position.height / spacing);

			Handles.BeginGUI();
			Handles.color = new Color(color.r, color.g, color.b, opacity);

			float offsetX = Mathf.Abs(NodeWeb.Offset.x) % spacing;
			float offsetY = Mathf.Abs(NodeWeb.Offset.y) % spacing;

			if (NodeWeb.Offset.x < 0) offsetX = spacing - offsetX;
			if (NodeWeb.Offset.y < 0) offsetY = spacing - offsetY;

			Vector3 finalOffset = new Vector3(offsetX, offsetY, 0);

			for (int i = 0; i < widthLineCount; i++)
				Handles.DrawLine(new Vector3(spacing * i, -spacing, 0f) + finalOffset, new Vector3(spacing * i, position.height, 0f) + finalOffset);

			for (int i = 0; i < heightLineCount; i++)
				Handles.DrawLine(new Vector3(-spacing, spacing * i, 0f) + finalOffset, new Vector3(position.width, spacing * i, 0f) + finalOffset);

			Handles.EndGUI();
		}

		void DrawCenterLines(float opacity, Color color)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Handles.BeginGUI();
			Handles.color = new Color(color.r, color.g, color.b, opacity);
			

			float offsetX = Mathf.Round(NodeWeb.Offset.x);
			float offsetY = Mathf.Round(NodeWeb.Offset.y);

			if (NodeWeb.Offset.x > -5f && NodeWeb.Offset.x < position.width + 5f)
				Handles.DrawLine(
					new Vector3(offsetX, -5, 0),
					new Vector3(offsetX, position.height + 5, 0)); //Vertical Line

			if (NodeWeb.Offset.y > -5f && NodeWeb.Offset.y < position.height + 5f)
				Handles.DrawLine(
					new Vector3(-5, offsetY, 0),
					new Vector3(position.width + 5, offsetY, 0)); //Horizontal Line

			Handles.EndGUI();
		}

		void DrawConnections()
		{
			if (_isDraggingConnector)
			{
				Vector2 nodePosition = NodeVisualPosition(_draggedNode.Position, _draggedNode.Size) + (_draggedNode.Size / 2);
				Handles.color = Color.yellow;
				Handles.DrawLine(new Vector3(nodePosition.x, nodePosition.y, 0), new Vector3(Event.current.mousePosition.x, Event.current.mousePosition.y, 0));
			}

			for (int i = 0; i < NodeWeb.Connections.Length; i++)
			{
				IConnection connection = NodeWeb.Connections[i];

				Vector2 nodeAPos = NodeVisualPosition(connection.NodeA.Position, connection.NodeA.Size) + (connection.NodeA.Size / 2);
				Vector2 nodeBPos = NodeVisualPosition(connection.NodeB.Position, connection.NodeB.Size) + (connection.NodeB.Size / 2);
				Vector2 centerPos = Vector2.Lerp(nodeAPos, nodeBPos, 0.5f) + new Vector2(0, -15);
				Vector2 floatPos = centerPos + new Vector2(0, 15);

				Handles.color = Color.white;
				Handles.DrawLine(new Vector3(nodeAPos.x, nodeAPos.y, 0), new Vector3(nodeBPos.x, nodeBPos.y, 0));

				if (GUI.Button(new Rect(centerPos.x - 10, centerPos.y - 10, 20, 20), "x"))
				{
					connection.NodeA.RemoveConnection(connection.NodeB);
					connection.NodeB.RemoveConnection(connection.NodeA);
					NodeWeb.RemoveConnection(connection);
					GUI.changed = true;
				}

				connection.Distance = EditorGUI.IntField(new Rect(floatPos.x - 25, floatPos.y, 50, 20), connection.Distance);
			}
		}

		void DrawNodes()
		{
			for (int i = 0; i < NodeWeb.Nodes.Length; i++)
			{
				INode node = NodeWeb.Nodes[i];
				GUI.Box(new Rect(NodeVisualPosition(node.Position, node.Size), node.Size), node.Name, EditorStyles.NodeStyle);
				GUI.Box(new Rect(NodeVisualPosition(node.Position, node.Size) + node.ConnectPosition, node.ConnectSize), "->");
			}
		}

		void DrawUI()
		{
			float labelWidth = 100f;
			float buttonWidth = 100f;
			float closeWidth = 50f;

			if (GUI.Button(new Rect(position.width - closeWidth - 5, 5, closeWidth, 25), $"Close"))
			{
				NodeWeb = null;
				SaveSettings();
				Initialize();
				throw new ClearGUIException();
			}

			GUI.Box(new Rect(position.width / 2, 5, labelWidth, 25), $"({NodeWeb.Offset.x - (position.width / 2)}, {NodeWeb.Offset.y - (position.height / 2)})");
			if (GUI.Button(new Rect((position.width / 2) - buttonWidth, 5, buttonWidth, 25), "Recenter"))
			{
				NodeWeb.Offset = Vector2.zero + (position.size / 2);
			}
		}
		#endregion
		#endregion

		#region Process Events
		void ProcessEvent(Event currentEvent)
		{
			switch (currentEvent.type)
			{
				case EventType.MouseDown:
					if (currentEvent.button == 0)
						OnLeftMouseDown(currentEvent.mousePosition);
					else if (currentEvent.button == 1)
						ShowContextMenu(currentEvent.mousePosition);
					break;

				case EventType.MouseDrag:
					if (currentEvent.button == 0)
						OnLeftMouseDrag(currentEvent.delta, currentEvent.mousePosition);
					else if (currentEvent.button == 2)
						OnDragCanvas(currentEvent.delta);
					break;

				case EventType.MouseUp:
					if (currentEvent.button == 0)
						OnLeftMouseUp(currentEvent.mousePosition);
					break;

				case EventType.DragUpdated:
					if ((DragAndDrop.objectReferences[0] as INode) != null || (DragAndDrop.objectReferences[0] as ScriptableObject) != null)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
						DragAndDrop.AcceptDrag();
					}
					break;

				case EventType.DragPerform:
					for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
					{
						Vector2 pos = currentEvent.mousePosition - NodeWeb.Offset;
						INode newNode = DragAndDrop.objectReferences[i] as INode;
						if (newNode != null)
						{
							currentEvent.Use();

							newNode.Position = pos;
							NodeWeb.AddNode(newNode);
						}
						else
						{
							ScriptableObject scriptable = DragAndDrop.objectReferences[i] as ScriptableObject;
							if (scriptable == null)
								continue;

							currentEvent.Use();

							IAssetNode assetNode = Factory.ProduceAssetNode();
							assetNode.Asset = scriptable;
							assetNode.Position = pos;
							NodeWeb.AddNode(assetNode);
						}

						AssetDatabase.SaveAssets();
					}
					break;
			}
		}

		private void ShowContextMenu(Vector2 mousePosition)
		{
			GenericMenu menu = new GenericMenu();

			bool didAdd = false;
			for (int i = 0; i < NodeWeb.Nodes.Length; i++)
			{
				INode node = NodeWeb.Nodes[i];
				if (new Rect(NodeVisualPosition(node.Position, node.Size), node.Size).Contains(mousePosition))
				{
					didAdd = true;
					menu.AddItem(new GUIContent($"Remove {node.Name}"), false, () => OnClickRemoveNode(node));
				}
			}

			if (didAdd)
				menu.AddSeparator(string.Empty);

			for (int i = 0; i < NodeEditorUtility.GetAllThatImplement<INode>().Length; i++)
			{
				Type nodeType = NodeEditorUtility.GetAllThatImplement<INode>()[i];
				int index = i;
				menu.AddItem(new GUIContent($"Add {nodeType.Name}"), false, () => OnClickAddNode(nodeType, mousePosition));
			}

			menu.ShowAsContext();
		}
		#endregion

		#region On Events
		void OnDragCanvas(Vector2 delta)
		{
			NodeWeb.Offset += delta;
			GUI.changed = true;
		}

		void OnLeftMouseDown(Vector2 mousePosition)
		{
			for (int i = 0; i < NodeWeb.Nodes.Length; i++)
			{
				INode node = NodeWeb.Nodes[i];
				if (new Rect(NodeVisualPosition(node.Position, node.Size) + node.ConnectPosition, node.ConnectSize).Contains(mousePosition))
				{
					_draggedNode = node;
					_isDraggingConnector = true;
					return;
				}

				if (new Rect(NodeVisualPosition(node.Position, node.Size), node.Size).Contains(mousePosition))
				{
					_draggedNode = node;
					_isDraggingNode = true;
					return;
				}
			}
		}

		void OnLeftMouseDrag(Vector2 delta, Vector2 mousePosition)
		{
			if (_isDraggingNode)
			{
				_draggedNode.Move(delta);
				GUI.changed = true;
			}

			else if (_isDraggingConnector)
			{
				GUI.changed = true;
			}
		}

		void OnLeftMouseUp(Vector2 mousePosition)
		{
			if (_isDraggingNode)
			{
				_draggedNode.Position = new Vector2(Mathf.Round(_draggedNode.Position.x / SNAP_VALUE) * SNAP_VALUE + SNAP_OFFSET, Mathf.Round(_draggedNode.Position.y / SNAP_VALUE) * SNAP_VALUE + SNAP_OFFSET);
				_draggedNode = null;
				_isDraggingNode = false;
				GUI.changed = true;
			}

			else if (_isDraggingConnector)
			{
				_isDraggingConnector = false;
				GUI.changed = true;

				for (int i = 0; i < NodeWeb.Nodes.Length; i++)
				{
					INode node = NodeWeb.Nodes[i];

					if (new Rect(NodeVisualPosition(node.Position, node.Size), node.Size).Contains(mousePosition))
					{
						for (int j = 0; j < node.Connections.Length; j++)
							if (node.Connections[j].GetOtherNode(node) == _draggedNode)
								return;

						if (node == _draggedNode)
							return;

						IConnection connection = Factory.ProduceConnection();
						connection.NodeA = _draggedNode;
						connection.NodeB = node;

						_draggedNode.AddConnection(connection);
						node.AddConnection(connection);
						NodeWeb.AddConnection(connection);
						return;
					}
				}
			}
		}

		void OnClickAddNode(Type nodeType, Vector2 mousePosition)
		{
			INode node = Activator.CreateInstance(nodeType) as INode;
			node.Move(mousePosition - NodeWeb.Offset);
			NodeWeb.AddNode(node);
			AssetDatabase.SaveAssets();
		}

		void OnClickRemoveNode(INode node)
		{
			NodeWeb.RemoveNode(node);
			for (int i = 0; i < node.Connections.Length; i++)
				NodeWeb.RemoveConnection(node.Connections[i]);
			
			AssetDatabase.SaveAssets();
		}
		#endregion

		#region Assistance
		Vector2 NodeVisualPosition(Vector2 nodePosition, Vector2 nodeSize)
		{
			Vector2 pos = new Vector2(Mathf.Round(nodePosition.x / SNAP_VALUE) * SNAP_VALUE + SNAP_OFFSET, Mathf.Round(nodePosition.y / SNAP_VALUE) * SNAP_VALUE + SNAP_OFFSET);
			pos = pos + NodeWeb.Offset - (nodeSize / 2);
			return pos;
		}
		#endregion
	}
}
