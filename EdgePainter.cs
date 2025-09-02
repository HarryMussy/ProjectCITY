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
        private float zoomLevel = 1.0f;
        public NameProvider nameProvider;
        public Background backgroundMap;
        public List<Point> waterNodes;
        public AudioManager audioManager;
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;
        private readonly Pen roadPenBlack = new Pen(Color.Black, 1);
        private readonly Brush whiteBrush = new SolidBrush(Color.White);
        private readonly Brush greenBrush = new SolidBrush(Color.Green);
        private readonly Brush redBrush = new SolidBrush(Color.Red);
        private readonly Brush blackBrush = new SolidBrush(Color.Black);
        private readonly Font roadFont = new Font("Comic Sans", 10);

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
        }
        public void RoadPaint(object? sender, Graphics g, Point mousePos)
        {
            foreach (Edge edge in grid.edges)
            {
                // If edge weight varies:
                roadPenBlack.Width = edge.edgeweight;
                g.DrawLine(roadPenBlack, edge.a, edge.b);

                if (!toggleRoadNames)
                {
                    form1.AddStrokeToText(sender, g, edge.name, 1, roadFont, blackBrush, new Point((edge.a.X + edge.b.X) / 2, (edge.a.Y + edge.b.Y) / 2));
                    g.DrawString(edge.name, roadFont, whiteBrush, new Point((edge.a.X + edge.b.X) / 2, (edge.a.Y + edge.b.Y) / 2));
                }

                foreach (IntersectingNode n in edge.intersections)
                {
                    g.FillEllipse(redBrush, n.coords.X - 5, n.coords.Y - 5, 10, 10);
                }
            }

            if (startPoint != null)
            {
                Pen invalidroad = new Pen(Color.Red, 4);
                Pen lightGrayPen = new Pen(Color.LightGray, 4);
                float cost = Grid.RoadCashCost(startPoint.Value, mousePos);

                if (cost > grid.cash)
                {
                    g.DrawLine(invalidroad, startPoint.Value, mousePos);
                }
                else
                {
                    g.DrawLine(lightGrayPen, startPoint.Value, mousePos);
                }
                Point linecenter = new Point((startPoint.Value.X + mousePos.X) / 2, (startPoint.Value.Y + mousePos.Y) / 2);
                string displayedcost = cost.ToString("F2");
                g.DrawString(displayedcost, new Font("Comic Sans", 10), greenBrush, linecenter);
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
                if (clickedPoint.X >= p.X - 20 && clickedPoint.X <= p.X + 20 &&
                    clickedPoint.Y >= p.Y - 20 && clickedPoint.Y <= p.Y + 20)
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
            var clickedPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X), (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));
            // Check if grid.intersections is null or empty
            if (grid.roadIntersections != null && grid.roadIntersections.Any())
            {
                List<Point> intersectionPoints = grid.roadIntersections.Select(n => n.coords).ToList();

                // Combine the points to check both road ends and intersections
                snappedPoint = this.GetClosestPoint(clickedPoint, intersectionPoints);
            }
            else
            {
                // Handle the case where grid.intersections is null or empty
                snappedPoint = clickedPoint;  // Or do something else if no intersections exist
            }


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
                if (startPoint == endPoint)
                {
                    isOverlapping = true;
                }
                if (IsOnWater(endPoint) == true) { }
                else
                {
                    float cost = Grid.RoadCashCost(startPoint.Value, endPoint);
                    if (isOverlapping == false && grid.cash >= cost)
                    {
                        string roadname = nameProvider.GetRandomName();
                        Road newroad = new Road(4, startPoint.Value, endPoint, roadname);
                        audioManager.PlayPlaceSound(@"audio\Effects\place.wav");
                        newroad.intersections.Add(new IntersectingNode(startPoint.Value));
                        newroad.intersections.Add(new IntersectingNode(endPoint));// Creates 2 new points on the dge for every intersection#
                        grid.cash = grid.cash - Grid.RoadCashCost(startPoint.Value, endPoint);
                        grid.edges.Add(newroad);  // This is now safe
                        grid.CheckIntersectingRoads();
                        closest_x = float.MaxValue; closest_y = float.MaxValue;
                        startPoint = null;
                        smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge> { newroad }, new List<House>());
                        foreach (IntersectingNode n in newroad.intersections)
                        {
                            grid.roadIntersections.Add(n);
                        }
                    }
                }
                
            }
        }
    }
}