using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RBush.Test
{
	public class RBushTests
	{
		private class Point : ISpatialData, IComparable<Point>
		{
			public Envelope Envelope { get; set; }

			public int CompareTo(Point other)
			{
				if (this.Envelope.MinX != other.Envelope.MinX)
					return this.Envelope.MinX.CompareTo(other.Envelope.MinX);
				if (this.Envelope.MinY != other.Envelope.MinY)
					return this.Envelope.MinY.CompareTo(other.Envelope.MinY);
				if (this.Envelope.MaxX != other.Envelope.MaxX)
					return this.Envelope.MaxX.CompareTo(other.Envelope.MaxX);
				if (this.Envelope.MaxY != other.Envelope.MaxY)
					return this.Envelope.MaxY.CompareTo(other.Envelope.MaxY);
				return 0;
			}
		}

		static double[,] data =
		{
			{0, 0, 0, 0},{10, 10, 10, 10},{20, 20, 20, 20},{25, 0, 25, 0},{35, 10, 35, 10},{45, 20, 45, 20},{0, 25, 0, 25},{10, 35, 10, 35},
			{20, 45, 20, 45},{25, 25, 25, 25},{35, 35, 35, 35},{45, 45, 45, 45},{50, 0, 50, 0},{60, 10, 60, 10},{70, 20, 70, 20},{75, 0, 75, 0},
			{85, 10, 85, 10},{95, 20, 95, 20},{50, 25, 50, 25},{60, 35, 60, 35},{70, 45, 70, 45},{75, 25, 75, 25},{85, 35, 85, 35},{95, 45, 95, 45},
			{0, 50, 0, 50},{10, 60, 10, 60},{20, 70, 20, 70},{25, 50, 25, 50},{35, 60, 35, 60},{45, 70, 45, 70},{0, 75, 0, 75},{10, 85, 10, 85},
			{20, 95, 20, 95},{25, 75, 25, 75},{35, 85, 35, 85},{45, 95, 45, 95},{50, 50, 50, 50},{60, 60, 60, 60},{70, 70, 70, 70},{75, 50, 75, 50},
			{85, 60, 85, 60},{95, 70, 95, 70},{50, 75, 50, 75},{60, 85, 60, 85},{70, 95, 70, 95},{75, 75, 75, 75},{85, 85, 85, 85},{95, 95, 95, 95}
		};

		static Point[] points =
			Enumerable.Range(0, data.GetLength(0))
				.Select(i => new Point
				{
					Envelope = new Envelope
					{
						MinX = data[i, 0],
						MinY = data[i, 1],
						MaxX = data[i, 2],
						MaxY = data[i, 3],
					},
				})
				.ToArray();

		private List<Point> GetPoints(int cnt) =>
			Enumerable.Range(0, cnt)
				.Select(i => new Point
				{
					Envelope = new Envelope
					{
						MinX = i,
						MinY = i,
						MaxX = i,
						MaxY = i,
					},
				})
				.ToList();

		[Fact]
		public void RootLeafSplitWorks()
		{
			var data = GetPoints(12);

			var tree = new RBush<Point>();
			for (int i = 0; i < 9; i++)
				tree.Insert(data[i]);

			Assert.Equal(1, tree.root.Height);
			Assert.Equal(9, tree.root.Children.Count);
			Assert.True(tree.root.IsLeaf);

			Assert.Equal(0, tree.root.Envelope.MinX);
			Assert.Equal(0, tree.root.Envelope.MinY);
			Assert.Equal(8, tree.root.Envelope.MaxX);
			Assert.Equal(8, tree.root.Envelope.MaxY);

			tree.Insert(data[9]);

			Assert.Equal(2, tree.root.Height);
			Assert.Equal(2, tree.root.Children.Count);
			Assert.False(tree.root.IsLeaf);

			Assert.Equal(0, tree.root.Envelope.MinX);
			Assert.Equal(0, tree.root.Envelope.MinY);
			Assert.Equal(9, tree.root.Envelope.MaxX);
			Assert.Equal(9, tree.root.Envelope.MaxY);
		}

		[Fact]
		public void InsertTestData()
		{
			var tree = new RBush<Point>();
			foreach (var p in points)
				tree.Insert(p);

			Assert.Equal(points.Length, tree.Count);
			Assert.Equal(points.OrderBy(x => x), tree.Search().OrderBy(x => x));
		}

		[Fact]
		public void BulkLoadTestData()
		{
			var tree = new RBush<Point>();
			tree.BulkLoad(points);

			Assert.Equal(points.Length, tree.Count);
			Assert.Equal(points.OrderBy(x => x).ToList(), tree.Search().OrderBy(x => x).ToList());
		}

		[Fact]
		public void BulkLoadSplitsTreeProperly()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);
			tree.BulkLoad(points);

			Assert.Equal(points.Length * 2, tree.Count);
			Assert.Equal(4, tree.root.Height);
		}

		[Fact]
		public void BulkLoadMergesTreesProperly()
		{
			var smaller = GetPoints(10);
			var tree1 = new RBush<Point>(maxEntries: 4);
			tree1.BulkLoad(smaller);
			tree1.BulkLoad(points);

			var tree2 = new RBush<Point>(maxEntries: 4);
			tree2.BulkLoad(points);
			tree2.BulkLoad(smaller);

			Assert.True(tree1.Count == tree2.Count);
			Assert.True(tree1.root.Height == tree2.root.Height);

			var allPoints = points.Concat(smaller).OrderBy(x => x).ToList();
			Assert.Equal(allPoints, tree1.Search().OrderBy(x => x).ToList());
			Assert.Equal(allPoints, tree2.Search().OrderBy(x => x).ToList());
		}

		[Fact]
		public void SearchReturnsEmptyResultIfNothingFound()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			Assert.Equal(new Point[] { }, tree.Search(new Envelope { MinX = 200, MinY = 200, MaxX = 210, MaxY = 210 }));
		}

		[Fact]
		public void SearchReturnsMatchingResults()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			var searchEnvelope = new Envelope { MinX = 40, MinY = 20, MaxX = 80, MaxY = 70 };
			var shouldFindPoints = points
				.Where(p => p.Envelope.Intersects(searchEnvelope))
				.OrderBy(x => x)
				.ToList();
			var foundPoints = tree.Search(searchEnvelope)
				.OrderBy(x => x)
				.ToList();

			Assert.Equal(shouldFindPoints, foundPoints);
		}

		[Fact]
		public void BasicRemoveTest()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			var len = points.Length;

			tree.Delete(points[0]);
			tree.Delete(points[1]);
			tree.Delete(points[2]);

			tree.Delete(points[len - 1]);
			tree.Delete(points[len - 2]);
			tree.Delete(points[len - 3]);

			var shouldFindPoints = points
				.Skip(3).Take(len - 6)
				.OrderBy(x => x)
				.ToList();
			var foundPoints = tree.Search()
				.OrderBy(x => x)
				.ToList();

			Assert.Equal(shouldFindPoints, foundPoints);
		}

		[Fact]
		public void NonExistentItemCanBeDeleted()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			tree.Delete(new Point
			{
				Envelope = new Envelope
				{
					MinX = 13,
					MinY = 13,
					MaxX = 13,
					MaxY = 13,
				},
			});
		}

		[Fact]
		public void ClearWorks()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);
			tree.Clear();

			Assert.Equal(0, tree.Count);
			Assert.Equal(0, tree.root.Children.Count);
		}
	}
}
