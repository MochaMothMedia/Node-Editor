using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	public class SryheniPathing : IPathingAlgorithm
	{
		List<List<RouteNode>> _distance;

		int GetLowestNodeIndex(List<PathNode> nodes)
		{
			int lowest = int.MaxValue;
			int index = 0;

			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].Cost < lowest)
				{
					lowest = nodes[i].Cost;
					index = i;
				}
			}

			return index;
		}

		void InitializeDistance(int nodeCount)
		{
			_distance = new List<List<RouteNode>>();

			for (int i = 0; i < nodeCount; i++)
			{
				List<RouteNode> nodes = new List<RouteNode>();
				for (int j = 0; j < byte.MaxValue; j++)
					nodes.Add(new RouteNode(i, 0));
				_distance.Add(nodes);
			}
		}

		void ResetDistance()
		{
			for (int i = 0; i < _distance.Count; i++)
			{
				for (int j = 0; j < _distance[i].Count; j++)
				{
					_distance[i][j].Cost = int.MaxValue;
				}
			}
		}

		public INode[] GetPath(INodeWeb nodeWeb, INode startNode, INode endNode, INode[] mustVisitNodes = null)
		{
			if (mustVisitNodes.Length > 8)
			{
				Debug.LogError($"Sryheni is set up to accept up to 8 nodes to visit along the path. The provided parameters include {mustVisitNodes.Length} nodes.");
				return new INode[] { startNode };
			}

			int startNodeIndex = 0;
			int endNodeIndex = 0;
			int[] mustVisitNodeIndices = new int[mustVisitNodes.Length];

			if (_distance == null)
				InitializeDistance(nodeWeb.Nodes.Length);
			else
				ResetDistance();

			List<PathNode> queue = new List<PathNode>();

			for (int i = 0; i < nodeWeb.Nodes.Length; i++)
			{
				if (startNode == nodeWeb.Nodes[i])
					startNodeIndex = i;
				if (endNode == nodeWeb.Nodes[i])
					endNodeIndex = i;

				for (int j = 0; j < mustVisitNodes.Length; j++)
					if (mustVisitNodes[j] == nodeWeb.Nodes[i])
						mustVisitNodeIndices[j] = i;
			}

			queue.Add(new PathNode(startNodeIndex, 0, 0, new List<int>() { startNodeIndex }));
			_distance[startNodeIndex][0].Cost = 0;

			while (queue.Count > 0)
			{
				int uIndex = GetLowestNodeIndex(queue);
				PathNode u = queue[uIndex];
				queue.RemoveAt(uIndex);

				if (u.Cost != _distance[u.NodeIndex][u.Bitmask].Cost)
					continue;

				foreach (IConnection connection in nodeWeb.Nodes[u.NodeIndex].Connections)
				{
					byte newBitmask = u.Bitmask;
					int otherNodeIndex = Array.FindIndex(nodeWeb.Nodes, n => n == connection.GetOtherNode(nodeWeb.Nodes[u.NodeIndex]));
					if (mustVisitNodeIndices.Contains(otherNodeIndex))
					{
						byte vid = (byte)Array.FindIndex(mustVisitNodeIndices, n => n == otherNodeIndex);
						newBitmask = (byte)(u.Bitmask | (1 << vid));
					}

					int newCost = u.Cost + connection.Distance;
					if (newCost < _distance[u.NodeIndex][newBitmask].Cost)
					{
						List<int> newPath = u.PathIndices;
						newPath.Insert(0, u.NodeIndex);
						_distance[u.NodeIndex][newBitmask].Cost = newCost;
						queue.Add(new PathNode(u.NodeIndex, newBitmask, newCost, newPath));
					}
				}
			}

			List<INode> path = new List<INode>();

			foreach (int i in _distance[endNodeIndex][(1 << mustVisitNodes.Length) - 1].PathIndices)
				path.Add(nodeWeb.Nodes[i]);

			return path.ToArray();
		}

		public class RouteNode
		{
			public List<int> PathIndices { get; set; }
			public int Cost { get; set; }

			public RouteNode(int nodeIndex, int cost)
			{
				PathIndices = new List<int>() { nodeIndex };
				Cost = cost;
			}
		}

		public class PathNode
		{
			public int NodeIndex { get; set; }
			public byte Bitmask { get; set; }
			public int Cost { get; set; }
			public List<int> PathIndices { get; set; }

			public PathNode(int nodeIndex, byte bitmask, int cost, List<int> pathIndices)
			{
				NodeIndex = nodeIndex;
				Bitmask = bitmask;
				Cost = cost;
				PathIndices = pathIndices;
			}
		}
	}
}
