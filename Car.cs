using CitySkylines0._5alphabeta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public Grid grid;
        public PointF currentPosition;
        public Edge currentEdge;
        public Edge startEdge;
        public Point startPoint;
        public Point[] currentTrack;
        public float Progress; // 0=start, 1=end
        public float Speed; // units per tick
        public Point destinationPoint;
        public Edge destinationEdge;
        public Queue<Edge> route = new Queue<Edge>();

        public Car(Edge startEdgeIn, Point spawnIN, float speed, Point destinationIN, Edge edgeDestinationIN)
        {
            startEdge = startEdgeIn;
            Progress = 0f;
            Speed = speed;
            currentTrack[0] = startEdge.a;
            currentTrack[1] = startEdge.b;
            currentPosition = new PointF(startPoint.X, startPoint.Y);
            currentEdge = startEdgeIn;
            startPoint = spawnIN;
            destinationPoint = destinationIN;
            destinationEdge = edgeDestinationIN;
        }

        public bool HasReachedEnd() => Progress >= 1f;
    }

    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>();
        public CarManager(Grid gridPassIn)
        {
            grid = gridPassIn;
        }


        private void SetNextEdge(Car car, Edge nextEdge)
        {
            car.currentEdge = nextEdge;
            car.Progress = 0f;

            // Find intersection point between current and next edge
            var intersection = car.currentEdge.intersections
                .FirstOrDefault(node => node.connectedEdges.Contains(nextEdge));

            if (intersection != null)
            {
                // Start at the intersection, drive towards the other end
                car.currentTrack[0] = intersection.coords;

                // Decide destination: whichever endpoint of nextEdge is NOT the intersection
                if (nextEdge.a == intersection.coords)
                    car.currentTrack[1] = nextEdge.b;
                else
                    car.currentTrack[1] = nextEdge.a;
            }
            else
            {
                // fallback (just go a→b)
                car.startPoint = nextEdge.a;
                car.destinationPoint = nextEdge.b;
            }
        }

        public void MoveCar()
        {
            List<Car> carsToBeRemoved = new List<Car>();

            foreach (Car car in cars)
            {
                MoveCarOnEdge(car);

                if (car.HasReachedEnd())
                {
                    if (car.route.Count > 0)
                    {
                        var nextEdge = car.route.Dequeue();
                        SetNextEdge(car, nextEdge); // ✅ ensures correct direction
                    }
                    else
                    {
                        // reached destination
                        carsToBeRemoved.Add(car);
                    }
                }
            }

            foreach (Car car in carsToBeRemoved)
            {
                cars.Remove(car);
            }
        }



        public void MoveCarOnEdge(Car car)
        {
            car.Progress += car.Speed;
            if (car.Progress > 1f) { car.Progress = 1f; }
            car.currentPosition = new PointF(car.currentTrack[0].X + (car.currentTrack[1].X - car.currentTrack[0].X) * car.Progress, car.currentTrack[0].Y + (car.currentTrack[1].Y - car.currentTrack[0].Y) * car.Progress);
        }

        public void CreateCarRoute(Car car)
        {
            var distances = new Dictionary<Edge, int>();
            var previous = new Dictionary<Edge, Edge>();
            var unvisited = new HashSet<Edge>(grid.edges);
            //initialize distances
            foreach (var edge in grid.edges)
            {
                distances[edge] = int.MaxValue;
                previous[edge] = null;
            }
            distances[car.startEdge] = 0;
            while (unvisited.Count > 0)
            {
                //pick edge with smallest distance
                Edge current = unvisited.OrderBy(e => distances[e]).First();
                unvisited.Remove(current);
                //if we reached destination, stop
                if (current == car.destinationEdge)
                    break;
                //check edges that share an intersection
                foreach (var neighbor in GetConnectedEdges(current))
                {
                    if (!unvisited.Contains(neighbor)) continue;
                    int tentativeDist = distances[current] + neighbor.edgeweight;
                    if (tentativeDist < distances[neighbor])
                    {
                        distances[neighbor] = tentativeDist;
                        previous[neighbor] = current;
                    }
                }
            }
            //reconstruct path
            var path = new Stack<Edge>();
            var crawl = car.destinationEdge;
            while (crawl != null)
            {
                path.Push(crawl);
                crawl = previous[crawl];
            }
            car.route = new Queue<Edge>(path);
        }
        private IEnumerable<Edge> GetConnectedEdges(Edge edge)
        {
            //all edges that share an intersection with this one
            return edge.intersections.SelectMany(node => node.connectedEdges).Where(e => e != edge);
        }
    }
}
