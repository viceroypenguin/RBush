namespace RBush;

public readonly record struct Envelope(double MinX, double MinY, double MaxX, double MaxY)
{
	public double Area => Math.Max(this.MaxX - this.MinX, 0) * Math.Max(this.MaxY - this.MinY, 0);
	public double Margin => Math.Max(this.MaxX - this.MinX, 0) + Math.Max(this.MaxY - this.MinY, 0);

	public Envelope Extend(in Envelope other) =>
		new(
			MinX: Math.Min(this.MinX, other.MinX),
			MinY: Math.Min(this.MinY, other.MinY),
			MaxX: Math.Max(this.MaxX, other.MaxX),
			MaxY: Math.Max(this.MaxY, other.MaxY));

	public Envelope Intersection(in Envelope other) =>
		new(
			MinX: Math.Max(this.MinX, other.MinX),
			MinY: Math.Max(this.MinY, other.MinY),
			MaxX: Math.Min(this.MaxX, other.MaxX),
			MaxY: Math.Min(this.MaxY, other.MaxY));

	public bool Contains(in Envelope other) =>
		this.MinX <= other.MinX &&
		this.MinY <= other.MinY &&
		this.MaxX >= other.MaxX &&
		this.MaxY >= other.MaxY;

	public bool Intersects(in Envelope other) =>
		this.MinX <= other.MaxX &&
		this.MinY <= other.MaxY &&
		this.MaxX >= other.MinX &&
		this.MaxY >= other.MinY;

	public static Envelope InfiniteBounds { get; } =
		new(
			MinX: double.NegativeInfinity,
			MinY: double.NegativeInfinity,
			MaxX: double.PositiveInfinity,
			MaxY: double.PositiveInfinity);

	public static Envelope EmptyBounds { get; } =
		new(
			MinX: double.PositiveInfinity,
			MinY: double.PositiveInfinity,
			MaxX: double.NegativeInfinity,
			MaxY: double.NegativeInfinity);
}
