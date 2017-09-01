using System;
using System.Windows.Forms;

namespace JH.Calculations
{
    public partial class FiveHouse : Form
    {
        string[,] GroupsAndElements = new string[,] { { "red", "yellow", "blue", "green", "ivory" },
                                                      { "English", "Spanish", "Norwegian", "Ukranian", "Japanese" }, 
                                                      { "orange juice", "tea", "coffee", "milk", "water" }, 
                                                      { "Kool", "Chesterfield", "Old Gold", "Lucky Strike", "Parliament" }, 
                                                      { "dog", "fox", "snails", "horse", "zebra" } };

        Node[,] tableau;
        AlgorithmX algorithmX;
        int index;

        public FiveHouse()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Solve();
        }

        public void Solve()
        {

            algorithmX = new AlgorithmX();
            algorithmX.callBack = CallBack;
            tableau = new Node[5 * 25 + 3 * 8, 130];

            index = 0;
            HouseConstraints();
            ElementConstraints();
            ExtraConstraints();
            SingleConstraints();
            PairConstraints();
            NextToConstraints();
            RightOfConstraints();

            algorithmX.Init(tableau);

            bool retVal = algorithmX.Search();

            if (retVal)
                Console.WriteLine("Congratulations !");
            else
                Console.WriteLine("No solution");

            PrintResult();
        }

        /// <summary>
        /// Only one house is e.g. red 
        /// </summary>
        private void HouseConstraints()
        {
            for (int j = 0; j < 5; j++) // group
                for (int k = 0; k < 5; k++) // element
                {
                    for (int i = 0; i < 5; i++) // house
                    {
                        MarkNode(j, k, i);
                    }
                    index++;
                }
        }

        /// <summary>
        /// A house can only have e.g one color
        /// </summary>
        private void ElementConstraints()
        {
            for (int j = 0; j < 5; j++) // group
                for (int i = 0; i < 5; i++) // house
                {
                    for (int k = 0; k < 5; k++) // element
                    {
                        MarkNode(j, k, i);
                    }
                    index++;
                }
        }

        /// <summary>
        /// The nextto constraints requires three times eight extra variabels for the eight possible combinations
        /// E.g. n0b1, n1b0, n1b2, n2b1, n2b3, n3b2, n3b4, n4b3, where nx refers to the norwegian living in house x and by refers to the y'th house being blue
        /// </summary>
        private void ExtraConstraints()
        {
            for (int n = 0; n < 8; n++)
                tableau[125 + n, index] = new Node(125 + n, index);
            index++;

            for (int n = 0; n < 8; n++)
                tableau[125 + 8 + n, index] = new Node(125 + 8 + n, index);
            index++;

            for (int n = 0; n < 8; n++)
                tableau[125 + 16 + n, index] = new Node(125 + 16 + n, index);
            index++;
        }

        /// <summary>
        /// Constraints regarding properties of a single house
        /// </summary>
        private void SingleConstraints()
        {
            // The Norwegian (1, 2) lives in house 0 (numbering starts at zero on the left)
            Single(1, 2, 0);

            // Milk (2, 3) is drunk in house 2 (numbering starts at zero on the left)
            Single(2, 3, 2);
        }

        /// <summary>
        /// Constraints regarding pair of houses
        /// </summary>
        private void PairConstraints()
        {
            // The Englishman (1, 0) lives in the red house (0, 0)
            Pair(1, 0, 0, 0);

            // The Spaniard (1, 1) owns the dog (4, 0)
            Pair(1, 1, 4, 0);

            // Kools (3, 0) are smoked in the yellow house (0, 1)
            Pair(3, 0, 0, 1);

            // The Old Gold (3, 2) smoker owns snails (4, 2)
            Pair(3, 2, 4, 2);

            // The Lucky Strike (3, 3) smoker drinks orange juice (2, 0)
            Pair(3, 3, 2, 0);

            // The Ukrainian (1, 3) drinks tea (2, 1)
            Pair(1, 3, 2, 1);

            // The Japanese (1, 4) smokes parliaments (3, 4)
            Pair(1, 4, 3, 4);

            // Coffee (2, 2) is drunk in the green house (0, 3) 
            Pair(2, 2, 0, 3);

        }

        /// <summary>
        /// Constraints regarding two houses next to each other
        /// </summary>
        private void NextToConstraints()
        {
            // The man who smokes Chesterfields (3, 1) lives in the house next to the man with the fox (4, 1)
            NextTo(125, 3, 1, 4, 1);

            // The Norwegian (1, 2) lives next to the blue house (0, 2)
            NextTo(125 + 8, 1, 2, 0, 2);

            // Kools (3, 0) are smoked in the house next to the house where the horse is kept (4, 3)
            NextTo(125 + 16, 3, 0, 4, 3);

        }

        /// <summary>
        /// Constraints regarding two houses where one is immediately to the right of the other
        /// </summary>
        private void RightOfConstraints()
        {
            // The green (0, 3) house is immediately to the right of the ivory house (0, 4)
            RightOf(0, 4, 0, 3);
        }

