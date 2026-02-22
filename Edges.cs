using System.Text.Json.Serialization;
using System.Windows.Controls;

namespace CitySkylines0._5alphabeta
{
    public class Edge
    {
        public int edgeWeight { get; set; }
        public string name { get; set; }
        public Point a { get; set; }
        public Point b { get; set; }
        public int angle { get; set; } //anywhere from 0 to 360
        /*public List<IntersectionNode> intersections { get; set; } = new();*/
        public List<Node> occupyingNodes { get; set; } = new();
        public List<Point> pointsOnTheEdge { get; set; } = new();

        public Edge() { }

        public Edge(int weight, Point a, Point b, string name, int angle)
        {
            edgeWeight = weight;
            this.a = a;
            this.b = b;
            this.name = name;
            FindAllPointOnEdge(this);
            this.angle = angle;
        }

        public void FindAllPointOnEdge(Edge edge)
        {
            Point dir = new Point(Math.Sign(edge.b.X - edge.a.X), Math.Sign(edge.b.Y - edge.a.Y));

            int rectSize = 16;
            int steps = Math.Max(Math.Abs(edge.b.X - edge.a.X) / rectSize, Math.Abs(edge.b.Y - edge.a.Y) / rectSize);

            Point current = edge.a;

            for (int i = 0; i <= steps; i++)
            {
                edge.pointsOnTheEdge.Add(current);
                current = new Point(current.X + (dir.X * rectSize), current.Y + (dir.Y * rectSize));
            }
        }
    }

    public class Road : Edge
    {
        public string type;
        public Edge lane1;
        public Edge lane2;

        public Road(int edgeweight, Point a, Point b, string name, int angle) : base(edgeweight, a, b, name, angle)
        {
            type = "road";

            Point roadDir = new Point(Math.Sign(b.X - a.X), Math.Sign(b.Y - a.Y));
            Point perp = new Point(-roadDir.Y, roadDir.X);
            int laneOffset = 16;

            // Lane 1 = original line
            lane1 = new Edge(edgeweight, a, b, name + "_L1", angle);

            // Lane 2 = shifted exactly one tile
            Point lane2A = new Point(a.X + perp.X * laneOffset, a.Y + perp.Y * laneOffset);
            Point lane2B = new Point(b.X + perp.X * laneOffset, b.Y + perp.Y * laneOffset);

            // Reverse direction for opposite traffic
            lane2 = new Edge(edgeweight, lane2B, lane2A, name + "_L2", angle);

        }
    }
}
