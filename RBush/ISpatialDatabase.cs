namespace RBush;

/// <summary>
/// Provides the base interface for the abstraction for
/// an updateable data store of elements on a 2-d plane.
/// </summary>
/// <typeparam name="T">The type of elements in the index.</typeparam>
public interface ISpatialDatabase<T> : ISpatialIndex<T>
{
	/// <summary>
	/// Adds an object to the <see cref="ISpatialDatabase{T}"/>
	/// </summary>
	/// <param name="item">
	/// The object to be added to <see cref="ISpatialDatabase{T}"/>.
	/// </param>
	void Insert(T item);

	/// <summary>
	/// Removes an object from the <see cref="ISpatialDatabase{T}"/>.
	/// </summary>
	/// <param name="item">
	/// The object to be removed from the <see cref="ISpatialDatabase{T}"/>.
	/// </param>
	/// <returns><see langword="bool" /> indicating whether the item was removed.</returns>
	bool Delete(T item);

	/// <summary>
	/// Removes all elements from the <see cref="ISpatialDatabase{T}"/>.
	/// </summary>
	void Clear();

	/// <summary>
	/// Adds all of the elements from the collection to the <see cref="ISpatialDatabase{T}"/>.
	/// </summary>
	/// <param name="items">
	/// A collection of items to add to the <see cref="ISpatialDatabase{T}"/>.
	/// </param>
	/// <remarks>
	/// For multiple items, this method is more performant than 
	/// adding items individually via <see cref="Insert(T)"/>.
	/// </remarks>
	void BulkLoad(IEnumerable<T> items);
}
