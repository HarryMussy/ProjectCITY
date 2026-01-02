using System.Text.Json.Serialization;

namespace CitySkylines0._5alphabeta
{
    public class Grid
    {
        public List<Node> nodes { get; set; } = new();
        public List<Node> roadNodes { get; set; } = new();
        public List<Node> buildableNodes { get; set; } = new();
        public List<Edge> edges { get; set; } = new();
        public List<IntersectingNode> roadIntersections { get; set; } = new();
        public List<Node> nodesIntersectingRoads { get; set; } = new();
        public List<Building> buildings { get; set; } = new();

        [JsonIgnore]
        public List<PictureBox> roadImages { get; set; }

        public float cash { get; set; }

        public int width { get; set; }
        public int height { get; set; }

        [JsonIgnore]
        public int rectSize;

        [JsonIgnore]
        public Background background { get; set; }

        public Grid() { } // required

        public Grid(int width, int height, Background background, int rectSize)
        {
            this.width = width;
            this.height = height;
            this.background = background;
            cash = 100000;
            this.rectSize = rectSize;
            CreateNodes();
            InitializeWithBackground(background);
        }
        public void CreateNodes()
        {
            int tempNum = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Point coords = new Point(x * rectSize, y * rectSize);
                    Node node = new Node(new Point(coords.X, coords.Y), null, false, tempNum++, false, false);
                    nodes.Add(node);
                }
            }

        }
        public float RoadCashCost(Point a, Point b)
        {
            float bsquared = (a.X - b.X) * (a.X - b.X);
            float csquared = (a.Y - b.Y) * (a.Y - b.Y);
            float roadLength = (float)Math.Sqrt(bsquared + csquared);

            float expense = roadLength * 2;
            return expense;
        }


        /*public void CheckIntersectingRoads()
        {
            //clear old data
            roadIntersections.Clear();
            List<Edge[]> checkedPairs = new List<Edge[]>();

            for (int i = 0; i < edges.Count; i++)
            {
                for (int j = 0; j < edges.Count; j++)
                {
                    Edge e1 = edges[i];
                    Edge e2 = edges[j];

                    Edge[] newPair = { e1, e2 };

                    foreach (Edge[] pair in checkedPairs)
                    {
                        if ((pair[0] == e1 && pair[1] == e2) || (pair[0] == e2 && pair[1] == e1))
                        {
                            return;
                        }
                    }

                    // --- CASE 1: Roads physically cross (mid-road intersection) ---
                    if (DoIntersect(e1.a, e1.b, e2.a, e2.b))
                    {
                        Point intersection = FindIntersectionPoint(e1.a, e1.b, e2.a, e2.b);
                        if (intersection != Point.Empty)
                            AddSharedIntersection(e1, e2, intersection);
                    }

                    // --- CASE 2: Roads meet end-to-end (share endpoints) ---
                    if (e1.a == e2.a) AddSharedIntersection(e1, e2, e1.a);
                    if (e1.a == e2.b) AddSharedIntersection(e1, e2, e1.a);
                    if (e1.b == e2.a) AddSharedIntersection(e1, e2, e1.b);
                    if (e1.b == e2.b) AddSharedIntersection(e1, e2, e1.b);

                    checkedPairs.Add(newPair);
                }
            }
        }


        private void AddSharedIntersection(Edge e1, Edge e2, Point intersection)
        {
            // Try to find an existing intersection node close to this point
            IntersectingNode sharedNode = roadIntersections.FirstOrDefault(node => Math.Abs(node.coords.X - intersection.X) < 10 && Math.Abs(node.coords.Y - intersection.Y) < 10);

            if (sharedNode == null)
            {
                sharedNode = new IntersectingNode(intersection);
                roadIntersections.Add(sharedNode);
            }

            // Make sure both edges reference the same intersection node
            if (!e1.intersections.Contains(sharedNode))
                e1.intersections.Add(sharedNode);
            if (!e2.intersections.Contains(sharedNode))
                e2.intersections.Add(sharedNode);

            // Register both roads in the node’s edge list
            if (!sharedNode.connectedEdges.Contains(e1))
                sharedNode.connectedEdges.Add(e1);
            if (!sharedNode.connectedEdges.Contains(e2))
                sharedNode.connectedEdges.Add(e2);
        }



        public int Orientation(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0; // collinear
            return (val > 0) ? 1 : 2; // clockwise or counterclockwise
        }
        public bool OnSegment(Point p, Point q, Point r)
        {
            return r.X <= Math.Max(p.X, q.X) && r.X >= Math.Min(p.X, q.X) &&
                   r.Y <= Math.Max(p.Y, q.Y) && r.Y >= Math.Min(p.Y, q.Y);
        }
        public bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            //find the four orientations needed for the general and special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            //general case
            if (o1 != o2 && o3 != o4)
                return true;

            //special cases: checking if the points are on the segment
            if (o1 == 0 && OnSegment(p1, q1, p2)) return true;
            if (o2 == 0 && OnSegment(p1, q1, q2)) return true;
            if (o3 == 0 && OnSegment(p2, q2, p1)) return true;
            if (o4 == 0 && OnSegment(p2, q2, q1)) return true;

            return false; // Otherwise, they don't intersect
        }
        public Point FindIntersectionPoint(Point p1, Point q1, Point p2, Point q2)
        {
            // calculate the intersection point of the two lines
            float a1 = q1.Y - p1.Y;
            float b1 = p1.X - q1.X;
            float c1 = a1 * p1.X + b1 * p1.Y;

            float a2 = q2.Y - p2.Y;
            float b2 = p2.X - q2.X;
            float c2 = a2 * p2.X + b2 * p2.Y;

            float determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                return Point.Empty; //no intersection (lines are parallel)
            }
            else
            {
                float x = (b2 * c1 - b1 * c2) / determinant;
                float y = (a1 * c2 - a2 * c1) / determinant;
                return new Point((int)x, (int)y); //return the intersection point
            }
        }*/

        public void InitializeWithBackground(Background bg)
        {
            background = bg;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].coords = new Point(bg.tiles[i].coords.X, bg.tiles[i].coords.Y);
                nodes[i].isGrass = bg.tiles[i].isGrass;
                nodes[i].tileData = bg.tiles[i].tileData;
            }
        }

        public List<Node> FindRoadNodeIntersectionsForSpecificEdge(Edge road)
        {
            List<Node> intersectingNodesWithEdge = new List<Node>();
            foreach (Node node in nodes)
            {
                node.isNearRoad = false;
                foreach (Point n in road.pointsOnTheEdge)
                {
                    //check if the node intersects with the road edge (within range)
                    if (node.coords.X + 8 <= n.X + rectSize && node.coords.X + 8 >= n.X - rectSize && node.coords.Y + 8 <= n.Y + rectSize && node.coords.Y + 8 >= n.Y - rectSize)
                    {
                        intersectingNodesWithEdge.Add(node);
                    }
                }
            }
            return intersectingNodesWithEdge;
        }

        public void FindRoadNodeIntersections()
        {
            if (edges == null) return;

            //clear previous state
            nodesIntersectingRoads.Clear();
            roadNodes.Clear();
            buildableNodes.Clear();

            foreach (Node node in nodes)
            {
                node.isNearRoad = false;
                node.isRoad = false;
            }

            foreach (Road road in edges)
            {
                foreach (Point n in road.pointsOnTheEdge)
                {
                    foreach (Node node in nodes)
                    {
                        //road node check
                        if (node.coords.X + 8 <= n.X + rectSize && node.coords.X + 8 >= n.X - rectSize &&
                            node.coords.Y + 8 <= n.Y + rectSize && node.coords.Y + 8 >= n.Y - rectSize)
                        {
                            node.isRoad = true;
                            nodesIntersectingRoads.Add(node);
                            roadNodes.Add(node);
                        }
                        //near-road check
                        else if (node.coords.X + 8 <= n.X + (rectSize * 8) && node.coords.X + 8 >= n.X - (rectSize * 8) &&
                                 node.coords.Y + 8 <= n.Y + (rectSize * 8) && node.coords.Y + 8 >= n.Y - (rectSize * 8))
                        {
                            node.isNearRoad = true;

                            node.IsNodeBuildable();
                            if (node.isBuildable)
                            {
                                if (!buildableNodes.Any(b => b.coords == node.coords))
                                    buildableNodes.Add(node);
                            }
                        }
                    }
                }
            }

            //cleanup: remove any buildables that are now roads
            buildableNodes.RemoveAll(n => n.isRoad);
        }

    }
}