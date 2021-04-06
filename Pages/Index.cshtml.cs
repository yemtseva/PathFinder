using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Security.Permissions;
using System.Reflection.Metadata;
using System.IO;
using Newtonsoft.Json;

namespace PathFindingVisualizer.Pages
{




    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly int INFINITY = 9999;
        private readonly int NIL = -1;
        static public readonly int START_ROW = 7;
        static public readonly int START_COL = 10;
        static public readonly int FINISH_ROW = 7;
        static public readonly int FINISH_COL = 10;


        public List<List<Node>> Grid = new List<List<Node>>();
        public List<Node> shortestPath = new List<Node>();
        public List<Node> visitedNodes = new List<Node>();
        public bool isDone { get; set; }

        public static int new_col { get; set; }


        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;




            for (int row = 0; row < Node.row_num; row++)
            {
                List<Node> currentRow = new List<Node>();
                for (int col = 0; col < Node.col_num; col++)
                {
                    if (Node.start_row == row && Node.start_col == col)
                        currentRow.Add(new Node(row, col, true, false, 0, true));
                    else if (Node.finish_row == row && Node.finish_col == col)
                        currentRow.Add(new Node(row, col, false, true));
                    else
                        currentRow.Add(new Node(row, col, false, false));

                }
                Grid.Add(currentRow);

            }

            int Wrow;
            int Wcol;
            if (Node.walls_rows != null)
            {
                for (int i = 0; i < Node.walls_rows.Length; i++)
                {
                    Wrow = Int32.Parse(Node.walls_rows[i]);
                    Wcol = Int32.Parse(Node.walls_cols[i]);
                    Grid[Wrow][Wcol].isWall = true;

                }
                Node.walls_rows = null;
                Node.walls_cols = null;
            }

