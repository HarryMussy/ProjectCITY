using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public PointF currentPosition;
        public Image image;
        public Node startNode;
        public Point[] currentTrack = new Point[2];
        public float Progress; //0=start, 1=end
        public double Speed; //units per tick
        public Node destinationNode;
        public Queue<Node> route = new Queue<Node>();
        public bool isMoving = true;
        public float RotationAngle;

        public Car(Node startNodeIn, double speed, Node destinationNodeIn)
        {
            startNode = startNodeIn;
            destinationNode = destinationNodeIn;
            Progress = 0f;
            Speed = speed;
            currentPosition = new PointF(startNodeIn.coords.X + 8, startNodeIn.coords.Y + 8);
        }
    }

    public class EmergencyServiceVehicle : Car
    {
        public string type;
        public EmergencyServiceVehicle(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath) : base(startNodeIn, speed, destinationNodeIn)
        {
            startNode = startNodeIn;
            destinationNode = destinationNodeIn;
            Progress = 0f;
            Speed = speed;
            currentPosition = new PointF(startNodeIn.coords.X + 8, startNodeIn.coords.Y + 8);
            image = Image.FromFile(imageFilePath);
        }
    }


    public class CarManager
    {
        public Grid grid;
        public List<Image> carImages;
        public List<Car> cars = new List<Car>();
        private readonly Calendar calendar;

        public CarManager(Grid gridPassIn, Calendar calendarPassIn)
        {
            grid = gridPassIn;
            LoadImages();
            calendar = calendarPassIn;
        }

        public void AssignImage(Car car)
        {
            car.image = carImages[new Random().Next(carImages.Count)];
        }

        public void CarPaint(object? sender, Graphics g)
        {
            foreach (Car car in cars)
            {
                var state = g.Save();
                g.TranslateTransform(car.currentPosition.X, car.currentPosition.Y);
                g.RotateTransform(car.RotationAngle + 90);

                if (calendar.GetHour() >= 21 || calendar.GetHour() <= 5)
                {
                    using Brush glow1 = new SolidBrush(Color.FromArgb(180, 255, 255, 200));  // bright center
                    using Brush glow2 = new SolidBrush(Color.FromArgb(90, 255, 255, 200));   // mid glow
                    using Brush glow3 = new SolidBrush(Color.FromArgb(40, 255, 255, 200));   // outer glow

                    const float sourceY = -4; 
                  
                    g.FillEllipse(glow3, -6, sourceY - 13, 12, 20);  // outer
                    g.FillEllipse(glow2, -4, sourceY - 8, 8, 15);  // mid
                    g.FillEllipse(glow1, -2, sourceY - 2, 4, 9);   // bright center
                }
                g.DrawImage(car.image, -4, -4, 12, 12);

                g.Restore(state);
            }
        }


        private void LoadImages()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));

            string carsFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Cars");
            carImages = new List<Image>();

            foreach (string path in Directory.GetFiles(carsFolder, "*.png"))
            {
                using var original = Image.FromFile(path);
                Bitmap transparentBitmap = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(transparentBitmap))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.DrawImage(original, 0, 0, original.Width, original.Height);
                }
                carImages.Add(transparentBitmap);
            }
        }

        public bool MoveCar(Car car)
        {
            try
            {
                if (car == null) return false;

                if (car.route == null || car.route.Count == 0)
                {
                    Console.WriteLine("Car has no route.");
                    // nothing to do — keep car stopped
                    car.isMoving = false;
                    return false;
                }

                //get the next node safely
                Node nextNode = car.route.Peek();
                if (nextNode == null)
                {
                    Console.WriteLine("Next node from route is null.");
                    car.isMoving = false;
                    return false;
                }

                PointF target = new PointF(nextNode.coords.X + 8, nextNode.coords.Y + 8);
                float dx = target.X - car.currentPosition.X;
                float dy = target.Y - car.currentPosition.Y;
                float angleRad = (float)Math.Atan2(dy, dx);
                car.RotationAngle = angleRad * 180f / (float)Math.PI;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (dist <= car.Speed) // reached or close enough
                {
                    car.currentPosition = target;
                    car.route.Dequeue();
                    car.Progress = 0f;
                }
                else
                {
                    car.currentPosition = new PointF(
                        car.currentPosition.X + (float)(dx / dist * car.Speed),
                        car.currentPosition.Y + (float)(dy / dist * car.Speed)
                    );
                }

                //after movement, check the node the car is occupying
                Node occupying = CarOccupyingNode(car);
                if (occupying == null)
                {
                    //no road nodes available — stop the car to avoid repeated exceptions
                    Console.WriteLine("CarOccupyingNode returned null.");
                    car.isMoving = false;
                    return false;
                }

                //if we still have nodes in route, compare occupying to next route node
                if (car.route.Count > 0)
                {
                    Node peekNode = car.route.Peek();
                    if (peekNode != null && occupying.coords == peekNode.coords)
                    {
                        //we are at the next queued node — consume it
                        car.route.Dequeue();
                        car.Progress = 0f;
                    }
                }
                else
                {
                    //route became empty after the earlier Dequeue. If we're at destination stop the car.
                    if (occupying.coords == car.destinationNode.coords)
                    {
                        car.isMoving = false;
                        return true; // signal finished
                    }
                }

                //if now at destination (double check) and route empty
                if (car.route.Count == 0 && occupying.coords == car.destinationNode.coords)
                {
                    car.isMoving = false;
                    return true;
                }

                return false; //not finished
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in MoveCar: " + ex.ToString());
                //stop the car to avoid crash loops
                car.isMoving = false;
                return false;
            }
        }

        private Node CarOccupyingNode(Car car)
        {
            Node n = grid.roadNodes.OrderBy(node => Distance(car.currentPosition, new PointF(node.coords.X + 8, node.coords.Y + 8))).First();
            return n;
        }

        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public Queue<Node> CreateCarRoute(Car car)
        {
            foreach (Node n in grid.roadNodes) //reset data to stop potential crashes/ freezes
            {
                n.parent = null;
                n.gCost = float.MaxValue;
                n.hCost = 0;
            }

            List<Node> openList = new List<Node>();
            HashSet<Node> closedList = new HashSet<Node>();

            car.startNode.gCost = 0;
            car.startNode.hCost = Heuristic(car.startNode, car.destinationNode);

            openList.Add(car.startNode);

            while (openList.Count > 0)
            {
                Node current = openList.OrderBy(n => n.fCost).First();
                openList.Remove(current);
                closedList.Add(current);

                if (current == car.destinationNode)
                {
                    //reconstruct path
                    Stack<Node> stack = new Stack<Node>();
                    Node pathNode = current;
                    while (pathNode != null)
                    {
                        stack.Push(pathNode);
                        pathNode = pathNode.parent;
                    }
                    return new Queue<Node>(stack);
                }

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (closedList.Contains(neighbor))
                        continue;

                    float tentativeG = current.gCost + Distance(current.coords, neighbor.coords);
                    bool isBetter = false;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                        isBetter = true;
                    }
                    else if (tentativeG < neighbor.gCost)
                    {
                        isBetter = true;
                    }

                    if (isBetter)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = tentativeG;
                        neighbor.hCost = Heuristic(neighbor, car.destinationNode);
                    }
                }
            }

            return null; //no path found
        }

        private float Heuristic(Node a, Node b)
        {
            return Math.Abs(a.coords.X - b.coords.X) + Math.Abs(a.coords.Y - b.coords.Y); // Manhattan
        }

        private List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();
            neighbors.AddRange(grid.roadNodes.Where(n => (Math.Abs(n.coords.X - node.coords.X) == 16 && n.coords.Y == node.coords.Y) ||
                                                        (Math.Abs(n.coords.Y - node.coords.Y) == 16 && n.coords.X == node.coords.X)));
            return neighbors;
        }

    }
}