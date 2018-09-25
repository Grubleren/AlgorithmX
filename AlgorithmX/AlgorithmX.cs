using System;
using System.Collections.Generic;
using System.Threading;

namespace JH.Applications
{
    public class Node
    {
        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Node u;
        public Node d;
        public Node r;
        public Node l;
        public Node c;
        public string name;
        public int x;
        public int y;
    }

    public class AlgorithmX
    {
        public Node[,] tableau;
        public Node[] headerRow;
        public List<int> result;
        public Node head;
        public delegate void CallBack(int index, bool draw);
        public CallBack callBack;

        public void Init(Node[,] tableau)
        {
            this.tableau = tableau;
            headerRow = new Node[tableau.GetLength(1)];
            result = new List<int>();
            head = new Node(-1, -1);
            head.name = "Head";
            for (int i = 0; i < headerRow.Length; i++)
            {
                headerRow[i] = new Node(-1, i);
                headerRow[i].name = i.ToString();
            }

            SetCRowLinks();
            SetAllCLinks();
            SetAllRLinks();
        }

        public bool Search()
        {
            if (head.r == head)
                throw new Exception("There are no columns");

            if (head.r.d == head)
                throw new Exception("There are no rows");

            int min = int.MaxValue;
            Node minNode = head;
            MinCountHeadRow(head.r, ref min, ref minNode); // Start searching the column with the minimum number of rows.

            if (minNode == null)
                throw new Exception("Something is rotten");

            // Start with the first row, so we do not wrap around
            return Search(minNode.d); // True: solution found, we are happy, False: there is no solution. 
            // The result is found in result
        }

        bool Search(Node n)
        {
            Node h = n.c;

            if (n == h)
                return false; //  There are no more rows to search and we could not find a solution in the column

            // Console.WriteLine("Add row\t\t\t{0}", n.x);
            // Console.WriteLine("Cover column\t{0}", h.name);

            result.Add(n.x);

            if (callBack != null)
                callBack(n.x, true);

            h.r.l = h.l; // Exclude head horizontally
            h.l.r = h.r;

            Hide(n.r, n); // Hide the row

            Cover(n.d, n); // Hide the rest of the rows in the column

            CoverRowColumns(n.r, n); // Cover the rest of the row and do it right

            if (head.r == head)
            {
                // Insert for all solutions
                // Backtrack
                //UnCoverRowColumns(n.r, n);

                //UnCover(n.d, n);

                //UnHide(n.r, n);

                //h.r.l = h;
                //h.l.r = h;

                //result.Remove(n.x);

                //bool retval = false;

                bool retval = true;
                
                return retval; // Header row is empty, solution found, we are happy
            }

            int min=int.MaxValue;
            Node minNode = head;
            MinCountHeadRow(head.r, ref min, ref minNode); // Continue searching the column with the minimum number of rows.

            bool retVal = Search(minNode.d); // Start with the first row, so we do not wrap around

            if (retVal)
            {
                return true; // Solution found, we are still happy 
            }
            // Could not find a solution in the column, try the next row in the previous column

            // Backtrack
            UnCoverRowColumns(n.r, n);

            UnCover(n.d, n);

            UnHide(n.r, n);

            h.r.l = h;
            h.l.r = h;

            result.Remove(n.x);

            if (callBack != null)
                callBack(n.x, false);

            // Console.WriteLine("Remove row\t\t{0}", n.x);
            // Console.WriteLine("Un cover column\t{0}", h.name);

            // Try the next row
            return Search(n.d);
        }

        void Cover(Node n, Node h)
        {
            if (n == h)
                return; // Threre are no more rows to hide

            if (n != n.c) // Skip the head row if we wrap around. We may very well not start at the top
                Hide(n.r, n); // Hide row

            Cover(n.d, h); // Cover the rest of the column
        }

        void Hide(Node n, Node h)
        {
            if (n == h)
                return; // There are no more nodes in the row to hide

            // Console.WriteLine("\t\t\t\t\t\tHide node\t\t{0}\t{1}", n.x, n.y);

            n.u.d = n.d; // Exclude the node vertically
            n.d.u = n.u;

            Hide(n.r, h); // Hide the rest of the row
        }

