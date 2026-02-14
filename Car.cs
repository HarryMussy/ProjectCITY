using System.Diagnostics;
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

            currentPosition = new PointF(startNodeIn.coords.X + 8, startNodeIn.coords.Y + 8);
            startNode.OccupyingCar = this;
        }
    }

    public class EmergencyServiceVehicle : Car
    {
        public string type;

        public EmergencyServiceVehicle(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath) : base(startNodeIn, speed, destinationNodeIn)
        {
            image = Image.FromFile(imageFilePath);
        }
    }


    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>();
        public List<Image> carImages = new List<Image>();
        private Random carRandom = new Random();
        private readonly Calendar calendar;
        private readonly Random rng = new Random();

        private const float TickDelta = 1f / 60f;

        public CarManager(Grid gridPassIn, Calendar calendarPassIn)
        {
            grid = gridPassIn;
            calendar = calendarPassIn;
            LoadImages();
            Random rng = new Random();
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

                    g.FillEllipse(glow3, -4, -20, 12, 20);
                    g.FillEllipse(glow2, -2, -15, 8, 14);
                    g.FillEllipse(glow1, 0, -10, 4, 8);
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
        public void SpawnCarNearBuilding()
        {
            if (grid.buildings == null || grid.buildings.Count() == 0) return;
            if (grid.roadNodes == null || grid.roadNodes.Count() == 0) return;

            // use the shared RNG
            var rng = carRandom;

            var startBuilding = grid.buildings[rng.Next(grid.buildings.Count())];
            if (startBuilding == null) return;

            var possibleNodes = grid.roadNodes.Where(n => n.OccupyingCar == null).OrderBy(n => Distance(startBuilding.coords, n.coords)).Take(5).ToList(); // take nearest few
            if (possibleNodes.Count == 0) return;
            Node closestNode = possibleNodes[carRandom.Next(possibleNodes.Count)];

            var endBuilding = grid.buildings[rng.Next(grid.buildings.Count)];
            if (endBuilding == null) return;

            Node nDest = grid.roadNodes.OrderBy(n => Distance(endBuilding.coords, n.coords)).FirstOrDefault();
            if (nDest == null) return;

            Car car = new Car(closestNode, 3, nDest);
            AssignImage(car);
            car.route = CreateCarRoute(car);

            if (car.route == null || car.route.Count == 0)
            {
                Debug.WriteLine("Car has no route — cannot move.");
                // decide: either don't add the car or add it but mark not moving
                return;
            }

            cars.Add(car);
        }

        public bool MoveCar(Car car)
        {
            if (!car.isMoving || car.route.Count == 0) { return true; }

            Node nextNode = car.route.Peek();

            if (IsBlocked(car, nextNode)) { return true; }

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

            if (car.stuckTimeSeconds >= rng.Next(1, 10))
            {
                TryRerouteCar(car);
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
            if (nextNode == car.currentNode)
            {
                car.route.Dequeue();
                return;
            }

            car.stuckTimeSeconds = 0f;

            // center of next tile
            PointF target = new PointF(nextNode.coords.X + 8, nextNode.coords.Y + 8);

            // direction from node → node (NOT position-based)
            Point dir = new Point(Math.Sign(nextNode.coords.X - car.currentNode.coords.X), Math.Sign(nextNode.coords.Y - car.currentNode.coords.Y));

            float dx = target.X - car.currentPosition.X;
            float dy = target.Y - car.currentPosition.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist < 0.01f) { return; }

            car.RotationAngle = MathF.Atan2(dy, dx) * 180f / MathF.PI;

            float step = (float)(car.Speed);

            if (dist <= step)
            {
                if (nextNode.OccupyingCar != null && nextNode.OccupyingCar != car && !car.hasPriority) { return; }
                if (car.currentNode.OccupyingCar == car) { car.currentNode.OccupyingCar = null; }

                car.currentNode = nextNode;
                nextNode.OccupyingCar = car;
                car.currentPosition = target;
                
                car.route.Dequeue();
                car.hasPriority = false;
            }
            else
            {
                car.currentPosition = new PointF(car.currentPosition.X + dx / dist * step,car.currentPosition.Y + dy / dist * step);
            }
        }
        private void DespawnCar(Car car)
        {
            if (car.currentNode != null && car.currentNode.OccupyingCar == car)
            {
                car.currentNode.OccupyingCar = null;
            }

            car.isMoving = false;
        }

        /* ----------DEADLOCK / ROUTING---------- */

        private bool IsHeadOnDeadlock(Car car, Node nextNode)
        {
            Car other = nextNode.OccupyingCar;
            if (other == null || other.route == null || other.route.Count == 0 || other == car)
                return false;

            return other.route.Peek() == car.currentNode;
        }

        private void TryRerouteCar(Car car)
        {
            if (car.blockedNodes.Count == 0) { return; }

            Queue<Node> newRoute = CreateCarRoute(car);

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
            Node start = car.currentNode;
            Node destination = car.destinationNode;

            List<Node> availableRoadNodes = grid.roadNodes.Where(n => !car.blockedNodes.Contains(n)).ToList();

            foreach (Node n in availableRoadNodes)
            {
                n.parent = null;
                n.gCost = float.MaxValue;
                n.hCost = 0;
            }

            List<Node> open = new();
            HashSet<Node> closed = new();

            start.gCost = 0;
            start.hCost = Heuristic(start, destination);
            open.Add(start);

            while (open.Count > 0)
            {
                Node current = open.OrderBy(n => n.fCost).First();
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

                foreach (Node neighbor in GetNeighbors(current, availableRoadNodes))
                {
                    if (closed.Contains(neighbor)) { continue; }

                    float trafficPenalty = neighbor.OccupyingCar != null ? 10000f : 0f;
                    float tentativeG = current.gCost + Distance(current.coords, neighbor.coords) + trafficPenalty;

                    if (!open.Contains(neighbor) || tentativeG < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = tentativeG;
                        neighbor.hCost = Heuristic(neighbor, destination);

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

        private List<Node> GetNeighbors(Node node, List<Node> availableNodes)
        {
            List<Node> result = new();

            foreach (Node n in availableNodes)
            {
                if (n == node) continue;

                int dx = n.coords.X - node.coords.X;
                int dy = n.coords.Y - node.coords.Y;

                // must be adjacent
                if (!((Math.Abs(dx) == 16 && dy == 0) ||
                      (Math.Abs(dy) == 16 && dx == 0)))
                    continue;

                Point dir = new Point(Math.Sign(dx), Math.Sign(dy));

                //must follow current node's allowed exit direction
                if (!node.allowedDirs.Contains(dir))
                    continue;

                //lane change only allowed at intersections
                if (!IsIntersection(node, availableNodes) &&
                    n.laneIndex != node.laneIndex)
                    continue;

                result.Add(n);
            }

            return result;
        }

        private bool IsIntersection(Node node, List<Node> availableNodes)
        {
            HashSet<Point> directions = new();

            foreach (Node n in availableNodes)
            {
                if (n == node) continue;

                int dx = n.coords.X - node.coords.X;
                int dy = n.coords.Y - node.coords.Y;

                if ((Math.Abs(dx) == 16 && dy == 0) ||
                    (Math.Abs(dy) == 16 && dx == 0))
                {
                    directions.Add(new Point(Math.Sign(dx), Math.Sign(dy)));
                }
            }

            // Intersection if we have more than 2 distinct directions
            return directions.Count > 2;
        }
    }
}