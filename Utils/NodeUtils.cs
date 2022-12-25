using MochaMoth.NodeEditor.Connections;
using MochaMoth.NodeEditor.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace MochaMoth.NodeEditor.Utils
{
    public static class NodeUtils
    {
		public static IConnection[] AddConnection(IConnection connection, IConnection[] connectionArray)
		{
			IConnection[] newConnections = new IConnection[connectionArray.Length + 1];

			for (int i = 0; i < connectionArray.Length; i++)
				newConnections[i] = connectionArray[i];

			newConnections[connectionArray.Length] = connection;
			return newConnections;
		}

		public static IConnection[] RemoveConnection(INode thisNode, INode otherNode, IConnection[] connectionArray)
		{
			List<IConnection> newConnections = new List<IConnection>();
			for (int i = 0; i < connectionArray.Length; i++)
				if (connectionArray[i].GetOtherNode(thisNode) != otherNode)
					newConnections.Add(connectionArray[i]);
			return newConnections.ToArray();
		}

		public static Vector2 Move(Vector2 position, Vector2 delta) => position + delta;
	}
}
