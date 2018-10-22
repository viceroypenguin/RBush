using RBush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KnnUtility.Test")]

namespace RBush.KnnUtility
{
	/// <summary>
	/// Wrapper for ISpatialData storing distance to query point
	/// </summary>
	internal class SpatialDataWrapper
	{
		public ISpatialData SpatialData { get; private set; }

		public double SquaredDistanceToBox { get; private set; }


		public SpatialDataWrapper(ISpatialData spatialData, double x1, double y1,
			double? x2, double? y2)
		{
			SpatialData = spatialData;
			if (x2 == null || y2 == null)
			{
				CalcBoxSquaredDistToPoint(x1, y1);
			}
			else
			{
				CalcBoxSquaredDistToLineSegment(x1, y1, x2.Value, y2.Value);
			}

		}

		private void CalcBoxSquaredDistToPoint(double x, double y)
		{
			double dx = AxisDistToPoint(x, SpatialData.Envelope.MinX, SpatialData.Envelope.MaxX);
			double dy = AxisDistToPoint(y, SpatialData.Envelope.MinY, SpatialData.Envelope.MaxY);
			SquaredDistanceToBox = dx * dx + dy * dy;
		}

		private double AxisDistToPoint(double k, double min, double max)
		{
			return k < min ? min - k : k <= max ? 0 : k - max;
		}

		private void CalcBoxSquaredDistToLineSegment
			(double x1, double y1, double x2, double y2)
		{
			Envelope env = SpatialData.Envelope;
			double[] lineP1 = { x1, y1 };
			double[] lineP2 = { x2, y2 };
			//if stored item is point than calc distance from point to line segment
			if (env.MinX == env.MaxX && env.MinY == env.MaxY)
			{
				SquaredDistanceToBox = SquaredPointDistanceToSegment(new double[] { env.MinX , env.MinY }, lineP1, lineP2);
				return;
			}


			//Check if line intersects box
			//One of points is inside box?
			if ((x1 >= env.MinX && x1 <= env.MaxX && y1 >= env.MinY && y1 <= env.MaxY)
				|| (x2 >= env.MinX && x2 <= env.MaxX && y2 >= env.MinY && y2 <= env.MaxY))
			{
				//line intersects box
				SquaredDistanceToBox = 0;
				return;
			}
			//Line segment intesects at least one side of the rectangle?
			
			double[][] rectangleVerts =
				{ new double[] { env.MinX, env.MinY }, new double[] { env.MinX, env.MaxY },
					 new double[] { env.MaxX, env.MaxY }, new double[] {env.MaxX, env.MinY} };
			for (int i = 0; i < 4; i++)
			{
				double[] sideP1 = rectangleVerts[i];
				double[] sideP2 = rectangleVerts[(i + 1) % 4];
				if (!SameVectors(sideP1, sideP2) && LineSegmentsAreIntersecting(lineP1, lineP2, sideP1, sideP2))
				{
					//line intersects box
					SquaredDistanceToBox = 0;
					return;
				}
			}

			//line is outside of box
			//calc distance from linesegment to each rectangle vertex
			//calc distance from each rectangle side to both line segment endpoints
			//TODO: can it be optimized?
			//return min distance
			double minDist = double.MaxValue;
			for (int i = 0; i < 4; i++)
			{
				double[] sideP1 = rectangleVerts[i];
				double[] sideP2 = rectangleVerts[(i + 1) % 4];
				if (!SameVectors(sideP1, sideP2))
				{
					CalcMinDistance(sideP1, lineP1, lineP2, ref minDist);
					CalcMinDistance(sideP2, lineP1, lineP2, ref minDist);
					CalcMinDistance(lineP1, sideP1, sideP2, ref minDist);
					CalcMinDistance(lineP2, sideP1, sideP2, ref minDist);
				}
				
			}
			SquaredDistanceToBox = minDist;
		}

		private void CalcMinDistance(double[] sideP1, double[] lineP1, double[] lineP2, ref double minDist)
		{
			double dist = SquaredPointDistanceToSegment(sideP1, lineP1, lineP2);
			if (dist < minDist)
				minDist = dist;
		}

		private double IsLeft(double[] pt0, double[] pt1, double[] pt2)
		{
			return ((pt1[0] - pt0[0]) * (pt2[1] - pt0[1]) - (pt2[0] - pt0[0]) * (pt1[1] - pt0[1]));
		}

		private bool LineSegmentsAreIntersecting(double[] p1, double[] p2, double[] p3, double[] p4)
		{
			double p3IsLeft = IsLeft(p1, p2, p3);
			double p4IsLeft = IsLeft(p1, p2, p4);
			double p1IsLeft = IsLeft(p3, p4, p1);
			double p2IsLeft = IsLeft(p3, p4, p2);

			int p3IsLeftSign = Math.Sign(p3IsLeft);
			int p4IsLeftSign = Math.Sign(p4IsLeft);
			int p1IsLeftSign = Math.Sign(p1IsLeft);
			int p2IsLeftSign = Math.Sign(p2IsLeft);

			if ((p3IsLeftSign == 0 && p4IsLeftSign == 0) || (p1IsLeftSign == 0 && p2IsLeftSign == 0))
			{
				return false;
			}

			return
			p3IsLeftSign != p4IsLeftSign
			&& p1IsLeftSign != p2IsLeftSign;
		}

		/// <summary>
		/// http://geomalgorithms.com/a02-_lines.html
		/// </summary>
		/// <param name="p"></param>
		/// <param name="segp0"></param>
		/// <param name="segp1"></param>
		/// <returns></returns>
		private double SquaredPointDistanceToSegment(double[] p, double[] segp0, double[] segp1)
		{
			double[] v = Subtr(segp1, segp0);
			double[] w = Subtr(p, segp0);
			double c1 = Dot(w, v);
			if (c1 <= 0)
				return SquaredDist(p, segp0);
			double c2 = Dot(v, v);
			if (c2 <= c1)
				return SquaredDist(p, segp1);

			double b = c1 / c2;
			double[] Pb = Add(segp0, ScalarMult(v, b));
			return SquaredDist(p, Pb);
		}

		private bool SameVectors(double[] v, double[] w)
		{
			return v[0] == w[0] && v[1] == w[1];
		}

		private double[] Subtr(double[] v, double[] w)
		{
			return new double[] { v[0] - w[0], v[1] - w[1] };
		}

		private double[] Add(double[] v, double[] w)
		{
			return new double[] { v[0] + w[0], v[1] + w[1] };
		}

		private double[] ScalarMult(double[] v, double s)
		{
			return new double[] { v[0] * s, v[1] * s };
		}

		private double Dot(double[] v, double[] w)
		{
			return v[0] * w[0] + v[1] * w[1];
		}

		private double SquaredDist(double[] v, double[] w)
		{
			double[] d = Subtr(v, w);
			return Dot(d, d);
		}
	}
}
