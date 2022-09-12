using System;
using System.Collections.Generic;

namespace MagneticField
{
	/// <summary>
	/// Структура тройки узлов для триангуляции.
	/// </summary>
	public class Triangle
	{
		public Node P1 { get; protected set; }
		public Node P2 { get; protected set; }
		public Node P3 { get; protected set; }

		/// <summary>
		/// Центр масс треугольника (центр описанной окружности).
		/// </summary>
		public PointD Pc
		{
			get
			{
				// Вычисление центра описанной окружности.
				double Z1 = P1.Pos.X * P1.Pos.X + P1.Pos.Y * P1.Pos.Y;
				double Z2 = P2.Pos.X * P2.Pos.X + P2.Pos.Y * P2.Pos.Y;
				double Z3 = P3.Pos.X * P3.Pos.X + P3.Pos.Y * P3.Pos.Y;
				double Zx = (P1.Pos.Y - P2.Pos.Y) * Z3 + (P2.Pos.Y - P3.Pos.Y) * Z1 + (P3.Pos.Y - P1.Pos.Y) * Z2;
				double Zy = (P1.Pos.X - P2.Pos.X) * Z3 + (P2.Pos.X - P3.Pos.X) * Z1 + (P3.Pos.X - P1.Pos.X) * Z2;
				double Z = 2 * ((P1.Pos.X - P2.Pos.X) * (P3.Pos.Y - P1.Pos.Y) - (P1.Pos.Y - P2.Pos.Y) * (P3.Pos.X - P1.Pos.X));
				// Вычисление радиуса описанной окружности.
				return new PointD(-Zx / Z, Zy / Z);
			}
			private set { }
		}

		/// <summary>
		/// Радиус описанной окружности.
		/// </summary>
		public double Rc
		{
			get
			{
				return PointD.Distance(Pc, P1.Pos);
			}
			private set { }
		}

		/// <summary>
		/// Площадь треугольника.
		/// </summary>
		public double S
		{
			get
			{
				double A = P2.Pos.Y - P3.Pos.X;
				double B = P2.Pos.X - P3.Pos.X;
				double C = P1.Pos.Y * (P3.Pos.X - P2.Pos.X) + P2.Pos.Y * (P1.Pos.X - P3.Pos.X) + P3.Pos.Y * (P2.Pos.X - P1.Pos.X);
				return 0.5 * Math.Sqrt(A * A + B * B + C * C);
			}
			private set { }
		}

		/// <summary>
		/// Конструктор создания треугольника.
		/// </summary>
		/// <param name="p1">Первая вершина.</param>
		/// <param name="p2">Вторая вершина.</param>
		/// <param name="p3">Третья вершина.</param>
		public Triangle(Node p1, Node p2, Node p3)
		{
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		/// <summary>
		/// Возвращает список вершин треугольника.
		/// </summary>
		/// <returns></returns>
		public List<Node> Nodes()
		{
			return new List<Node>
			{
				P1,
				P2,
				P3
			};
		}

		/// <summary>
		/// Проверка Делоне.
		/// </summary>
		/// <param name="point">Проверяемая точка.</param>
		/// <returns></returns>        
		public bool IsСheckingDelaunay(Node point)
		{
			return PointD.Distance(Pc, point.Pos) > Rc;
		}

		/// <summary>
		/// Проверка, есть ли точка?
		/// </summary>
		/// <param name="newNode"></param>
		/// <returns></returns>
		public bool IsNode(Node newNode)
		{
			return Nodes().Contains(newNode);
		}

		public bool Contains(Node node)
		{
			double ka = (P1.Pos.X - node.Pos.X) * (P2.Pos.Y - P1.Pos.Y) - (P2.Pos.X - P1.Pos.X) * (P1.Pos.Y - node.Pos.Y);
			double kb = (P2.Pos.X - node.Pos.X) * (P3.Pos.Y - P2.Pos.Y) - (P3.Pos.X - P2.Pos.X) * (P2.Pos.Y - node.Pos.Y);
			double kc = (P3.Pos.X - node.Pos.X) * (P1.Pos.Y - P3.Pos.Y) - (P1.Pos.X - P3.Pos.X) * (P3.Pos.Y - node.Pos.Y);
			return ka >= 0 && kb >= 0 && kc >= 0 || ka <= 0 && kb <= 0 && kc <= 0;
		}
	}
}