        /// <summary>
        ///  Marks the node (group, element, house) in the tableau
        ///  and advance the constraint index
        /// </summary>
        /// <param name="group"> E.g. nationality (1)</param>
        /// <param name="element">E.g. Norwegian (2)</param>
        /// <param name="house">House number</param>
        private void Single(int group, int element, int house)
        {
            MarkNode(group, element, house);
            index++;
        }

        /// <summary>
        ///  Marks the node (group, element, house) in the tableau
        /// </summary>
        /// <param name="group"> E.g. nationality (1)</param>
        /// <param name="element">E.g. Norwegian (2)</param>
        /// <param name="house">House number</param>
        private void MarkNode(int group, int element, int house)
        {
            int x = 25 * group + 5 * element + house;
            tableau[x, index] = new Node(x, index);
        }

        /// <summary>
        /// Marks the nodes not being element, e.g. yellow, blue, green, ivory (red is not marked) 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="element"></param>
        /// <param name="house"></param>
        private void MarkNotNode(int group, int element, int house)
        {
            for (int k = 0; k < 5; k++) // element
            {
                if (k != element)
                {
                    MarkNode(group, k, house);
                }
            }
        }

        /// <summary>
        /// Example: The Spaniard owns the dog
        /// Either the Spaniard is in the i'th house or the dog is not in the i'th house,
        /// meaning that one of the other pets is in the i'th house
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="element1"></param>
        /// <param name="group2"></param>
        /// <param name="element2"></param>
        private void Pair(int group1, int element1, int group2, int element2)
        {
            for (int i = 0; i < 5; i++) // house
            {
                MarkNode(group1, element1, i);
                MarkNotNode(group2, element2, i);
                index++;
            }
        }

        /// <summary>
        /// Marks the extra variables except those containing house (i)
        /// </summary>
        /// <param name="offset">Row for extra variables</param>
        /// <param name="i">house</param>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        private void MarkExtra(int offset, int i, int m1, int m2)
        {
            for (int j = 0; j < 4; j++)
            {
                int n = j + m1;
                if (n != i)
                    tableau[offset + j, index] = new Node(offset + j, index);
            }

            for (int j = 0; j < 4; j++)
            {
                int n = j + m2;
                if (n != i)
                    tableau[offset + j + 4, index] = new Node(offset + j, index);
            }
        }
        /// <summary>
        /// Example: The Norwegiam lives next to the blue house
        /// The eight extra variables are needed: n0b1, n1b0, n1b2, n2b1, n2b3, n3b2, n3b4, n4b3
        /// Either the Norwegian lives in house (3) or the blue house is not next to house (3) (n0b1 or n1b0 or n1b2 or n2b1 or n2b3 or n4b3)
        /// and either house (2) is blue or the Norwegian does not live next to house (2) (n0b1 or n1b0 or n2b1 or n2b3 or n3b4 or n4b3)
        /// </summary>
        /// <param name="offset">Row of extra variables</param>
        /// <param name="group1"></param>
        /// <param name="element1"></param>
        /// <param name="group2"></param>
        /// <param name="element2"></param>
        private void NextTo(int offset, int group1, int element1, int group2, int element2)
        {
            for (int i = 0; i < 5; i++)
            {
                MarkNode(group1, element1, i);
                MarkExtra(offset, i, 0, 1);
                index++;
            }

            for (int i = 0; i < 5; i++)
            {
                MarkNode(group2, element2, i);
                MarkExtra(offset, i, 1, 0);
                index++;
            }

        }

        /// <summary>
        /// Example: The green house is to the right of the ivory house
        /// Either the i'th house is ivory or the (i + 1)'th house is not green
        /// and
        /// The house 4 is not ivory
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="element1"></param>
        /// <param name="group2"></param>
        /// <param name="element2"></param>
        private void RightOf(int group1, int element1, int group2, int element2)
        {
            for (int i = 0; i < 4; i++) // only the first 4 houses have one to the right
            {
                MarkNode(group1, element1, i);
                MarkNotNode(group2, element2, i + 1);
                index++;
            }

            MarkNotNode(group1, element1, 4); // There is no house to the right of house 4, so element1 cannot belong to house 4
            index++;
        }

        void CallBack(int x, bool draw)
        {
            if (draw)
                Console.WriteLine("Forward:");
            else
                Console.WriteLine("Back:");

            PrintResult();
        }

        void PrintResult()
        {
            string[,] solution = new string[5, 5];

            for (int i = 0; i < algorithmX.result.Count; i++)
            {
                int result = algorithmX.result[i];
                if (result < 125)
                {
                    int group = result / 25;
                    int element = (result - 25 * group) / 5;
                    int house = result - 25 * group - 5 * element;
                    solution[house, group] = GroupsAndElements[group, element];
                }
            }

            for (int i = 0; i < 5; i++)
            {
                Console.Write("{0,-4}", i + 1);
                for (int j = 0; j < 5; j++)
                    Console.Write("{0,-15}", solution[i, j]);

                Console.WriteLine();
            }

            Console.WriteLine();
        }

    }
}
