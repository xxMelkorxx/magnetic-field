using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MagneticField
{
	public partial class MainForm : Form
	{
		Triangulation triangulation;
		Drawing drawing;
		Physic physic;

		public MainForm()
		{
			InitializeComponent();
		}

		private void OnStartTriangulation(object sender, EventArgs e)
		{
			int nodesX = (int)numUpDown_nodesX.Value;
			int nodesY = (int)numUpDown_nodesY.Value;

			PointD center1 = new PointD((double)numUpDown_CenterX1.Value, (double)numUpDown_CenterY1.Value);
			PointD center2 = new PointD((double)numUpDown_CenterX2.Value, (double)numUpDown_CenterY2.Value);
			double width1 = (double)numUpDown_Width1.Value / 10.0;
			double width2 = (double)numUpDown_Width2.Value / 10.0;
			double height1 = (double)numUpDown_Height1.Value / 10.0;
			double height2 = (double)numUpDown_Height2.Value / 10.0;
			int nodesDensity1 = (int)numUpDown_NodesDensity1.Value;
			int nodesDensity2 = (int)numUpDown_NodesDensity2.Value;
			double rotate1 = (double)numUpDown_Rotate1.Value * Math.PI / 180;
			double rotate2 = (double)numUpDown_Rotate2.Value * Math.PI / 180;
			double Q = (double)numUpDown_Q.Value;

			triangulation = new Triangulation();
			drawing = new Drawing(pictBox_Visualization, 0, 0, 1, 1);

			Task taskTriangulation = Task.Factory.StartNew(() =>
			{
				// Генерация узлов.
				triangulation.GenerateNodesGrid(nodesX, nodesY);
				triangulation.GenerateNodesMagnet(center1, width1, height1, rotate1, nodesDensity1, Q);
				triangulation.GenerateNodesMagnet(center2, width2, height2, rotate2, nodesDensity2, -Q);
				triangulation.RecurrentDelaunayTriangulation();

				// Отрисовка триангуляции.
				drawing.DrawTriangulation(triangulation.nodes, triangulation.triangles);
			});

			taskTriangulation.Wait();

			while (!taskTriangulation.IsCompleted)
				Application.DoEvents();

			button_StartPhysic.Enabled = true;
		}

		private void OnClickStartPhysic(object sender, EventArgs e)
		{
			physic = new Physic(triangulation.nodes, triangulation.triangles);
			Task taskPhysic = Task.Factory.StartNew(() =>
			{
				physic.CreateNeighbours();
				physic.Calculate();
			});
			taskPhysic.Wait();

			while (!taskPhysic.IsCompleted)
				Application.DoEvents();

			Draw();

			button_StartPhysic.Enabled = false;
		}

		private void OnCheckedChangedIsolines(object sender, EventArgs e)
		{
			Draw();
		}

		private void OnCheckedChangedFieldlines(object sender, EventArgs e)
		{
			Draw();
		}

		private void Draw()
		{
			if (physic != null)
			{
				drawing = new Drawing(pictBox_Visualization, 0, 0, 1, 1);
				drawing.DrawTriangulation(physic.nodes, physic.triangles);
				if (checkBox_IsIsolines.Checked)
					drawing.DrawIsolines(physic.isolines);
				if (checkBox_IsFieldLines.Checked)
					drawing.DrawFieldLines(physic.fieldLines);
			}
		}
	}
}
