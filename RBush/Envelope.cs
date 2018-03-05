using System;

namespace RBush
{
	public readonly struct Envelope
	{
		public double MinX { get; }
		public double MinY { get; }
		public double MaxX { get; }
		public double MaxY { get; }

		public double Area => Math.Max(this.MaxX - this.MinX, 0) * Math.Max(this.MaxY - this.MinY, 0);
		public double Margin => Math.Max(this.MaxX - this.MinX, 0) + Math.Max(this.MaxY - this.MinY, 0);

		public Envelope(double minX, double minY, double maxX, double maxY)
		{
			this.MinX = minX;
			this.MinY = minY;
			this.MaxX = maxX;
			this.MaxY = maxY;
		}

		public Envelope Extend(in Envelope other) =>
			new Envelope(
				minX: Math.Min(this.MinX, other.MinX),
				minY: Math.Min(this.MinY, other.MinY),
				maxX: Math.Max(this.MaxX, other.MaxX),
				maxY: Math.Max(this.MaxY, other.MaxY));

		public Envelope Clone()
		{
			return new Envelope(this.MinX, this.MinY, this.MaxX, this.MaxY);
		}

		public Envelope Intersection(in Envelope other) =>
			new Envelope(
				minX: Math.Max(this.MinX, other.MinX),
				minY: Math.Max(this.MinY, other.MinY),
				maxX: Math.Min(this.MaxX, other.MaxX),
				maxY: Math.Min(this.MaxY, other.MaxY)
			);

		public Envelope Enlargement(in Envelope other)
		{
			var clone = this.Clone();
			clone.Extend(other);
			return clone;
		}

		public bool Contains(in Envelope other)
		{
			return
				this.MinX <= other.MinX &&
				this.MinY <= other.MinY &&
				this.MaxX >= other.MaxX &&
				this.MaxY >= other.MaxY;
		}

		public bool Intersects(in Envelope other)
		{
			return
				this.MinX <= other.MaxX &&
				this.MinY <= other.MaxY &&
				this.MaxX >= other.MinX &&
				this.MaxY >= other.MinY;
		}

		public static Envelope InfiniteBounds { get; } =
			new Envelope(
				minX: double.NegativeInfinity,
				minY: double.NegativeInfinity,
				maxX: double.PositiveInfinity,
				maxY: double.PositiveInfinity);

		public static Envelope EmptyBounds { get; } =
			new Envelope(
				minX: double.PositiveInfinity,
				minY: double.PositiveInfinity,
				maxX: double.NegativeInfinity,
				maxY: double.NegativeInfinity);
	}
}
