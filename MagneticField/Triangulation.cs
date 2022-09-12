using System;
using System.Linq;
using System.Collections.Generic;

namespace MagneticField
{
	public class Triangulation
	{
		/// <summary>
		/// Список узлов.
		/// </summary>
		public List<Node> nodes;
		/// <summary>
		/// Список треугольников.
		/// </summary>
		public List<Triangle> triangles;
		private readonly Random rnd;

		private double dx;
		private double dy;

		public Triangulation()
		{
			rnd = new Random(DateTime.Now.Millisecond);
			nodes = new List<Node>();
			triangles = new List<Triangle>();
		}

		/// <summary>
		/// Генерация узлов сетки.
		/// </summary>
		/// <param name="nodesX">Узлов по Х.</param>
		/// <param name="nodesY">Узлов по Y.</param>
		public void GenerateNodesGrid(int nodesX, int nodesY)
		{
			dx = 1.0 / nodesX;
			dy = 1.0 / nodesY;

			// Генерация сверхструктуры.
			nodes.Add(new Node(new PointD(0, 0), TypeNode.Boundary, (1 + 2 * rnd.NextDouble()) * 1e-5));
			nodes.Add(new Node(new PointD(0, 1), TypeNode.Boundary, (1 + 2 * rnd.NextDouble()) * 1e-5));
			nodes.Add(new Node(new PointD(1, 0), TypeNode.Boundary, (1 + 2 * rnd.NextDouble()) * 1e-5));
			nodes.Add(new Node(new PointD(1, 1), TypeNode.Boundary, (1 + 2 * rnd.NextDouble()) * 1e-5));

			// Добавление прямоугольной решётки.
			for (int i = 0; i < nodesX + 1; i++)
				for (int j = 0; j < nodesY + 1; j++)
				{
					if ((i == 0 && j == 0) || (i == 0 && j == nodesY) || (i == nodesX && j == 0) || (i == nodesX && j == nodesY))
						continue;
					// Смещение узлов.
					double rx = (-1.0 + 2.0 * rnd.NextDouble()) * dx * 0.001;
					double ry = (-1.0 + 2.0 * rnd.NextDouble()) * dy * 0.001;

					double x = i * dx + rx;
					double y = j * dy + ry;

					if (x < 0)
						x = i * dx + Math.Abs(rx);
					else if (x > 1)
						x = i * dx - Math.Abs(rx);
					if (y < 0)
						y = j * dy + Math.Abs(ry);
					else if (y > 1)
						y = j * dy - Math.Abs(ry);

					if (i == 0 || j == 0 || i == nodesX || j == nodesY)
						nodes.Add(new Node(new PointD(x, y), TypeNode.Boundary, (1 + 2 * rnd.NextDouble()) * 1e-5));
					else
						nodes.Add(new Node(new PointD(x, y), TypeNode.Grid));
				}
		}

