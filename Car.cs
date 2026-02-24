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
        public Point FacingDir = new Point(0, 0); // normalized grid direction (-1,0,1)
        public float TargetRotationAngle;
        public float RotationSpeed = 6f; // degrees per tick

        public bool isMoving = true;
        public bool hasPriority = false;

        public float stuckTimeSeconds = 0f;
        public HashSet<Node> blockedNodes = new HashSet<Node>();

        public Car() { }

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
        public bool inService = false;
        public Building destBuilding;

        public EmergencyServiceVehicle(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath, string typeIn, Building destBuildingIn) : base()
        {
            image = Image.FromFile(imageFilePath);
            type = typeIn;
            destBuilding = destBuildingIn;
        }
    }


    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>();
        public List<Image> carImages = new List<Image>();
        private Random carRandom = new Random();
        private readonly Calendar calendar;
        private const float tickDelta = 1f / 60f;
        static Point left = new Point(-1, 0);
        static Point right = new Point(1, 0);
        static Point up = new Point(0, 1);
        static Point down = new Point(0, -1);

        public CarManager(Grid gridPassIn, Calendar calendarPassIn)
        {
            grid = gridPassIn;
            calendar = calendarPassIn;
            LoadImages();
            Random rng = new Random();
        }

        //visuals

        public void AssignImage(Car car)
        {
            car.image = carImages[carRandom.Next(carImages.Count)];
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

                    g.FillEllipse(glow3, -6, -20, 12, 20);
                    g.FillEllipse(glow2, -4, -15, 8, 14);
                    g.FillEllipse(glow1, -2, -10, 4, 8);
                }

                g.DrawImage(car.image, -8, -8, 16, 16);
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

        //movement
        public void SpawnCarNearBuilding()
        {
            if (grid.buildings == null || grid.buildings.Count() <= 1) return;
            if (grid.roadNodes == null || grid.roadNodes.Count() == 0) return;

            // use the shared RNG
            var rng = carRandom;

            var startBuilding = grid.buildings[rng.Next(grid.buildings.Count)];
            if (startBuilding == null) return;

            Node startNode = grid.roadNodes.Where(n => n.OccupyingCar == null).OrderBy(n => Distance(startBuilding.coords, n.coords)).FirstOrDefault();
            if (startNode == null) return;

            var endBuilding = grid.buildings[rng.Next(grid.buildings.Count)];
            if (endBuilding == null || endBuilding == startBuilding) return;

            Node destinationNode = grid.roadNodes.OrderBy(n => Distance(endBuilding.coords, n.coords)).FirstOrDefault();
            if (destinationNode == null) return;

            Car car = new Car(startNode, 3f, destinationNode);
            AssignImage(car);
            car.route = CreateCarRoute(car);

            if (car.route != null && car.route.Count > 0)
            {
                Node next = car.route.Peek();

                Point dir = new Point(Math.Sign(next.coords.X - car.currentNode.coords.X), Math.Sign(next.coords.Y - car.currentNode.coords.Y));

                car.FacingDir = dir;
                car.RotationAngle = MathF.Atan2(dir.Y, dir.X) * 180f / MathF.PI;
                car.TargetRotationAngle = car.RotationAngle;
            }

            if (car.route == null || car.route.Count == 0)
            {
                Debug.WriteLine("Car has no route — cannot move.");
                // decide: either don't add the car or add it but mark not moving
                return;
            }

            cars.Add(car);
        }

        public void SendSpecificCarToAndFromSpecificBuilding(Car car, Building buildingA, Building buildingB)
        {
            if (grid.buildings == null || grid.buildings.Count() == 0) return;
            if (grid.roadNodes == null || grid.roadNodes.Count() == 0) return;

            // use the shared RNG
            var rng = carRandom;

            Node startNode = grid.roadNodes.Where(n => n.OccupyingCar == null).OrderBy(n => Distance(buildingA.coords, n.coords)).FirstOrDefault();
            if (startNode == null || startNode.OccupyingCar != null) return;

            Node destinationNode = grid.roadNodes.OrderBy(n => Distance(buildingB.coords, n.coords)).FirstOrDefault();
            if (destinationNode == null) return;

            car.startNode = startNode;
            car.currentNode = startNode;
            car.destinationNode = destinationNode;
            car.route = CreateCarRoute(car);

            if (car.route != null && car.route.Count > 0)
            {
                Node next = car.route.Peek();
                Point dir = new Point(Math.Sign(next.coords.X - car.currentNode.coords.X), Math.Sign(next.coords.Y - car.currentNode.coords.Y));

                car.FacingDir = dir;
                car.RotationAngle = MathF.Atan2(dir.Y, dir.X) * 180f / MathF.PI;
                car.TargetRotationAngle = car.RotationAngle;
            }

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
            //despawn cars that are occupying the same space
            var carsOccupyingDifferentNodes = cars.Where(c => c.currentNode != null).GroupBy(c => c.currentNode).ToDictionary(g => g.Key, g => g.ToList());

            // despawn extras, keep one car per node (if you want to remove all, adjust accordingly)
            foreach (var kv in carsOccupyingDifferentNodes)
            {
                var carsAtNode = kv.Value;
                if (carsAtNode.Count >= 2)
                {
                    // keep the first, despawn the rest
                    for (int i = 1; i < carsAtNode.Count; i++)
                    {
                        DespawnCar(carsAtNode[i]);
                    }
                }
            }

            if (car.route != null && car.route.Count > 0)
            {
                Node nextNode = car.route.Peek();

                //determine grid direction to next node
                Point desiredDir = new Point(Math.Sign(nextNode.coords.X - car.currentNode.coords.X), Math.Sign(nextNode.coords.Y - car.currentNode.coords.Y));

                car.FacingDir = desiredDir;
                car.TargetRotationAngle = MathF.Atan2(desiredDir.Y, desiredDir.X) * 180f / MathF.PI;

                RotateTowardsTarget(car);

                if (IsBlocked(car, nextNode)) return false;

                MoveTowardsNode(car, nextNode);
            }

            if (car.route != null && car.route.Count == 0 && car.currentNode == car.destinationNode)
            {
                DespawnCar(car);
                return true;
            }

            return false;
        }

        private void RotateTowardsTarget(Car car)
        {
            float diff = NormalizeAngle(car.TargetRotationAngle - car.RotationAngle);

            if (Math.Abs(diff) < car.RotationSpeed)
            {
                car.RotationAngle = car.TargetRotationAngle;
                return;
            }

            car.RotationAngle += Math.Sign(diff) * car.RotationSpeed;
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private bool IsBlocked(Car car, Node nextNode)
        {
            if (nextNode.OccupyingCar == null || nextNode.OccupyingCar == car) { return false; }

            else
            {
                car.stuckTimeSeconds += tickDelta;

                if (car.stuckTimeSeconds >= 10)
                {
                    car.blockedNodes.Add(nextNode);
                    TryRerouteCar(car);
                    car.stuckTimeSeconds = 0f;
                }

                return true;
            }
        }

        private void MoveTowardsNode(Car car, Node nextNode)
        {
            if (nextNode == car.currentNode)
            {
                car.route.Dequeue();
                return;
            }

            car.stuckTimeSeconds = 0f;

            //center of next tile
            PointF target = new PointF(nextNode.coords.X + 8, nextNode.coords.Y + 8);

            //direction from node → node (NOT position-based)
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

        public void DespawnCar(Car car)
        {
            foreach (Node n in grid.nodes)
            {
                if (n.OccupyingCar == car)
                {
                    n.OccupyingCar = null;
                }
            }

            cars.Remove(car);
            car.isMoving = false;
        }

        //deadlocks and rerouting
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

        //pathfinding
        private float Heuristic(Node a, Node b)
        {
            return Math.Abs(a.coords.X - b.coords.X) + Math.Abs(a.coords.Y - b.coords.Y);
        }

        public Queue<Node> CreateCarRoute(Car car)
        {
            Node start = car.currentNode ?? car.startNode;
            Node destination = car.destinationNode;

            List<Node> availableRoadNodes = grid.roadNodes.Where(n => !car.blockedNodes.Contains(n)).ToList();

            /*if (!availableRoadNodes.Contains(destination))
            {
                Debug.WriteLine("DO NOT CONTAIN DESTINATION!!!");
                return null;
            }*/

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

                if (current.coords == destination.coords)
                {
                    Stack<Node> path = new();
                    while (current != null)
                    {
                        path.Push(current);
                        current = current.parent;
                    }
                    return new Queue<Node>(path);
                }

                List<Node> nodes = GetNeighbors(current, car);

                if (nodes == null) { return null; }
                foreach (Node neighbor in nodes)
                {
                    if (closed.Contains(neighbor)) { continue; }

                    /*float trafficPenalty = neighbor.OccupyingCar != null ? 1000f : 0f;*/
                    float tentativeG = current.gCost + Distance(current.coords, neighbor.coords) /*+ trafficPenalty*/;

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

        private List<Node> GetNeighbors(Node node, Car car)
        {
            return node.neighbors.Where(n => !car.blockedNodes.Contains(n)).ToList();
        }
    }
}