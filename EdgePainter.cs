using System.IO;

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
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;
        private readonly Brush whiteBrush = new SolidBrush(Color.White);
        private readonly Brush greenBrush = new SolidBrush(Color.Green);
        private readonly Brush blackBrush = new SolidBrush(Color.Black);
        private readonly Brush redBrush = new SolidBrush(Color.FromArgb(100, Color.Red));
        private readonly Font roadFont = new Font("Comic Sans", 10);
        private Dictionary<string, Image> roadImages = new();
        private Random rng = new Random();
        private List<string> roadImagePaths = new();

        public EdgePainter(Grid gridPassIn, Form1 Form1PassIn, NameProvider nameProviderPassIn, Background backgroundMap, Graphics g)
        {
            grid = gridPassIn;
            form1 = Form1PassIn;
            this.g = g;
            screencentre = new Point(form1.Width / 2, form1.Height / 2);
            nameProvider = nameProviderPassIn;
            this.backgroundMap = backgroundMap;
            waterNodes = FindWaterNodePoints(this.backgroundMap);
            audioManager = form1.audioManager;
            smokeParticleManager = form1.smokeParticleManager;
            LoadRoadTiles();
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

        public void RoadPaint(object? sender, Graphics g, Point mousePos)
        {
            foreach (Node node in grid.buildableNodes)
            {
                if (viewBuildingSpaces == true)
                {
                    g.FillRectangle(redBrush, node.coords.X, node.coords.Y, 16, 16);
                }
            }

            Image img = null;
            foreach (Node node in grid.roadNodes)
            {
                if (!string.IsNullOrEmpty(node.imageKey))
                {
                    // try exact key, then filename fallback
                    if (!roadImages.TryGetValue(node.imageKey, out img))
                    {
                        string filename = Path.GetFileName(node.imageKey);
                        roadImages.TryGetValue(filename, out img);
                    }
                }

                if (img != null)
                {
                    g.DrawImage(img, node.coords.X, node.coords.Y, 16, 16);
                }
            }

            if (startPoint != null)
            {
                float cost = grid.RoadCashCost(startPoint.Value, mousePos);
                Point setPoint = SnapTo8Directions(startPoint.Value, mousePos);

                // highlight intersecting nodes as before
                Edge tempEdge = new Edge(8, startPoint.Value, setPoint, "temp");
                List<Node> nodes = grid.FindAdjacentTilesToARoad(tempEdge);
                if (cost > grid.cash)
                {
                    using var invalidroad = new SolidBrush(Color.Red);
                    foreach (Node n in nodes) g.FillRectangle(invalidroad, n.coords.X, n.coords.Y, 16, 16);
                }
                else
                {
                    using var lightGrayBrush = new SolidBrush(Color.LightGray);
                    foreach (Node n in nodes) g.FillRectangle(lightGrayBrush, n.coords.X, n.coords.Y, 16, 16);
                }

                // cost label
                Point linecenter = new Point((startPoint.Value.X + mousePos.X) / 2, (startPoint.Value.Y + mousePos.Y) / 2);
                string displayedcost = cost.ToString("F2");
                g.DrawString(displayedcost, new Font("Comic Sans", 10), greenBrush, linecenter);

                tempEdge = null;
            }

            foreach (Edge edge in grid.edges)
            {
                if (!toggleRoadNames)
                {
                    form1.AddStrokeToText(sender, g, edge.name, 1, roadFont, blackBrush, new Point((edge.a.X + edge.b.X) / 2, (edge.a.Y + edge.b.Y) / 2));
                    g.DrawString(edge.name, roadFont, whiteBrush, new Point((edge.a.X + edge.b.X) / 2, (edge.a.Y + edge.b.Y) / 2));
                }
            }
        }

        public Point SnapPoint(Point a, Point b)
        {
            // Check if it's within 10-pixel radius
            if (a.X <= b.X + 10 && a.X >= b.X - 10 && a.Y <= b.Y + 10 && a.Y >= b.Y - 10)
            {
                // Snap a to b
                a = b;

                // Check if the difference in the 2 points is the closest value
                if (Math.Abs(b.X - a.X) < closest_x)
                {
                    closest_x = Math.Abs(b.X - a.X);
                }

                if (Math.Abs(b.Y - a.Y) < closest_y)
                {
                    closest_y = Math.Abs(b.Y - a.Y);
                }
            }

            // Return the (possibly snapped) point a
            return a;
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

            Point closestPoint = b.First(); // Start with the first point in the list
            double initialDistance = CalculateDistance(a, closestPoint);

            if (initialDistance <= snapRadius)
            {
                closestDistance = initialDistance;
            }

            // Check extra points (intersections or any other points)
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

            // Check the road end points
            foreach (Point point in b)
            {
                double distance = CalculateDistance(a, point);

                if (distance <= snapRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }

            // Return the closest point if within the snap radius
            if (closestDistance <= snapRadius)
            {
                return closestPoint;
            }

            return null;
        }


        // Helper function to calculate the Euclidean distance between two points
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
            clickedPoint = new Point(
                (clickedPoint.X / tile) * tile,
                (clickedPoint.Y / tile) * tile
            );

            snappedPoint = clickedPoint;


            //if graphics is null, not already drawing a road
            if (startPoint == null)
            {
                startPoint = snappedPoint == null ? clickedPoint : snappedPoint; // If closest point is not snapable, dont snap together
                if (IsOnWater(startPoint.Value) == true)
                {
                    startPoint = null;
                }
            }
            else
            {
                bool isOverlapping = false;
                Point endPoint = snappedPoint == null ? clickedPoint : snappedPoint.Value;
                endPoint = SnapTo8Directions(startPoint.Value, endPoint);
                if (startPoint == endPoint)
                {
                    isOverlapping = true;
                }
                if (IsOnWater(endPoint) == true) { }
                else
                {
                    float cost = grid.RoadCashCost(startPoint.Value, endPoint);
                    if (isOverlapping == false && grid.cash >= cost)
                    {
                        string roadname = nameProvider.GetRandomName();
                        Road newroad = new Road(8, startPoint.Value, endPoint, roadname);
                        audioManager.PlayPlaceSound();
                        newroad.AddIntersection(startPoint.Value, newroad);
                        newroad.AddIntersection(endPoint, newroad);
                        grid.cash = grid.cash - grid.RoadCashCost(startPoint.Value, endPoint);
                        grid.edges.Add(newroad);  // This is now safe
                        //grid.CheckIntersectingRoads();
                        grid.FindRoadTilesAndAdjacentRoadTiles();
                        newroad.occupyingNodes = grid.FindRoadTilesForSpecificEdge(newroad);

                        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                        string roadFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Roads");
                        foreach (Node n in newroad.occupyingNodes)
                        {
                            int num = rng.Next(100);
                            n.imageKey = "road_000.png";
                            if (num > 95) n.imageKey = "road_001.png";
                            if (num > 96) n.imageKey = "road_002.png";
                            if (num > 97) n.imageKey = "road_003.png";
                            if (num > 98) n.imageKey = "road_004.png";
                            if (num > 99) n.imageKey = "road_005.png";
                        }

                        closest_x = float.MaxValue; closest_y = float.MaxValue;
                        startPoint = null;
                        smokeParticleManager.SpawnParticlesOnEdge(newroad);
                        foreach (IntersectingNode n in newroad.intersections)
                        {
                            grid.roadIntersections.Add(n);
                        }
                    }
                }

            }
        }

        public Point SnapTo8Directions(Point a, Point b)
        {
            int[] allowedAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };

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

            //pick nearest allowed angle
            int nearest = allowedAngles.OrderBy(ang => AngleDiff(ang, angle)).First();

            //keep distance and find snapped point
            double length = Math.Sqrt(changeX * changeX + changeY * changeY);
            double rad = nearest * Math.PI / 180.0;
            int newX = (int)Math.Round(a.X + length * Math.Cos(rad)); //no clue why it only works in radians
            int newY = (int)Math.Round(a.Y + length * Math.Sin(rad));

            return new Point(newX, newY);
        }
    }
}
