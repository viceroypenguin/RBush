using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBush;
using RBush.KnnUtility;

namespace KnnUtility.Test
{
	[TestClass]
	public class LineKnnTests
	{
		//
		static double[,] data =
		{
			{6377.5, 3330.5, 6380.0, 3332.5}, {6374.5, 3333.5, 6375.0, 3335.5}, {6371.5, 3332.0, 6373.5, 3333.0},
			{6362.5, 3329.5, 6369.0, 3332.5}, {6374.5, 3327.0, 6380.5, 3329.5}, {6385.0, 3332.5, 6388.0, 3335.0},
			{6388.5, 3338.5, 6390.5, 3342.5}, {6379.5, 3336.0, 6381.5, 3339.0}, {6375.5, 3339.5, 6377.5, 3342.0},
			{6364.0, 3354.0, 6364.0, 3354.0}, {6362.5, 3353.5, 6362.5, 3353.5}, {6361.0, 3352.5, 6361.0, 3352.5},
			{6363.0, 3351.0, 6363.0, 3351.0}, {6365.0, 3351.5, 6365.0, 3351.5}, {6363.5, 3352.0, 6363.5, 3352.0},
			{6379.5, 3343.5, 6382.0, 3349.0}, {6372.5, 3347.5, 6377.0, 3348.0}, {6378.5, 3348.5, 6379.5, 3351.0},
			{6378.0, 3345.5, 6379.5, 3348.0}, {6375.5, 3348.5, 6378.0, 3350.0}, {6375.0, 3344.0, 6377.5, 3347.0},
			{6371.0, 3336.5, 6372.0, 3340.0}, {6367.0, 3342.0, 6368.0, 3344.0}, {6364.5, 3340.5, 6368.0, 3341.0},
			{6360.5, 3340.0, 6361.5, 3342.0}, {6361.0, 3339.0, 6363.0, 3340.0}, {6363.5, 3337.5, 6365.0, 3340.0},
			{6357.0, 3342.0, 6363.5, 3343.0}, {6364.0, 3342.0, 6365.0, 3344.5}, {6359.0, 3343.0, 6362.5, 3345.0},
			{6362.0, 3340.5, 6363.0, 3341.5}, {6358.5, 3337.5, 6360.5, 3341.5}
		};

		static Box[] boxes =
			Box.CreateBoxes(data);

		[TestMethod]
		public void FindsNNeighbors1()
		{
			RBush<Box> bush = new RBush<Box>();
			bush.BulkLoad(boxes);
			IEnumerable<Box> result = bush.KnnSearch(6362, 3343.5, 10, x2: 6360.75, y2: 3344);
			Box[] mustBeReturned = Box.CreateBoxes(new double[,]
			{{6359.0, 3343.0, 6362.5, 3345.0}, {6357.0, 3342.0, 6363.5, 3343.0}, {6360.5, 3340.0, 6361.5, 3342.0},
			{6364.0, 3342.0, 6365.0, 3344.5}, {6362.0, 3340.5, 6363.0, 3341.5},{6358.5, 3337.5, 6360.5, 3341.5},
			{6361.0, 3339.0, 6363.0, 3340.0},{6364.5, 3340.5, 6368.0, 3341.0},{6363.5, 3337.5, 6365.0, 3340.0},
			{6367.0, 3342.0, 6368.0, 3344.0},
			});
			Assert.IsTrue(mustBeReturned.Length == result.Count());


			Assert.IsTrue(result.ElementAt(0).CompareTo(mustBeReturned[0]) == 0);
			Assert.IsTrue(result.ElementAt(1).CompareTo(mustBeReturned[1]) == 0);
			Assert.IsTrue(result.ElementAt(2).CompareTo(mustBeReturned[2]) == 0);
			Assert.IsTrue(result.ElementAt(3).CompareTo(mustBeReturned[3]) == 0
				|| result.ElementAt(3).CompareTo(mustBeReturned[4]) == 0);
			Assert.IsTrue(result.ElementAt(4).CompareTo(mustBeReturned[3]) == 0
				|| result.ElementAt(4).CompareTo(mustBeReturned[4]) == 0);
			Assert.IsTrue(result.ElementAt(5).CompareTo(mustBeReturned[5]) == 0);

			Assert.IsTrue(result.ElementAt(6).CompareTo(mustBeReturned[6]) == 0);
			Assert.IsTrue(result.ElementAt(7).CompareTo(mustBeReturned[7]) == 0);
			Assert.IsTrue(result.ElementAt(8).CompareTo(mustBeReturned[8]) == 0);
			Assert.IsTrue(result.ElementAt(9).CompareTo(mustBeReturned[9]) == 0);
		}


