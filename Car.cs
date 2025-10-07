using CitySkylines0._5alphabeta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public Grid grid;
        public PointF currentPosition;
        public Edge currentEdge;
        public Edge previousEdge;            // <-- new
        public Edge startEdge;
        public Point startPoint;
        public Point[] currentTrack = new Point[2];
        public float Progress; // 0=start, 1=end
        public double Speed; // units per tick
        public Point destinationPoint;
        public Edge destinationEdge;
        public Queue<Edge> route = new Queue<Edge>();
        public bool isMoving = true;         // <-- new

        public Car(Edge startEdgeIn, Point spawnIN, double speed, Point destinationIN, Edge edgeDestinationIN)
        {
            startEdge = startEdgeIn;
            previousEdge = null;
            Progress = 0f;
            Speed = speed;
            startPoint = spawnIN;
            currentTrack[0] = startEdge.a;
            currentTrack[1] = startEdge.b;
            currentPosition = new PointF(startPoint.X, startPoint.Y);
            currentEdge = startEdgeIn;
            destinationPoint = destinationIN;
            destinationEdge = edgeDestinationIN;
        }

        public bool HasReachedEnd() => Progress >= 1f;
    }


    public class CarManager
    {
        public Grid grid;
        public List<Car> cars = new List<Car>();
        private readonly Random rng = new Random(); // <-- single RNG for reasonable randomness

        public CarManager(Grid gridPassIn)
        {
            grid = gridPassIn;
        }

        public void CreateCarRoute(Car car)
        {
            // If the car doesn't have a current edge, start anywhere
            if (car.currentEdge == null)
            {
                car.currentEdge = grid.edges[Random.Shared.Next(grid.edges.Count)];
                car.startPoint = car.currentEdge.a;
                car.destinationPoint = car.currentEdge.b;
                return;
            }

            // --- Find all roads connected to the current one ---
            var connectedEdges = GetConnectedEdges(car.currentEdge).ToList();

            if (connectedEdges.Count == 0)
            {
                // Nowhere to go, stop here
                car.isMoving = false;
                return;
            }

            // --- Avoid immediate U-turns (don’t go back where we came from) ---
            if (car.previousEdge != null)
            {
                connectedEdges.Remove(car.previousEdge);
            }

            // --- Choose the next edge randomly ---
            Edge nextEdge;
            if (connectedEdges.Count == 0)
            {
                // If no other options (dead end), allow a U-turn
                nextEdge = car.previousEdge ?? car.currentEdge;
            }
            else
            {
                nextEdge = connectedEdges[Random.Shared.Next(connectedEdges.Count)];
            }

            // --- Determine the nearest valid intersection ---
            var shared = car.currentEdge.intersections
                .Where(n => n.connectedEdges.Contains(nextEdge))
                .ToList();

            IntersectingNode intersection = null;
            if (shared.Count > 0)
            {
                intersection = shared
                    .OrderBy(n => Distance(car.currentPosition, n.coords))
                    .First();
            }

            if (intersection != null)
            {
                // Pick the direction based on intersection position
                car.startPoint = intersection.coords;
                // Choose the far end of the next road as destination
                car.destinationPoint =
                    (Distance(intersection.coords, nextEdge.a) < Distance(intersection.coords, nextEdge.b))
                    ? nextEdge.b
                    : nextEdge.a;
            }
            else
            {
                // fallback if no intersection found
                car.startPoint = nextEdge.a;
                car.destinationPoint = nextEdge.b;
            }

            car.previousEdge = car.currentEdge;
            car.currentEdge = nextEdge;
            car.isMoving = true;
        }

        // Simple helper for distance comparison
        private float Distance(Point p1, Point p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }


        private void SetNextEdge(Car car, Edge nextEdge)
        {
            Edge fromEdge = car.currentEdge;
            car.previousEdge = car.currentEdge;
            car.currentEdge = nextEdge;
            car.Progress = 0f;

            // Find all intersections shared by both edges
            var shared = fromEdge.intersections
                .Where(n => n.connectedEdges.Contains(nextEdge))
                .ToList();

            IntersectingNode intersection = null;

            if (shared.Count > 0)
            {
                // Pick the intersection closest to where the car actually is
                intersection = shared
                    .OrderBy(n => Distance(car.currentPosition, n.coords))
                    .First();
            }
            else
            {
                // fallback — maybe only nextEdge has the reference
                shared = nextEdge.intersections
                    .Where(n => n.connectedEdges.Contains(fromEdge))
                    .ToList();

                if (shared.Count > 0)
                {
                    intersection = shared
                        .OrderBy(n => Distance(car.currentPosition, n.coords))
                        .First();
                }
            }

            if (intersection != null)
            {
                // Start at the intersection, move toward the far end of nextEdge
                car.currentTrack[0] = intersection.coords;
                car.currentTrack[1] = Distance(intersection.coords, nextEdge.a) < Distance(intersection.coords, nextEdge.b)
                    ? nextEdge.b
                    : nextEdge.a;

                car.startPoint = car.currentTrack[0];
                car.destinationPoint = car.currentTrack[1];
            }
            else
            {
                // fallback (no shared intersection found)
                car.currentTrack[0] = nextEdge.a;
                car.currentTrack[1] = nextEdge.b;
                car.startPoint = nextEdge.a;
                car.destinationPoint = nextEdge.b;
            }
        }


        public void MoveCar()
        {
            List<Car> carsToBeRemoved = new List<Car>();

            // iterate a snapshot to avoid modification while enumerating
            foreach (Car car in cars.ToList())
            {
                if (!car.isMoving) continue;

                MoveCarOnEdge(car);

                if (car.HasReachedEnd())
                {
                    if (car.route != null && car.route.Count > 0)
                    {
                        // follow precomputed route if available
                        var nextEdge = car.route.Dequeue();
                        SetNextEdge(car, nextEdge);
                    }
                    else
                    {
                        // No preset route: decide dynamically at this intersection
                        var connected = GetConnectedEdges(car.currentEdge).ToList();

                        // avoid U-turn: remove the edge we just came from if possible
                        if (car.previousEdge != null)
                            connected = connected.Where(e => e != car.previousEdge).ToList();

                        if (connected.Count == 0)
                        {
                            // dead-end: allow U-turn back if there's a previousEdge
                            if (car.previousEdge != null)
                            {
                                SetNextEdge(car, car.previousEdge);
                            }
                            else
                            {
                                // nowhere to go
                                carsToBeRemoved.Add(car);
                            }
                        }
                        else
                        {
                            // pick one of the outgoing edges randomly
                            Edge nextEdge = connected[rng.Next(connected.Count)];
                            SetNextEdge(car, nextEdge);
                        }
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
            PointF carCoords = car.currentPosition;

            // THEN move once (was previously getting moved multiple times because the move was inside the loop)
            car.Progress += (float)car.Speed;
            if (car.Progress > 1f) { car.Progress = 1f; }

            car.currentPosition = new PointF(
                car.currentTrack[0].X + (car.currentTrack[1].X - car.currentTrack[0].X) * car.Progress,
                car.currentTrack[0].Y + (car.currentTrack[1].Y - car.currentTrack[0].Y) * car.Progress
            );
        }

        private float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private IEnumerable<Edge> GetConnectedEdges(Edge edge)
        {
            HashSet<Edge> connected = new HashSet<Edge>();

            if (edge == null)
                return connected;

            // go through every intersection this edge has
            foreach (IntersectingNode node in edge.intersections)
            {
                // every edge that shares this intersection is a possible connection
                foreach (Edge e in node.connectedEdges)
                {
                    // don’t include itself
                    if (e != edge)
                        connected.Add(e);
                }
            }

            return connected;
        }
    }
}