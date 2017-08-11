using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace JH.Calculations
{
    public partial class Pentominoes : Form
    {
        class Pentomino
        {
            public int name;
            public int variant;
            List<string> operations;
            public List<Point> content;

            public Pentomino(int name, int variant, List<string> operations)
            {
                this.name = name;
                this.variant = variant;
                this.operations = operations;
            }

            public Pentomino(int name, int variant, List<string> operations, List<Point> content)
            {
                this.name = name;
                this.variant = variant;
                this.operations = operations;
                this.content = content;
            }

            public int Index(Pentomino p)
            {
                return 12 * p.variant + p.name;
            }

            public Pentomino Variant(string operation, Pentomino p)
            {
                switch (operation)
                {
                    case "r":
                        return Rotate(p);
                    case "rr":
                        return Rotate(Rotate(p));
                    case "rrr":
                        return Rotate(Rotate(Rotate(p)));
                    case "m":
                        return Mirror(p);
                    case "mr":
                        return Mirror(Rotate(p));
                    case "mrr":
                        return Mirror(Rotate(Rotate(p)));
                    case "mrrr":
                        return Mirror(Rotate(Rotate(Rotate(p))));
                }
                throw new Exception("Invalid operation");
            }

            Pentomino Rotate(Pentomino p)
            {
                Pentomino n = new Pentomino(p.name, p.variant + 1, p.operations);

                n.content = new List<Point>();
                Rectangle enclosure = p.Enclosure();
                foreach (Point element in p.content)
                {
                    Point newElement = new Point(enclosure.Height - 1 - element.Y, element.X);
                    n.content.Add(newElement);
                }
                return n;
            }

            Pentomino Mirror(Pentomino p)
            {
                Pentomino n = new Pentomino(p.name, p.variant + 4, p.operations);

                n.content = new List<Point>();
                Rectangle enclosure = p.Enclosure();
                foreach (Point element in p.content)
                {
                    Point newElement = new Point(enclosure.Width - 1 - element.X, element.Y);
                    n.content.Add(newElement);
                }
                return n;
            }

            public Rectangle Enclosure()
            {
                int width = int.MinValue;
                int height = int.MinValue;
                foreach (Point element in content)
                {
                    width = Math.Max(width, element.X);
                    height = Math.Max(height, element.Y);
                }
                return new Rectangle(0, 0, width + 1, height + 1);
            }

            public void Print()
            {
                Rectangle enclosure = Enclosure();
                for (int i = enclosure.Height - 1; i >= 0; i--)
                {
                    for (int j = 0; j < enclosure.Width; j++)
                    {
                        if (Contains(new Point(j, i)))
                            Console.Write("x");
                        else
                            Console.Write(" ");
                    }
                    Console.WriteLine();
                }
            }

            bool Contains(Point p)
            {
                foreach (Point element in content)
                    if (element.X == p.X && element.Y == p.Y)
                        return true;

                return false;
            }
        }

        class Position
        {
            public Pentomino p;
            public Point position;

            public Position(Point position, Pentomino p)
            {
                this.p = p;
                this.position = position;
            }
        }

        List<Position> positions;
        Rectangle center;
        List<Pentomino> pentominoes;
        Node[,] tableau;
        AlgorithmX algorithmX;
        PictureBox pictureBox;
        Bitmap graphArea;
        Graphics graphGraphics;
        int width;
        int height;
        int scaling;

        public Pentominoes()
        {
            InitializeComponent();

            pentominoes = new List<Pentomino>();
            positions = new List<Position>();

            width = 8;
            height = 8;
            scaling = 50;
            center = new Rectangle(3, 3, 2, 2);

            algorithmX = new AlgorithmX();
            algorithmX.callBack = CallBack;

            BasicPentominoes();

            foreach (Pentomino p in pentominoes)
            {
                p.Print();
                Console.WriteLine();
            }

            GeneratePositions(width, height, center);
            tableau = new Node[positions.Count, 76];

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

            int count = 0;
            foreach (Position position in positions)
            {
                int column = 64 + position.p.name;
                tableau[count, column] = new Node(count, column);
                foreach (Point element in position.p.content)
                {
                    column = height * (element.X + position.position.X) + element.Y + position.position.Y;

                    tableau[count, column] = new Node(count, column);
                }
                count++;
            }

            bool retVal = true;

            algorithmX.Init(tableau);

            algorithmX.headerRow[27].l.r = algorithmX.headerRow[27].r;
            algorithmX.headerRow[27].r.l = algorithmX.headerRow[27].l;
            algorithmX.headerRow[28].l.r = algorithmX.headerRow[28].r;
            algorithmX.headerRow[28].r.l = algorithmX.headerRow[28].l;
            algorithmX.headerRow[35].l.r = algorithmX.headerRow[35].r;
            algorithmX.headerRow[35].r.l = algorithmX.headerRow[35].l;
            algorithmX.headerRow[36].l.r = algorithmX.headerRow[36].r;
            algorithmX.headerRow[36].r.l = algorithmX.headerRow[36].l;

            DateTime t0 = DateTime.Now;
            retVal = algorithmX.Search();
            DateTime t1 = DateTime.Now;

            if (retVal)
                Console.WriteLine("\nCongratulations !, solved in {0} ms", (t1 - t0).TotalMilliseconds);
            else
                Console.WriteLine("\nSorry, no solution !");

            CheckSolution(algorithmX.result, width, height);

            foreach (int index in algorithmX.result)
            {
                Console.WriteLine("{0}  {1}", positions[index].position.X, positions[index].position.Y);
                positions[index].p.Print();
                Console.WriteLine();
            }

        }

        protected void OnDraw(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;

            graphics.DrawImage(graphArea, 0, 0);
        }

        void CallBack(int index, bool draw)
        {
            Position position = positions[index];
            List<Point> content = position.p.content;
            SolidBrush pen = null;
            int name = position.p.name + 4;
            int r = name >> 2;
            int g = (name & 7) >> 1;
            int b = (name & 3);
            int scale = 50;
            int offset = 50;
            if (draw)
                pen = new SolidBrush(Color.FromArgb(scale * r + offset, scale * g + offset, scale * b + offset));
            else
                pen = new SolidBrush(Color.White);

            foreach (Point element in content)
                graphGraphics.FillRectangle(pen, new Rectangle((element.X + position.position.X) * scaling, (element.Y + position.position.Y) * scaling, scaling, scaling));

            pictureBox.Invalidate();
            Thread.Sleep(10);
        }

        void CheckSolution(List<int> result, int width, int height)
        {
            int[,] solution = new int[width, height];

            foreach (int index in result)
            {
                Position position = positions[index];
                List<Point> content = position.p.content;
                foreach (Point element in content)
                {
                    solution[element.X + position.position.X, element.Y + position.position.Y] = solution[element.X + position.position.X, element.Y + position.position.Y] ^ 1;
                }
            }

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    if (solution[i, j] != 1)
                         throw new Exception("Error in solution");// error for 8x8, test incorrect

            Console.WriteLine("Solution OK");

        }

        void GeneratePositions(int width, int height, Rectangle center)
        {
            foreach (Pentomino pentomino in pentominoes)
            {
                Rectangle enclosure = pentomino.Enclosure();
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (enclosure.Width - 1 + i < width && enclosure.Height - 1 + j < height)
                        {
                            Position position = new Position(new Point(i, j), pentomino);
                            if (!Overlap(position, center))
                                positions.Add(position);
                        }
                    }
                }
            }
        }

        bool Overlap(Position position, Rectangle center)
        {
            if (center.Width == 0 && center.Height == 0)
                return false;

            List<Point> content = position.p.content;

            foreach (Point p in content)
                if (center.Contains(new Point(position.position.X + p.X, position.position.Y + p.Y)))
                    return true;

            return false;
        }

        void BasicPentominoes()
        {
            List<string> operations;
            List<Point> content;
            Pentomino p;


            //*********************
            // xxxxx
            //*********************

            operations = new List<string>();
            operations.Add("r");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(1, 0));
            content.Add(new Point(2, 0));
            content.Add(new Point(3, 0));
            content.Add(new Point(4, 0));

            p = new Pentomino(0, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // x
            // xxxx
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");
            operations.Add("m");
            operations.Add("mr");
            operations.Add("mrr");
            operations.Add("mrrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(0, 1));
            content.Add(new Point(1, 0));
            content.Add(new Point(2, 0));
            content.Add(new Point(3, 0));

            p = new Pentomino(1, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            //  x
            // xxxx
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");
            operations.Add("m");
            operations.Add("mr");
            operations.Add("mrr");
            operations.Add("mrrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(1, 0));
            content.Add(new Point(1, 1));
            content.Add(new Point(2, 0));
            content.Add(new Point(3, 0));

            p = new Pentomino(2, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            //   xx
            // xxx
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");
            operations.Add("m");
            operations.Add("mr");
            operations.Add("mrr");
            operations.Add("mrrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(1, 0));
            content.Add(new Point(2, 0));
            content.Add(new Point(2, 1));
            content.Add(new Point(3, 1));

            p = new Pentomino(3, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // x
            // xxx
            // x
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(0, 1));
            content.Add(new Point(0, 2));
            content.Add(new Point(1, 1));
            content.Add(new Point(2, 1));

            p = new Pentomino(4, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            //  x
            // xxx
            //  x
            //*********************

            operations = new List<string>();

            content = new List<Point>();
            content.Add(new Point(0, 1));
            content.Add(new Point(1, 0));
            content.Add(new Point(1, 1));
            content.Add(new Point(1, 2));
            content.Add(new Point(2, 1));

            p = new Pentomino(5, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // xxxx
            // x
            // x
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(0, 1));
            content.Add(new Point(0, 2));
            content.Add(new Point(1, 2));
            content.Add(new Point(2, 2));

            p = new Pentomino(6, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // xx
            //  xx
            //   x
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");

            content = new List<Point>();
            content.Add(new Point(0, 2));
            content.Add(new Point(1, 1));
            content.Add(new Point(1, 2));
            content.Add(new Point(2, 0));
            content.Add(new Point(2, 1));

            p = new Pentomino(7, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // x
            // xxx
            //  x 
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");
            operations.Add("m");
            operations.Add("mr");
            operations.Add("mrr");
            operations.Add("mrrr");

            content = new List<Point>();
            content.Add(new Point(0, 1));
            content.Add(new Point(0, 2));
            content.Add(new Point(1, 0));
            content.Add(new Point(1, 1));
            content.Add(new Point(2, 1));

            p = new Pentomino(8, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // x
            // xxx
            //   x 
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("m");
            operations.Add("mr");

            content = new List<Point>();
            content.Add(new Point(0, 1));
            content.Add(new Point(0, 2));
            content.Add(new Point(1, 1));
            content.Add(new Point(2, 0));
            content.Add(new Point(2, 1));

            p = new Pentomino(9, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // x x
            // xxx
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(0, 1));
            content.Add(new Point(1, 0));
            content.Add(new Point(2, 0));
            content.Add(new Point(2, 1));

            p = new Pentomino(10, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

            //*********************
            // xx
            // xxx
            //*********************

            operations = new List<string>();
            operations.Add("r");
            operations.Add("rr");
            operations.Add("rrr");
            operations.Add("m");
            operations.Add("mr");
            operations.Add("mrr");
            operations.Add("mrrr");

            content = new List<Point>();
            content.Add(new Point(0, 0));
            content.Add(new Point(0, 1));
            content.Add(new Point(1, 0));
            content.Add(new Point(1, 1));
            content.Add(new Point(2, 0));

            p = new Pentomino(11, 0, operations, content);
            pentominoes.Add(p);

            foreach (string operation in operations)
            {
                pentominoes.Add(p.Variant(operation, p));
            }

        }
    }
}
