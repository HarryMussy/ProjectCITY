using System;
using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public Grid grid;
        public PointF currentPosition;
        public Node startNode;
        public Point[] currentTrack = new Point[2];
        public float Progress; //0=start, 1=end
        public double Speed; //units per tick
        public Node destinationNode;
        public Queue<Node> route = new Queue<Node>();
        public bool isMoving = true;

        public Car(Node startNodeIn, double speed, Node destinationNodeIn)
        {
            startNode = startNodeIn;
            destinationNode = destinationNodeIn;
            Progress = 0f;
            Speed = speed;
            currentPosition = new PointF(startNodeIn.coords.X + 8, startNodeIn.coords.Y + 8);
        }
    }


    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>();

        public CarManager(Grid gridPassIn)
        {
            grid = gridPassIn;
        }

        public void MoveCar(Car car)
        {
            if (car.route.Count > 0)
            {
                Node nextNode = car.route.Peek();
                PointF targetPosition = new PointF(nextNode.coords.X + 8, nextNode.coords.Y + 8);
                float distanceToTarget = Distance(car.currentPosition, targetPosition);
                float distanceToMove = (float)car.Speed;
                if (distanceToMove >= distanceToTarget)
                {
                    car.currentPosition = targetPosition;
                    car.route.Dequeue();
                    car.Progress += 1f / (car.route.Count + 1); //update progress
                }
                else
                {
                    float ratio = distanceToMove / distanceToTarget;
                    car.currentPosition.X += (targetPosition.X - car.currentPosition.X) * ratio;
                    car.currentPosition.Y += (targetPosition.Y - car.currentPosition.Y) * ratio;
                    car.Progress += ratio * (1f / (car.route.Count + 1)); //update progress
                }
            }
            else
            {
                car.isMoving = false; //reached destination
            }
        }

        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public Queue<Node> CreateCarRoute(Car car)
        {
            List<Node> nodesBy16 = new List<Node>();
            foreach (Node node in grid.roadNodes)
            {
                nodesBy16.Add(new Node
                (
                    new Point(node.coords.X / 16, node.coords.Y / 16),
                    node.tiledata,
                    node.isNearRoad,
                    node.nodeNumber
                ));
            }
            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();
            Queue<Node> path = new Queue<Node>();
            /*
             * g- cost to move from the start point to a given square
             * h- estimated cost to move from a given square to the destination
             * f- total cost of a square- g + h
             */
            openList.Add(car.startNode);

            foreach (Node node in nodesBy16)
            {
                //calculate g, h, f for each node
                node.gCost = Math.Abs(node.coords.X - car.startNode.coords.X) + Math.Abs(node.coords.Y - car.startNode.coords.Y);
                node.hCost = Math.Abs(node.coords.X - car.destinationNode.coords.X) + Math.Abs(node.coords.Y - car.destinationNode.coords.Y);
            }

            while (openList.Count > 0)
            {
                Node q = nodesBy16.OrderBy(n => n.fCost).First();
                openList.Remove(q);

                foreach (Node n in nodesBy16)
                {
                    //generate q's 8 successors
                    //if successor is goal, stop search
                    if (n.coords == car.destinationNode.coords)
                    {
                        //reconstruct path
                        Node currentNode = n;
                        while (currentNode != null)
                        {
                            path.Append(currentNode);
                            currentNode = currentNode.parent;
                        }
                        path.Reverse();
                        foreach (Node pathNode in path)
                        {
                            car.route.Enqueue(pathNode);
                        }
                        nodesBy16 = null;
                        return path;
                    }
                    else
                    {
                        //compute g and h for successor
                        n.gCost = q.gCost + 1; //assuming distance between nodes is 1
                        n.hCost = Math.Abs(n.coords.X - car.destinationNode.coords.X) + Math.Abs(n.coords.Y - car.destinationNode.coords.Y);
                    }
                    //check open and closed lists
                    if (openList.Any(node => node.coords == n.coords && node.fCost < n.fCost))
                    {
                        continue;
                    }
                    if (closedList.Any(node => node.coords == n.coords && node.fCost < n.fCost))
                    {
                        continue;
                    }
                    openList.Add(n);
                }
            }
            nodesBy16 = null;
            return path; //return empty path if no path found
        }
    }
}