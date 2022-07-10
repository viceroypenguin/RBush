namespace RBush;

/// <summary>
/// An implementation of the R-tree data structure for 2-d spatial indexing.
/// </summary>
/// <typeparam name="T">The type of elements in the index.</typeparam>
public partial class RBush<T> : ISpatialDatabase<T>, ISpatialIndex<T> where T : ISpatialData
{
	private const int DefaultMaxEntries = 9;
	private const int MinimumMaxEntries = 4;
	private const int MinimumMinEntries = 2;
	private const double DefaultFillFactor = 0.4;

	private readonly IEqualityComparer<T> _comparer;
	private readonly int _maxEntries;
	private readonly int _minEntries;

	/// <summary>
	/// The root of the R-tree.
	/// </summary>
	public Node Root { get; private set; }

	/// <summary>
	/// The bounding box of all elements currently in the data structure.
	/// </summary>
	public ref readonly Envelope Envelope => ref Root.Envelope;

	/// <summary>
	/// Initializes a new instance of the <see cref="RBush{T}"/> that is
	/// empty and has the default tree width and default <see cref="IComparer{T}"/>.
	/// </summary>
	public RBush()
		: this(DefaultMaxEntries, EqualityComparer<T>.Default) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="RBush{T}"/> that is
	/// empty and has a custom max number of elements per tree node
	/// and default <see cref="IEqualityComparer{T}"/>.
	/// </summary>
	/// <param name="maxEntries"></param>
	public RBush(int maxEntries)
		: this(maxEntries, EqualityComparer<T>.Default) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="RBush{T}"/> that is
	/// empty and has a custom max number of elements per tree node
	/// and a custom <see cref="IEqualityComparer{T}"/>.
	/// </summary>
	/// <param name="maxEntries"></param>
	/// <param name="comparer"></param>
	public RBush(int maxEntries, IEqualityComparer<T> comparer)
	{
		this._comparer = comparer;
		this._maxEntries = Math.Max(MinimumMaxEntries, maxEntries);
		this._minEntries = Math.Max(MinimumMinEntries, (int)Math.Ceiling(this._maxEntries * DefaultFillFactor));

		this.Clear();
	}

	/// <summary>
	/// Gets the number of items currently stored in the <see cref="RBush{T}"/>
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	/// Removes all elements from the <see cref="RBush{T}"/>.
	/// </summary>
	public void Clear()
	{
		this.Root = new Node(new List<ISpatialData>(), 1);
		this.Count = 0;
	}

	/// <summary>
	/// Get all of the elements within the current <see cref="RBush{T}"/>.
	/// </summary>
	/// <returns>
	/// A list of every element contained in the <see cref="RBush{T}"/>.
	/// </returns>
	public IReadOnlyList<T> Search() =>
		GetAllChildren(new List<T>(), this.Root);

	/// <summary>
	/// Get all of the elements from this <see cref="RBush{T}"/>
	/// within the <paramref name="boundingBox"/> bounding box.
	/// </summary>
	/// <param name="boundingBox">The area for which to find elements.</param>
	/// <returns>
	/// A list of the points that are within the bounding box
	/// from this <see cref="RBush{T}"/>.
	/// </returns>
	public IReadOnlyList<T> Search(in Envelope boundingBox) =>
		DoSearch(boundingBox);

	/// <summary>
	/// Adds an object to the <see cref="RBush{T}"/>
	/// </summary>
	/// <param name="item">
	/// The object to be added to <see cref="RBush{T}"/>.
	/// </param>
	public void Insert(T item)
	{
		Insert(item, this.Root.Height);
		this.Count++;
	}

	/// <summary>
	/// Adds all of the elements from the collection to the <see cref="RBush{T}"/>.
	/// </summary>
	/// <param name="items">
	/// A collection of items to add to the <see cref="RBush{T}"/>.
	/// </param>
	/// <remarks>
	/// For multiple items, this method is more performant than 
	/// adding items individually via <see cref="Insert(T)"/>.
	/// </remarks>
	public void BulkLoad(IEnumerable<T> items)
	{
		var data = items.Cast<ISpatialData>().ToArray();
		if (data.Length == 0) return;

		if (this.Root.IsLeaf &&
			this.Root.Items.Count + data.Length < _maxEntries)
		{
			foreach (var i in data)
				Insert((T)i);
			return;
		}

		if (data.Length < this._minEntries)
		{
			foreach (var i in data)
				Insert((T)i);
			return;
		}

		var dataRoot = BuildTree(data);
		this.Count += data.Length;

		if (this.Root.Items.Count == 0)
			this.Root = dataRoot;
		else if (this.Root.Height == dataRoot.Height)
		{
			if (this.Root.Items.Count + dataRoot.Items.Count <= this._maxEntries)
			{
				foreach (var isd in dataRoot.Items)
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

	/// <summary>
	/// Removes an object from the <see cref="RBush{T}"/>.
	/// </summary>
	/// <param name="item">
	/// The object to be removed from the <see cref="RBush{T}"/>.
	/// </param>
	public void Delete(T item)
	{
		var candidates = DoPathSearch(item.Envelope);

		foreach (var c in candidates
			.Where(c =>
			{
				if (c.Peek() is T _item)
					return _comparer.Equals(item, _item);
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

				if (n.Items.Count != 0)
					n.ResetEnvelope();
				else
					if (!path.IsEmpty) (path.Peek() as Node).Remove(n);
			}
		}
	}
}
