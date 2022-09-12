using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MagneticField
{
    public class Drawing
    {
        private readonly PictureBox pictureBox;
        public readonly Graphics graphics;
        private readonly Bitmap bitmap;

        private readonly double wnd_Xmin, wnd_Xmax, wnd_Ymin, wnd_Ymax;
        private readonly double alpha, beta;

        public Drawing(PictureBox pB, double x1, double y1, double x2, double y2)
        {
            pictureBox = pB;
            bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(0, pictureBox.Height);
            // Относительные размеры окна рисования.
            wnd_Xmin = x1; wnd_Xmax = x2;
            wnd_Ymin = y1; wnd_Ymax = y2;
            // Вычисление коэффициентов преобразование.
            alpha = pictureBox.Width / (wnd_Xmax - wnd_Xmin);
            beta = -pictureBox.Height / (wnd_Ymax - wnd_Ymin);

            pictureBox.Image = bitmap;
        }

        /// <summary>
        /// Очищает область.
        /// </summary>
        public void Clear()
        {
            graphics.Clear(Color.Black);
        }

        /// <summary>
        /// Преобразует мировые координаты в координаты окна (пиксели).
        /// </summary>
        /// <param name="x">X в мировых координатах.</param>
        /// <returns>Координата, преобразованная в координаты окна (пиксели).</returns>
        private float OutX(double x)
        {
            return (float)(x * alpha);
        }

        /// <summary>
        /// Преобразует мировые координаты в координаты окна (пиксели).
        /// </summary>
        /// <param name="y">Y мировых координатах.s</param>
        /// <returns>Координата, преобразованная в координаты окна (пиксели).</returns>
        private float OutY(double y)
        {
            return (float)(y * beta);
        }

        /// <summary>
        /// Рисует прямую линию между двумя заданными точками.
        /// </summary>
        /// <param name="color">Цвет линии.</param>
        /// <param name="x1">X-положение начальной точки в мировых координатах.</param>
        /// <param name="y1">Y-положение начальной точки в мировых координатах.</param>
        /// <param name="x2">X-положение конечной точки в мировых координатах.</param>
        /// <param name="y2">Y-положение конечной точки в мировых координатах.</param>
        private void DrawLine(Color color, double x1, double y1, double x2, double y2, float width = 1f)
        {
            Pen pen = new Pen(color)
            {
                Width = width,
                DashStyle = DashStyle.Solid
            };
            graphics.DrawLine(pen, OutX(x1), OutY(y1), OutX(x2), OutY(y2));
            graphics.Flush();
        }

        /// <summary>
        /// Рисует заполненный эллипс.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void DrawFillEllipse(Color color, double x, double y, double width, double height)
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillEllipse(brush, OutX(x) - OutX(width) / 2, OutY(y) - OutY(height) / 2, OutX(width), OutY(height));
            graphics.Flush();
        }

        /// <summary>
        /// Отрисовка триангуляции.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="triangles"></param>
        public void DrawTriangulation(List<Node> nodes, List<Triangle> triangles)
        {
            Color color;

            // Отрисовка треугольников.
            foreach (Triangle triangle in triangles)
            {
                color = Color.FromArgb(50, 50, 50);
                DrawLine(color, triangle.P1.Pos.X, triangle.P1.Pos.Y, triangle.P2.Pos.X, triangle.P2.Pos.Y);
                DrawLine(color, triangle.P1.Pos.X, triangle.P1.Pos.Y, triangle.P3.Pos.X, triangle.P3.Pos.Y);
                DrawLine(color, triangle.P2.Pos.X, triangle.P2.Pos.Y, triangle.P3.Pos.X, triangle.P3.Pos.Y);
            }

            // Отрисовка узлов.
            double max = nodes.Max(node => Math.Abs(node.Value));
            color = Color.Yellow;
            foreach (Node node in nodes)
            {
                if (double.IsNaN(node.Value))
                    color = Color.Yellow;
                else if (node.Value == 0)
                    color = Color.White;
                else if (node.Value >= 0)
                    color = Color.FromArgb((int)(255 * node.Value / max), 0, 0);
                else
                    color = Color.FromArgb(0, 0, (int)(-255 * node.Value / max));

                DrawFillEllipse(color, node.Pos.X, node.Pos.Y, 0.003, 0.003);
            }
        }

        /// <summary>
        /// Отрисовка изолиний.
        /// </summary>
        /// <param name="isolines"></param>
        public void DrawIsolines(Dictionary<double, List<Node>> isolines)
        {
            Color color;
            double max = isolines.Max(node => Math.Abs(node.Key));
            foreach (var line in isolines)
            {
                if (double.IsNaN(line.Key))
                    color = Color.White;
                else if (line.Key >= 0)
                    color = Color.FromArgb((int)(255 * line.Key / max), 0, 0);
                else
                    color = Color.FromArgb(0, 0, (int)(-255 * line.Key / max));
                //color = Color.White;

                for (int i = 0; i < line.Value.Count - 1; i += 2)
                    DrawLine(color, line.Value[i].Pos.X, line.Value[i].Pos.Y, line.Value[i + 1].Pos.X, line.Value[i + 1].Pos.Y, 1f);
            }
        }

        /// <summary>
        /// Отрисовка силовых линий.
        /// </summary>
        /// <param name="fieldlines"></param>
        public void DrawFieldLines(List<List<PointD>> fieldlines)
        {
            foreach (var line in fieldlines)
                for (int i = 0; i < line.Count - 1; i++)
                    DrawLine(Color.White, line[i].X, line[i].Y, line[i + 1].X, line[i + 1].Y);
        }
    }
}