		/// <summary>
		/// Добавление узлов магнита.
		/// </summary>
		/// <param name="center">Центр магнита.</param>
		/// <param name="a">Ширина.</param>
		/// <param name="b">Высота.</param>
		/// <param name="angle">Поворот.</param>
		/// <param name="nodesDensity">Плотность узлов.</param>
		public void GenerateNodesMagnet(PointD center, double a, double b, double angle, int nodesDensity, double q)
		{
			List<Node> nodesMagnet = new List<Node>();

			// Число узлов на границе магнита.
			int nodesA = nodesDensity;
			int nodesB = (int)(nodesDensity * (b / a)) + 1;

			// Шаг между узлами на границе магнитов.
			double da = a / nodesA;
			double db = b / nodesB;

			PointD A = new PointD(-a / 2, -b / 2);
			PointD B = new PointD(-a / 2, b / 2);
			PointD C = new PointD(a / 2, b / 2);
			PointD D = new PointD(a / 2, -b / 2);

			// Добавление узлов магнита.
			for (int i = 0; i < nodesA + 1; i++)
			{
				double rb = rnd.NextDouble() * db * 0.001;
				nodesMagnet.Add(new Node((A + new PointD(da * i, rb)).Rotate(angle) + center, TypeNode.Magnets, q + (1 + 2*rnd.NextDouble()) * 1e-5));   // AD.
				if (i == nodesA) continue;
				nodesMagnet.Add(new Node((C - new PointD(da * i, rb)).Rotate(angle) + center, TypeNode.Magnets, q + (1 + 2 * rnd.NextDouble()) * 1e-5));   // BC.
			}
			for (int i = 0; i < nodesB + 1; i++)
			{
				double ra = rnd.NextDouble() * da * 0.001;
				nodesMagnet.Add(new Node((A + new PointD(ra, db * i)).Rotate(angle) + center, TypeNode.Magnets, q + (1 + 2 * rnd.NextDouble()) * 1e-5));   // AC.
				if (i == nodesB) continue;
				nodesMagnet.Add(new Node((C - new PointD(ra, db * i)).Rotate(angle) + center, TypeNode.Magnets, q + (1 + 2 * rnd.NextDouble()) * 1e-5));   // BD.
			}

			A = (A + new PointD(-dx * 0.001, -dy * 0.001)).Rotate(angle) + center;
			B = (B + new PointD(-dx * 0.001, dy * 0.001)).Rotate(angle) + center;
			C = (C + new PointD(dx * 0.001, dy * 0.001)).Rotate(angle) + center;
			D = (D + new PointD(dx * 0.001, -dy * 0.001)).Rotate(angle) + center;

			// Удаление узлов внутри магнитов.
			for (int i = 0; i < nodes.Count; i++)
				if (nodes[i].InsideRectangle(A, B, C, D))
				{
					nodes.RemoveAt(i);
					i--;
				}

			nodes.AddRange(nodesMagnet);
		}

		/// <summary>
		/// Реккурентная процедура триангуляции.
		/// </summary>
		public void RecurrentDelaunayTriangulation()
		{
			List<Node> nodes2 = new List<Node>()
			{
				nodes[0],
				nodes[1],
				nodes[2],
				nodes[3],
				nodes[4]
			};

			// Начальная триангуляция Делоне.
			DelaunayTriangulation(nodes2, true);

			for (int i = 5; i < nodes.Count; i++)
			{
				nodes2.Clear();
				// Проверка Делоне.
				for (int t = 0; t < triangles.Count; t++)
					if (!triangles[t].IsСheckingDelaunay(nodes[i]))
					{
						nodes2.AddRange(triangles[t].Nodes());      // Добавляю вершины удалённых треугольников в список. 
						triangles.RemoveAt(t);                      // Удаляю треугольники.
						t--;
					}
				nodes2.Add(nodes[i]);
				DelaunayTriangulation(nodes2.Distinct().ToList());
			}
		}

		/// <summary>
		/// Триангуляция методом простого перебора с правилом Делоне.
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="isInit"></param>
		private void DelaunayTriangulation(List<Node> nodes, bool isInit = false)
		{
			for (int i = 0; i < nodes.Count; i++)
				for (int j = i + 1; j < nodes.Count; j++)
					for (int k = j + 1; k < nodes.Count; k++)
					{
						if (nodes[i].Type == TypeNode.Magnets && nodes[j].Type == TypeNode.Magnets && nodes[k].Type == TypeNode.Magnets)
							continue;

						// Выбор тройки узлов.
						Triangle triangle = new Triangle(nodes[i], nodes[j], nodes[k]);

						// Проверка Делоне.
						bool isCheckDelaunay = true;
						for (int p = 0; p < nodes.Count; p++)
						{
							if (p == i || p == j || p == k)	continue;
							isCheckDelaunay = triangle.IsСheckingDelaunay(nodes[p]);
							if (!isCheckDelaunay) break;
						}

						// Проверка на то, что в треугольнике есть новый узел.
						if (isInit && isCheckDelaunay)
							triangles.Add(triangle);
						else if (isCheckDelaunay && triangle.IsNode(nodes[nodes.Count - 1]))
							triangles.Add(triangle);
					}
		}
	}
}