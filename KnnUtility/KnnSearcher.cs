using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBush;

namespace RBush.KnnUtility
{
	/// <summary>
	/// Adapted from a Javascript library https://github.com/mourner/rbush-knn/blob/master/index.js
	/// </summary>
	public static class KnnSearcher
	{
		/// <summary>
		/// Search k nearest neighbors to given point or to to given line segment
		/// </summary>
		/// <typeparam name="T">type of RBush items</typeparam>
		/// <param name="tree">RBush object</param>
		/// <param name="x1">x coordinate of query point.
		/// Or if x2 and y2 not null, x coordinate of first endpoint of query line segment</param>
		/// <param name="y1">y coordinate of query point.
		/// Or if x2 and y2 not null, y coordinate of first endpoint of query line segment</param>
		/// <param name="n">number of nearest neighbors to get</param>
		/// <param name="predicate">condition for neighbors</param>
		/// <param name="maxDist">max distance for nearest neighbors</param>
		/// <param name="x2">if not null is x coordinate of second endpoint of query line segment</param>
		/// <param name="y2">if not null is y coordinate of second endpoint of query line segment</param>
		/// <returns></returns>
		public static IReadOnlyList<T> KnnSearch<T>(this RBush<T> tree, double x1, double y1, int n,
			Func<T, bool> predicate = null, double maxDist = -1, double? x2 = null, double? y2 = null) where T : ISpatialData
		{
			if (maxDist > 0)
				maxDist = maxDist * maxDist;//All distances are quadratic!!!

			List<T> result = new List<T>();

			//priority queue
			C5.IntervalHeap<SpatialDataWrapper> queue = new C5.IntervalHeap<SpatialDataWrapper>(new DistComparer());

			RBush<T>.Node node = tree.Root;

			while (node != null)
			{
				foreach (ISpatialData child in node.Children)//for each child
				{
					SpatialDataWrapper childDistData = new SpatialDataWrapper(child, x1, y1, x2, y2);//calc distance to box
					if (maxDist < 0 || childDistData.SquaredDistanceToBox <= maxDist)//check if distance less than max distance
					{
						queue.Add(childDistData);//add to queue
					}
				}

				//dequeue all objects that are items stored in RBush
				while (queue.Count > 0 && queue.FindMin().SpatialData is T)
				{
					SpatialDataWrapper candidate = queue.DeleteMin();//this item goes to result
					T _candidate = (T)candidate.SpatialData;
					if (predicate == null || predicate.Invoke(_candidate))//if element satisfy the condition
					{
						result.Add(_candidate);//add to result
					}
					if (n > 0 && result.Count == n)//if the desired amount is already in the result
					{
						return result;//return result
					}
				}

				//process next element in queue
				if (queue.Count > 0)
				{
					node = queue.DeleteMin().SpatialData as RBush<T>.Node;
				}
				else
				{
					node = null;
				}
			}

			return result;
		}



		private class DistComparer : IComparer<SpatialDataWrapper>
		{
			public int Compare(SpatialDataWrapper n1, SpatialDataWrapper n2)
			{
				//TODO?: Если расстояния до прямоугольника равны нулю
				//(точка попала внутрь двух прямоугольников),
				//то сравнивать по расстоянию до центральной точки прямоугольника
				return n1.SquaredDistanceToBox.CompareTo(n2.SquaredDistanceToBox);
			}

		}
	}
}