		[TestMethod]
		public void FindsNNeighbors2()
		{
			RBush<Box> bush = new RBush<Box>();
			bush.BulkLoad(boxes);
			IEnumerable<Box> result = bush.KnnSearch(6361.5, 3352.5, 6, x2: 6367, y2: 3354);
			Box[] mustBeReturned = Box.CreateBoxes(new double[,]
			{{6361.0, 3352.5, 6361.0, 3352.5}, {6362.5, 3353.5, 6362.5, 3353.5}, {6364.0, 3354.0, 6364.0, 3354.0},
			{6363.5, 3352.0, 6363.5, 3352.0}, {6363.0, 3351.0, 6363.0, 3351.0}, {6365.0, 3351.5, 6365.0, 3351.5},
			});

			Assert.IsTrue(mustBeReturned.Length == result.Count());
			Assert.IsTrue(result.ElementAt(0).CompareTo(mustBeReturned[0]) == 0);
			Assert.IsTrue(result.ElementAt(1).CompareTo(mustBeReturned[1]) == 0);
			Assert.IsTrue(result.ElementAt(2).CompareTo(mustBeReturned[2]) == 0);
			Assert.IsTrue(result.ElementAt(3).CompareTo(mustBeReturned[3]) == 0);
			Assert.IsTrue(result.ElementAt(4).CompareTo(mustBeReturned[4]) == 0);
			Assert.IsTrue(result.ElementAt(5).CompareTo(mustBeReturned[5]) == 0);
		}

		[TestMethod]
		public void FindAllNeighborsForMaxDistance1()
		{
			RBush<Box> bush = new RBush<Box>();
			bush.BulkLoad(boxes);
			IEnumerable<Box> result = bush.KnnSearch(6377.5, 3351.5, 0, maxDist: 4, x2: 6379, y2: 3350.5);
			Box[] mustBeReturned = Box.CreateBoxes(new double[,]
			{
				{6378.5, 3348.5, 6379.5, 3351.0}, {6375.5, 3348.5, 6378.0, 3350.0}, {6379.5, 3343.5, 6382.0, 3349.0},
				{6378.0, 3345.5, 6379.5, 3348.0}, {6372.5, 3347.5, 6377.0, 3348.0}, {6375.0, 3344.0, 6377.5, 3347.0},
			});

			Assert.IsTrue(mustBeReturned.Length == result.Count());
			Assert.IsTrue(result.ElementAt(0).CompareTo(mustBeReturned[0]) == 0);
			Assert.IsTrue(result.ElementAt(1).CompareTo(mustBeReturned[1]) == 0);
			Assert.IsTrue(result.ElementAt(2).CompareTo(mustBeReturned[2]) == 0);
			Assert.IsTrue(result.ElementAt(3).CompareTo(mustBeReturned[3]) == 0);
			Assert.IsTrue(result.ElementAt(4).CompareTo(mustBeReturned[4]) == 0);
			Assert.IsTrue(result.ElementAt(5).CompareTo(mustBeReturned[5]) == 0);
		}


