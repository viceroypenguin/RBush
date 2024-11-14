using System.Runtime.InteropServices;

namespace RBush;

/// <summary>
/// Extension methods for the <see cref="RBush{T}"/> object.
/// </summary>
public static class RBushExtensions
{
	[StructLayout(LayoutKind.Sequential)]
	private record struct ItemDistance<T>(T Item, double Distance);

	/// <summary>
	/// Get the <paramref name="k"/> nearest neighbors to a specific point.
	/// </summary>
	/// <typeparam name="T">The type of elements in the index.</typeparam>
	/// <param name="tree">An index of points.</param>
	/// <param name="k">The number of points to retrieve.</param>
	/// <param name="x">The x-coordinate of the center point.</param>
	/// <param name="y">The y-coordinate of the center point.</param>
	/// <param name="maxDistance">The maximum distance of points to be considered "near"; optional.</param>
	/// <param name="predicate">A function to test each element for a condition; optional.</param>
	/// <returns>The list of up to <paramref name="k"/> elements nearest to the given point.</returns>
	public static IReadOnlyList<T> Knn<T>(
		this ISpatialIndex<T> tree,
		int k,
		double x,
		double y,
		double? maxDistance = null,
		Func<T, bool>? predicate = null)
		where T : ISpatialData
	{
		ArgumentNullException.ThrowIfNull(tree);

		var items = maxDistance == null
			? tree.Search()
			: tree.Search(
				new Envelope(
					MinX: x - maxDistance.Value,
					MinY: y - maxDistance.Value,
					MaxX: x + maxDistance.Value,
					MaxY: y + maxDistance.Value));

		var distances = items
			.Select(i => new ItemDistance<T>(i, i.Envelope.DistanceTo(x, y)))
			.OrderBy(i => i.Distance)
			.AsEnumerable();

		if (maxDistance.HasValue)
			distances = distances.TakeWhile(i => i.Distance <= maxDistance.Value);

		if (predicate != null)
			distances = distances.Where(i => predicate(i.Item));

		if (k > 0)
			distances = distances.Take(k);

		return distances
			.Select(i => i.Item)
			.ToList();
	}

	/// <summary>
	/// Calculates the distance from the borders of an <see cref="Envelope"/>
	/// to a given point.
	/// </summary>
	/// <param name="envelope">The <see cref="Envelope"/> from which to find the distance</param>
	/// <param name="x">The x-coordinate of the given point</param>
	/// <param name="y">The y-coordinate of the given point</param>
	/// <returns>The calculated Euclidean shortest distance from the <paramref name="envelope"/> to a given point.</returns>
	public static double DistanceTo(this in Envelope envelope, double x, double y)
	{
		var dX = AxisDistance(x, envelope.MinX, envelope.MaxX);
		var dY = AxisDistance(y, envelope.MinY, envelope.MaxY);
		return Math.Sqrt((dX * dX) + (dY * dY));

		static double AxisDistance(double p, double min, double max) =>
		   p < min ? min - p :
		   p > max ? p - max :
		   0;
	}
}
