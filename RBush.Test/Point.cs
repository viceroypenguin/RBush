namespace RBush.Test;

internal class Point : ISpatialData, IEquatable<Point>
{
	private readonly Envelope _envelope;

	public Point(double minX, double minY, double maxX, double maxY)
	{
		_envelope = new Envelope(
			MinX: minX,
			MinY: minY,
			MaxX: maxX,
			MaxY: maxY);
	}

	public ref readonly Envelope Envelope => ref _envelope;

	public bool Equals(Point? other) =>
		other != null
		&& Envelope.Equals(other.Envelope);

	public override bool Equals(object? obj) =>
		Equals(obj as Point);

	public override int GetHashCode() =>
		_envelope.GetHashCode();

	public double DistanceTo(double x, double y) =>
		Envelope.DistanceTo(x, y);

	public static Point[] CreatePoints(double[,] data) =>
		Enumerable.Range(0, data.GetLength(0))
			.Select(i => new Point(
				minX: data[i, 0],
				minY: data[i, 1],
				maxX: data[i, 2],
				maxY: data[i, 3]))
			.ToArray();
}
