namespace RBush;

/// <summary>
/// Provides the base interface for the abstraction of
/// an index to find points within a bounding box.
/// </summary>
/// <typeparam name="T">The type of elements in the index.</typeparam>
public interface ISpatialIndex<out T>
{
	/// <summary>
	/// Get all of the elements within the current <see cref="ISpatialIndex{T}"/>.
	/// </summary>
	/// <returns>
	/// A list of every element contained in the <see cref="ISpatialIndex{T}"/>.
	/// </returns>
	IReadOnlyList<T> Search();

	/// <summary>
	/// Get all of the elements from this <see cref="ISpatialIndex{T}"/>
	/// within the <paramref name="boundingBox"/> bounding box.
	/// </summary>
	/// <param name="boundingBox">The area for which to find elements.</param>
	/// <returns>
	/// A list of the points that are within the bounding box
	/// from this <see cref="ISpatialIndex{T}"/>.
	/// </returns>
	IReadOnlyList<T> Search(in Envelope boundingBox);
}
