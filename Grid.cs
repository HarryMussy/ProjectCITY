using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace CitySkylines0._5alphabeta
{
    public class Grid
    {
        public List<Node> nodes;
        public List<Node> backgroundNodes;
        public List<Node> buildableNodes;
        public List<Edge> edges;
        public List<IntersectingNode> roadIntersections;
        public List<Node> nodesIntersectingRoads;
        public List<Building> buildings;
        public List<PictureBox> roadImages;
        public float cash; //called Musbux
        private Background background;
        public List<Point> AllPoints => edges.SelectMany(edge => new List<Point> { edge.a, edge.b }).Distinct().ToList();
        private int width, height;
        public Grid(int width, int height, Background background)
        {
            nodesIntersectingRoads = new List<Node>();
            nodes = new List<Node>();
            backgroundNodes = new List<Node>();
            edges = new List<Edge>();
            buildableNodes = new List<Node>();
            buildings = new List<Building>();
            roadIntersections = new List<IntersectingNode>();
            this.background = background;
            //generate a grid with buildable nodes
            this.width = width;
            this.height = height;
            CreateNodes();
            cash = 100000;
            InitializeWithBackground(background);
        }
        public void CreateNodes()
        {
            int tempNum = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Point coords = new Point(x * 20, y * 20);
                    Node node = new Node(coords.X, coords.Y, null, false, tempNum++, false);
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


        public void CheckIntersectingRoads()
        {
            for (int i = 0; i < edges.Count; i++)
            {
                for (int j = i + 1; j < edges.Count; j++)
                {
                    Edge edge1 = edges[i];
                    Edge edge2 = edges[j];

                    if (DoIntersect(edge1.a, edge1.b, edge2.a, edge2.b))
                    {
                        Point intersection = FindIntersectionPoint(edge1.a, edge1.b, edge2.a, edge2.b);

                        //add the intersection point as a new node to the edges
                        edge1.AddIntersection(intersection);
                        edge2.AddIntersection(intersection);
                    }
                }
            }
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
        }

        public void InitializeWithBackground(Background bg)
        {
            background = bg;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] = bg.tiles[i];
            }
        }
        public void FindRoadNodeIntersections()
        {
            if (edges != null)
            {
                foreach (Node node in nodes)
                {
                    node.isNearRoad = false;
                    foreach (Road road in edges)
                    {
                        foreach (Point n in road.pointsOnTheEdge)
                        {
                            //check if the node intersects with the road edge (within range)
                            if (node.coords.X + 10 <= n.X + 20 && node.coords.X + 10 >= n.X - 20 && node.coords.Y + 10 <= n.Y + 20 && node.coords.Y + 10 >= n.Y - 20)
                            {
                                nodesIntersectingRoads.Add(node);
                            }
                            else if (node.coords.X + 10 <= n.X + 80 && node.coords.X + 10 >= n.X - 80 && node.coords.Y + 10 <= n.Y + 80 && node.coords.Y + 10 >= n.Y - 80)
                            {
                                node.isNearRoad = true;

                                //only add to buildableNodes if the node is not water and is not already occupied by a building
                                bool alreadyExists = buildableNodes.Any(b => b.coords == node.coords);

                                node.IsNodeBuildable(); //update buildable status based on current conditions
                                if (node.isBuildable && !alreadyExists) //ensure the node is not water
                                {
                                    buildableNodes.Add(node);
                                }
                            }
                        }
                    }
                }
            }

            //clean up nodes that were previously marked as near roads but should not be anymore (if they're no longer valid)
            foreach (Node n in nodesIntersectingRoads)
            {
                n.isNearRoad = false;
                bool alreadyExists = buildableNodes.Any(b => b.coords == n.coords);
                if (!alreadyExists)
                {
                    n.IsNodeBuildable(); //update buildable status based on current conditions
                    buildableNodes.Remove(n);
                }
            }
        }
    }
}