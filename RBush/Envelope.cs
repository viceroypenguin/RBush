using System;

namespace RBush
{
	public class Envelope
	{
		public double MinX;
		public double MinY;
		public double MaxX;
		public double MaxY;

		public double Area => Math.Max(this.MaxX - this.MinX, 0) * Math.Max(this.MaxY - this.MinY, 0);
		public double Margin => Math.Max(this.MaxX - this.MinX, 0) + Math.Max(this.MaxY - this.MinY, 0);

		public void Extend(Envelope other)
		{
			this.MinX = Math.Min(this.MinX, other.MinX);
			this.MinY = Math.Min(this.MinY, other.MinY);
			this.MaxX = Math.Max(this.MaxX, other.MaxX);
			this.MaxY = Math.Max(this.MaxY, other.MaxY);
		}

		public Envelope Clone()
		{
			return new Envelope
			{
				MinX = this.MinX,
				MinY = this.MinY,
				MaxX = this.MaxX,
				MaxY = this.MaxY,
			};
		}

		public Envelope Intersection(Envelope other)
		{
			return new Envelope
			{
				MinX = Math.Max(this.MinX, other.MinX),
				MinY = Math.Max(this.MinY, other.MinY),
				MaxX = Math.Min(this.MaxX, other.MaxX),
				MaxY = Math.Min(this.MaxY, other.MaxY),
			};
		}

		public Envelope Enlargement(Envelope other)
		{
			var clone = this.Clone();
			clone.Extend(other);
			return clone;
		}

		public bool Contains(Envelope other)
		{
			return
				this.MinX <= other.MinX &&
				this.MinY <= other.MinY &&
				this.MaxX >= other.MaxX &&
				this.MaxY >= other.MaxY;
		}

		public bool Intersects(Envelope other)
		{
			return
				this.MinX <= other.MaxX &&
				this.MinY <= other.MaxY &&
				this.MaxX >= other.MinX &&
				this.MaxY >= other.MinY;
		}

		public static Envelope InfiniteBounds =>
			new Envelope
			{
				MinX = double.NegativeInfinity,
				MinY = double.NegativeInfinity,
				MaxX = double.PositiveInfinity,
				MaxY = double.PositiveInfinity,
			};

		public static Envelope EmptyBounds =>
			new Envelope
			{
				MinX = double.PositiveInfinity,
				MinY = double.PositiveInfinity,
				MaxX = double.NegativeInfinity,
				MaxY = double.NegativeInfinity,
			};
	}
}