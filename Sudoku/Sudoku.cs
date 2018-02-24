using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Drawing;

namespace JH.Applications
{
    public partial class Sudoku : Form
    {
        int[,] sudoku = new int[9, 9];
        PictureBox pictureBox;
        Bitmap graphArea;
        Graphics graphGraphics;
        int scaling;
        int width;
        int height;
        Font font = new Font("Courier", 10, FontStyle.Regular);
        SolidBrush brushBlack = new SolidBrush(Color.Black);
        SolidBrush brushWhite = new SolidBrush(Color.White);

        public Sudoku()
        {
            InitializeComponent();

            scaling = 30;
            width = 9;
            height = 9;
            pictureBox = new PictureBox();
            pictureBox.Location = new Point(10, 10);
            pictureBox.Size = new Size(scaling * width, scaling * height);
            pictureBox.Paint += new PaintEventHandler(OnDraw);
            Controls.Add(pictureBox);

            graphArea = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphGraphics = Graphics.FromImage(graphArea);
            graphGraphics.Clear(Color.White);
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(Go));
            thread.Name = "MyThread";
            thread.Start();
        }

        void Go()
        {
            AlgorithmX algorithmX = new AlgorithmX();
            algorithmX.callBack = CallBack;
            Node[,] tableau = new Node[729, 324];
            StreamReader stream = new StreamReader(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/sudoku22533.txt");

            string s = stream.ReadLine();
            string[] split = s.Split(new char[] { '=' });
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    string c = split[split.Length - 1].Substring(9 * i + j, 1);
                    sudoku[i, j] = int.Parse(c == "." ? "0" : c);
                }
            }

            Print();

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    for (int n = 0; n < 9; n++)
                    {
                        int tr = 81 * r + 9 * c + n;
                        int tc = 9 * r + c;
                        if (sudoku[r, c] == 0 || sudoku[r, c] - 1 == n)
                            tableau[tr, tc] = new Node(tr, tc);
                    }

            for (int r = 0; r < 9; r++)
                for (int n = 0; n < 9; n++)
                    for (int c = 0; c < 9; c++)
                    {
                        int tr = 81 * r + 9 * c + n;
                        int tc = 81 + 9 * r + n;
                        if (sudoku[r, c] == 0 || sudoku[r, c] - 1 == n)
                            tableau[tr, tc] = new Node(tr, tc);
                    }

            for (int c = 0; c < 9; c++)
                for (int n = 0; n < 9; n++)
                    for (int r = 0; r < 9; r++)
                    {
                        int tr = 81 * r + 9 * c + n;
                        int tc = 162 + 9 * c + n;
                        if (sudoku[r, c] == 0 || sudoku[r, c] - 1 == n)
                            tableau[tr, tc] = new Node(tr, tc);
                    }

            for (int i1 = 0; i1 < 3; i1++)
                for (int i2 = 0; i2 < 3; i2++)
                    for (int n = 0; n < 9; n++)
                        for (int j1 = 0; j1 < 3; j1++)
                            for (int j2 = 0; j2 < 3; j2++)
                            {
                                int tr = 81 * (3 * i1 + j1) + 9 * (3 * i2 + j2) + n;
                                int tc = 243 + 9 * (3 * i1 + i2) + n;
                                if (sudoku[3 * i1 + j1, 3 * i2 + j2] == 0 || sudoku[3 * i1 + j1, 3 * i2 + j2] - 1 == n)
                                    tableau[tr, tc] = new Node(tr, tc);
                            }

            bool retVal = true;

            algorithmX.Init(tableau);

            DateTime t0 = DateTime.Now;
            retVal = algorithmX.Search();
            DateTime t1 = DateTime.Now;


            if (retVal)
                Console.WriteLine("\nCongratulations !, solved in {0} ms", (t1 - t0).TotalMilliseconds);
            else
                Console.WriteLine("\nSorry, no solution !");

            CheckSolution(algorithmX.result);
        }

        protected void OnDraw(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;

            graphics.DrawImage(graphArea, 0, 0);
        }

        void CallBack(int index, bool draw)
        {
            int r, c, v;
            Decode(index, out r, out c, out v);

            PrintCell(r, c, v + 1, draw);
            pictureBox.Invalidate();

            Thread.Sleep(200);
        }

        private void CheckSolution(List<int> list)
        {
            int r, c, v;
            foreach (int cell in list)
            {
                Decode(cell, out r, out c, out v);
                sudoku[r, c] = v + 1;

            }
            Print();

            int[] reference = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] comp = new int[9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                    comp[j] = sudoku[i, j];

                Array.Sort(comp);
                for (int k = 0; k < 9; k++)
                    if (comp[k] != reference[k])
                        throw new Exception("Error in Sudoku");
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                    comp[j] = sudoku[j, i];

                Array.Sort(comp);
                for (int k = 0; k < 9; k++)
                    if (comp[k] != reference[k])
                        throw new Exception("Error in Sudoku");
            }
            for (int i1 = 0; i1 < 3; i1++)
                for (int i2 = 0; i2 < 3; i2++)
                {
                    for (int j1 = 0; j1 < 3; j1++)
                        for (int j2 = 0; j2 < 3; j2++)
                            comp[3 * j1 + j2] = sudoku[j1 + 3 * i1, j2 + 3 * i2];

                    Array.Sort(comp);
                    for (int k = 0; k < 9; k++)
                        if (comp[k] != reference[k])
                            throw new Exception("Error in Sudoku");
                }

        }

        void Decode(int cell, out int r, out int c, out int v)
        {
            r = cell / 81;
            c = (cell - 81 * r) / 9;
            v = cell - 81 * r - 9 * c;
        }

        void Print()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    PrintCell(i, j, sudoku[i, j], sudoku[i, j] != 0);

            pictureBox.Invalidate();
        }

        void PrintCell(int r, int c, int v, bool draw)
        {
            graphGraphics.FillRectangle(brushWhite, new Rectangle(scaling * c, scaling * r, scaling, scaling));

            if (draw)
                graphGraphics.DrawString(v.ToString(), font, brushBlack, scaling * c, scaling * r);
        }
    }
}