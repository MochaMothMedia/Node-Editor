using FedoraDev.NodeEditor.Abstract;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	public class PathBridge : INodeBridge
	{
		public string Name => "Path";
		public Vector2 Size => new Vector2(75, 50);
		public IConnection[] Connections { get => _connections; set => _connections = value; }

		public Rect Position
		{
			get
			{
				Rect rect = _position;
				rect.position = new Vector2((Mathf.Round(rect.position.x / 10) * 10) + (Size.x / 2), (Mathf.Round(rect.position.y / 10) * 10) + (Size.y / 2));
				return rect;
			}
			set => _position = value;
		}

		[SerializeField, ReadOnly] IConnection[] _connections = new IConnection[0];
		[SerializeField, ReadOnly] Rect _position;

		bool _isDragged;

		public void Place()
		{
			Rect rect = _position;
			rect.position = new Vector2(Mathf.Round(rect.position.x / 10) * 10, Mathf.Round(rect.position.y / 10) * 10);
			_position = rect;
		}

		public void AddConnection(IConnection connection)
		{
			IConnection[] newConnections = new IConnection[Connections.Length + 1];

			for (int i = 0; i < Connections.Length; i++)
				newConnections[i] = Connections[i];

			newConnections[Connections.Length] = connection;
			Connections = newConnections;
		}

		public void RemoveConnection(INode node)
		{
			List<IConnection> newConnections = new List<IConnection>();
			//for (int i = 0; i < Connections.Length; i++)
			//	if (Connections[i].Node != node)
			//		newConnections.Add(Connections[i]);
			Connections = newConnections.ToArray();
		}

		public bool ProcessEvents(Event currentEvent) => NodeDragHelper.ProcessDrag(currentEvent, Move, Place, Position, ref _isDragged);
		public INodeBridge CreateCopy() => new PathBridge();
		public void Move(Vector2 delta) => _position.position += delta;
	}
}
