using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MagneticField
{
    public class Galerkin
    {
        private static double Kij(Node Ni, Node Nj)
        {
            List<Triangle> tempTri = Ni.nTriangles.Where(t => Nj.nTriangles.Contains(t)).ToList();

            double k = 0;
            if (tempTri.Count != 2 && tempTri.Count != 0) throw new InvalidOperationException();

            foreach (var t in tempTri)
            {
                Point3D NA = new Point3D(t.P1.Pos.X, t.P1.Pos.Y, 0);
                Point3D NB = new Point3D(t.P2.Pos.X, t.P2.Pos.Y, 0);
                Point3D NC = new Point3D(t.P3.Pos.X, t.P3.Pos.Y, 0);

                Point3D N1 = new Point3D(t.P1.Pos.X, t.P1.Pos.Y, 0);
                Point3D N2 = new Point3D(t.P2.Pos.X, t.P2.Pos.Y, 0);
                Point3D N3 = new Point3D(t.P3.Pos.X, t.P3.Pos.Y, 0);

                if (t.P1 == Ni)
                {
                    if (t.P2 == Nj) { N1 = NA; N2 = NB; N3 = NC; }
                    if (t.P3 == Nj) { N1 = NA; N2 = NC; N3 = NB; }
                }
                if (t.P2 == Ni)
                {
                    if (t.P1 == Nj) { N1 = NB; N2 = NA; N3 = NC; }
                    if (t.P3 == Nj) { N1 = NB; N2 = NC; N3 = NA; }
                }
                if (t.P3 == Ni)
                {
                    if (t.P1 == Nj) { N1 = NC; N2 = NA; N3 = NB; }
                    if (t.P2 == Nj) { N1 = NC; N2 = NB; N3 = NA; }
                }

                Vector3D vxi = Vector3D.CrossProduct(
                    new Vector3D(N2.X - N1.X, N2.Y - N1.Y, -1),
                    new Vector3D(N3.X - N1.X, N3.Y - N1.Y, -1));
                Vector3D vxj = Vector3D.CrossProduct(
                    new Vector3D(N2.X - N1.X, N2.Y - N1.Y, 1),
                    new Vector3D(N3.X - N1.X, N3.Y - N1.Y, 0));

                k += 0.5 * vxi.Length * (vxi.X * vxj.X + vxi.Y * vxj.Y);
            }
            return k;
        }

        /// <summary>
        /// Составление Аij и Rj.
        /// </summary>
        /// <returns></returns>
        public static void GetAR(List<Node> nodes, out double[,] A, out double[] R)
        {
            List<Node> nodesBoundary = new List<Node>();
            List<Node> nodesInner = new List<Node>();

            foreach (Node node in nodes)
			{
                if (node.Type == TypeNode.Boundary || node.Type == TypeNode.Magnets)
                    nodesBoundary.Add(node);
                else nodesInner.Add(node);
			}

            double[,] Aij = new double[nodesInner.Count, nodesInner.Count];
            double[] Rj = new double[nodesInner.Count];

			Parallel.For(0, nodesInner.Count, i =>
			{
				Parallel.For(0, nodesInner.Count, j =>
				{
					if (j == i)
					{
						Aij[i, j] = 0;
						foreach (Triangle t in nodesInner[i].nTriangles)
						{
							Point3D NA = new Point3D(t.P1.Pos.X, t.P1.Pos.Y, 0);
							Point3D NB = new Point3D(t.P2.Pos.X, t.P2.Pos.Y, 0);
							Point3D NC = new Point3D(t.P3.Pos.X, t.P3.Pos.Y, 0);
							if (t.P1 == nodesInner[i])
								NA = new Point3D(t.P1.Pos.X, t.P1.Pos.Y, 1);
							else if (t.P2 == nodesInner[i])
								NB = new Point3D(t.P2.Pos.X, t.P2.Pos.Y, 1);
							else if (t.P3 == nodesInner[i])
								NC = new Point3D(t.P3.Pos.X, t.P3.Pos.Y, 1);
							else throw new InvalidOperationException("The triangle is in 2D plane!");

							Vector3D vx = Vector3D.CrossProduct(NB - NA, NC - NA);
							Aij[i, j] += 0.5 * vx.Length * (vx.X * vx.X + vx.Y * vx.Y);
						}
					}
					else Aij[i, j] = Kij(nodesInner[i], nodesInner[j]);
				});

				Rj[i] = 0;
				foreach (Node node in nodesBoundary)
				{
					if (!nodesInner[i].nNodes.Contains(node)) continue;
					Rj[i] -= node.Value * Kij(nodesInner[i], node);
				}
			});


			//for (int i = 0; i < nodesInner.Count; i++)
   //         {
   //             for (int j = 0; j < nodesInner.Count; j++)
   //             {
   //                 if (j == i)
   //                 {
   //                     Aij[i, j] = 0;
   //                     foreach (Triangle t in nodesInner[i].nTriangles)
   //                     {
   //                         Point3D NA = new Point3D(t.P1.Pos.X, t.P1.Pos.Y, 0);
   //                         Point3D NB = new Point3D(t.P2.Pos.X, t.P2.Pos.Y, 0);
   //                         Point3D NC = new Point3D(t.P3.Pos.X, t.P3.Pos.Y, 0);
   //                         if (t.P1 == nodesInner[i])
   //                             NA = new Point3D(t.P1.Pos.X, t.P1.Pos.Y, 1);
   //                         else if (t.P2 == nodesInner[i])
   //                             NB = new Point3D(t.P2.Pos.X, t.P2.Pos.Y, 1);
   //                         else if (t.P3 == nodesInner[i])
   //                             NC = new Point3D(t.P3.Pos.X, t.P3.Pos.Y, 1);
   //                         else throw new InvalidOperationException("The triangle is in 2D plane!");

   //                         Vector3D vx = Vector3D.CrossProduct(NB - NA, NC - NA);
   //                         Aij[i, j] += 0.5 * vx.Length * (vx.X * vx.X + vx.Y * vx.Y);
   //                     }
   //                 }
   //                 else Aij[i, j] = Kij(nodesInner[i], nodesInner[j]);
   //             }

   //             Rj[i] = 0;
   //             foreach (Node node in nodesBoundary)
   //             {
   //                 if (!nodesInner[i].nNodes.Contains(node)) continue;
   //                 Rj[i] -= node.Value * Kij(nodesInner[i], node);
   //             }
   //         }

            A = Aij; R = Rj;
        }
    }
}
