using System.Text.Json.Serialization;
using System.Windows.Controls;

namespace CitySkylines0._5alphabeta
{
    public class Grid
    {
        public List<Node> nodes { get; set; } = new();
        public List<Node> roadNodes { get; set; } = new();
        public List<Node> buildableNodes { get; set; } = new();
        public List<Edge> edges { get; set; } = new();
        public List<Building> buildings { get; set; } = new();
        [JsonIgnore] public List<PictureBox> roadImages { get; set; }
        public float cash { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        [JsonIgnore] public int rectSize;
        [JsonIgnore] public Background background { get; set; }

        public Grid() { } // required

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
        public void CreateNodes()
        {
            int tempNum = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Point coords = new Point(x * rectSize, y * rectSize);
                    Node node = new Node(new Point(coords.X, coords.Y), false, false, tempNum++, false, false);
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

        public List<Node> FindAdjacentTilesToARoad(Road road)
        {
            List<Node> intersectingNodesWithEdge = new List<Node>();
            foreach (Node node in nodes)
            {
                node.isNearRoad = false;
                foreach (Point point in road.lane1.pointsOnTheEdge)
                {
                    //check if the node intersects with the road edge (within range)
                    if (node.coords.X + 8 < point.X + rectSize && node.coords.X + 8 > point.X - rectSize && node.coords.Y + 8 < point.Y + rectSize && node.coords.Y + 8 > point.Y - rectSize)
                    {
                        intersectingNodesWithEdge.Add(node);
                    }
                }

                foreach (Point point in road.lane2.pointsOnTheEdge)
                {
                    //check if the node intersects with the road edge (within range)
                    if (node.coords.X + 8 < point.X + rectSize && node.coords.X + 8 > point.X - rectSize && node.coords.Y + 8 < point.Y + rectSize && node.coords.Y + 8 > point.Y - rectSize)
                    {
                        intersectingNodesWithEdge.Add(node);
                    }
                }
            }
            return intersectingNodesWithEdge;
        }

        public List<Node> FindRoadTilesForSpecificEdge(Edge e, int laneIndex)
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
                    if (!roadNodes.Contains(roadNode)) { roadNodes.Add(roadNode); }
                }
            }

            return newRoadNodes;
        }

        private bool IsNodeAt(Node node, Point p)
        {
            return node.coords == new Point( (p.X / rectSize) * rectSize, (p.Y / rectSize) * rectSize );
        }


        public void FindRoadTilesAndAdjacentRoadTiles()
        {
            if (edges == null) return;

            //clear previous state
            roadNodes.Clear();
            buildableNodes.Clear();

            foreach (Node node in nodes)
            {
                node.isNearRoad = false;
                node.isRoad = false;
            }


            foreach (Road road in edges)
            {
                foreach (Point n in road.lane1.pointsOnTheEdge)
                {
                    foreach (Node node in nodes)
                    {
                        //is road node check
                        if (node.coords.X + 8 < n.X + rectSize && node.coords.X + 8 > n.X - rectSize && node.coords.Y + 8 < n.Y + rectSize && node.coords.Y + 8 > n.Y - rectSize)
                        {
                            node.isRoad = true;
                            if (!roadNodes.Contains(node)) { roadNodes.Add(node); }
                        }

                        //near-road check
                        else if (node.coords.X + 8 < n.X + (rectSize * 4) && node.coords.X + 8 > n.X - (rectSize * 4) && node.coords.Y + 8 < n.Y + (rectSize * 4) && node.coords.Y + 8 > n.Y - (rectSize * 4))
                        {
                            node.isNearRoad = true;
                            node.IsNodeBuildable();
                            if (node.isBuildable)
                            {
                                if (!buildableNodes.Any(b => b.coords == node.coords)) { buildableNodes.Add(node); }
                            }
                        }
                    }
                }

                foreach (Point n in road.lane2.pointsOnTheEdge)
                {
                    foreach (Node node in nodes)
                    {
                        //is road node check
                        if (node.coords.X + 8 < n.X + rectSize && node.coords.X + 8 > n.X - rectSize && node.coords.Y + 8 < n.Y + rectSize && node.coords.Y + 8 > n.Y - rectSize)
                        {
                            node.isRoad = true;
                            if (!roadNodes.Contains(node)) { roadNodes.Add(node); }
                        }

                        //near-road check
                        else if (node.coords.X + 8 < n.X + (rectSize * 4) && node.coords.X + 8 > n.X - (rectSize * 4) && node.coords.Y + 8 < n.Y + (rectSize * 4) && node.coords.Y + 8 > n.Y - (rectSize * 4))
                        {
                            node.isNearRoad = true;
                            node.IsNodeBuildable();
                            if (node.isBuildable)
                            {
                                if (!buildableNodes.Any(b => b.coords == node.coords)) { buildableNodes.Add(node); }
                            }
                        }
                    }
                }
            }

            //cleanup: remove any buildables that are now roads
            buildableNodes.RemoveAll(n => n.isRoad);
        }

