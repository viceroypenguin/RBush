namespace RBush;

public partial class RBush<T> : ISpatialDatabase<T>, ISpatialIndex<T> where T : ISpatialData
{
	private const int DefaultMaxEntries = 9;
	private const int MinimumMaxEntries = 4;
	private const int MinimumMinEntries = 2;
	private const double DefaultFillFactor = 0.4;

	private readonly IEqualityComparer<T> comparer;
	private readonly int maxEntries;
	private readonly int minEntries;

	public Node Root { get; private set; }
	public ref readonly Envelope Envelope => ref Root.Envelope;

	public RBush() : this(DefaultMaxEntries) { }
	public RBush(int maxEntries)
		: this(maxEntries, EqualityComparer<T>.Default) { }
	public RBush(int maxEntries, IEqualityComparer<T> comparer)
	{
		this.comparer = comparer;
		this.maxEntries = Math.Max(MinimumMaxEntries, maxEntries);
		this.minEntries = Math.Max(MinimumMinEntries, (int)Math.Ceiling(this.maxEntries * DefaultFillFactor));

		this.Clear();
	}

	public int Count { get; private set; }

	public void Clear()
	{
		this.Root = new Node(new List<ISpatialData>(), 1);
		this.Count = 0;
	}

	public IReadOnlyList<T> Search() => GetAllChildren(new List<T>(), this.Root);

	public IReadOnlyList<T> Search(in Envelope boundingBox) =>
		DoSearch(boundingBox);

	public void Insert(T item)
	{
		Insert(item, this.Root.Height);
		this.Count++;
	}

	public void BulkLoad(IEnumerable<T> items)
	{
		var data = items.Cast<ISpatialData>().ToArray();
		if (data.Length == 0) return;

		if (this.Root.IsLeaf &&
			this.Root.children.Count + data.Length < maxEntries)
		{
			foreach (var i in data)
				Insert((T)i);
			return;
		}

		if (data.Length < this.minEntries)
		{
			foreach (var i in data)
				Insert((T)i);
			return;
		}

		var dataRoot = BuildTree(data);
		this.Count += data.Length;

		if (this.Root.children.Count == 0)
			this.Root = dataRoot;
		else if (this.Root.Height == dataRoot.Height)
		{
			if (this.Root.children.Count + dataRoot.children.Count <= this.maxEntries)
			{
				foreach (var isd in dataRoot.children)
					this.Root.Add(isd);
			}
			else
				SplitRoot(dataRoot);
		}
		else
		{
			if (this.Root.Height < dataRoot.Height)
			{
				var tmp = this.Root;
				this.Root = dataRoot;
				dataRoot = tmp;
			}

			this.Insert(dataRoot, this.Root.Height - dataRoot.Height);
		}
	}

	public void Delete(T item)
	{
		var candidates = DoPathSearch(item.Envelope);

		foreach (var c in candidates
			.Where(c =>
			{
				if (c.Peek() is T _item)
					return comparer.Equals(item, _item);
				return false;
			}))
		{
			var path = c.Pop();
			(path.Peek() as Node).Remove(item);
			Count--;
			while (!path.IsEmpty)
			{
				path = path.Pop(out var e);
				var n = e as Node;

				if (n.children.Count != 0)
					n.ResetEnvelope();
				else
					if (!path.IsEmpty) (path.Peek() as Node).Remove(n);
			}
		}
	}
}
