using System.Collections.Immutable;

namespace RBush;

public partial class RBush<T>
{
	#region Sort Functions
	private static readonly IComparer<ISpatialData> s_compareMinX =
		Comparer<ISpatialData>.Create((x, y) => Comparer<double>.Default.Compare(x.Envelope.MinX, y.Envelope.MinX));
	private static readonly IComparer<ISpatialData> s_compareMinY =
		Comparer<ISpatialData>.Create((x, y) => Comparer<double>.Default.Compare(x.Envelope.MinY, y.Envelope.MinY));
	#endregion

	#region Search
	private List<ImmutableStack<ISpatialData>> DoPathSearch(in Envelope boundingBox)
	{
		if (!Root.Envelope.Intersects(boundingBox))
			return new List<ImmutableStack<ISpatialData>>();

		var intersections = new List<ImmutableStack<ISpatialData>>();
		var queue = new Queue<ImmutableStack<ISpatialData>>();
		queue.Enqueue(ImmutableStack<ISpatialData>.Empty.Push(Root));

		do
		{
			var current = queue.Dequeue();
			foreach (var c in (current.Peek() as Node).children)
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

	private List<T> DoSearch(in Envelope boundingBox)
	{
		if (!Root.Envelope.Intersects(boundingBox))
			return new List<T>();

		var intersections = new List<T>();
		var queue = new Queue<Node>();
		queue.Enqueue(Root);

		while (queue.Count != 0)
		{
			var item = queue.Dequeue();

			if (item.IsLeaf)
			{
				for (var index = 0; index < item.children.Count; index++)
				{
					var leafChildItem = item.children[index];
					if (leafChildItem.Envelope.Intersects(boundingBox))
						intersections.Add((T)leafChildItem);
				}
			}
			else
			{
				for (var index = 0; index < item.children.Count; index++)
				{
					var childNode = item.children[index];
					if (childNode.Envelope.Intersects(boundingBox))
						queue.Enqueue((Node)childNode);
				}
			}
		}

		return intersections;
	}
	#endregion

	#region Insert
	private List<Node> FindCoveringArea(in Envelope area, int depth)
	{
		var path = new List<Node>();
		var node = this.Root;
		var _area = area; //FIX CS1628

		while (true)
		{
			path.Add(node);
			if (node.IsLeaf || path.Count == depth) return path;

			node = node.children
				.Select(c => new { EnlargedArea = c.Envelope.Extend(_area).Area, c.Envelope.Area, Node = c as Node, })
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
			if (path[depth].children.Count > maxEntries)
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
		this.Root = new Node(new List<ISpatialData> { this.Root, newNode }, this.Root.Height + 1);

	private Node SplitNode(Node node)
	{
		SortChildren(node);

		var splitPoint = GetBestSplitIndex(node.children);
		var newChildren = node.children.Skip(splitPoint).ToList();
		node.RemoveRange(splitPoint, node.children.Count - splitPoint);
		return new Node(newChildren, node.Height);
	}

	#region SortChildren
	private void SortChildren(Node node)
	{
		node.children.Sort(s_compareMinX);
		var splitsByX = GetPotentialSplitMargins(node.children);
		node.children.Sort(s_compareMinY);
		var splitsByY = GetPotentialSplitMargins(node.children);

		if (splitsByX < splitsByY)
			node.children.Sort(s_compareMinX);
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
			return height == 1
				? new Node(data.GetRange(left, num), height)
				: new Node(
					new List<ISpatialData>
					{
						BuildNodes(data, left, right, height - 1, this.maxEntries),
					},
					height);
		}

		data.Sort(left, num, s_compareMinX);

		var nodeSize = (num + (maxEntries - 1)) / maxEntries;
		var subSortLength = nodeSize * (int)Math.Ceiling(Math.Sqrt(maxEntries));

		var children = new List<ISpatialData>(maxEntries);
		for (int subCounter = left; subCounter <= right; subCounter += subSortLength)
		{
			var subRight = Math.Min(subCounter + subSortLength - 1, right);
			data.Sort(subCounter, subRight - subCounter + 1, s_compareMinY);

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

	private List<T> GetAllChildren(List<T> list, Node n)
	{
		if (n.IsLeaf)
		{
			list.AddRange(
				n.children.Cast<T>());
		}
		else
		{
			foreach (var node in n.children.Cast<Node>())
				GetAllChildren(list, node);
		}

		return list;
	}

}
