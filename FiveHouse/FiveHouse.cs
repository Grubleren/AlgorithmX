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
            tableau = new Node[125, 112];

            index = 0;
            ElementConstraints();
            HouseConstraints();
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

            Print();
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
            NextTo(3, 1, 4, 1);

            // The Norwegian (1, 2) lives next to the blue house (0, 2)
            NextTo(1, 2, 0, 2);

            // Kools (3, 0) are smoked in the house next to the house where the horse is kept (4, 3)
            NextTo(3, 0, 4, 3);

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
            for (int k = 0; k < 5; k++)
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
        /// Example: The Norwegiam lives next to the blue house
        /// Either the Norwegian lives in house 1 or house 1 is blue or the Norwegian lives in house 3 or house 3 is blue.
        /// This is only a necessary condition covering part of the nextto problem. The situations with houses (0, 3),  (3, 0), (1, 4), (4, 1) is not covered.
        /// Four extra constraints are needed:
        /// Either the Norwegian lives in house 0 or house 1 is not blue
        /// Either house 0 is blue or the Norwegian does not live in house 1
        /// Either the Norwegian lives in house 4 or house 3 is not blue
        /// Either house 4 is blue or the Norwegian does not live in house 3
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="element1"></param>
        /// <param name="group2"></param>
        /// <param name="element2"></param>
        private void NextTo(int group1, int element1, int group2, int element2)
        {
            MarkNode(group1, element1, 1);
            MarkNode(group2, element2, 1);
            MarkNode(group1, element1, 3);
            MarkNode(group2, element2, 3);
            index++;

            MarkNode(group1, element1, 0);
            MarkNotNode(group2, element2, 1);
            index++;

            MarkNode(group2, element2, 0);
            MarkNotNode(group1, element1, 1);
            index++;

            MarkNode(group1, element1, 4);
            MarkNotNode(group2, element2, 3);
            index++;

            MarkNode(group2, element2, 4);
            MarkNotNode(group1, element1, 3);
            index++;

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

            Print();
        }

        void Print()
        {
            string[,] solution = new string[5, 5];

            for (int i = 0; i < algorithmX.result.Count; i++)
            {
                int result = algorithmX.result[i];
                int group = result / 25;
                int element = (result - 25 * group) / 5;
                int house = result - 25 * group - 5 * element;
                solution[house, group] = GroupsAndElements[group, element];
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
