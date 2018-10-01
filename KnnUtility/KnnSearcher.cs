using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBush;

namespace KnnUtility
{
	/// <summary>
	/// Adapted from a Javascript library https://github.com/mourner/rbush-knn/blob/master/index.js
	/// </summary>
	public class KnnSearcher<T> where T : ISpatialData
	{
		private RBush<T> tree;
		public KnnSearcher(RBush<T> tree)
		{
			this.tree = tree;
		}

		public IEnumerable<T> KnnSearch(double x, double y, int n,
			Func<T, bool> predicate = null, double maxDist = -1)
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
					SpatialDataWrapper childDistData = new SpatialDataWrapper(child, x, y);//calc distance to box
					if (maxDist < 0 || childDistData.DistanceToBox <= maxDist)//check if distance less than max distance
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

		/// <summary>
		/// Wrapper for ISpatialData storing distance to query point
		/// </summary>
		private class SpatialDataWrapper
		{
			public ISpatialData SpatialData { get; private set; }

			/// <summary>
			/// Distance to box is quadratic!!!
			/// </summary>
			public double DistanceToBox { get; private set; }


			public SpatialDataWrapper(ISpatialData spatialData, double x, double y)
			{
				SpatialData = spatialData;
				CalcBoxDist(x, y);
			}

			private void CalcBoxDist(double x, double y)
			{
				double dx = AxisDist(x, SpatialData.Envelope.MinX, SpatialData.Envelope.MaxX);
				double dy = AxisDist(y, SpatialData.Envelope.MinY, SpatialData.Envelope.MaxY);
				DistanceToBox = dx * dx + dy * dy;//Distance to box is quadratic!!!
			}

			private double AxisDist(double k, double min, double max)
			{
				return k < min ? min - k : k <= max ? 0 : k - max;
			}
		}



		private class DistComparer : IComparer<SpatialDataWrapper>
		{
			public int Compare(SpatialDataWrapper n1, SpatialDataWrapper n2)
			{
				//TODO?: Если расстояния до прямоугольника равны нулю
				//(точка попала внутрь двух прямоугольников),
				//то сравнивать по расстоянию до центральной точки прямоугольника
				return n1.DistanceToBox.CompareTo(n2.DistanceToBox);
			}

		}
	}
}