		[TestMethod]
		public void FindAllNeighborsForMaxDistance2()
		{
			RBush<Box> bush = new RBush<Box>();
			bush.BulkLoad(boxes);
			IEnumerable<Box> result = bush.KnnSearch(6373, 3324, 0, maxDist: 8, x2: 6396.5, y2: 3336);
			Box[] mustBeReturned = Box.CreateBoxes(new double[,]
			{
				{6374.5, 3327.0, 6380.5, 3329.5}, {6385.0, 3332.5, 6388.0, 3335.0}, {6377.5, 3330.5, 6380.0, 3332.5},
				{6388.5, 3338.5, 6390.5, 3342.5}, {6362.5, 3329.5, 6369.0, 3332.5}, {6379.5, 3336.0, 6381.5, 3339.0},
				{6371.5, 3332.0, 6373.5, 3333.0}, {6374.5, 3333.5, 6375.0, 3335.5},
			});

			Assert.IsTrue(mustBeReturned.Length == result.Count());
			Assert.IsTrue(result.ElementAt(0).CompareTo(mustBeReturned[0]) == 0);
			Assert.IsTrue(result.ElementAt(1).CompareTo(mustBeReturned[1]) == 0);
			Assert.IsTrue(result.ElementAt(2).CompareTo(mustBeReturned[2]) == 0);
			Assert.IsTrue(result.ElementAt(3).CompareTo(mustBeReturned[3]) == 0);
			Assert.IsTrue(result.ElementAt(4).CompareTo(mustBeReturned[4]) == 0);
			Assert.IsTrue(result.ElementAt(5).CompareTo(mustBeReturned[5]) == 0);
			Assert.IsTrue(result.ElementAt(6).CompareTo(mustBeReturned[6]) == 0);
		}

		[TestMethod]
		public void FindNNeighborsForMaxDistance()
		{
			RBush<Box> bush = new RBush<Box>();
			bush.BulkLoad(boxes);
			IEnumerable<Box> result = bush.KnnSearch(6373, 3324, 3, maxDist: 8, x2: 6396.5, y2: 3336);
			Box[] mustBeReturned = Box.CreateBoxes(new double[,]
			{
				{6374.5, 3327.0, 6380.5, 3329.5}, {6385.0, 3332.5, 6388.0, 3335.0}, {6377.5, 3330.5, 6380.0, 3332.5}
			});

			Assert.IsTrue(mustBeReturned.Length == result.Count());
			Assert.IsTrue(result.ElementAt(0).CompareTo(mustBeReturned[0]) == 0);
			Assert.IsTrue(result.ElementAt(1).CompareTo(mustBeReturned[1]) == 0);
			Assert.IsTrue(result.ElementAt(2).CompareTo(mustBeReturned[2]) == 0);
		}




		static Box[] richData
			= Box.CreateBoxes((new double[,]
			{
				{6387.5, 3349.5, 6389.5, 3352.0}, {6390.0, 3350.5, 6391.5, 3353.5}, {6393.8, 3354.0, 6395.3, 3356.0},
				{6388.5, 3346.0, 6390.5, 3348.5}, {6391.0, 3347.5, 6393.0, 3349.5}, {6393.5, 3345.0, 6395.5, 3348.5},
			}));

		static LineKnnTests()
		{
			for (int i = 0; i < richData.Length; i++)
			{
				richData[i].Version = i + 1;
			}
		}

		[TestMethod]
		public void FindNeighborsThatSatisfyAGivenPredicate()
		{
			RBush<Box> bush = new RBush<Box>();
			bush.BulkLoad(richData);

			IEnumerable<Box> result = bush
				.KnnSearch(6386.5, 3349.5, 1, b => b.Version > 2, x2: 6393, y2: 3353.5);

			if (result.Count() == 1)
			{
				Box item = result.First();
				if (item.Envelope.MinX == 6393.8 && item.Envelope.MinY == 3354.0
					&& item.Envelope.MaxX == 6395.3 && item.Envelope.MaxY == 3356.0
					&& item.Version == 3)
				{
					//Test passes. Found the correct item
				}
				else
				{
					Assert.Fail("Could not find the correct item");
				}
			}
			else
			{
				Assert.Fail("Could not find the correct item");
			}

		}
	}
}
