using System;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	public class ThorupPathing : IPathingAlgorithm
	{
		INodeWeb _graph; //g
		INodeWeb _msbMinimumSpanningTree; //m
		ComponentTree _componentTree; //t
		UnvisitedDataStructure _unvisitedDataStructure; //u
		int _nodeCount; //n
		int _sourceNode; //source
		bool[] _visitedVertices; //s

		public void ConstructMinimumSpanningTree(INodeWeb graph, IMinimumSpanningTreeAlgorithm algorithm)
		{
			_graph = graph;
			_nodeCount = graph.Nodes.Length;
			_msbMinimumSpanningTree = graph;
		}

		public void ConstructOtherDataStructures(IUnionFindStructure<int, UnionFindNodePlaceholder> unionStructure, ISplitFindminStructure<int> findminStructure)
		{
			_visitedVertices = new bool[_nodeCount];
			_componentTree = ConstructTree(unionStructure);
			_unvisitedDataStructure = new UnvisitedDataStructure(_nodeCount, _componentTree, findminStructure);
		}

		public void CleanUpBetweenQueries(ISplitFindminStructure<int> findminStructure)
		{
			_visitedVertices = new bool[_nodeCount];
			DeepCleanUpNodes(_componentTree.Root);
			_unvisitedDataStructure.Containers = new ISplitFindminStructureElement<int>[_nodeCount];

			for (int i = 0; i < _nodeCount; i++)
				_unvisitedDataStructure.Containers[i] = findminStructure.Add(i, double.PositiveInfinity);

			findminStructure.Initialize();
		}

		public int[] FindShortestPaths(int source)
		{
			if (source < 0 || source >= _nodeCount)
			{
				Debug.LogError($"{source} is not a valid source vertex");
				return null;
			}

			_sourceNode = source;
			_visitedVertices[source] = true;

			foreach (IConnection connection in _graph.Nodes[source].Connections)
				_unvisitedDataStructure.DecreaseSuperDistance(Array.IndexOf(_graph.Nodes, connection.GetOtherNode(_graph.Nodes[source])), connection.Distance);

			Visit(_componentTree.Root);

			int[] shortestDistances = new int[_nodeCount];

			for (int i = 0; i < _nodeCount; i++)
				shortestDistances[i] = _unvisitedDataStructure.GetSuperDistance(i);

			shortestDistances[source] = 0;

			return shortestDistances;
		}

		public static int MostSignificantBit(int i)
		{
			const int numIntBits = sizeof(int) * 8;

			i |= i >> 1;
			i |= i >> 2;
			i |= i >> 4;
			i |= i >> 8;
			i |= i >> 16;

			i -= (i >> 1) & 0x55555555;
			i = ((i >> 2) & 0x33333333) + (i & 0x33333333);
			i = ((i >> 4) + i) & 0x0f0f0f0f;
			i += i >> 8;
			i += i >> 16;
			return 31 - numIntBits - (i & 0x0000003f);
		}

		ComponentTree ConstructTree(IUnionFindStructure<int, UnionFindNodePlaceholder> unionStructure)
		{
			UnionFindNodePlaceholder[] unionFindNodes = new UnionFindNodePlaceholder[_nodeCount];
			for (int i = 0; i < _nodeCount; i++)
				unionFindNodes[i] = unionStructure.MakeSet(i);

			IConnection[] connections = BucketSortMSTEdges();

			int[] c = new int[_nodeCount];
			int[] s = new int[_nodeCount];
			ComponentTree t = new ComponentTree(_nodeCount);
			int[] newC = new int[_nodeCount];
			bool[] representsInternalNode = new bool[_nodeCount];

			for (int i = 0; i < _nodeCount; i++)
			{
				c[i] = i;
				s[i] = 0;
			}

			int comp = 0;
			List<int> x = new List<int>();

			IConnection connection;
			int nodeAIndex;
			int nodeBIndex;
			int nodeAItem;
			int nodeBItem;

			for (int i = 0; i < connections.Length - 1; i++)
			{
				connection = connections[i];
				nodeAIndex = Array.IndexOf(_graph.Nodes, connection.NodeA);
				nodeBIndex = Array.IndexOf(_graph.Nodes, connection.NodeB);
				nodeAItem = unionStructure.Find(unionFindNodes[nodeAIndex]).GetItem();
				nodeBItem = unionStructure.Find(unionFindNodes[nodeBIndex]).GetItem();

				x.Add(nodeAItem);
				x.Add(nodeBItem);

				int newS =
					s[nodeAItem] +
					s[nodeBItem] +
					connection.Distance;

				unionStructure.Union(unionFindNodes[nodeAIndex], unionFindNodes[nodeBIndex]);

				_visitedVertices[nodeAItem] = newS > 0; //TODO: inspect this to see if it's what was intended

				if (MostSignificantBit(connection.Distance) < MostSignificantBit(connections[i + 1].Distance))
				{
					List<int> newX = new List<int>();

					foreach (int v in x)
						newX.Add(unionStructure.Find(unionFindNodes[v]).GetItem());

					foreach (int v in newX)
					{
						comp++;
						newC[v] = comp;
					}

					foreach (int v in x)
						if (!representsInternalNode[v])
							t.SetParentLeaf(c[v], newC[unionStructure.Find(unionFindNodes[v]).GetItem()]);
						else
							t.SetParentOfInternalNode(c[v], newC[unionStructure.Find(unionFindNodes[v]).GetItem()]);

					x.Clear();
				}
			}

			int index = connections.Length - 1;
			connection = connections[index];

			nodeAIndex = Array.IndexOf(_graph.Nodes, connection.NodeA);
			nodeBIndex = Array.IndexOf(_graph.Nodes, connection.NodeB);
			nodeAItem = unionStructure.Find(unionFindNodes[nodeAIndex]).GetItem();
			nodeBItem = unionStructure.Find(unionFindNodes[nodeBIndex]).GetItem();

			x.Add(nodeAItem);
			x.Add(nodeBItem);

			unionStructure.Union(unionFindNodes[nodeAIndex], unionFindNodes[nodeBIndex]);

			if (MostSignificantBit(connection.Distance) < MostSignificantBit(int.MaxValue))
			{
				List<int> newX = new List<int>();

				foreach (int v in x)
					newX.Add(unionStructure.Find(unionFindNodes[v]).GetItem());

				foreach (int v in newX)
				{
					comp++;
					newC[v] = comp;
				}

				foreach (int v in x)
				{
					if (!representsInternalNode[v])
						t.SetParentLeaf(c[v], newC[unionStructure.Find(unionFindNodes[v]).GetItem()]);
					else
						t.SetParentOfInternalNode(c[v], newC[unionStructure.Find(unionFindNodes[v]).GetItem()]);
				}

				foreach (int v in newX)
				{
					c[v] = newC[v];
					representsInternalNode[v] = true;
					t.SetDelta(c[v], (int)Math.Ceiling(s[v] / Math.Pow(2, MostSignificantBit(connection.Distance))));
					t.SetHierarchyLevel(c[v], MostSignificantBit(connection.Distance) + 1);
				}

				x.Clear();
			}

			return t;
		}

		IConnection[] BucketSortMSTEdges()
		{
			List<IConnection>[] buckets = new List<IConnection>[MostSignificantBit(int.MaxValue)];

			for (int i = 0; i < buckets.Length; i++)
				buckets[i] = new List<IConnection>();

			for (int i = 0; i < _msbMinimumSpanningTree.Connections.Length; i++)
			{
				foreach (IConnection connection in _msbMinimumSpanningTree.Connections)
				{
					if (Array.IndexOf(_graph.Connections, connection.NodeA) < Array.IndexOf(_graph.Connections, connection.NodeB))
						buckets[MostSignificantBit(connection.Distance)].Add(connection);
				}
			}

			IConnection[] a = new IConnection[_msbMinimumSpanningTree.Connections.Length / 2];
			int j = 0;

			for (int i = 0; i < buckets.Length; i++)
			{
				foreach (IConnection connection in buckets[i])
				{
					a[j] = connection;
					j++;
				}
			}

			return a;
		}

		void Expand(TreeNode treeNode)
		{
			treeNode.LowestBucket = _unvisitedDataStructure.GetMinDviMinus(treeNode) >> (treeNode.HierarchyLevel - 1);
			treeNode.HighestBucket = treeNode.LowestBucket + treeNode.Delta;

			treeNode.InitializeBuckets();
			_unvisitedDataStructure.DeleteRoot(treeNode);

			foreach (TreeNode child in treeNode.Children)
			{
				int min = _unvisitedDataStructure.GetMinDviMinus(child);

				if (min == -1)
					continue;

				if (!(child.Children.Count == 0 && child.Index == _sourceNode))
					treeNode.Bucket(child, min >> (treeNode.HierarchyLevel - 1));
				else
				{
					TreeNode current = treeNode;

					while (current != null)
					{
						current.UnvisitedCount--;
						current = current.Parent;
					}
				}
			}

			treeNode.Visited = true;
		}

		void Visit(int nodeIndex)
		{
			if (nodeIndex == _sourceNode)
				return;

			_visitedVertices[nodeIndex] = true;

			foreach (IConnection connection in _graph.Nodes[nodeIndex].Connections)
			{
				int newDistanceValue = _unvisitedDataStructure.GetSuperDistance(nodeIndex) + connection.Distance;
				int target = Array.IndexOf(_graph.Nodes, connection.GetOtherNode(_graph.Nodes[nodeIndex]));

				if (newDistanceValue > 0 && newDistanceValue < _unvisitedDataStructure.GetSuperDistance(target))
				{
					TreeNode root = _unvisitedDataStructure.GetUnvisitedRootOf(_componentTree, target);
					TreeNode parent = root.Parent;

					int oldValue = _unvisitedDataStructure.GetMinDviMinus(root) >> (parent.HierarchyLevel - 1);
					_unvisitedDataStructure.DecreaseSuperDistance(target, newDistanceValue);
					int newValue = _unvisitedDataStructure.GetMinDviMinus(root) >> (parent.HierarchyLevel - 1);

					if (oldValue == -1 || newValue < oldValue)
						root.MoveToBucket(parent, _unvisitedDataStructure.GetMinDviMinus(root) >> (parent.HierarchyLevel - 1));
				}
			}
		}

		void Visit(TreeNode treeNode)
		{
			TreeNode parent = treeNode.Parent;
			int j = parent == null ? 32 : parent.HierarchyLevel;

			if (treeNode.HierarchyLevel == 0)
			{
				Visit(treeNode.Index);

				TreeNode current = treeNode.Parent;
				while (current != null)
				{
					current.UnvisitedCount--;
					current = current.Parent;
				}

				treeNode.RemoveFromParentBucket();

				return;
			}

			if (!treeNode.Visited)
			{
				Expand(treeNode);
				treeNode.NextBucket = treeNode.LowestBucket;
			}

			int oldShiftedBucket = treeNode.NextBucket >> (j - treeNode.HierarchyLevel);

			while (treeNode.UnvisitedCount > 0 && treeNode.NextBucket >> (j - treeNode.HierarchyLevel) == oldShiftedBucket)
			{
				while (!(treeNode.GetBucket(treeNode.NextBucket).Count > 0))
					Visit(treeNode.GetBucket(treeNode.NextBucket)[0]);

				treeNode.NextBucket++;
			}

			if (treeNode.UnvisitedCount > 0)
				treeNode.MoveToBucket(parent, treeNode.NextBucket >> (j - treeNode.HierarchyLevel));
			else if (treeNode.Parent != null)
				treeNode.Parent.RemoveFromParentBucket();
		}

		void DeepCleanUpNodes(TreeNode node)
		{
			node.UnvisitedCount = node.UnvisitedInitialCount;
			node.Visited = false;

			foreach (TreeNode child in node.Children)
				DeepCleanUpNodes(child);
		}

		public INode[] GetPath(INodeWeb nodeWeb, INode startNode, INode endNode, INode[] mustVisitNodes = null)
		{
			return null;
		}
	}

	public class ComponentTree
	{
		public TreeNode[] Leaves { get; set; }
		public TreeNode Root { get; set; }

		TreeNode[] _internalNodes;

		public ComponentTree(int leafCount)
		{
			Leaves = new TreeNode[leafCount];

			for (int i = 0; i < leafCount; i++)
				Leaves[i] = new TreeNode(i);

			_internalNodes = new TreeNode[leafCount];
		}

		public void SetParentLeaf(int leaf, int parent)
		{
			SanitizeInternalNode(parent);

			Leaves[leaf].SetParent(_internalNodes[parent]);
			_internalNodes[parent].UnvisitedCount++;
			_internalNodes[parent].UnvisitedInitialCount++;
		}

		public void SetParentOfInternalNode(int internalNode, int parent)
		{
			SanitizeInternalNode(parent);

			_internalNodes[internalNode].SetParent(_internalNodes[parent]);
			_internalNodes[parent].UnvisitedCount += _internalNodes[internalNode].UnvisitedCount;
			_internalNodes[parent].UnvisitedInitialCount += _internalNodes[internalNode].UnvisitedInitialCount;
		}

		private void SanitizeInternalNode(int parent)
		{
			if (_internalNodes[parent] == null)
			{
				_internalNodes[parent] = new TreeNode(parent);
				Root = _internalNodes[parent];
			}
		}

		public void SetDelta(int _internalNode, int delta) => _internalNodes[_internalNode].Delta = delta;
		public void SetHierarchyLevel(int internalNode, int level) => _internalNodes[internalNode].HierarchyLevel = level;
	}

	public class TreeNode
	{
		public List<TreeNode> Children { get; set; }
		public TreeNode Parent { get; set; }
		public int Index { get; set; }
		public int Delta { get; set; }
		public int HierarchyLevel { get; set; }
		public int UnvisitedCount { get; set; }
		public int UnvisitedInitialCount { get; set; }
		public int MaxIndexUnvisited { get; set; }
		public bool Visited { get; set; }
		public int LowestBucket { get; set; }
		public int HighestBucket { get; set; }
		public int NextBucket { get; set; }

		List<TreeNode>[] _buckets;
		List<TreeNode> _containingBucket;
		int _bucketIndexOffset;

		public TreeNode(int index)
		{
			Index = index;
			Children = new List<TreeNode>();
		}

		public void MoveToBucket(TreeNode targetBucket, int index)
		{
			if (_containingBucket != null)
				_containingBucket.Remove(this);
			targetBucket.Bucket(this, index);
		}

		public void Bucket(TreeNode treeNode, int index)
		{
			if (index - _bucketIndexOffset < _buckets.Length)
			{
				_buckets[index - _bucketIndexOffset].Add(treeNode);
				treeNode._containingBucket = _buckets[index - _bucketIndexOffset];
			}
		}

		public void SetParent(TreeNode parent)
		{
			Parent = parent;
			parent.Children.Add(this);
		}

		public void InitializeBuckets()
		{
			_bucketIndexOffset = LowestBucket;
			_buckets = new List<TreeNode>[HighestBucket - LowestBucket + 1];

			for (int i = 0; i < HighestBucket - LowestBucket; i++)
				_buckets[i] = new List<TreeNode>();
		}

		public List<TreeNode> GetBucket(int index) => _buckets[index - _bucketIndexOffset];
		public void RemoveFromParentBucket() => _containingBucket.Remove(this);
	}

	public class UnvisitedDataStructure
	{
		public ISplitFindminStructureElement<int>[] Containers { get; set; }

		int[] _indexOfVertex;

		public UnvisitedDataStructure(int nodeCount, ComponentTree componentTree, ISplitFindminStructure<int> splitFindminStructure)
		{
			_indexOfVertex = new int[nodeCount];
			Containers = new ISplitFindminStructureElement<int>[nodeCount];

			InitializeMapping(componentTree.Root, 0);

			for (int i = 0; i < nodeCount; i++)
				Containers[i] = splitFindminStructure.Add(i, double.PositiveInfinity);

			splitFindminStructure.Initialize();
		}

		public int GetMinDviMinus(TreeNode treeNode)
		{
			double cost = Containers[treeNode.MaxIndexUnvisited].GetListCost();
			return double.IsPositiveInfinity(cost) ? -1 : (int)cost;
		}

		public TreeNode GetUnvisitedRootOf(ComponentTree componentTree, int leafIndex)
		{
			TreeNode current = componentTree.Leaves[leafIndex];
			while (!current.Parent.Visited)
				current = current.Parent;

			return current;
		}

		public void DeleteRoot(TreeNode treeNode)
		{
			foreach (TreeNode child in treeNode.Children)
				if (child != treeNode.Children[treeNode.Children.Count - 1])
					Containers[child.MaxIndexUnvisited].Split();
		}

		public int InitializeMapping(TreeNode rootNode, int index)
		{
			if (rootNode.Children.Count == 0)
			{
				_indexOfVertex[rootNode.Index] = index;
				rootNode.MaxIndexUnvisited = index;
				return index + 1;
			}
			else
			{
				int nextIndex = index;

				foreach (TreeNode child in rootNode.Children)
					nextIndex = InitializeMapping(child, nextIndex);

				rootNode.MaxIndexUnvisited = nextIndex - 1;

				return nextIndex;
			}
		}

		public void DecreaseSuperDistance(int nodeIndex, int newSuperDistance) => Containers[_indexOfVertex[nodeIndex]].DecreaseCost(newSuperDistance);
		public int GetSuperDistance(int nodeIndex) => (int)Containers[_indexOfVertex[nodeIndex]].GetCost();
	}

	public class UnionFindNodePlaceholder : IUnionFindNode<int>
	{
		public int GetItem() => throw new System.NotImplementedException();
	}

	public interface IMinimumSpanningTreeAlgorithm
	{
		public INodeWeb FindSolution(INodeWeb nodeWeb);
	}

	public interface ISplitFindminStructureElement<T>
	{
		ISplitFindminStructureElement<T> DecreaseCost(double newCost);
		ISplitFindminStructureElement<T> Split();
		double GetCost();
		double GetListCost();
	}
	
	public interface ISplitFindminStructure<T>
	{
		ISplitFindminStructureElement<T> Add(T item, double cost);
		void Initialize();
	}

	public interface IUnionFindNode<T>
	{
		T GetItem();
	}

	public interface IUnionFindStructure<T, U> where U: IUnionFindNode<T>
	{
		U MakeSet(T item);
		U Find(U item);
		void Union(U firstElement, U secondElement);
	}
}
