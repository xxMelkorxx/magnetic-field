using System;
using System.Collections.Generic;
using System.Linq;

namespace MagneticField
{
	public enum TypeNode
	{
		Boundary,
		Grid,
		Magnets,
		None
	}

	public class Node : ICloneable
	{
		public PointD Pos;
		public double Value;
		public TypeNode Type;
		public List<Triangle> nTriangles;
		public List<Node> nNodes;

		public Node(PointD pos, TypeNode type = TypeNode.None, double value = 0)
		{
			Pos = pos;
			Type = type;
			Value = value;
			nTriangles = new List<Triangle>();
			nNodes = new List<Node>();
		}
		public double Distance(Node node)
		{
			return Math.Sqrt((Pos.X - node.Pos.X) * (Pos.X - node.Pos.X) + (Pos.Y - node.Pos.Y) * (Pos.Y - node.Pos.Y));
		}
		/// <summary>
		/// Проверка на то, что узел находиться внутри указанного прямоугольника.
		/// </summary>
		/// <param name="node">Узел, который проверяем.</param>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="C"></param>
		/// <param name="D"></param>
		/// <returns></returns>
		public bool InsideRectangle(PointD A, PointD B, PointD C, PointD D)
		{
			double y1(double x)
			{
				if (x >= B.X && x <= C.X) return (x - C.X) * (B.Y - C.Y) / (B.X - C.X) + C.Y;
				else if (x >= B.X && x <= D.X) return (x - D.X) * (C.Y - D.Y) / (C.X - D.X) + D.Y;
				else return 0;
			}
			double y2(double x)
			{
				if (x >= B.X && x <= A.X) return (x - A.X) * (B.Y - A.Y) / (B.X - A.X) + A.Y;
				else if (x >= A.X && x <= D.X) return (x - D.X) * (A.Y - D.Y) / (A.X - D.X) + D.Y;
				else return 0;
			}

			double Y1 = y1(Pos.X);
			double Y2 = y2(Pos.X);

			return Pos.Y <= Y1 && Pos.Y >= Y2;
		}

		/// <summary>
		/// Список треугольников, принадлежащих узлу.
		/// </summary>
		/// <param name="triangles"></param>
		/// <returns></returns>
		public List<Triangle> NeighborsTriangles(List<Triangle> triangles)
		{
			List<Triangle> neighborsTriangles = new List<Triangle>();
			for (int i = 0; i < triangles.Count; i++)
				if (Pos == triangles[i].P1.Pos || Pos == triangles[i].P2.Pos || Pos == triangles[i].P3.Pos)
					neighborsTriangles.Add(triangles[i]);
			return neighborsTriangles;
		}

		/// <summary>
		/// Список соседей узла.
		/// </summary>
		/// <param name="triangles"></param>
		/// <returns></returns>
		public List<Node> NeigborsNodes(List<Triangle> triangles)
        {
			List<Triangle> neighborsTriangles = NeighborsTriangles(triangles);
			List<Node> neighborsNodes = new List<Node>();
            for (int i = 0; i < neighborsTriangles.Count; i++)
            {
				if (Pos == neighborsTriangles[i].P1.Pos)
                {
					neighborsNodes.Add(neighborsTriangles[i].P2);
					neighborsNodes.Add(neighborsTriangles[i].P3);
                }
				if (Pos == neighborsTriangles[i].P2.Pos)
				{
					neighborsNodes.Add(neighborsTriangles[i].P1);
					neighborsNodes.Add(neighborsTriangles[i].P3);
				}
				if (Pos == neighborsTriangles[i].P3.Pos)
				{
					neighborsNodes.Add(neighborsTriangles[i].P1);
					neighborsNodes.Add(neighborsTriangles[i].P2);
				}
			}
			return neighborsNodes.Distinct().ToList();
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