            if (Node.Rwalls_rows != null)
            {
                Console.WriteLine(Node.Rwalls_rows[0]);
                for (int i = 0; i < Node.Rwalls_rows.Length; i++)
                {
                    Wrow = Int32.Parse(Node.Rwalls_rows[i]);
                    Wcol = Int32.Parse(Node.Rwalls_cols[i]);
                    Grid[Wrow][Wcol].isWall = true;

                }
                Node.Rwalls_rows = null;
                Node.Rwalls_cols = null;

            }


        }


        public void Dijkstras()
        {
            List<Node> notVisitedNodes = new List<Node>();
            List<Node> currentNeighbors = new List<Node>();
            int min_index = NIL;
            int weight = 1;

            notVisitedNodes = RefreshBoard();

            while (true)
            {
                min_index = TempVertexMinPL(notVisitedNodes);

                if (min_index == NIL)
                    return;
                else if (Grid[Node.finish_row][Node.finish_col].isVisited == true)
                    return;

                notVisitedNodes[min_index].isVisited = true;
                visitedNodes.Add(notVisitedNodes[min_index]);

                currentNeighbors = findNeighbors(notVisitedNodes[min_index]);

                for (int i = 0; i < currentNeighbors.Count; i++)
                {
                    UpdateNode(currentNeighbors[i], notVisitedNodes[min_index], weight);
                }
                notVisitedNodes.RemoveAt(min_index);
            }
        }

        public List<Node> FindPath(Node startNode, Node finishNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = finishNode;


            while (true)
            {
                if (path.Contains(startNode))
                    break;

                path.Add(currentNode.previousNode);
                currentNode = currentNode.previousNode;
            }

            return path;
        }



        public List<Node> RefreshBoard()
        {
            List<Node> notVisitedNodes = new List<Node>();
            foreach (var rows in Grid)
            {
                foreach (var nodes in rows)
                {
                    nodes.isVisited = false;
                    nodes.cost = INFINITY;
                    nodes.isCurrent = false;
                    nodes.previousNode = null;
                    notVisitedNodes.Add(nodes);
                }

            }
            Grid[Node.start_row][Node.start_col].cost = 0;

            return notVisitedNodes;
        }

        public ActionResult ClearWalls()
        {
            for (int row = 0; row < Node.row_num; row++)
            {
                for (int col = 0; col < Node.col_num; col++)
                {
                    Grid[row][col].isWall = false;

                }

            }
            return Page();
        }

        public void UpdateNode(Node currentNode, Node previousNode, int weight)
        {
            if (currentNode.cost > previousNode.cost + weight)
            {
                currentNode.cost = previousNode.cost + weight;
                currentNode.previousNode = previousNode;
            }

        }

        public int TempVertexMinPL(List<Node> notVisitedNodes)
        {
            int min = INFINITY;
            int min_index = NIL;
            for (int i = 0; i < notVisitedNodes.Count; i++)
            {
                if (notVisitedNodes[i].cost < min)
                {
                    min = notVisitedNodes[i].cost;
                    min_index = i;
                }
            }
            return min_index;
        }

        public List<Node> findNeighbors(Node currentNode)
        {
            List<Node> neighbors = new List<Node>();
            Node t_currentNode = currentNode;
            Node rightNeighbor = null;
            Node leftNeighbor = null;
            Node upNeighbor = null;
            Node downNeighbor = null;

            if (currentNode.col + 1 <= Node.col_num - 1)
                rightNeighbor = Grid[currentNode.row].Find(x => x.col == (t_currentNode.col + 1));
            if (currentNode.col - 1 >= 0)
                leftNeighbor = Grid[currentNode.row].Find(x => x.col == (t_currentNode.col - 1));
            if (currentNode.row - 1 >= 0)
                upNeighbor = Grid[currentNode.row - 1].Find(x => x.col == (t_currentNode.col));
            if (currentNode.row + 1 <= Node.row_num - 1)
                downNeighbor = Grid[currentNode.row + 1].Find(x => x.col == (t_currentNode.col));

            if (rightNeighbor != null && rightNeighbor.isVisited == false && rightNeighbor.isWall == false)
                neighbors.Add(rightNeighbor);
            if (downNeighbor != null && downNeighbor.isVisited == false && downNeighbor.isWall == false)
                neighbors.Add(downNeighbor);
            if (leftNeighbor != null && leftNeighbor.isVisited == false && leftNeighbor.isWall == false)
                neighbors.Add(leftNeighbor);
            if (upNeighbor != null && upNeighbor.isVisited == false && upNeighbor.isWall == false)
                neighbors.Add(upNeighbor);

            return neighbors;

        }

        public void OnPost()
        {

            Dijkstras();
            if (Grid[Node.finish_row][Node.finish_col].previousNode != null)
            {
                shortestPath = FindPath(Grid[Node.start_row][Node.start_col], Grid[Node.finish_row][Node.finish_col]);
            }
            Node.isAnimating = true;


        }

        public void OnGet()
        {
            Node.isAnimating = false;
            ClearWalls();

        }

        public ActionResult OnPostRandomWalls()
        {
            Console.WriteLine("Rand");
            MemoryStream stream = new MemoryStream();

            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var obj = JsonConvert.DeserializeObject<PostData>(requestBody);
                    if (obj != null)
                    {
                        Node.Rwalls_rows = obj.Item7; // rows of walls
                        Node.Rwalls_cols = obj.Item8; // cols of walls

                    }
                }
            }

            return null;

        }


        public ActionResult OnPostWalls()
        {
            Console.WriteLine("defa");
            MemoryStream stream = new MemoryStream();

            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var obj = JsonConvert.DeserializeObject<PostData>(requestBody);
                    if (obj != null)
                    {
                        Node.walls_rows = obj.Item4; // rows of walls
                        Node.walls_cols = obj.Item5; // cols of walls

                    }
                }
            }
            return null;

        }

        public ActionResult OnPostStarting()
        {
            string sPostValue1 = "";
            string sPostValue2 = "";
            string sPostValue3 = "";



            MemoryStream stream = new MemoryStream();

            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var obj = JsonConvert.DeserializeObject<PostData>(requestBody);
                    if (obj != null)
                    {
                        sPostValue1 = obj.Item1; // row
                        sPostValue2 = obj.Item2; // col
                        sPostValue3 = obj.Item3; // node_class

                    }
                }
            }

            int new_row = Int32.Parse(sPostValue1);
            int new_col = Int32.Parse(sPostValue2);
            string node_class = sPostValue3;

            if (node_class == "start")
            {
                Grid[Node.start_row][Node.start_col].isStart = false;
                Node.start_col = new_col;
                Node.start_row = new_row;
                Grid[Node.start_row][Node.start_col].isStart = true;
            }
            else
            {
                Grid[Node.finish_row][Node.finish_col].isFinish = false;
                Node.finish_col = new_col;
                Node.finish_row = new_row;
                Grid[Node.finish_row][Node.finish_col].isFinish = true;
            }
            return null;
        }



    }

    public class Node
    {

        public bool isStart { get; set; }
        public bool isFinish { get; set; }
        public bool isVisited { get; set; }
        public bool isCurrent { get; set; }
        public bool isWall { get; set; }
        public int cost { get; set; }
        public int col { get; set; }
        public int row { get; set; }
        public Node previousNode { get; set; }


        public static int col_num { get; set; } = 89;
        public static int row_num { get; set; } = 49;
        public static int start_row { get; set; } = 20;
        public static int start_col { get; set; } = 36;
        public static int finish_row { get; set; } = 20;
        public static int finish_col { get; set; } = 52;
        public static string[] walls_rows { get; set; } = null;
        public static string[] walls_cols { get; set; } = null;
        public static string[] Rwalls_rows { get; set; } = null;
        public static string[] Rwalls_cols { get; set; } = null;
        public static bool isAnimating { get; set; } = false;



        public Node(int row, int col, bool isStart, bool isFinish, int cost = 9999, bool isCurrent = false)
        {
            this.row = row;
            this.col = col;
            this.isStart = isStart;
            this.isFinish = isFinish;
            this.cost = cost;
            this.isCurrent = isCurrent;
            this.isWall = false;
            this.previousNode = null;

        }
    }
    public class PostData
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public string Item3 { get; set; }
        public string[] Item4 { get; set; }
        public string[] Item5 { get; set; }
        public int Item6 { get; set; }
        public string[] Item7 { get; set; }
        public string[] Item8 { get; set; }

    }
}
