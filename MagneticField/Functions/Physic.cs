using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MagneticField
{
    public class Physic
    {
        public List<Node> nodes;
        public List<Triangle> triangles;
        public Dictionary<double, List<Node>> isolines;
        public List<List<PointD>> fieldLines;

        public Physic(List<Node> nodes, List<Triangle> triangles)
        {
            this.nodes = nodes;
            this.triangles = triangles;
        }

        /// <summary>
        /// Вычисление изолиний.
        /// </summary>
        /// <param name="Z"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private static Dictionary<double, List<Node>> CalculateIsolines(double[] Z, List<Triangle> triangles)
        {
            var isolines = new Dictionary<double, List<Node>>();

            for (int z = 0; z < Z.Length; z++)
            {
                List<Node> isoline = new List<Node>();

                foreach (var t in triangles)
                {
                    List<Node> order = t.Nodes();
                    // Сортировка тройки узлов по возрастанию.
                    order.Sort((n1, n2) => n1.Value.CompareTo(n2.Value));

                    if ((Z[z] >= order[0].Value) && (Z[z] <= order[2].Value))
                    {
                        if (Z[z] >= order[1].Value)
                            isoline.Add(GetIsoPoint(Z[z], order[1], order[2]));
                        else isoline.Add(GetIsoPoint(Z[z], order[0], order[1]));

                        isoline.Add(GetIsoPoint(Z[z], order[0], order[2]));
                    }
                }
                isolines.Add(Z[z], isoline);
            }

            return isolines;
        }

        private static Node GetIsoPoint(double T, Node node1, Node node2)
        {
            if (double.IsPositiveInfinity(node1.Value))
                return new Node(node1.Pos);
            if (double.IsPositiveInfinity(node2.Value))
                return new Node(node2.Pos);

            double ratio = (node2.Value - T) / (node2.Value - node1.Value);
            if (double.IsNaN(ratio))
                ratio = 0;

            double x = node2.Pos.X + (node1.Pos.X - node2.Pos.X) * ratio;
            double y = node2.Pos.Y + (node1.Pos.Y - node2.Pos.Y) * ratio;
            return new Node(new PointD(x, y));
        }

        private static List<List<PointD>> CalculateFieldLines(List<Node> nodes, double k = 0.01f)
        {
            var fieldLines = new System.Collections.Concurrent.ConcurrentBag<List<PointD>>();
            var startPoints = nodes.Where(node => (node.Type == TypeNode.Boundary || node.Type == TypeNode.Magnets) && node.Value != 0).ToList();

            Parallel.ForEach(startPoints, sNode =>
            {
                List<PointD> fieldLine = new List<PointD>();
                fieldLine.Add(new PointD(sNode.Pos.X, sNode.Pos.Y));

                double gX = 0;
                double gY = 0;
                double gI = 0;
                foreach (Triangle t in sNode.nTriangles)
                {
                    PointD g = Grad(t);
                    gX += g.X;
                    gY += g.Y;
                    gI++;
                }
                gX /= gI;
                gY /= gI;
                gI = k / Math.Sqrt(gX * gX + gY * gY);
                fieldLine.Add(new PointD(fieldLine.Last().X + gX * gI, fieldLine.Last().Y + gY * gI));

                Triangle triangle = sNode.nTriangles.Find(t => t.Contains(new Node(new PointD(fieldLine.Last().X, fieldLine.Last().Y))));

                while (triangle != null)
                {
                    PointD grad = Grad(triangle);
                    gI = k / Math.Sqrt(grad.X * grad.X + grad.Y * grad.Y);
                    fieldLine.Add(new PointD(fieldLine.Last().X + grad.X * gI, fieldLine.Last().Y + grad.Y * gI));

                    if (grad.X * grad.X + grad.Y * grad.Y < float.Epsilon)
                        break;

                    PointD fieldLineLast = new PointD(fieldLine.Last().X, fieldLine.Last().Y);
                    double distance1 = PointD.Distance(new PointD(triangle.P1.Pos.X, triangle.P1.Pos.Y), fieldLineLast);
                    double distance2 = PointD.Distance(new PointD(triangle.P2.Pos.X, triangle.P2.Pos.Y), fieldLineLast);
                    double distance3 = PointD.Distance(new PointD(triangle.P3.Pos.X, triangle.P3.Pos.Y), fieldLineLast);

                    Node searchNode = triangle.P1;
                    if (distance2 < distance1 && distance2 < distance3)
                        searchNode = triangle.P2;
                    if (distance3 < distance1 && distance3 < distance2)
                        searchNode = triangle.P3;

                    try
                    {
                        triangle = searchNode.nTriangles.First(t => t.Contains(new Node(fieldLineLast)));
                    }
                    catch (Exception)
                    {
                        triangle = null;
                    }
                }
                fieldLine.RemoveAt(fieldLine.Count - 1);
                fieldLines.Add(fieldLine);
            });

            Parallel.ForEach(startPoints, sNode =>
            {
                List<PointD> fieldLine = new List<PointD>();
                fieldLine.Add(new PointD(sNode.Pos.X, sNode.Pos.Y));

                double gX = 0;
                double gY = 0;
                double gI = 0;
                foreach (Triangle t in sNode.nTriangles)
                {
                    PointD g = Grad(t);
                    gX += g.X;
                    gY += g.Y;
                    gI++;
                }
                gX /= gI;
                gY /= gI;
                gI = k / Math.Sqrt(gX * gX + gY * gY);
                fieldLine.Add(new PointD(fieldLine.Last().X - gX * gI, fieldLine.Last().Y - gY * gI));

                Triangle triangle = sNode.nTriangles.Find(t => t.Contains(new Node(new PointD(fieldLine.Last().X, fieldLine.Last().Y))));

                while (triangle != null)
                {
                    PointD grad = Grad(triangle);
                    gI = k / Math.Sqrt(grad.X * grad.X + grad.Y * grad.Y);
                    fieldLine.Add(new PointD(fieldLine.Last().X - grad.X * gI, fieldLine.Last().Y - grad.Y * gI));

                    if (grad.X * grad.X + grad.Y * grad.Y < double.Epsilon)
                        break;

                    PointD fieldLineLast = new PointD(fieldLine.Last().X, fieldLine.Last().Y);
                    double distance1 = PointD.Distance(new PointD(triangle.P1.Pos.X, triangle.P1.Pos.Y), fieldLineLast);
                    double distance2 = PointD.Distance(new PointD(triangle.P2.Pos.X, triangle.P2.Pos.Y), fieldLineLast);
                    double distance3 = PointD.Distance(new PointD(triangle.P3.Pos.X, triangle.P3.Pos.Y), fieldLineLast);

                    Node searchNode = triangle.P1;
                    if (distance2 < distance1 && distance2 < distance3)
                        searchNode = triangle.P2;
                    if (distance3 < distance1 && distance3 < distance2)
                        searchNode = triangle.P3;

                    try
                    {
                        triangle = searchNode.nTriangles.First(t => t.Contains(new Node(fieldLineLast)));
                    }
                    catch (Exception)
                    {
                        triangle = null;
                    }
                }
                fieldLine.RemoveAt(fieldLine.Count - 1);
                fieldLines.Add(fieldLine);
            });

            return fieldLines.ToList();
        }

        public static PointD Grad(Triangle triangle)
        {
            Vector3D norm = Vector3D.CrossProduct(
                new Vector3D(triangle.P1.Pos.X - triangle.P2.Pos.X, triangle.P1.Pos.Y - triangle.P2.Pos.Y, triangle.P1.Value - triangle.P2.Value),
                new Vector3D(triangle.P1.Pos.X - triangle.P3.Pos.X, triangle.P1.Pos.Y - triangle.P3.Pos.Y, triangle.P1.Value - triangle.P3.Value)
                );
            if (norm.Z < 0)
                norm *= -1; // Разворачиваем вектор.

            Vector3D tang = Vector3D.CrossProduct(new Vector3D(0, 0, 1), norm);
            Vector3D grad = Vector3D.CrossProduct(tang, norm);

            return new PointD(grad.X, grad.Y);
        }

        /// <summary>
        /// Определение соседних треугольников и узлов.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="triangles"></param>
        public void CreateNeighbours()
        {
            // Устанавливаем соседей
            foreach (Triangle t in triangles)
            {
                if (!t.P1.nNodes.Contains(t.P2)) t.P1.nNodes.Add(t.P2);
                if (!t.P1.nNodes.Contains(t.P3)) t.P1.nNodes.Add(t.P3);
                if (!t.P2.nNodes.Contains(t.P1)) t.P2.nNodes.Add(t.P1);
                if (!t.P3.nNodes.Contains(t.P1)) t.P3.nNodes.Add(t.P1);
                if (!t.P3.nNodes.Contains(t.P2)) t.P3.nNodes.Add(t.P2);

                t.P1.nTriangles.Add(t);
                t.P2.nTriangles.Add(t);
                t.P3.nTriangles.Add(t);
            }
            foreach (Node node in nodes)
                node.nNodes = node.nNodes.Distinct().ToList();
        }

        public void Calculate()
        {
            Galerkin.GetAR(nodes, out double[,] A, out double[] R);

            // Решение системы уравнений.
            double[] result = Accord.Math.Matrix.Decompose(A).Solve(R);
            int idx = 0;
            foreach (Node node in nodes)
            {
                if (node.Type == TypeNode.Boundary || node.Type == TypeNode.Magnets)
                    continue;
                node.Value = result[idx];
                idx++;
            }

            double[] Z = new double[201];
            double max = nodes.Max(p => Math.Abs(p.Value));
            int kZ = Z.Length / 2;
            for (int i = -kZ; i <= kZ; i++)
                Z[i + kZ] = max * i / kZ;

            isolines = CalculateIsolines(Z, triangles);
            fieldLines = CalculateFieldLines(nodes, 0.5 / Math.Sqrt(nodes.Count));
        }
    }
}
