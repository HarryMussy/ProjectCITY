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
        public List<Node> blockedNodes = new List<Node>();

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

                    g.FillEllipse(glow3, -16, -15, 12, 20);
                    g.FillEllipse(glow2, -4, -15, 8, 15);
                    g.FillEllipse(glow1, 0, -15, 4, 9);
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
            PointF target = new PointF(
                nextNode.coords.X + 8,
                nextNode.coords.Y + 8
            );

            // direction from node → node (NOT position-based)
            Point dir = new Point(
                Math.Sign(nextNode.coords.X - car.currentNode.coords.X),
                Math.Sign(nextNode.coords.Y - car.currentNode.coords.Y)
            );

            float dx = target.X - car.currentPosition.X;
            float dy = target.Y - car.currentPosition.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist < 0.01f)
                return;

            car.RotationAngle = MathF.Atan2(dy, dx) * 180f / MathF.PI;

            float step = (float)(car.Speed);

            if (dist <= step)
            {
                if (nextNode.OccupyingCar != null &&
                    nextNode.OccupyingCar != car &&
                    !car.hasPriority)
                    return;

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
                    car.currentPosition.X + dx / dist * step,
                    car.currentPosition.Y + dy / dist * step
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
            if (other == null || other.route == null || other.route.Count == 0)
                return false;

            return other.route.Peek() == car.currentNode;
        }

        private void TryRerouteCar(Car car)
        {
            if (car.blockedNodes.Count == 0)
                return;

            Queue<Node> newRoute = CreateCarRouteAvoidingNodes(car.currentNode, car.destinationNode, car.blockedNodes
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

            foreach (Node n in grid.roadNodes)
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

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (closed.Contains(neighbor))
                        continue;

                    float trafficPenalty =
                        neighbor.OccupyingCar != null ? 50f : 0f;

                    float tentativeG =
                        current.gCost +
                        Distance(current.coords, neighbor.coords) +
                        trafficPenalty;

                    if (!open.Contains(neighbor) || tentativeG < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = tentativeG;
                        neighbor.hCost = Heuristic(neighbor, destination);

                        if (!open.Contains(neighbor))
                            open.Add(neighbor);
                    }
                }
            }

            return null;
        }
        private Queue<Node> CreateCarRouteAvoidingNodes(Node start, Node destination, List<Node> forbidden)
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
                    if (forbidden.Contains(neighbor) || closed.Contains(neighbor))
                        continue;

                    float g =
                        current.gCost +
                        Distance(current.coords, neighbor.coords);

                    if (g < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = g;

                        if (!open.Contains(neighbor))
                            open.Add(neighbor);
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
            List<Node> result = new();
            node.isIntersection = IsIntersection(node);

            foreach (Node n in grid.roadNodes)
            {
                if (n == node) { continue; }

                int dx = n.coords.X - node.coords.X;
                int dy = n.coords.Y - node.coords.Y;

                // Must be cardinally adjacent
                if (!((Math.Abs(dx) == 16 && dy == 0) || (Math.Abs(dy) == 16 && dx == 0)))
                {
                    continue;
                }

                Point dir = new Point(Math.Sign(dx), Math.Sign(dy));
                // 🚫 MID-ROAD LANE CHANGES ARE ILLEGAL
                if (!node.isIntersection)
                {
                    // must stay on same lane
                    if (n.laneIndex == node.laneIndex) { continue; }

                    // must follow lane direction
                    if (!node.allowedDirs.Contains(dir)) { continue; }
                }

                // 🟢 INTERSECTION: allow all
                else
                {
                    // but still enforce neighbor accepts us
                    Point reverse = new Point(-dir.X, -dir.Y);
                    if (n.allowedDirs.Count > 0 &&
                        !n.allowedDirs.Contains(reverse))
                        continue;
                }

                result.Add(n);
            }

            return result;
        }


        private bool IsIntersection(Node node)
        {
            int count = 0;
            foreach (Node n in grid.roadNodes)
            {
                if (n == node) { continue; }

                int dx = Math.Abs(n.coords.X - node.coords.X);
                int dy = Math.Abs(n.coords.Y - node.coords.Y);

                if ((dx == 16 && dy == 0) || (dx == 0 && dy == 16))
                {
                    count++;
                }    
            }
            return count >= 3;
        }


    }
}