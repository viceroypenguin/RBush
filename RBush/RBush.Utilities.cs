using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace RBush
{
	public partial class RBush<T>
	{
		#region Sort Functions
		private static readonly IComparer<ISpatialData> CompareMinX =
			ProjectionComparer<ISpatialData>.Create(d => d.Envelope.MinX);
		private static readonly IComparer<ISpatialData> CompareMinY =
			ProjectionComparer<ISpatialData>.Create(d => d.Envelope.MinY);
		#endregion

		#region Search
		private List<ImmutableStack<ISpatialData>> DoSearch(in Envelope boundingBox)
		{
			var node = this.root;
			if (!node.Envelope.Intersects(boundingBox))
				return new List<ImmutableStack<ISpatialData>>();

			var intersections = new List<ImmutableStack<ISpatialData>>();
			var queue = new Queue<ImmutableStack<ISpatialData>>();
			queue.Enqueue(ImmutableStack<ISpatialData>.Empty.Push(node));

			do
			{
				var current = queue.Dequeue();
				foreach (var c in (current.Peek() as Node).Children)
				{
					if (c.Envelope.Intersects(boundingBox))
					{
						if (c is T)
							intersections.Add(current.Push(c));
						else
							queue.Enqueue(current.Push(c));
					}
				}
			} while (queue.Count != 0);

			return intersections;
		}
		#endregion

		#region Insert
		private List<Node> FindCoveringArea(in Envelope area, int depth)
		{
			var path = new List<Node>();
			var node = this.root;
			var _area = area; //FIX CS1628

			while (true)
			{
				path.Add(node);
				if (node.IsLeaf || path.Count == depth) return path;

				node = node.Children
					.Select(c => new { EnlargedArea = c.Envelope.Enlargement(_area).Area, c.Envelope.Area, Node = c as Node, })
					.OrderBy(x => x.EnlargedArea)
					.ThenBy(x => x.Area)
					.Select(x => x.Node)
					.First();
			}
		}

		private void Insert(ISpatialData data, int depth)
		{
			var path = FindCoveringArea(data.Envelope, depth);

			var insertNode = path.Last();
			insertNode.Add(data);

			while (--depth >= 0)
			{
				if (path[depth].Children.Count > maxEntries)
				{
					var newNode = SplitNode(path[depth]);
					if (depth == 0)
						SplitRoot(newNode);
					else
						path[depth - 1].Add(newNode);
				}
				else
					path[depth].ResetEnvelope();
			}
		}

		#region SplitNode
		private void SplitRoot(Node newNode) =>
			this.root = new Node(new List<ISpatialData> { this.root, newNode }, this.root.Height + 1);

		private Node SplitNode(Node node)
		{
			SortChildren(node);

			var splitPoint = GetBestSplitIndex(node.Children);
			var newChildren = node.Children.Skip(splitPoint).ToList();
			node.Children.RemoveRange(splitPoint, node.Children.Count - splitPoint);
			return new Node(newChildren, node.Height);
		}

		#region SortChildren
		private void SortChildren(Node node)
		{
			node.Children.Sort(CompareMinX);
			var splitsByX = GetPotentialSplitMargins(node.Children);
			node.Children.Sort(CompareMinY);
			var splitsByY = GetPotentialSplitMargins(node.Children);

			if (splitsByX < splitsByY)
				node.Children.Sort(CompareMinX);
		}

		private double GetPotentialSplitMargins(List<ISpatialData> children) =>
			GetPotentialEnclosingMargins(children) +
			GetPotentialEnclosingMargins(children.AsEnumerable().Reverse().ToList());

		private double GetPotentialEnclosingMargins(List<ISpatialData> children)
		{
			var envelope = Envelope.EmptyBounds;
			int i = 0;
			for (; i < minEntries; i++)
			{
				envelope = envelope.Extend(children[i].Envelope);
			}

			var totalMargin = envelope.Margin;
			for (; i < children.Count - minEntries; i++)
			{
				envelope = envelope.Extend(children[i].Envelope);
				totalMargin += envelope.Margin;
			}

			return totalMargin;
		}
		#endregion

		private int GetBestSplitIndex(List<ISpatialData> children)
		{
			return Enumerable.Range(minEntries, children.Count - minEntries)
				.Select(i =>
				{
					var leftEnvelope = GetEnclosingEnvelope(children.Take(i));
					var rightEnvelope = GetEnclosingEnvelope(children.Skip(i));

					var overlap = leftEnvelope.Intersection(rightEnvelope).Area;
					var totalArea = leftEnvelope.Area + rightEnvelope.Area;
					return new { i, overlap, totalArea };
				})
				.OrderBy(x => x.overlap)
				.ThenBy(x => x.totalArea)
				.Select(x => x.i)
				.First();
		}
		#endregion
		#endregion

		#region BuildTree
		private Node BuildTree(List<ISpatialData> data)
		{
			var treeHeight = GetDepth(data.Count);
			var rootMaxEntries = (int)Math.Ceiling(data.Count / Math.Pow(this.maxEntries, treeHeight - 1));
			return BuildNodes(data, 0, data.Count - 1, treeHeight, rootMaxEntries);
		}

		private int GetDepth(int numNodes) =>
			(int)Math.Ceiling(Math.Log(numNodes) / Math.Log(this.maxEntries));

		private Node BuildNodes(List<ISpatialData> data, int left, int right, int height, int maxEntries)
		{
			var num = right - left + 1;
			if (num <= maxEntries)
			{
				if (height == 1)
					return new Node(data.Skip(left).Take(num).ToList(), height);
				else
					return new Node(new List<ISpatialData> { BuildNodes(data, left, right, height - 1, this.maxEntries) }, height);
			}

			data.Sort(left, num, CompareMinX);

			var nodeSize = (num + (maxEntries - 1)) / maxEntries;
			var subSortLength = nodeSize * (int)Math.Ceiling(Math.Sqrt(maxEntries));

			var children = new List<ISpatialData>(maxEntries);
			for (int subCounter = left; subCounter <= right; subCounter += subSortLength)
			{
				var subRight = Math.Min(subCounter + subSortLength - 1, right);
				data.Sort(subCounter, subRight - subCounter + 1, CompareMinY);

				for (int nodeCounter = subCounter; nodeCounter <= subRight; nodeCounter += nodeSize)
				{
					children.Add(
						BuildNodes(
							data,
							nodeCounter,
							Math.Min(nodeCounter + nodeSize - 1, subRight),
							height - 1,
							this.maxEntries));
				}
			}

			return new Node(children, height);
		}
		#endregion

		private static Envelope GetEnclosingEnvelope(IEnumerable<ISpatialData> items)
		{
			var envelope = Envelope.EmptyBounds;
			foreach (var data in items)
			{
				envelope = envelope.Extend(data.Envelope);
			}
			return envelope;
		}

		private IEnumerable<T> GetAllChildren(Node n)
		{
			if (n.IsLeaf)
				return n.Children.Cast<T>();
			else
				return n.Children.Cast<Node>().SelectMany(GetAllChildren);
		}

	}
}
