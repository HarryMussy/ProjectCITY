using System.IO;
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
        private int rectSize;

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
            smokeParticleManager = form1.smokeParticleManager;
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
                    g.DrawImage(img, node.coords.X, node.coords.Y, 17, 17);
                }
            }

            foreach (Road r in grid.edges)
            {
                using Pen p = new Pen(Color.FromArgb(55, 255, 255, 255), 5) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
                DrawArrowLine(g, p, r.lane1.a, r.lane1.b);
                DrawArrowLine(g, p, r.lane2.a, r.lane2.b);
            }

            if (startPoint != null)
            {
                float cost = grid.RoadCashCost(startPoint.Value, mousePos);
                Point setPoint = SnapTo4Directions(startPoint.Value, mousePos);

                Point pointB = new Point(setPoint.X - (setPoint.X % 8), setPoint.Y - (setPoint.Y % 8));
                // highlight intersecting nodes as before
                int edgeAngle = FindAngle(startPoint.Value, pointB);
                Road tempRoad = new Road(8, startPoint.Value, pointB, "temp", edgeAngle);

                List<Node> nodes = grid.FindAdjacentTilesToARoad(tempRoad);

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

                using Pen p = new Pen(Color.Black, 5) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

                DrawArrowLine(g, p, tempRoad.lane1.a, tempRoad.lane1.b);
                DrawArrowLine(g, p, tempRoad.lane2.a, tempRoad.lane2.b);

                g.DrawLine(new Pen(new SolidBrush(Color.Yellow), 3), tempRoad.lane1.a, tempRoad.lane1.b);
                g.DrawLine(new Pen(new SolidBrush(Color.Yellow), 3), tempRoad.lane2.a, tempRoad.lane2.b);
                tempRoad = null;
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

        private void DrawArrowLine(Graphics g, Pen p, Point a, Point b)
        {
            g.DrawLine(p, a.X, a.Y, b.X, b.Y);
        }

        public int FindAngle(Point a, Point b)
        {
            if (b.X == a.X)
            {
                return 0;
            }
            else
            {
                return (int)Math.Tanh((b.Y - a.Y) / (b.X - a.X));
            }
                
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
            clickedPoint = new Point((clickedPoint.X / tile) * tile, (clickedPoint.Y / tile) * tile);
            clickedPoint.X -= 8;
            clickedPoint.Y -= 8;
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
                endPoint = SnapTo4Directions(startPoint.Value, endPoint);
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
                        Road newroad = new Road(8, startPoint.Value, endPoint, roadname, FindAngle(startPoint.Value, endPoint));
                        audioManager.PlayPlaceSound();
                        grid.cash -= grid.RoadCashCost(startPoint.Value, endPoint);
                        grid.edges.Add(newroad); //this is now safe

                        grid.FindRoadTilesAndAdjacentRoadTiles();

                        newroad.lane1.occupyingNodes = grid.FindRoadTilesForSpecificEdge(newroad.lane1, 0);
                        newroad.lane2.occupyingNodes = grid.FindRoadTilesForSpecificEdge(newroad.lane2, 1);

                        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                        string roadFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Roads");

                        foreach (Node n in newroad.lane1.occupyingNodes)
                        {
                            int num = rng.Next(100);
                            n.imageKey = "road_000.png";
                            if (num > 95) n.imageKey = "road_001.png";
                            if (num > 96) n.imageKey = "road_002.png";
                            if (num > 97) n.imageKey = "road_003.png";
                            if (num > 98) n.imageKey = "road_004.png";
                            if (num > 99) n.imageKey = "road_005.png";
                        }
                        foreach (Node n in newroad.lane2.occupyingNodes)
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
