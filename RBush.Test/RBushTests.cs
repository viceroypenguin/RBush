using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RBush.Test
{
	public class RBushTests
	{
		private class Point : ISpatialData, IComparable<Point>, IEquatable<Point>
		{
			private readonly Envelope _envelope;

			public Point(double minX, double minY, double maxX, double maxY)
			{
				_envelope = new Envelope(
					minX: minX,
					minY: minY,
					maxX: maxX,
					maxY: maxY);
			}

			public ref readonly Envelope Envelope => ref _envelope;

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

			public bool Equals(Point other) =>
				this._envelope == other._envelope;
		}

		private static readonly double[,] data =
		{
			{0, 0, 0, 0},       {10, 10, 10, 10},   {20, 20, 20, 20},   {25, 0, 25, 0},     {35, 10, 35, 10},   {45, 20, 45, 20},   {0, 25, 0, 25},     {10, 35, 10, 35},
			{20, 45, 20, 45},   {25, 25, 25, 25},   {35, 35, 35, 35},   {45, 45, 45, 45},   {50, 0, 50, 0},     {60, 10, 60, 10},   {70, 20, 70, 20},   {75, 0, 75, 0},
			{85, 10, 85, 10},   {95, 20, 95, 20},   {50, 25, 50, 25},   {60, 35, 60, 35},   {70, 45, 70, 45},   {75, 25, 75, 25},   {85, 35, 85, 35},   {95, 45, 95, 45},
			{0, 50, 0, 50},     {10, 60, 10, 60},   {20, 70, 20, 70},   {25, 50, 25, 50},   {35, 60, 35, 60},   {45, 70, 45, 70},   {0, 75, 0, 75},     {10, 85, 10, 85},
			{20, 95, 20, 95},   {25, 75, 25, 75},   {35, 85, 35, 85},   {45, 95, 45, 95},   {50, 50, 50, 50},   {60, 60, 60, 60},   {70, 70, 70, 70},   {75, 50, 75, 50},
			{85, 60, 85, 60},   {95, 70, 95, 70},   {50, 75, 50, 75},   {60, 85, 60, 85},   {70, 95, 70, 95},   {75, 75, 75, 75},   {85, 85, 85, 85},   {95, 95, 95, 95}
		};

		private static readonly Point[] points =
			Enumerable.Range(0, data.GetLength(0))
				.Select(i => new Point(
					minX: data[i, 0],
					minY: data[i, 1],
					maxX: data[i, 2],
					maxY: data[i, 3]))
				.ToArray();

		private List<Point> GetPoints(int cnt) =>
			Enumerable.Range(0, cnt)
				.Select(i => new Point(
					minX: i,
					minY: i,
					maxX: i,
					maxY: i))
				.ToList();

		[Fact]
		public void RootLeafSplitWorks()
		{
			var data = GetPoints(12);

			var tree = new RBush<Point>();
			for (var i = 0; i < 9; i++)
				tree.Insert(data[i]);

			Assert.Equal(1, tree.Root.Height);
			Assert.Equal(9, tree.Root.children.Count);
			Assert.True(tree.Root.IsLeaf);

			Assert.Equal(0, tree.Root.Envelope.MinX);
			Assert.Equal(0, tree.Root.Envelope.MinY);
			Assert.Equal(8, tree.Root.Envelope.MaxX);
			Assert.Equal(8, tree.Root.Envelope.MaxY);

			tree.Insert(data[9]);

			Assert.Equal(2, tree.Root.Height);
			Assert.Equal(2, tree.Root.children.Count);
			Assert.False(tree.Root.IsLeaf);

			Assert.Equal(0, tree.Root.Envelope.MinX);
			Assert.Equal(0, tree.Root.Envelope.MinY);
			Assert.Equal(9, tree.Root.Envelope.MaxX);
			Assert.Equal(9, tree.Root.Envelope.MaxY);
		}

		[Fact]
		public void InsertTestData()
		{
			var tree = new RBush<Point>();
			foreach (var p in points)
				tree.Insert(p);

			Assert.Equal(points.Length, tree.Count);
			Assert.Equal(points.OrderBy(x => x), tree.Search().OrderBy(x => x));

			Assert.Equal(
				points.Aggregate(Envelope.EmptyBounds, (e, p) => e.Extend(p.Envelope)),
				tree.Envelope);
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
			Assert.Equal(4, tree.Root.Height);
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
			Assert.True(tree1.Root.Height == tree2.Root.Height);

			var allPoints = points.Concat(smaller).OrderBy(x => x).ToList();
			Assert.Equal(allPoints, tree1.Search().OrderBy(x => x).ToList());
			Assert.Equal(allPoints, tree2.Search().OrderBy(x => x).ToList());
		}

		[Fact]
		public void SearchReturnsEmptyResultIfNothingFound()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			Assert.Equal(new Point[] { }, tree.Search(new Envelope(200, 200, 210, 210)));
		}

		[Fact]
		public void SearchReturnsMatchingResults()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			var searchEnvelope = new Envelope(40, 20, 80, 70);
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
			Assert.Equal(shouldFindPoints.Count, tree.Count);
			Assert.Equal(
				shouldFindPoints.Aggregate(Envelope.EmptyBounds, (e, p) => e.Extend(p.Envelope)),
				tree.Envelope);
		}

		[Fact]
		public void NonExistentItemCanBeDeleted()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);

			tree.Delete(new Point(13, 13, 13, 13));
			Assert.Equal(points.Length, tree.Count);
		}

		[Fact]
		public void Delete_TreeIsEmpty_ShouldNotThrow()
		{
			var tree = new RBush<Point>();

			tree.Delete(new Point(1, 1, 1, 1));

			Assert.Equal(0, tree.Count);
		}

		[Fact]
		public void Delete_DeletingLastPoint_ShouldNotThrow()
		{
			var tree = new RBush<Point>();
			Point p = new Point(1, 1, 1, 1);
			tree.Insert(p);

			tree.Delete(p);

			Assert.Equal(0, tree.Count);
		}

		[Fact]
		public void ClearWorks()
		{
			var tree = new RBush<Point>(maxEntries: 4);
			tree.BulkLoad(points);
			tree.Clear();

			Assert.Equal(0, tree.Count);
			Assert.Empty(tree.Root.children);
		}

		[Fact]
		public void TestSearchAfterInsert()
		{
			var maxEntries = 9;
			var tree = new RBush<Point>(maxEntries);

			var firstSet = points.Take(maxEntries);
			var firstSetEnvelope =
				firstSet.Aggregate(Envelope.EmptyBounds, (e, p) => e.Extend(p.Envelope));

			foreach (var p in firstSet)
				tree.Insert(p);

			Assert.Equal(firstSet.OrderBy(x => x), tree.Search(firstSetEnvelope).OrderBy(x => x));
		}

		[Fact]
		public void TestSearchAfterInsertWithSplitRoot()
		{
			var maxEntries = 4;
			var tree = new RBush<Point>(maxEntries);

			var numFirstSet = maxEntries * maxEntries + 2;  // Split-root will occur twice.
			var firstSet = points.Take(numFirstSet);

			foreach (var p in firstSet)
				tree.Insert(p);

			var numExtraPoints = 5;
			var extraPointsSet = points.Skip(points.Length - numExtraPoints);
			var extraPointsSetEnvelope =
				extraPointsSet.Aggregate(Envelope.EmptyBounds, (e, p) => e.Extend(p.Envelope));

			foreach (var p in extraPointsSet)
				tree.Insert(p);

			// first 10 entries and last 5 entries are completely mutually exclusive
			// so searching the bounds of the new set should only return the new set exactly
			Assert.Equal(extraPointsSet.OrderBy(x => x), tree.Search(extraPointsSetEnvelope).OrderBy(x => x));
		}

		[Fact]
		public void TestSearchAfterBulkLoadWithSplitRoot()
		{
			var maxEntries = 4;
			var tree = new RBush<Point>(maxEntries);

			var numFirstSet = maxEntries * maxEntries + 2;  // Split-root will occur twice.
			var firstSet = points.Take(numFirstSet);

			tree.BulkLoad(firstSet);

			var numExtraPoints = 5;
			var extraPointsSet = points.Skip(points.Length - numExtraPoints);
			var extraPointsSetEnvelope =
				extraPointsSet.Aggregate(Envelope.EmptyBounds, (e, p) => e.Extend(p.Envelope));

			tree.BulkLoad(extraPointsSet);

			// first 10 entries and last 5 entries are completely mutually exclusive
			// so searching the bounds of the new set should only return the new set exactly
			Assert.Equal(extraPointsSet.OrderBy(x => x), tree.Search(extraPointsSetEnvelope).OrderBy(x => x));
		}

		[Fact]
		public void AdditionalRemoveTest()
		{
			var tree = new RBush<Point>();
			var numDelete = 18;

			foreach (var p in points)
				tree.Insert(p);

			foreach (var p in points.Take(numDelete))
				tree.Delete(p);

			Assert.Equal(points.Length - numDelete, tree.Count);
			Assert.Equal(points.Skip(numDelete).OrderBy(x => x), tree.Search().OrderBy(x => x));
		}

		[Fact]
		public void BulkLoadAfterDeleteTest1()
		{
			var pts = GetPoints(20);
			var ptsDelete = pts.Take(18);
			var tree = new RBush<Point>(maxEntries: 4);

			tree.BulkLoad(pts);

			foreach (var item in ptsDelete)
				tree.Delete(item);

			tree.BulkLoad(ptsDelete);

			Assert.Equal(pts.Count, tree.Search().Count);
			Assert.Equal(pts.OrderBy(x => x).ToList(), tree.Search().OrderBy(x => x).ToList());
		}

		[Fact]
		public void BulkLoadAfterDeleteTest2()
		{
			var pts = GetPoints(20);
			var ptsDelete = pts.Take(4);
			var tree = new RBush<Point>(maxEntries: 4);

			tree.BulkLoad(pts);

			foreach (var item in ptsDelete)
				tree.Delete(item);

			tree.BulkLoad(ptsDelete);

			Assert.Equal(pts.Count, tree.Search().Count);
			Assert.Equal(pts.OrderBy(x => x).ToList(), tree.Search().OrderBy(x => x).ToList());
		}

		[Fact]
		public void InsertAfterDeleteTest1()
		{
			var pts = GetPoints(20);
			var ptsDelete = pts.Take(18);
			var tree = new RBush<Point>(maxEntries: 4);

			foreach (var item in pts)
				tree.Insert(item);

			foreach (var item in ptsDelete)
				tree.Delete(item);

			foreach (var item in ptsDelete)
				tree.Insert(item);

			Assert.Equal(pts.Count, tree.Search().Count);
			Assert.Equal(pts.OrderBy(x => x).ToList(), tree.Search().OrderBy(x => x).ToList());
		}

		[Fact]
		public void InsertAfterDeleteTest2()
		{
			var pts = GetPoints(20);
			var ptsDelete = pts.Take(4);
			var tree = new RBush<Point>(maxEntries: 4);

			foreach (var item in pts)
				tree.Insert(item);

			foreach (var item in ptsDelete)
				tree.Delete(item);

			foreach (var item in ptsDelete)
				tree.Insert(item);

			Assert.Equal(pts.Count, tree.Search().Count);
			Assert.Equal(pts.OrderBy(x => x).ToList(), tree.Search().OrderBy(x => x).ToList());
		}

		private readonly List<Point> missingEnvelopeTestData = new List<Point>
		{
			new Point(minX: 35.0457204123358, minY: 31.5946330633669, maxX: 35.1736414417038, maxY: 31.7658263429689),
			new Point(minX: 35.0011136524732, minY: 31.6701999643473, maxX: 35.0119650302309, maxY: 31.6763344627552),
			new Point(minX: 35.4519996266397, minY: 33.0521061332025, maxX: 35.6225745715679, maxY: 33.2873426178667),
			new Point(minX: 35.3963660077949, minY: 31.9833569998672, maxX: 35.609059834246, maxY: 32.6939307443726),
			new Point(minX: 34.8283506083251, minY: 32.2548085664601, maxX: 35.074434567496, maxY: 32.3931011267767),
			new Point(minX: 34.8331658736056, minY: 31.7799489556277, maxX: 35.0591449537042, maxY: 32.0096644072503),
			new Point(minX: 35.4232929081405, minY: 32.8928176841194, maxX: 35.6402606700131, maxY: 33.0831804221654),
			new Point(minX: 35.1547685550823, minY: 32.6409460084027, maxX: 35.3829851953318, maxY: 32.8357311630527),
			new Point(minX: 34.8921664127959, minY: 31.6053844677954, maxX: 35.0343017543245, maxY: 31.7780322047787),
			new Point(minX: 34.9263969975396, minY: 32.65352493197, maxX: 35.1011727083577, maxY: 32.8432505478028),
			new Point(minX: 35.2394825164923, minY: 31.8026823309661, maxX: 35.2967869133878, maxY: 31.8621074757081),
			new Point(minX: 35.3717873599347, minY: 32.9175807550062, maxX: 35.6478183130483, maxY: 33.1679615668549),
			new Point(minX: 35.196978533143, minY: 31.5634195882077, maxX: 35.6432919646234, maxY: 32.3029676465732),
			new Point(minX: 35.0004845245009, minY: 31.7630003816153, maxX: 35.1719739039302, maxY: 31.8400308568811),
			new Point(minX: 34.280847167853, minY: 31.0494793743498, maxX: 34.8906355571353, maxY: 31.5639320874003),
			new Point(minX: 34.6095401534748, minY: 31.7162092103111, maxX: 34.9053153865301, maxY: 31.9217629772612),
			new Point(minX: 34.836714064777, minY: 32.1214817541366, maxX: 34.9971028731628, maxY: 32.2220684750668),
			new Point(minX: 34.8718260788802, minY: 31.599197115334, maxX: 35.0378162094246, maxY: 31.8420049154987),
			new Point(minX: 35.1186400639205, minY: 31.8691974363715, maxX: 35.1868012907541, maxY: 31.8937001264381),
			new Point(minX: 34.7893345961988, minY: 32.1180328589326, maxX: 34.8912984464358, maxY: 32.2478719368633),
			new Point(minX: 35.0560255797127, minY: 32.6886762251772, maxX: 35.2038027609729, maxY: 32.8736600563471),
			new Point(minX: 34.5118108403562, minY: 31.482226934431, maxX: 35.034306207294, maxY: 31.7778320509551),
			new Point(minX: 34.7421834416939, minY: 32.0294016078206, maxX: 34.8522596224097, maxY: 32.1466473164001),
			new Point(minX: 35.2850369921205, minY: 31.736701022482, maxX: 35.3671187142204, maxY: 31.8582696930051),
			new Point(minX: 34.3920880329569, minY: 30.3321812616403, maxX: 35.2259162784014, maxY: 30.9730153115276),
			new Point(minX: 34.3430710039186, minY: 30.9079503533306, maxX: 35.3548545278468, maxY: 31.4431220155161),
			new Point(minX: 34.6081103075024, minY: 31.5923121167122, maxX: 35.1015149543026, maxY: 32.1534717814552),
			new Point(minX: 34.7988787882969, minY: 32.0164071891458, maxX: 34.9092712663857, maxY: 32.1086750906882),
			new Point(minX: 34.9359618915959, minY: 32.172467059147, maxX: 35.0574178548968, maxY: 32.283413702632),
			new Point(minX: 34.8656317039855, minY: 32.3738879752974, maxX: 35.0578412581373, maxY: 32.5522748453893),
			new Point(minX: 35.3422616609697, minY: 32.9728983905125, maxX: 35.8977567880068, maxY: 33.2954674685247),
			new Point(minX: 34.6588626003523, minY: 31.0609203542086, maxX: 34.9847215585919, maxY: 31.4067422387869),
			new Point(minX: 34.5795324959208, minY: 29.4630762907736, maxX: 35.2158119200309, maxY: 30.3333639093901),
			new Point(minX: 35.3205147130136, minY: 32.6967048307378, maxX: 35.646707739197, maxY: 32.9039553396515),
			new Point(minX: 35.1178267119206, minY: 30.3384402926262, maxX: 35.490247102555, maxY: 31.4709171748353),
			new Point(minX: 34.8226151100133, minY: 32.2534003894817, maxX: 36.0421996128722, maxY: 33.3817549409005),
			new Point(minX: 34.2676635415493, minY: 30.6224958789486, maxX: 34.6920096335759, maxY: 31.0234624221816),
			new Point(minX: 35.1817645082021, minY: 32.4848680398321, maxX: 35.5646264318558, maxY: 32.7613293133493),
			new Point(minX: 34.9166085325228, minY: 31.7671694138349, maxX: 35.1725615321371, maxY: 31.8549410969553),
			new Point(minX: 34.9337083845948, minY: 31.876626985909, maxX: 35.0264053464613, maxY: 32.0150065654742),
			new Point(minX: 34.835347572723, minY: 32.220362279336, maxX: 35.0109342273868, maxY: 32.3417009250043),
			new Point(minX: 35.2948832076848, minY: 30.9104180607267, maxX: 35.5915134856063, maxY: 31.5574185582711),
			new Point(minX: 34.7905250664059, minY: 32.1179577036489, maxX: 35.0581623045431, maxY: 32.3417009250043),
			new Point(minX: 34.956024319859, minY: 31.7012421676793, maxX: 35.0116424969789, maxY: 31.7745298632115),
			new Point(minX: 35.0202255429427, minY: 30.2435917751572, maxX: 35.4679175364723, maxY: 30.9740219831866),
			new Point(minX: 34.8966295938088, minY: 30.9115962957363, maxX: 35.4390784816745, maxY: 31.4930353034973),
			new Point(minX: 35.0662920752962, minY: 32.7971835290283, maxX: 35.4601689230693, maxY: 33.107112234543),
			new Point(minX: 34.7340665896918, minY: 31.9989943342548, maxX: 34.8204120930481, maxY: 32.0474591284795),
			new Point(minX: 34.6941456072459, minY: 31.8964808285729, maxX: 34.8640822052443, maxY: 32.0383069680269),
			new Point(minX: 34.8811115785741, minY: 31.6065949131837, maxX: 35.0267092460506, maxY: 31.7698965550383),
			new Point(minX: 34.8003277513142, minY: 32.052551536477, maxX: 34.8331150100439, maxY: 32.0806274386815),
			new Point(minX: 34.8449718901336, minY: 32.003746842197, maxX: 35.0354837295605, maxY: 32.1384163064534),
			new Point(minX: 34.6192253584107, minY: 31.7651896373863, maxX: 34.7668538532351, maxY: 31.8527172183214),
			new Point(minX: 34.9038898727945, minY: 32.4160192641257, maxX: 35.2017974842687, maxY: 32.7047870690854),
			new Point(minX: 34.5711602610893, minY: 30.484129839607, maxX: 34.9186022069881, maxY: 31.089343112111),
			new Point(minX: 34.1949967186793, minY: 30.3364832392137, maxX: 35.475053605904, maxY: 31.7707872010952),
			new Point(minX: 35.5779734622088, minY: 32.7292601351151, maxX: 35.8581674148078, maxY: 33.2881090529044),
			new Point(minX: 35.2749705330965, minY: 31.7397911324406, maxX: 35.3704140716109, maxY: 31.8401766909486),
			new Point(minX: 35.3673933021049, minY: 32.6843626605618, maxX: 35.9062174225714, maxY: 33.3147865589053),
			new Point(minX: 34.9008058486684, minY: 31.3746779045228, maxX: 35.5757203915414, maxY: 31.9598496573455),
			new Point(minX: 35.598379933386, minY: 32.6537719349482, maxX: 35.9300290636195, maxY: 33.0086059628293),
			new Point(minX: 34.6095401534748, minY: 31.7162092103111, maxX: 34.8405932932086, maxY: 31.9110715079679),
			new Point(minX: 34.8216158424473, minY: 32.0658349197875, maxX: 34.8603966863607, maxY: 32.1086750906882),
			new Point(minX: 34.7332714270174, minY: 31.9886947121707, maxX: 34.8215417657172, maxY: 32.046276160073),
			new Point(minX: 34.8149293733854, minY: 31.9417187138479, maxX: 34.9091599462782, maxY: 32.0574043905775),
			new Point(minX: 34.8449718901336, minY: 32.0178299374316, maxX: 35.0354837295605, maxY: 32.1394735385545),
			new Point(minX: 34.7988787882969, minY: 32.0178353029891, maxX: 34.8569010984257, maxY: 32.1055722745323),
			new Point(minX: 34.8280655613399, minY: 31.7799489556277, maxX: 35.0591449537042, maxY: 32.0377438786708),
			new Point(minX: 34.7408084826793, minY: 31.7662100576601, maxX: 34.9053153865301, maxY: 31.9217629772612),
			new Point(minX: 34.6847053378812, minY: 31.8964808285729, maxX: 34.8640822052443, maxY: 32.0105347066497),
			new Point(minX: 34.7701973085286, minY: 32.0901266819317, maxX: 34.8524324985417, maxY: 32.1473290327111),
			new Point(minX: 34.741988219981, minY: 32.0294093609064, maxX: 34.8140075719916, maxY: 32.1044330767071),
			new Point(minX: 34.835347572723, minY: 32.2091924053645, maxX: 35.0109342273868, maxY: 32.3417009250043),
			new Point(minX: 34.836714064777, minY: 32.1215021618748, maxX: 34.9971028731628, maxY: 32.2220684750668),
			new Point(minX: 34.7873539009184, minY: 32.1180328589326, maxX: 34.8883177683104, maxY: 32.2478719368633),
			new Point(minX: 34.92831830183, minY: 32.1710611024405, maxX: 35.0574178548968, maxY: 32.2903664069317),
			new Point(minX: 35.3205147130136, minY: 32.6967048307378, maxX: 35.6457702965872, maxY: 32.9039553396515),
			new Point(minX: 34.8952563054852, minY: 31.3899298627969, maxX: 35.5736607373982, maxY: 31.9625925083573),
			new Point(minX: 34.8449718901336, minY: 32.0178299374316, maxX: 35.0354837295605, maxY: 32.1384163064534),
			new Point(minX: 34.8890894442614, minY: 32.3883416928594, maxX: 35.2017974842687, maxY: 32.7047870690854),
			new Point(minX: 34.8656317039855, minY: 32.3738879752974, maxX: 35.0423849643375, maxY: 32.4979457789334),
			new Point(minX: 34.7701973085286, minY: 32.090031514185, maxX: 34.8524324985417, maxY: 32.1473290327111),
			new Point(minX: 34.993192032199, minY: 31.9080191148317, maxX: 35.5996892102095, maxY: 32.3212391412162),
			new Point(minX: 35.3963660077949, minY: 32.2180076624529, maxX: 35.609059834246, maxY: 32.6939307443726),
			new Point(minX: 34.9325067524752, minY: 32.0409113304089, maxX: 35.0356750377975, maxY: 32.1386430270217),
			new Point(minX: 35.043472159742, minY: 32.6886762251772, maxX: 35.2038027609729, maxY: 32.8736600563471),
			new Point(minX: 34.8449718901336, minY: 32.0178299374316, maxX: 34.9490546869212, maxY: 32.1297427854206),
			new Point(minX: 34.9263969975396, minY: 32.65352493197, maxX: 35.1011727083577, maxY: 32.8384413743296),
			new Point(minX: 35.1026228484405, minY: 32.6274637163331, maxX: 35.3827736665259, maxY: 32.8356933279248),
			new Point(minX: 34.8883898699046, minY: 32.3888924841112, maxX: 35.2011562856838, maxY: 32.6849848327049),
			new Point(minX: 34.4036190736879, minY: 31.4707834956166, maxX: 35.0902632353055, maxY: 32.3567064968128),
			new Point(minX: 34.6095401534748, minY: 31.7162092103111, maxX: 35.0591449537042, maxY: 32.0383069680269),
			new Point(minX: 34.7421834416939, minY: 32.003746842197, maxX: 35.0354837295605, maxY: 32.1466473164001),
			new Point(minX: 34.7905250664059, minY: 32.1179577036489, maxX: 35.0581623045431, maxY: 32.3417009250043)
		};

		[Fact]
		public void MissingEnvelopeTestInsertIndividually()
		{
			var tree = new RBush<Point>();
			foreach (var p in missingEnvelopeTestData)
				tree.Insert(p);

			var envelope = new Envelope(
				minX: 34.73274678,
				minY: 31.87729923,
				maxX: 34.73274678,
				maxY: 31.87729923);
			Assert.Equal(
				expected: tree.Search().Where(p => p.Envelope.Intersects(envelope)).Count(),
				actual: tree.Search(envelope).Count);
		}

		[Fact]
		public void TestBulk()
		{
			var tree = new RBush<Point>();
			tree.BulkLoad(missingEnvelopeTestData);

			var envelope = new Envelope(
				minX: 34.73274678,
				minY: 31.87729923,
				maxX: 34.73274678,
				maxY: 31.87729923);
			Assert.Equal(
				expected: tree.Search().Where(p => p.Envelope.Intersects(envelope)).Count(),
				actual: tree.Search(envelope).Count);
		}
	}
}