        void CoverRowColumns(Node n, Node h)
        {
            if (n == h)
                return; // We have reached the end of the row

            // Console.WriteLine("Covercolumn\t{0}", n.c.name);

            n.c.r.l = n.c.l; // Exclude head horizontally
            n.c.l.r = n.c.r;

            Cover(n.c.d, n.c); // Cover the rest of the rows i the column. Start from the top, the node (n) is already hidden and cannot be used to terminate

            CoverRowColumns(n.r, h); // Cover the rest of the row
        }

        void UnCover(Node n, Node h) // Unhide rows on the way back
        {
            if (n == h)
                return;

            UnCover(n.d, h);
    
            if (n != n.c)
                UnHide(n.r, n);
        }

        void UnHide(Node n, Node h) // Unhide nodes on the way back
        {
            if (n == h)
                return;

            UnHide(n.r, h);

            n.u.d = n;
            n.d.u = n;

            // Console.WriteLine("\t\t\t\t\t\tUn hide node\t{0}\t{1}", n.x, n.y);

        }

        void UnCoverRowColumns(Node n, Node h) // UnCover columns on the way back
        {
            if (n == h)
                return;

            UnCoverRowColumns(n.r, h);

            UnCover(n.c.d, n.c);

            n.c.r.l = n.c;
            n.c.l.r = n.c;

            // Console.WriteLine("Un cover column\t{0}", n.c.name);

        }

        void MinCountHeadRow(Node h, ref int min, ref Node minNode) // Find the column with the minimum number of rows
        {
            if (h == head)
                return;

            if (h.d == h)
            {
                min = 0;
                minNode = h;
                return;
            }
            else
            {
                int count = CountCol(h.d, h);

                if (count < min)
                {
                    min = count;
                    minNode = h;
                }
                MinCountHeadRow(h.r, ref min, ref minNode);
            }

        }

        int CountCol(Node n, Node h)
        {
            if (n == h)
                return 0;

            return CountCol(n.d, h) + 1;
        }

        void SetCRowLinks()
        {
            Node p = headerRow[0];
            p.c = p;
            Node b = p;
            for (int i = 1; i < headerRow.Length; i++)
            {
                Node n = headerRow[i];
                n.c = n;
                n.l = p;
                p.r = n;
                p = n;
            }
            b.l = p;
            p.r = b;

            head.r = headerRow[0];
            head.l = headerRow[0].l;
            head.d = head;
            head.u = head;
            head.c = head;
            headerRow[0].l.r = head;
            headerRow[0].l = head;
        }

        void SetAllCLinks()
        {
            Node n;
            for (int i = 0; i < headerRow.Length; i++)
            {
                n = headerRow[i];

                SetCLinks(i, n);
            }

        }

        void SetCLinks(int column, Node h)
        {
            Node p = h;

            for (int i = 0; i < tableau.GetLength(0); i++)
            {
                Node n = tableau[i, column];
                if (n != null)
                {
                    n.u = p;
                    p.d = n;
                    n.c = h;
                    p = n;
                }
            }
            h.u = p;
            p.d = h;
        }

        void SetAllRLinks()
        {
            for (int i = 0; i < tableau.GetLength(0); i++)
            {
                Node n;
                n = FindFirstRNode(i);

                SetRLinks(i, n);
            }

        }

        Node FindFirstRNode(int row)
        {
            for (int i = 0; i < tableau.GetLength(1); i++)
            {
                Node n = tableau[row, i];
                if (n != null)
                    return n;
            }

            return new Node(row, 0);
        }

        void SetRLinks(int row, Node h)
        {
            Node p = h;

            for (int i = 0; i < tableau.GetLength(1); i++)
            {
                Node n = tableau[row, i];
                if (n != h && n != null)
                {
                    n.l = p;
                    p.r = n;
                    p = n;
                }
                h.l = p;
                p.r = h;
            }

        }

    }
}
