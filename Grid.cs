using System.Text.Json.Serialization;
using System.Windows.Controls;

namespace CitySkylines0._5alphabeta
{
    public class Grid
    {
        public List<Node> nodes { get; set; } = new();
        public List<Road> roads { get; set; } = new();
        public List<Building> buildings { get; set; } = new();
        public float cash { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public int rectSize;
        [JsonIgnore] public Background background { get; set; }

        public Grid() { } // required for JSON deserialisation

        public Grid(int width, int height, Background background, int rectSize)
        {
            this.width = width;
            this.height = height;
            this.background = background;
            cash = 500000;
            this.rectSize = rectSize;
            CreateNodes();
            InitializeWithBackground(background);
        }

        //creates the 2D flat array of nodes that represents the game map
        public void CreateNodes()
        {
            int nodeNumber = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Point coords = new Point(x * rectSize, y * rectSize);
                    Node node = new Node(new Point(coords.X, coords.Y), false, false, false, false, nodeNumber++);
                    nodes.Add(node);
                }
            }
        }

        //calculates road placement cost using euclidean distance between two points
        public float RoadCashCost(Point a, Point b)
        {
            float bsquared = (a.X - b.X) * (a.X - b.X);
            float csquared = (a.Y - b.Y) * (a.Y - b.Y);
            float roadLength = (float)Math.Sqrt(bsquared + csquared);

            float expense = roadLength * 2;
            return expense;
        }

