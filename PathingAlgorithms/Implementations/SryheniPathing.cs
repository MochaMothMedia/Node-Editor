using MochaMoth.NodeEditor.Connections;
using MochaMoth.NodeEditor.Nodes;
using MochaMoth.NodeEditor.NodeWebs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochaMoth.NodeEditor.PathingAlgorithms.Implementations
{
	public class SryheniPathing : IPathingAlgorithm
	{
		// 2-dimensional array of RouteNodes.
		// dimension 1 is the node index.
		// dimension 2 is the bitmask for visited nodes.
		List<List<RouteNode>> _routeDistances;

		// Get the node in the list with the lowest cost.
		int GetLowestNodeIndex(List<QueueNode> nodes)
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

		// Generate the Route Distance array with 256 possible mask combinations to accomodate the 8 bits in the bitmask.
		void InitializeRouteDistance(int nodeCount)
		{
			_routeDistances = new List<List<RouteNode>>();

			for (int i = 0; i < nodeCount; i++)
			{
				List<RouteNode> nodes = new List<RouteNode>();
				for (int j = 0; j < byte.MaxValue; j++)
					nodes.Add(new RouteNode(i, int.MaxValue));
				_routeDistances.Add(nodes);
			}
		}

		// Reset all routes to have infinite cost.
		void ResetRouteDistance()
		{
			for (int i = 0; i < _routeDistances.Count; i++)
			{
				for (int j = 0; j < _routeDistances[i].Count; j++)
				{
					_routeDistances[i][j].Cost = int.MaxValue;
					_routeDistances[i][j].PathIndices = new List<int>();
				}
			}
		}

		public INode[] GetPath(INodeWeb nodeWeb, INode startNode, INode endNode, INode[] mustVisitNodes = null)
		{
			// Only allow up to 8 must-visit nodes to stay within the byte-sized bitmask.
			if (mustVisitNodes.Length > 8)
			{
				Debug.LogError($"Sryheni is set up to accept up to 8 nodes to visit along the path. The provided parameters include {mustVisitNodes.Length} nodes. Ignoring anything after the 8th element.");
				INode[] newMustVisitNodes = new INode[8];

				for (int i = 0; i < 8; i++)
					newMustVisitNodes[i] = mustVisitNodes[i];

				mustVisitNodes = newMustVisitNodes;
			}

			int startNodeIndex = 0;
			int endNodeIndex = 0;
			int[] mustVisitNodeIndices = new int[mustVisitNodes.Length];

			// Initialize the Route Distances for this run.
			if (_routeDistances == null)
				InitializeRouteDistance(nodeWeb.Nodes.Length);
			else
				ResetRouteDistance();

			List<QueueNode> queue = new List<QueueNode>();

			// Cache the indices of the important nodes so references don't need to be used.
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

			// Add the initial node and set its cost to 0.
			queue.Add(new QueueNode(startNodeIndex, 0, 0, new List<int>() { startNodeIndex }));
			_routeDistances[startNodeIndex][0].Cost = 0;

			while (queue.Count > 0)
			{
				// Pop the queue'd node with the lowest cost.
				int currentNodeIndex = GetLowestNodeIndex(queue);
				QueueNode currentNode = queue[currentNodeIndex];
				queue.RemoveAt(currentNodeIndex);

				// If a shorter path has already been found on this node, skip it.
				if (currentNode.Cost > _routeDistances[currentNode.NodeIndex][currentNode.Bitmask].Cost)
					continue;

				// Consider each connection on the current node.
				foreach (IConnection connection in nodeWeb.Nodes[currentNode.NodeIndex].Connections)
				{
					// Create a new bitmask so the current one doesn't get changed.
					byte newBitmask = currentNode.Bitmask;
					INode otherNode = connection.GetOtherNode(nodeWeb.Nodes[currentNode.NodeIndex]);
					int connectedNodeIndex = Array.FindIndex(nodeWeb.Nodes, n => n == otherNode);

					// Check if this connection is a required node and set the bitmask accordingly.
					if (mustVisitNodeIndices.Contains(connectedNodeIndex))
					{
						byte visitID = (byte)Array.FindIndex(mustVisitNodeIndices, n => n == connectedNodeIndex);
						newBitmask = (byte)(currentNode.Bitmask | (1 << visitID));
					}

					// If the connected node has a higher cost than the current cost, set its values and add it to the queue.
					int newCost = currentNode.Cost + connection.Distance;
					if (newCost < _routeDistances[connectedNodeIndex][newBitmask].Cost)
					{
						// Create a path as the current path + the next node.
						List<int> newPath = new List<int>(currentNode.PathIndices);
						newPath.Add(connectedNodeIndex);

						// Set the cost and the path to the Route Distances array.
						_routeDistances[connectedNodeIndex][newBitmask].Cost = newCost;
						_routeDistances[connectedNodeIndex][newBitmask].PathIndices = newPath;
						queue.Add(new QueueNode(connectedNodeIndex, newBitmask, newCost, newPath));
					}
				}
			}

			// Get the cheapest path that hits all required nodes and create a list of INodes using their indices.
			List<INode> path = new List<INode>();
			foreach (int i in _routeDistances[endNodeIndex][(1 << mustVisitNodes.Length) - 1].PathIndices)
				path.Add(nodeWeb.Nodes[i]);

			// Return the created path.
			return path.ToArray();
		}

		// Information needed for storing the overall cost and path of a node.
		class RouteNode
		{
			public List<int> PathIndices { get; set; }
			public int Cost { get; set; }

			public RouteNode(int nodeIndex, int cost)
			{
				PathIndices = new List<int>();
				Cost = cost;
			}
		}

		// Information needed for nodes within the queue.
		class QueueNode
		{
			public int NodeIndex { get; set; }
			public byte Bitmask { get; set; }
			public int Cost { get; set; }
			public List<int> PathIndices { get; set; }

			public QueueNode(int nodeIndex, byte bitmask, int cost, List<int> pathIndices)
			{
				NodeIndex = nodeIndex;
				Bitmask = bitmask;
				Cost = cost;
				PathIndices = pathIndices;
			}
		}
	}
}
