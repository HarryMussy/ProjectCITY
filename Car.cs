using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public PointF currentPosition;
        public Image image;

        public Node startNode;
        public Node currentNode;
        public Node destinationNode;

        public Queue<Node> route = new Queue<Node>();

        public double Speed;
        public float RotationAngle;

        public bool isMoving = true;
        public bool hasPriority = false;

        public float stuckTimeSeconds = 0f;
        public HashSet<Node> blockedNodes = new HashSet<Node>();

        public Car(Node startNodeIn, double speed, Node destinationNodeIn)
        {
            startNode = startNodeIn;
            currentNode = startNodeIn;
            destinationNode = destinationNodeIn;
            Speed = speed;

            currentPosition = new PointF(
                startNodeIn.coords.X + 8,
                startNodeIn.coords.Y + 8
            );

            startNode.OccupyingCar = this;
        }
    }

    public class EmergencyServiceVehicle : Car
    {
        public string type;

        public EmergencyServiceVehicle(
            Node startNodeIn,
            double speed,
            Node destinationNodeIn,
            string imageFilePath
        ) : base(startNodeIn, speed, destinationNodeIn)
        {
            image = Image.FromFile(imageFilePath);
        }
    }


    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>();
        public List<Image> carImages = new List<Image>();

        private readonly Calendar calendar;
        private readonly Random rng = new Random();

        private const float TickDelta = 1f / 60f;

        public CarManager(Grid gridPassIn, Calendar calendarPassIn)
        {
            grid = gridPassIn;
            calendar = calendarPassIn;
            LoadImages();
        }

        /* ----------CAR VISUALS---------- */

        public void AssignImage(Car car)
        {
            car.image = carImages[rng.Next(carImages.Count)];
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
                    using Brush glow1 = new SolidBrush(Color.FromArgb(180, 255, 255, 200));
                    using Brush glow2 = new SolidBrush(Color.FromArgb(90, 255, 255, 200));
                    using Brush glow3 = new SolidBrush(Color.FromArgb(40, 255, 255, 200));

                    g.FillEllipse(glow3, -6, -17, 12, 20);
                    g.FillEllipse(glow2, -4, -12, 8, 15);
                    g.FillEllipse(glow1, -2, -6, 4, 9);
                }

                g.DrawImage(car.image, -4, -4, 12, 12);
                g.Restore(state);
            }
        }

        private void LoadImages()
        {
            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string folder = Path.Combine(root, "gameAssets", "gameArt", "Cars");

            foreach (string path in Directory.GetFiles(folder, "*.png"))
            {
                using var original = Image.FromFile(path);
                Bitmap bmp = new Bitmap(original.Width, original.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(original, 0, 0);
                }
                carImages.Add(bmp);
            }
        }

        /* ----------MOVEMENT---------- */

        public bool MoveCar(Car car)
        {
            if (!car.isMoving || car.route.Count == 0)
                return false;

            Node nextNode = car.route.Peek();

            if (IsBlocked(car, nextNode))
                return false;

            MoveTowardsNode(car, nextNode);

            if (car.route.Count == 0 && car.currentNode == car.destinationNode)
            {
                DespawnCar(car);
                return true;
            }

            return false;
        }

        private bool IsBlocked(Car car, Node nextNode)
        {
            if (nextNode.OccupyingCar == null || nextNode.OccupyingCar == car)
                return false;

            car.stuckTimeSeconds += TickDelta;
            car.blockedNodes.Add(nextNode);

            if (IsHeadOnDeadlock(car, nextNode) && car.stuckTimeSeconds >= 1.5f)
            {
                car.hasPriority = true;
                return false;
            }

            if (car.stuckTimeSeconds >= 1f)
            {
                TryRerouteCar(car);
                car.stuckTimeSeconds = 0f;
            }

            if (car.stuckTimeSeconds >= 10f)
            {
                DespawnCar(car);
                return true;
            }

            return true;
        }

        private void MoveTowardsNode(Car car, Node nextNode)
        {
            car.stuckTimeSeconds = 0f;
            car.blockedNodes.Clear();

            PointF target = new PointF(
                nextNode.coords.X + 8,
                nextNode.coords.Y + 8
            );

            float dx = target.X - car.currentPosition.X;
            float dy = target.Y - car.currentPosition.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            car.RotationAngle = MathF.Atan2(dy, dx) * 180f / MathF.PI;

            if (dist <= car.Speed)
            {
                if (nextNode.OccupyingCar != null &&
                    nextNode.OccupyingCar != car &&
                    !car.hasPriority)
                    return;

                // free old node
                if (car.currentNode.OccupyingCar == car)
                    car.currentNode.OccupyingCar = null;

                car.currentNode = nextNode;
                nextNode.OccupyingCar = car;

                car.currentPosition = target;
                car.route.Dequeue();
                car.hasPriority = false;
            }
            else
            {
                car.currentPosition = new PointF(
                    car.currentPosition.X + dx / dist * (float)car.Speed,
                    car.currentPosition.Y + dy / dist * (float)car.Speed
                );
            }
        }

        private void DespawnCar(Car car)
        {
            if (car.currentNode != null && car.currentNode.OccupyingCar == car)
            {
                car.currentNode.OccupyingCar = null;
                for (int i = cars.Count - 1; i >= 0; i--) { if (MoveCar(cars[i])) { cars.RemoveAt(i); } }
            }
            

            car.isMoving = false;
        }

        /* ----------DEADLOCK / ROUTING---------- */

        private bool IsHeadOnDeadlock(Car car, Node nextNode)
        {
            Car other = nextNode.OccupyingCar;
            if (other == null || other.route.Count == 0)
                return false;

            return other.route.Peek() == car.currentNode;
        }

        private void TryRerouteCar(Car car)
        {
            if (car.blockedNodes.Count == 0)
                return;

            Queue<Node> newRoute = CreateCarRouteAvoidingNodes(
                car.currentNode,
                car.destinationNode,
                car.blockedNodes
            );

            if (newRoute != null && newRoute.Count > 0)
            {
                car.route = newRoute;
                car.blockedNodes.Clear();
            }
        }

        /* ----------PATHFINDING---------- */
        private float Heuristic(Node a, Node b)
        {
            return Math.Abs(a.coords.X - b.coords.X) + Math.Abs(a.coords.Y - b.coords.Y);
        }

        public Queue<Node> CreateCarRoute(Car car)
        {
            Node start = car.startNode;
            Node destination = car.destinationNode;
            // reset pathfinding data
            foreach (Node n in grid.roadNodes)
            {
                n.parent = null;
                n.gCost = float.MaxValue;
                n.hCost = 0;
            }

            List<Node> openList = new List<Node>();
            HashSet<Node> closedList = new HashSet<Node>();

            start.gCost = 0;
            start.hCost = Heuristic(start, destination);
            openList.Add(start);

            while (openList.Count > 0)
            {
                Node current = openList.OrderBy(n => n.fCost).First();
                openList.Remove(current);
                closedList.Add(current);

                if (current == destination)
                {
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

                    float tentativeG = current.gCost +
                                       Distance(current.coords, neighbor.coords);

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
                        neighbor.hCost = Heuristic(neighbor, destination);
                    }
                }
            }

            return null; // no path found
        }

        private Queue<Node> CreateCarRouteAvoidingNodes(Node start, Node destination, HashSet<Node> forbidden)
        {
            foreach (Node n in grid.roadNodes)
            {
                n.parent = null;
                n.gCost = float.MaxValue;
            }

            List<Node> open = new();
            HashSet<Node> closed = new();

            start.gCost = 0;
            open.Add(start);

            while (open.Count > 0)
            {
                Node current = open.OrderBy(n => n.gCost).First();
                open.Remove(current);
                closed.Add(current);

                if (current == destination)
                {
                    Stack<Node> path = new();
                    while (current != null)
                    {
                        path.Push(current);
                        current = current.parent;
                    }
                    return new Queue<Node>(path);
                }

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (forbidden.Contains(neighbor) || closed.Contains(neighbor)) { continue; }
                        

                    float g = current.gCost + Distance(current.coords, neighbor.coords);

                    if (g < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = g;
                        if (!open.Contains(neighbor)) { open.Add(neighbor); }
                            
                    }
                }
            }
            return null;
        }

        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private List<Node> GetNeighbors(Node node)
        {
            return grid.roadNodes.Where(n => (Math.Abs(n.coords.X - node.coords.X) == 16 && n.coords.Y == node.coords.Y) ||(Math.Abs(n.coords.Y - node.coords.Y) == 16 && n.coords.X == node.coords.X)).ToList();
        }
    }
}