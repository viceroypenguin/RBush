using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RBush;
using KnnUtility;


namespace KnnUtility.Test
{
	[TestClass]
	public class SpatialDataWrapperTests
	{
		[TestMethod]
		public void SquaredDistanceToSegmentCorrect()
		{
			Box box = Box.CreateBox(new double[] { 6344, 3312.5, 6351.5, 3316 });

			double[] line = new double[] { 6347.5, 3317.5, 6352.5, 3312.5 };
			SpatialDataWrapper spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6350, 3311.5, 6349.5, 3313.5 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6343, 3314.5, 6345.5, 3316.5 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6350.5, 3318.5, 6354, 3312 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 0.30477845766521439);

			line = new double[] { 6355, 3317, 6359, 3314.5 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 3.640054944640259);

			line = new double[] { 6343, 3314, 6337, 3314 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 1);

			line = new double[] { 6343, 3317, 6340.5, 3318 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 1.4142135623730952);

			line = new double[] { 6341, 3311, 6355, 3311 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 1.5);

			line = new double[] { 6341, 3312.5, 6345, 3311 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 1.0533703247653881);

			line = new double[] { 6356.5, 3318, 6356.5, 3309.5 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 5);

			line = new double[] { 6347, 3316.5, 6351, 3320.5 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 0.5);

			line = new double[] { 6338.5, 3319, 6350.5, 3321.5 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 4.058689656828709);

			line = new double[] { 6350, 3317, 6353, 3315 };
			spatial = new SpatialDataWrapper(box, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 0);



			Box point = Box.CreateBox(new double[] { 6365.5, 3314, 6365.5, 3314 });

			line = new double[] { 6365.5, 3314, 6367, 3315.5 };
			spatial = new SpatialDataWrapper(point, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6365, 3315, 6366.5, 3312 };
			spatial = new SpatialDataWrapper(point, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6365.5, 3315, 6365.5, 3317.5 };
			spatial = new SpatialDataWrapper(point, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 1);

			line = new double[] { 6365, 3314.5, 6364, 3314.5 };
			spatial = new SpatialDataWrapper(point, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 0.70710678118654757);

			line = new double[] { 6364.5, 3314, 6366, 3313 };
			spatial = new SpatialDataWrapper(point, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 0.55470019622535527);

			line = new double[] { 6366, 3315.5, 6367, 3313 };
			spatial = new SpatialDataWrapper(point, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox) == 1.0213243599736164);


			Box hor = Box.CreateBox(new double[] { 6372, 3314, 6375.5, 3314 });

			line = new double[] { 6374, 3315, 6375.5, 3313 };
			spatial = new SpatialDataWrapper(hor, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6372, 3316, 6372, 3314 };
			spatial = new SpatialDataWrapper(hor, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6375.5, 3314, 6377.5, 3314 };
			spatial = new SpatialDataWrapper(hor, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(spatial.SquaredDistanceToBox == 0);

			line = new double[] { 6373, 3314.5, 6374, 3315.5 };
			spatial = new SpatialDataWrapper(hor, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox).AlmostEquals(0.5));

			line = new double[] { 6371.5, 3313.5, 6370.5, 3313.5 };
			spatial = new SpatialDataWrapper(hor, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox).AlmostEquals(0.70710678));

			line = new double[] { 6372.5, 3313, 6374, 3313 };
			spatial = new SpatialDataWrapper(hor, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox).AlmostEquals(1));


			Box vert = Box.CreateBox(new double[] { 6381, 3315, 6381, 3313 });

			line = new double[] { 6382, 3314, 6380, 3313.5 };
			spatial = new SpatialDataWrapper(vert, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox).AlmostEquals(0));

			line = new double[] { 6380.5, 3316.5, 6382, 3315.5 };
			spatial = new SpatialDataWrapper(vert, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox).AlmostEquals(0.97072534));

			line = new double[] { 6381, 3311.5, 6382, 3310 };
			spatial = new SpatialDataWrapper(vert, line[0], line[1], line[2], line[3]);
			Assert.IsTrue(Math.Sqrt(spatial.SquaredDistanceToBox).AlmostEquals(1.5));
		}
	}


	public static class DoubleExt
	{
		public static bool AlmostEquals(this double d, double other)
		{
			return Math.Abs(d - other) <= 1E-8;
		}
	}
}
