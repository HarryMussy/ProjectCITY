using System.Diagnostics;
using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public PointF currentPosition;
        public Image image;
        public string type;
        public Node startNode;
        public Node currentNode;
        public Node destinationNode;

        public Queue<Node> route = new Queue<Node>();

        public double speed;
        public float rotationAngle; //current angle
        public Point facingDir = new Point(0, 0); // normalized grid direction (-1,0,1)
        public float targetRotationAngle; //angle the car is rotating towards
        public float rotationSpeed = 6f; // degrees per tick

        public bool isMoving = true;
        public bool hasPriority = false; //allows the car to overtake other cars

        public float stuckTimeSeconds = 0f;
        public HashSet<Node> blockedNodes = new HashSet<Node>();

        public Car() { }
        public Car(Node startNodeIn, double speed, Node destinationNodeIn)
        {
            //assemble car data
            startNode = startNodeIn;
            currentNode = startNodeIn;
            destinationNode = destinationNodeIn;
            this.speed = speed;
            type = "car";
            if (startNodeIn != null)
            {
                currentPosition = new PointF(startNodeIn.coords.X + 8, startNodeIn.coords.Y + 8);
                startNode.OccupyingCar = this;
            }
        }
    }

    //for vehicles such as police cars, ambulances and fire trucks
    public class EmergencyServiceVehicle : Car
    {
        public bool inService = false;
        public Building destBuilding;

        public EmergencyServiceVehicle(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath, string typeIn, Building destBuildingIn) : base(startNodeIn, speed, destinationNodeIn)
        {
            image = Image.FromFile(imageFilePath);
            destBuilding = destBuildingIn;
            base.speed = speed;
        }
    }

    public class Ambulance : EmergencyServiceVehicle
    {
        public Ambulance(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath, string typeIn, Building destBuildingIn) : base(startNodeIn, speed, destinationNodeIn, imageFilePath, typeIn, destBuildingIn)
        {
            type = "ambulance";
        }
    }

    public class PoliceCar : EmergencyServiceVehicle
    {
        public PoliceCar(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath, string typeIn, Building destBuildingIn) : base(startNodeIn, speed, destinationNodeIn, imageFilePath, typeIn, destBuildingIn)
        {
            type = "policecar";
        }
    }

    public class FireTruck : EmergencyServiceVehicle
    {
        public FireTruck(Node startNodeIn, double speed, Node destinationNodeIn, string imageFilePath, string typeIn, Building destBuildingIn) : base(startNodeIn, speed, destinationNodeIn, imageFilePath, typeIn, destBuildingIn)
        {
            type = "firetruck";
        }
    }


    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>(); //all cars currently on the map
        public List<Image> carImages = new List<Image>(); //stores all of the available car images on load
        private Random carRandom = new Random();
        private readonly Calendar calendar;
        private const float tickDelta = 1f / 60f;

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
            car.image = carImages[carRandom.Next(carImages.Count)]; //assign the car image a random image from the available images
        }

        public void CarPaint(object? sender, Graphics g)
        {
            foreach (Car car in cars) //for every car on the map
            {
                var state = g.Save(); //save the current transformation state (used in Form1_Paint)

                //appply rotations and transformation to the car based off of location and rotation
                g.TranslateTransform(car.currentPosition.X, car.currentPosition.Y);
                g.RotateTransform(car.rotationAngle + 90);

                //if it is night, apply a glow effect around the cars headlight
                if (calendar.GetHour() >= 21 || calendar.GetHour() <= 5)
                {
                    using Brush glow1 = new SolidBrush(Color.FromArgb(180, 255, 255, 200)); //brightest glow
                    using Brush glow2 = new SolidBrush(Color.FromArgb(90, 255, 255, 200)); //second- most bright glow
                    using Brush glow3 = new SolidBrush(Color.FromArgb(40, 255, 255, 200)); //dim glow 

                    g.FillEllipse(glow3, -6, -20, 12, 20);
                    g.FillEllipse(glow2, -4, -15, 8, 14);
                    g.FillEllipse(glow1, -2, -10, 4, 8);
                }

                g.DrawImage(car.image, -8, -8, 16, 16); //draw the cars assigned imaged based off of 
                g.Restore(state); //restore Form1_Paint transformations
            }
        }

        //load all available car images from the folder
        private void LoadImages()
        {
            string projectRoot = AppContext.BaseDirectory; //base folder
            string folder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Cars"); //directory from the base folder

            foreach (string path in Directory.GetFiles(folder, "*.png")) //for every image in the folder
            {
                using var original = Image.FromFile(path);
                Bitmap bmp = new Bitmap(original.Width, original.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(original, 0, 0);
                }
                carImages.Add(bmp); //add the bitmap image to the available images
            }
        }

        //movement

        //creates a new car
        public void SpawnCarNearBuilding()
        {
            if (grid.buildings == null || grid.buildings.Count() <= 1) return; //if there are no buildings dont spawn a car
            if (grid.nodes == null || grid.nodes.Where(node => node.isRoad).Count() == 0) return; //if there are no roads dont spawn a car

            // use the shared RNG
            var rng = carRandom;

            var startBuilding = grid.buildings[rng.Next(grid.buildings.Count)]; //random start building
            if (startBuilding == null) return;

            Node startNode = grid.nodes.Where(n => n.OccupyingCar == null && n.isRoad).OrderBy(n => Distance(startBuilding.coords, n.coords)).FirstOrDefault(); //finds the nearest node to the building
            if (startNode == null) return;

            var endBuilding = grid.buildings[rng.Next(grid.buildings.Count)]; //random end building
            if (endBuilding == null || endBuilding == startBuilding) return;

            Node destinationNode = grid.nodes.Where(node => node.isRoad).OrderBy(n => Distance(endBuilding.coords, n.coords)).FirstOrDefault(); //finds the closest node to the building
            if (destinationNode == null) return;

            Car car = new Car(startNode, 3f, destinationNode); //create a car
            AssignImage(car); //give the car an image
            car.route = CreateCarRoute(car); //create a route for the car using the A* pathfinding algorithm

            if (car.route != null && car.route.Count > 0) //if there is a valid route
            {
                Node next = car.route.Peek(); //find the next node in its path

                Point dir = new Point(Math.Sign(next.coords.X - car.currentNode.coords.X), Math.Sign(next.coords.Y - car.currentNode.coords.Y)); //assign a rotation to the car

                car.facingDir = dir;
                car.rotationAngle = MathF.Atan2(dir.Y, dir.X) * 180f / MathF.PI; //converts the vector angle to degrees
                car.targetRotationAngle = car.rotationAngle;
            }

            if (car.route == null || car.route.Count == 0) //if there is no valid route
            {
                Debug.WriteLine("Car has no route — cannot move.");
                return;
            }

            if (!cars.Contains(car)) { cars.Add(car); } //if the car doesn't exist in the cars list, add it to the list
        }

        //used for sending an emergency service vehicle to a specific building (ESV => Emergency Service Vehicle)
        public void SendSpecificCarToAndFromSpecificBuilding(EmergencyServiceVehicle esv, Building buildingA, Building buildingB)
        {
            // use the shared RNG
            var rng = carRandom;

            Node startNode = grid.nodes.Where(n => n.isRoad && n.OccupyingCar == null).OrderBy(n => Distance(buildingA.coords, n.coords)).FirstOrDefault();
            if (startNode == null) return;

            Node destinationNode = grid.nodes.Where(n => n.isRoad).OrderBy(n => Distance(buildingB.coords, n.coords)).FirstOrDefault();
            if (destinationNode == null) return;

            //constructs necessary car data for the ESV
            esv.startNode = startNode;
            esv.destinationNode = destinationNode;
            esv.currentNode = startNode;
            esv.currentPosition = new PointF(startNode.coords.X + 8, startNode.coords.Y + 8);
            startNode.OccupyingCar = esv;
            esv.route = CreateCarRoute(esv); //creates the route
            esv.isMoving = true;

            if (esv.route != null && esv.route.Count > 0) //if there is a valid route
            {
                Node next = esv.route.Peek(); //find the next node in its path

                Point dir = new Point(Math.Sign(next.coords.X - esv.currentNode.coords.X), Math.Sign(next.coords.Y - esv.currentNode.coords.Y)); //assign a rotation to the ESV

                esv.facingDir = dir;
                esv.rotationAngle = MathF.Atan2(dir.Y, dir.X) * 180f / MathF.PI; //converts the vector angle to degrees
                esv.targetRotationAngle = esv.rotationAngle;
            }

            if (esv.route == null || esv.route.Count == 0) //if there is no valid route
            {
                Debug.WriteLine("Car has no route — cannot move.");
                return;
            }

            if (!cars.Contains(esv)) { cars.Add(esv); } //if the ESV doesn't exist in the cars list, add it to the list
        }

        public bool MoveCar(Car car)
        {
            if (car.route != null && car.route.Count > 0)
            {
                Node nextNode = car.route.Peek(); //find the next point in the route
                Point desiredDir = new Point(Math.Sign(nextNode.coords.X - car.currentNode.coords.X), Math.Sign(nextNode.coords.Y - car.currentNode.coords.Y)); //find the desired angle of the car
                car.facingDir = desiredDir;
                car.targetRotationAngle = MathF.Atan2(desiredDir.Y, desiredDir.X) * 180f / MathF.PI; //converts the point directions into degrees
                RotateTowardsTarget(car); //rotate the car over time (if the target is the same as the current direction, visually nothing changes)

                //only block on non-intersection nodes, let cars flow through intersections
                bool nextIsIntersection = nextNode.neighbors.Count >= 3;
                if (!nextIsIntersection && IsBlocked(car, nextNode)) return false;

                MoveTowardsNode(car, nextNode); //move the car towards the next node
            }

            if (car.route != null && car.route.Count == 0 && car.currentNode == car.destinationNode) //if the route is empty and its current node is the destination node
            {
                if (car.type == "car") { DespawnCar(car); } //if it is a car despawn it
                else { car.isMoving = false; DespawnEmergencyServiceVehicle(car); } //if it is an ESV, despawn the ESV
                return true;
            }
            return false;
        }

        private void RotateTowardsTarget(Car car)
        {
            if (car.targetRotationAngle != car.rotationAngle) //if the target angle and current angle are different
            {
                float diff = NormalizeAngle(car.targetRotationAngle - car.rotationAngle); //normalise the difference in the angles

                if (Math.Abs(diff) < car.rotationSpeed) //if the difference is less than the rotation speed: set the angle to the desired angle
                {
                    car.rotationAngle = car.targetRotationAngle;
                    return;
                }

                car.rotationAngle += Math.Sign(diff) * car.rotationSpeed; //otherwise increase the rotation angle by the cars rotation speed
            }
        }

        private float NormalizeAngle(float angle) //make the angle between -180 and 180
        {
            while (angle > 180f) { angle -= 360f; }
            while (angle < -180f) { angle += 360f; }
            return angle;
        }

        private bool IsBlocked(Car car, Node nextNode)
        {
            if (nextNode.OccupyingCar == null || nextNode.OccupyingCar == car) { return false; } //if there is no car in front then the car is not blocked

            car.stuckTimeSeconds += tickDelta; //increase the stuck time by the tickDelta

            if (car.stuckTimeSeconds >= 3f) //if the car has been stuck for 3 seconds give the car priority
            {
                car.hasPriority = true;
            }

            if (car.stuckTimeSeconds >= 6f) //if the car has been stuck for 6 seconds, remove priority, block the nodes and reroute the car
            {
                car.blockedNodes.Add(nextNode);
                car.hasPriority = false;
                car.stuckTimeSeconds = 0f;
                TryRerouteCar(car);
            }

            if (car.type == "car" && car.stuckTimeSeconds >= 10f) //if the car has been stuck for 10 seconds, despawn the car
            {
                DespawnCar(car);
                return true;
            }

            if (car.blockedNodes.Count > 20) { car.blockedNodes.Clear(); } //if the car blocks to many nodes, clear the blocked nodes to open up potential routes again
            return true;
        }

        private void MoveTowardsNode(Car car, Node nextNode) //advance the cars position towards the next node in the route
        {
            if (nextNode == car.currentNode) //if the car has reached the next node, dequeue the cars route
            {
                car.route.Dequeue();
                return;
            }

            PointF target = new PointF(nextNode.coords.X + 8, nextNode.coords.Y + 8); //give the car a new target point
            float dx = target.X - car.currentPosition.X;
            float dy = target.Y - car.currentPosition.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy); //find the distance between the cars current position and next position

            if (dist < 0.01f) { return; } //if the distance is really small, car doesnt need to move

            car.rotationAngle = MathF.Atan2(dy, dx) * 180f / MathF.PI; //find the angle in degrees for the cars rotation

            float step = (float)car.speed; //find the distance the car travels

            //if the car will reach the destination in its current step
            if (dist <= step)
            {
                bool nextIsIntersection = nextNode.neighbors.Count >= 3;

                //only force occupancy on non-intersection nodes
                if (!nextIsIntersection && nextNode.OccupyingCar != null && nextNode.OccupyingCar != car && !car.hasPriority)
                {
                    return;
                }

                //ensure the node the car was occupying is cleared
                if (car.currentNode != null && car.currentNode.OccupyingCar == car)
                {
                    car.currentNode.OccupyingCar = null;
                }

                car.currentNode = nextNode;

                //don't mark intersection nodes as occupied, they're shared space
                if (!nextIsIntersection)
                {
                    nextNode.OccupyingCar = car;
                }

                car.currentPosition = target;
                car.route.Dequeue();

                if (car.type == "car") { car.hasPriority = false; }
            }

            //otherwise update the cars position
            else
            {
                car.currentPosition = new PointF(car.currentPosition.X + dx / dist * step, car.currentPosition.Y + dy / dist * step);
            }
        }

        public void DespawnCar(Car car)
        {
            if (car.type == "car")
            {
                if (car.currentNode != null)
                {
                    car.currentNode.OccupyingCar = null;
                    car.currentNode = null;
                }

                cars.Remove(car);
                car.isMoving = false;
            }
            else { DespawnEmergencyServiceVehicle(car); }
        }
        public void DespawnEmergencyServiceVehicle(Car e)
        {
            if (e.currentNode != null && e.currentNode.OccupyingCar != null) { e.currentNode.OccupyingCar = null; }
            if (e.currentNode != null) { e.currentNode = null; }
            e.route?.Clear();
            e.isMoving = false;
            cars.Remove(e);
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

        //manhattan distance heuristic, movement is axis-aligned only
        private float Heuristic(Node a, Node b)
        {
            return Math.Abs(a.coords.X - b.coords.X) + Math.Abs(a.coords.Y - b.coords.Y);
        }

        //A* pathfinding algorithm, finds the shortest route between two road nodes
        //applies traffic penalties to encourage cars to avoid congested routes
        public Queue<Node> CreateCarRoute(Car car)
        {
            Node start = car.currentNode ?? car.startNode;
            Node destination = car.destinationNode;

            //exclude nodes the car has previously identified as blocked
            List<Node> availableRoadNodes = grid.nodes.Where(node => node.isRoad).Where(n => !car.blockedNodes.Contains(n)).ToList();

            //reset pathfinding costs from a previous search
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
                //select the node with the lowest fCost (gCost + hCost) from the open list
                Node current = open.OrderBy(n => n.fCost).First();
                open.Remove(current);
                closed.Add(current);

                //destination reached, reconstruct the path by walking back through parent references
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

                    //count how many neighbours of this node are also occupied to detect and heavily penalise queue build-up
                    int surroundingCars = neighbor.neighbors.Count(n => n.OccupyingCar != null && n.OccupyingCar != car);

                    //traffic penalties: occupied node is heavily penalised, surrounding congestion adds a milder penalty
                    float trafficPenalty = 0f;
                    if (neighbor.OccupyingCar != null && neighbor.OccupyingCar != car) { trafficPenalty = 1000f; }
                    else if (surroundingCars >= 2) { trafficPenalty = 500f; }  //heavily penalise approach to a backed-up area
                    else if (surroundingCars == 1) { trafficPenalty = 150f; }  //mild penalty for approaching traffic

                    float tentativeG = current.gCost + Distance(current.coords, neighbor.coords) + trafficPenalty;

                    //update the neighbour if this path is cheaper than any previously found path to it
                    if (!open.Contains(neighbor) || tentativeG < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = tentativeG;
                        neighbor.hCost = Heuristic(neighbor, destination);
                        if (!open.Contains(neighbor)) { open.Add(neighbor); }
                    }
                }
            }

            return null; //no path found
        }

        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        //returns neighbours of a node that are not on the car's blocked list
        private List<Node> GetNeighbors(Node node, Car car)
        {
            return node.neighbors.Where(n => !car.blockedNodes.Contains(n)).ToList();
        }
    }
}