using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;

namespace CitySkylines0._5alphabeta
{
    public class EdgePainter
    {
        private readonly Grid grid;
        public Point? startPoint = null;
        public Form1 form1;
        public float closest_x = float.MaxValue;
        public float closest_y = float.MaxValue;
        public Point screencentre;
        public bool toggleRoadNames = false;
        public bool viewBuildingSpaces = true;
        private float zoomLevel = 1.0f;
        public NameProvider nameProvider;
        public Background backgroundMap;
        public List<Point> waterNodes;
        public AudioManager audioManager;
        public Graphics g;
        private readonly Brush whiteBrush = new SolidBrush(Color.White);
        private readonly Brush greenBrush = new SolidBrush(Color.Green);
        private readonly Brush blackBrush = new SolidBrush(Color.Black);
        private readonly Brush redBrush = new SolidBrush(Color.FromArgb(100, Color.Red));
        private readonly Font roadFont = new Font("Comic Sans", 10);
        private Dictionary<string, Image> roadImages = new();
        private Random rng = new Random();
        private List<string> roadImagePaths = new();
        private int rectSize;
        private bool doesNewRoadContainTileWithTileData;

        public EdgePainter(Grid gridPassIn, Form1 Form1PassIn, NameProvider nameProviderPassIn, Background backgroundMap, Graphics g, int rectSize)
        {
            grid = gridPassIn;
            form1 = Form1PassIn;
            this.g = g;
            screencentre = new Point(form1.Width / 2, form1.Height / 2);
            nameProvider = nameProviderPassIn;
            this.backgroundMap = backgroundMap;
            waterNodes = FindWaterNodePoints(this.backgroundMap);
            audioManager = form1.audioManager;
            LoadRoadTiles();
            this.rectSize = rectSize;
        }

        private void LoadRoadTiles()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string roadFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Roads");
            foreach (string path in Directory.GetFiles(roadFolder, "*.png"))
            {
                Image img = Image.FromFile(path);
                string filename = Path.GetFileName(path);
                roadImages[path] = img;
                if (!roadImages.ContainsKey(filename))
                {
                    roadImages[filename] = img;
                }
                roadImagePaths.Add(path);
            }
        }

        /// <summary>
        /// Validates that a point is within the grid boundaries.
        /// </summary>
        public bool IsPointOnGrid(Point p)
        {
            int maxX = grid.width * rectSize;
            int maxY = grid.height * rectSize;
            
            return p.X >= 0 && p.X <= maxX && p.Y >= 0 && p.Y <= maxY;
        }

        public void RoadPaint(object? sender, Graphics g, Point mousePos)
        {
            foreach (Node node in grid.nodes.Where(node => node.isBuildable))
            {
                if (viewBuildingSpaces == true)
                {
                    g.FillRectangle(redBrush, node.coords.X, node.coords.Y, 16, 16);
                }
            }

            Image img = null;
            foreach (Node node in grid.nodes.Where(node => node.isRoad))
            {
                if (!string.IsNullOrEmpty(node.imagePath))
                {
                    if (!roadImages.TryGetValue(node.imagePath, out img))
                    {
                        string filename = Path.GetFileName(node.imagePath);
                        roadImages.TryGetValue(filename, out img);
                    }
                }

                if (img != null)
                {
                    g.DrawImage(img, node.coords.X, node.coords.Y, 17, 17);
                }
            }

            foreach (Road r in grid.roads)
            {
                using Pen p = new Pen(Color.FromArgb(55, 255, 255, 255), 5) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
                DrawArrowLine(g, p, r.lane1.a, r.lane1.b);
                DrawArrowLine(g, p, r.lane2.a, r.lane2.b);
            }

            if (startPoint != null)
            {
                float cost = grid.RoadCashCost(startPoint.Value, mousePos);
                Point setPoint = SnapTo4Directions(startPoint.Value, mousePos);

                foreach (Node n in grid.nodes)
                {
                    if (grid.IsNodeAt(n, setPoint))
                    {
                        setPoint = n.Center(rectSize);
                    }
                }
                Point pointB = setPoint;

                // Validate endpoint is on grid
                bool endPointOnGrid = IsPointOnGrid(pointB);

                // highlight intersecting nodes as before
                int edgeAngle = FindAngle(startPoint.Value, pointB);
                Road tempRoad = new Road(startPoint.Value, pointB, "temp", edgeAngle);

                List<Node> nodes = grid.FindAdjacentTilesToARoad(tempRoad);

                if (nodes.Contains(nodes.FirstOrDefault(n => n.hasTileData)))
                {
                    doesNewRoadContainTileWithTileData = true;
                }
                else { doesNewRoadContainTileWithTileData = false; }

                if (cost > grid.cash || doesNewRoadContainTileWithTileData || !endPointOnGrid)
                {
                    using var invalidroad = new SolidBrush(Color.Red);
                    foreach (Node n in nodes) { g.FillRectangle(invalidroad, n.coords.X, n.coords.Y, 16, 16); }
                }
                else
                {
                    using var lightGrayBrush = new SolidBrush(Color.LightGray);
                    foreach (Node n in nodes) { g.FillRectangle(lightGrayBrush, n.coords.X, n.coords.Y, 16, 16); }
                }

                // cost label
                Point linecenter = new Point((startPoint.Value.X + mousePos.X) / 2, (startPoint.Value.Y + mousePos.Y) / 2);
                string displayedcost = cost.ToString("F2");
                g.DrawString(displayedcost, new Font("Comic Sans", 10), greenBrush, linecenter);

                using Pen p = new Pen(Color.Black, 5) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

                DrawArrowLine(g, p, tempRoad.lane1.a, tempRoad.lane1.b);
                DrawArrowLine(g, p, tempRoad.lane2.a, tempRoad.lane2.b);
                tempRoad = null;
            }

            foreach (Edge edge in grid.roads)
            {
                if (!toggleRoadNames)
                {
                    form1.AddStrokeToText(sender, g, edge.name, 1, roadFont, blackBrush, new Point((edge.a.X + edge.b.X) / 2, (edge.a.Y + edge.b.Y) / 2));
                    g.DrawString(edge.name, roadFont, whiteBrush, new Point((edge.a.X + edge.b.X) / 2, (edge.a.Y + edge.b.Y) / 2));
                }
            }
        }