        //copies terrain data (grass/water) from the background tiles into the grid nodes
        public void InitializeWithBackground(Background bg)
        {
            background = bg;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].coords = new Point(bg.tiles[i].coords.X, bg.tiles[i].coords.Y);
                nodes[i].isGrass = bg.tiles[i].isGrass;
                nodes[i].hasTileData = bg.tiles[i].hasTileData;
            }
        }

        //returns all nodes within one tile of either lane of a given road
        public List<Node> FindAdjacentTilesToARoad(Road road)
        {
            List<Node> intersectingNodesWithEdge = new List<Node>();
            foreach (Node node in nodes)
            {
                node.isNearRoad = false;
                foreach (Point point in road.lane1.pointsOnTheEdge)
                {
                    if (IsNodeAt(node, point))
                    {
                        intersectingNodesWithEdge.Add(node);
                    }
                }

                foreach (Point point in road.lane2.pointsOnTheEdge)
                {
                    if (IsNodeAt(node, point))
                    {
                        intersectingNodesWithEdge.Add(node);
                    }
                }
            }
            return intersectingNodesWithEdge;
        }

        //marks each node on a specific lane as a road node and returns their indices
        //also sets the allowed travel direction on each node based on the lane's orientation
        public List<int> FindRoadTilesForSpecificEdge(Edge e, int laneIndex)
        {
            List<Node> newRoadNodes = new();
            Point roadDir = new Point(Math.Sign(e.b.X - e.a.X), Math.Sign(e.b.Y - e.a.Y));

            foreach (Point p in e.pointsOnTheEdge)
            {
                Point roadPos = p;
                Node roadNode = null;

                foreach (Node node in nodes)
                {
                    if (IsNodeAt(node, roadPos)) { roadNode = node; }
                }

                if (roadNode != null)
                {
                    roadNode.isRoad = true;
                    roadNode.allowedDirs.Clear();
                    roadNode.allowedDirs.Add(roadDir);
                    roadNode.laneIndex = laneIndex;

                    newRoadNodes.Add(roadNode);
                }
            }

            return newRoadNodes.Select(n => n.nodeNumber).ToList();
        }

        //checks whether a node's centre falls within half a tile of a given point
        public bool IsNodeAt(Node node, Point p)
        {
            return Math.Abs(node.Center(rectSize).X - p.X) <= rectSize / 2 && Math.Abs(node.Center(rectSize).Y - p.Y) <= rectSize / 2;
        }

        //checks whether a node's centre falls within tileCheckWidth tiles of a given point
        public bool IsNodeNear(Node node, Point p, int tileCheckWidth)
        {
            var center = node.Center(rectSize);

            return Math.Abs(center.X - p.X) <= rectSize * tileCheckWidth && Math.Abs(center.Y - p.Y) <= rectSize * tileCheckWidth;
        }

        //iterates every road and marks which nodes are road tiles and which are near-road (buildable) tiles
        //called whenever a road is added or removed
        public void FindRoadTilesAndAdjacentRoadTiles()
        {
            if (roads == null) return;

            //clear previous state before recalculating
            foreach (Node node in nodes)
            {
                node.isNearRoad = false;
                node.isRoad = false;
            }


            foreach (Road road in roads)
            {
                road.lane1.occupyingNodesIndex.Clear();
                road.lane2.occupyingNodesIndex.Clear();

                foreach (Point p in road.lane1.pointsOnTheEdge)
                {
                    foreach (Node node in nodes)
                    {
                        if (IsNodeAt(node, p))
                        {
                            node.isRoad = true;
                            road.lane1.occupyingNodesIndex.Add(node.nodeNumber);
                        }

                        //nodes within one tile of a road edge become buildable
                        else if (IsNodeNear(node, p, 1))
                        {
                            node.isNearRoad = true;
                            node.IsNodeBuildable();
                        }
                    }
                }

                foreach (Point p in road.lane2.pointsOnTheEdge)
                {
                    foreach (Node node in nodes)
                    {
                        if (IsNodeAt(node, p))
                        {
                            node.isRoad = true;
                            road.lane2.occupyingNodesIndex.Add(node.nodeNumber);
                        }

                        else if (IsNodeNear(node, p, 1))
                        {
                            node.isNearRoad = true;
                            node.IsNodeBuildable();
                        }
                    }
                }
            }
        }

        //rebuilds the neighbour graph for every road node used by A* pathfinding
        //called after any change to the road network
        public void RebuildEntireRoadGraph()
        {
            //clear existing graph data before rebuilding
            foreach (Node n in nodes.Where(node => node.isRoad))
            {
                n.allowedDirs.Clear();
                n.neighbors.Clear();
            }

            foreach (Road road in roads)
            {
                Point lane1Dir = new Point(Math.Sign(road.lane1.b.X - road.lane1.a.X), Math.Sign(road.lane1.b.Y - road.lane1.a.Y));
                foreach (int index in road.lane1.occupyingNodesIndex)
                {
                    BuildNodeGraph(nodes.FirstOrDefault(node => node.nodeNumber == index), lane1Dir, road.lane1, road.lane2);
                }

                Point lane2Dir = new Point(Math.Sign(road.lane2.b.X - road.lane2.a.X), Math.Sign(road.lane2.b.Y - road.lane2.a.Y));
                foreach (int index in road.lane2.occupyingNodesIndex)
                {
                    BuildNodeGraph(nodes.FirstOrDefault(node => node.nodeNumber == index), lane2Dir, road.lane2, road.lane1);
                }
            }
        }

        //assigns allowed directions and neighbours to a single node
        //lane ends connect to the opposite lane to allow u-turns like a culdesac
        private void BuildNodeGraph(Node node, Point roadDir, Edge lane, Edge otherLane)
        {
            if (!node.allowedDirs.Contains(roadDir)) { node.allowedDirs.Add(roadDir); }

            //if this node is at the start or end of its lane, connect it to the opposite lane
            if (IsLaneEnd(lane, node))
            {
                AddOppositeLaneDirection(lane, node, otherLane);
            }

            //for every allowed direction, find and register the neighbouring road node
            foreach (Point dir in node.allowedDirs)
            {
                int targetX = node.coords.X + dir.X * rectSize;
                int targetY = node.coords.Y + dir.Y * rectSize;

                Node neighbor = nodes.Where(n => n.isRoad).FirstOrDefault(other => other.coords.X == targetX && other.coords.Y == targetY);

                if (neighbor != null && !node.neighbors.Contains(neighbor))
                {
                    node.neighbors.Add(neighbor);
                }
            }

            //nodes on the same tile but different lanes are neighbours to allow lane switching at intersections
            Node opposite = nodes.Where(n => n.isRoad).FirstOrDefault(other => other.coords == node.coords && other.laneIndex != node.laneIndex);
            if (opposite != null && !node.neighbors.Contains(opposite))
            {
                node.neighbors.Add(opposite);
            }
        }

        //returns true if this node is the first or last node in the lane
        private bool IsLaneEnd(Edge lane, Node n)
        {
            return lane.occupyingNodesIndex[0] == n.nodeNumber || lane.occupyingNodesIndex[lane.occupyingNodesIndex.Count() - 1] == n.nodeNumber;
        }

        //connects a lane-end node to the corresponding end of the opposite lane so traffic can loop back
        private void AddOppositeLaneDirection(Edge lane, Node node, Edge otherLane)
        {
            if (lane.occupyingNodesIndex[0] == node.nodeNumber)
            {
                int newNeighbourIndex = otherLane.occupyingNodesIndex[otherLane.occupyingNodesIndex.Count() - 1];
                Node newNeighbour = nodes.FirstOrDefault(n => n.nodeNumber == newNeighbourIndex);
                node.neighbors.Add(newNeighbour);
                Point dirToOtherNode = new Point(Math.Sign(newNeighbour.coords.X - node.coords.X), Math.Sign(newNeighbour.coords.Y - node.coords.Y));
                node.allowedDirs.Add(dirToOtherNode);
            }

            if (lane.occupyingNodesIndex[lane.occupyingNodesIndex.Count() - 1] == node.nodeNumber)
            {
                int newNeighbourIndex = otherLane.occupyingNodesIndex[0];
                Node newNeighbour = nodes.FirstOrDefault(n => n.nodeNumber == newNeighbourIndex);
                node.neighbors.Add(newNeighbour);
                Point dirToOtherNode = new Point(Math.Sign(newNeighbour.coords.X - node.coords.X), Math.Sign(newNeighbour.coords.Y - node.coords.Y));
                node.allowedDirs.Add(dirToOtherNode);
            }
        }
    }
}