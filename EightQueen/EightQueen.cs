using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Drawing;

namespace JH.Applications
{
    public partial class EightQueen : Form
    {
        PictureBox pictureBox;
        Bitmap graphArea;
        Graphics graphGraphics;
        int scaling;
        int width;
        int height;
        SolidBrush brushBlack = new SolidBrush(Color.Black);
        SolidBrush brushWhite = new SolidBrush(Color.White);

        public EightQueen()
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

        private void EightQueen_Load(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(Go));
            thread.Name = "MyThread";
            thread.Start();
        }

        void Go()
        {
            AlgorithmX algorithmX = new AlgorithmX();
            algorithmX.callBack = CallBack;
            Node[,] tableau = new Node[94, 46];


            for (int e = 0; e < 94; e++)
            {
                int r = e / 8;
                int c = e % 8;
                for (int rule = 0; rule < 46; rule++)
                {
                    if (e < 64 && (rule == r || rule == 8 + c))
                        tableau[e, rule] = new Node(e, rule);
                    else if (e < 64 && rule == 16 + r + c)
                        tableau[e, rule] = new Node(e, rule);
                    else if (e < 64 && rule == 31 + 7 - r + c)
                        tableau[e, rule] = new Node(e, rule);
                    else if (e >= 64 && rule == e - 64 + 16)
                        tableau[e, rule] = new Node(e, rule);
                }
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

            if (v == 0)
                PrintCell(r, c, v + 1, draw);
            pictureBox.Invalidate();

            Thread.Sleep(50);
        }

        private void CheckSolution(List<int> list)
        {
        }

        void Decode(int cell, out int r, out int c, out int v)
        {
            if (cell < 64)
            {
                r = cell / 8;
                c = cell % 8;
                v = 0;
            }
            else
            {
                r = -1;
                c = -1;
                v = cell - 64;
            }
        }

        void PrintCell(int r, int c, int v, bool draw)
        {
            for (int i = 0; i < 9; i++)
                graphGraphics.DrawLine(new Pen(Color.Black), 0, scaling * i, scaling * 8, scaling * i);
            for (int i = 0; i < 9; i++)
                graphGraphics.DrawLine(new Pen(Color.Black), scaling * i, 0, scaling * i, scaling * 8);

            graphGraphics.FillRectangle(brushWhite, new Rectangle(scaling * c, scaling * r, scaling, scaling));

            if (draw)
                graphGraphics.FillRectangle(brushBlack, new Rectangle(scaling * c, scaling * r, scaling, scaling));
        }

    }
}