        private void DrawArrowLine(Graphics g, Pen p, Point a, Point b)
        {
            g.DrawLine(p, a.X, a.Y, b.X, b.Y);
        }

        public int FindAngle(Point a, Point b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float angle = MathF.Atan2(dy, dx) * 180f / MathF.PI;
            return (int)angle;
        }

        public List<Point> FindWaterNodePoints(Background bg)
        {
            List<Point> waterNodePoints = new List<Point>();
            foreach (Node node in bg.tiles)
            {
                if (!node.isGrass)
                {
                    waterNodePoints.Add(node.coords);
                }
            }
            return waterNodePoints;
        }

        public bool IsOnWater(Point clickedPoint)
        {
            foreach (Point p in waterNodes)
            {
                if (clickedPoint.X >= p.X - form1.rectSize && clickedPoint.X <= p.X + form1.rectSize &&
                    clickedPoint.Y >= p.Y - form1.rectSize && clickedPoint.Y <= p.Y + form1.rectSize)
                {
                    return true;
                }
            }
            return false;
        }

        public Point? GetClosestPoint(Point a, List<Point> b)
        {
            List<Point> extraPoints = null;
            double closestDistance = int.MaxValue;
            int snapRadius = 60;
            if (b.Count == 0 && (extraPoints == null || extraPoints.Count == 0))
            {
                return null;
            }

            Point closestPoint = b.First();
            double initialDistance = CalculateDistance(a, closestPoint);

            if (initialDistance <= snapRadius)
            {
                closestDistance = initialDistance;
            }

            if (extraPoints != null)
            {
                foreach (Point point in extraPoints)
                {
                    double distance = CalculateDistance(a, point);

                    if (distance <= snapRadius && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = point;
                    }
                }
            }

            foreach (Point point in b)
            {
                double distance = CalculateDistance(a, point);

                if (distance <= snapRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }

            if (closestDistance <= snapRadius)
            {
                return closestPoint;
            }

            return null;
        }

        private double CalculateDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public void LeftMouseDown(object? sender, MouseEventArgs m)
        {
            Point? snappedPoint;
            Point worldMousePos = ((Form1)sender).Mouse_Pos(sender, m);
            var clickedPoint = worldMousePos;

            int tile = form1.rectSize;
            foreach (Node n in grid.nodes)
            {
                if (grid.IsNodeAt(n, clickedPoint))
                {
                    clickedPoint = n.Center(rectSize);
                    break;
                }
            }

            if (startPoint == null)
            {
                startPoint = clickedPoint;
                foreach (Node n in grid.nodes)
                {
                    if (IsOnWater(startPoint.Value) == true && grid.IsNodeAt(n, startPoint.Value))
                    {
                        startPoint = null;
                    }
                }
            }

            else
            {
                bool isOverlapping = false;
                Point endPoint = clickedPoint;
                endPoint = SnapTo4Directions(startPoint.Value, endPoint);

                // snap final result to node centre
                foreach (Node n in grid.nodes)
                {
                    if (grid.IsNodeAt(n, endPoint))
                    {
                        endPoint = n.Center(rectSize);
                        break;
                    }
                }

                if (startPoint == endPoint)
                {
                    isOverlapping = true;
                }

                // Validate both start and end points are on the grid
                bool startPointOnGrid = IsPointOnGrid(startPoint.Value);
                bool endPointOnGrid = IsPointOnGrid(endPoint);

                if (IsOnWater(endPoint) || doesNewRoadContainTileWithTileData || !startPointOnGrid || !endPointOnGrid) { }
                else
                {
                    float cost = grid.RoadCashCost(startPoint.Value, endPoint);
                    if (isOverlapping == false && grid.cash >= cost)
                    {
                        string roadname = nameProvider.GetRandomName();
                        Road newroad = new Road(startPoint.Value, endPoint, roadname, FindAngle(startPoint.Value, endPoint));
                        audioManager.PlayPlaceSound();
                        grid.cash -= grid.RoadCashCost(startPoint.Value, endPoint);
                        grid.roads.Add(newroad);

                        grid.FindRoadTilesAndAdjacentRoadTiles();

                        newroad.lane1.occupyingNodesIndex = grid.FindRoadTilesForSpecificEdge(newroad.lane1, 0);
                        newroad.lane2.occupyingNodesIndex = grid.FindRoadTilesForSpecificEdge(newroad.lane2, 1);

                        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                        string roadFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Roads");

                        foreach (int index in newroad.lane1.occupyingNodesIndex)
                        {
                            Node n = grid.nodes.Where(node => node.nodeNumber == index).FirstOrDefault();
                            int num = rng.Next(100);
                            n.imagePath = "road_000.png";
                            if (num > 95) n.imagePath = "road_001.png";
                            if (num > 96) n.imagePath = "road_002.png";
                            if (num > 97) n.imagePath = "road_003.png";
                            if (num > 98) n.imagePath = "road_004.png";
                            if (num > 99) n.imagePath = "road_005.png";
                        }
                        foreach (int index in newroad.lane2.occupyingNodesIndex)
                        {
                            Node n = grid.nodes.Where(node => node.nodeNumber == index).FirstOrDefault();
                            int num = rng.Next(100);
                            n.imagePath = "road_000.png";
                            if (num > 95) n.imagePath = "road_001.png";
                            if (num > 96) n.imagePath = "road_002.png";
                            if (num > 97) n.imagePath = "road_003.png";
                            if (num > 98) n.imagePath = "road_004.png";
                            if (num > 99) n.imagePath = "road_005.png";
                        }

                        closest_x = float.MaxValue; closest_y = float.MaxValue;
                        startPoint = null;
                        grid.RebuildEntireRoadGraph();

                        foreach (Car car in form1.carManager.cars)
                        {
                            car.route = form1.carManager.CreateCarRoute(car);
                        }
                    }
                }
            }
        }

        public Point SnapTo4Directions(Point a, Point b)
        {
            int[] allowedAngles = { 0, 90, 180, 270 };

            double changeX = b.X - a.X;
            double changeY = b.Y - a.Y;
            if (Math.Abs(changeX) < double.Epsilon && Math.Abs(changeY) < double.Epsilon) { return a; }

            double angle = Math.Atan2(changeY, changeX) * (180.0 / Math.PI);
            if (angle < 0) angle += 360.0;

            static double AngleDiff(double a, double b)
            {
                double difference = Math.Abs(a - b) % 360.0;
                if (difference > 180.0) { difference = 360.0 - difference; }
                return difference;
            }

            int nearest = allowedAngles.OrderBy(ang => AngleDiff(ang, angle)).First();

            double length = Math.Sqrt(changeX * changeX + changeY * changeY);
            double rad = nearest * Math.PI / 180.0;
            int newX = (int)Math.Round(a.X + length * Math.Cos(rad));
            int newY = (int)Math.Round(a.Y + length * Math.Sin(rad));

            return new Point(newX, newY);
        }
    }
}
