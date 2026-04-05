using System.Text.Json.Serialization;
using System.Windows.Controls;

namespace CitySkylines0._5alphabeta
{
    public class Edge
    {
        public string name { get; set; }
        public Point a { get; set; }
        public Point b { get; set; }
        public int angle { get; set; } // anywhere from 0 to 360
        public List<int> occupyingNodesIndex { get; set; }
        public List<Point> pointsOnTheEdge { get; set; }

        public Edge() { }
        public Edge(Point a, Point b, string name, int angle)
        {
            this.a = a;
            this.b = b;
            this.name = name;
            this.angle = angle;
            occupyingNodesIndex = new List<int>();
            pointsOnTheEdge = new List<Point>();
            FindAllPointOnEdge();
        }

        public void FindAllPointOnEdge()
        {
            pointsOnTheEdge.Clear();

            Point dir = new Point(Math.Sign(b.X - a.X), Math.Sign(b.Y - a.Y));

            int rectSize = 16;

            int steps = Math.Max(
                Math.Abs(b.X - a.X) / rectSize,
                Math.Abs(b.Y - a.Y) / rectSize
            );

            Point current = a;

            for (int i = 0; i <= steps; i++)
            {
                pointsOnTheEdge.Add(current);

                current = new Point(
                    current.X + (dir.X * rectSize),
                    current.Y + (dir.Y * rectSize)
                );
            }
        }
    }

    public class Road : Edge
    {
        public string type { get; set; }
        public string name { get; set; }

        public Edge lane1 { get; set; }
        public Edge lane2 { get; set; }

        public Road() { }

        public Road(Point a, Point b, string nameIn, int angle)
        {
            type = "road";
            name = nameIn;

            int dx = b.X - a.X;
            int dy = b.Y - a.Y;

            Point roadDir = new Point(Math.Sign(dx), Math.Sign(dy));
            Point perp = new Point(-roadDir.Y, roadDir.X);

            int laneOffset = 16; // FULL TILE

            // lane1 stays on original line
            lane1 = new Edge(a, b, name + "_L1", angle);

            // lane2 offset one tile perpendicular
            Point lane2A = new Point(
                a.X + perp.X * laneOffset,
                a.Y + perp.Y * laneOffset
            );

            Point lane2B = new Point(
                b.X + perp.X * laneOffset,
                b.Y + perp.Y * laneOffset
            );

            lane2 = new Edge(lane2B, lane2A, name + "_L2", angle);
        }

        public void RebuildAfterLoad()
        {
            lane1.FindAllPointOnEdge();
            lane2.FindAllPointOnEdge();
        }
    }
}