        public void RebuildEntireRoadGraph()
        {
            foreach (Node n in roadNodes)
            {
                n.allowedDirs.Clear();
                n.neighbors.Clear();
            }

            foreach (Road road in edges)
            {
                Point lane1Dir = new Point(Math.Sign(road.lane1.b.X - road.lane1.a.X), Math.Sign(road.lane1.b.Y - road.lane1.a.Y));
                foreach (Node node in road.lane1.occupyingNodes)
                {
                    BuildNodeGraph(node, lane1Dir, road.lane1, road.lane2);
                }

                Point lane2Dir = new Point(Math.Sign(road.lane2.b.X - road.lane2.a.X), Math.Sign(road.lane2.b.Y - road.lane2.a.Y));
                foreach (Node node in road.lane2.occupyingNodes)
                {
                    BuildNodeGraph(node, lane2Dir, road.lane2, road.lane1);
                }
            }
        }

        private void BuildNodeGraph(Node node, Point roadDir, Edge lane, Edge otherLane)
        {
            if (!node.allowedDirs.Contains(roadDir)) { node.allowedDirs.Add(roadDir); }

            if (IsLaneEnd(lane, node))
            {
                AddOppositeLaneDirection(lane, node, otherLane);
            }

            //for every allowed direction, create a neighbor
            foreach (Point dir in node.allowedDirs)
            {
                int targetX = node.coords.X + dir.X * rectSize;
                int targetY = node.coords.Y + dir.Y * rectSize;

                Node neighbor = roadNodes.FirstOrDefault(other => other.coords.X == targetX && other.coords.Y == targetY);

                if (neighbor != null && !node.neighbors.Contains(neighbor))
                {
                    node.neighbors.Add(neighbor);
                }
            }

            //lane switch if same coordinate but different lane
            Node opposite = roadNodes.FirstOrDefault(other => other.coords == node.coords && other.laneIndex != node.laneIndex);
            if (opposite != null && !node.neighbors.Contains(opposite))
            {
                node.neighbors.Add(opposite);
            }
        }

        private bool IsLaneEnd(Edge lane, Node n)
        {
            return lane.occupyingNodes[0] == n || lane.occupyingNodes[lane.occupyingNodes.Count() - 1] == n;
        }

        private void AddOppositeLaneDirection(Edge lane, Node node, Edge otherLane)
        {
            if (lane.occupyingNodes[0] == node)
            {
                Node newNeighbour = otherLane.occupyingNodes[otherLane.occupyingNodes.Count() -1];
                node.neighbors.Add(newNeighbour);
                Point dirToOtherNode = new Point(Math.Sign(newNeighbour.coords.X - node.coords.X), Math.Sign(newNeighbour.coords.Y - node.coords.Y));
                node.allowedDirs.Add(dirToOtherNode);
            }

            if (lane.occupyingNodes[lane.occupyingNodes.Count() -1] == node)
            {
                Node newNeighbour = otherLane.occupyingNodes[0];
                node.neighbors.Add(newNeighbour);
                Point dirToOtherNode = new Point(Math.Sign(newNeighbour.coords.X - node.coords.X), Math.Sign(newNeighbour.coords.Y - node.coords.Y));
                node.allowedDirs.Add(dirToOtherNode);
            }
        }
    }
}