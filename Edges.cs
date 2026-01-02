namespace CitySkylines0._5alphabeta
{
    public class Edge
    {
        public int edgeWeight { get; set; }
        public string name { get; set; }
        public Point a { get; set; }
        public Point b { get; set; }

        public List<IntersectingNode> intersections { get; set; } = new();
        public List<Point> pointsOnTheEdge { get; set; } = new();

        public Edge() { }

        public Edge(int weight, Point a, Point b, string name)
        {
            edgeWeight = weight;
            this.a = a;
            this.b = b;
            this.name = name;
            FindAllPointOnEdge(this);
        }


        public void AddIntersection(Point p, Edge e)
        {
            var existingNode = intersections.FirstOrDefault(n => n.coords == p);

            if (existingNode == null)
            {
                existingNode = new IntersectingNode(p);
                intersections.Add(existingNode);
            }

            if (!existingNode.connectedEdges.Contains(e))
            {
                existingNode.connectedEdges.Add(e);
            }
        }


        public void FindAllPointOnEdge(Edge road)
        {
            int steps = Math.Max(Math.Abs(road.b.X - road.a.X), Math.Abs(road.b.Y - road.a.Y));
            for (int step = 0; step <= steps; step += 16)
            {
                float t = step / (float)steps; // t is a parameter from 0 to 1
                float x = road.a.X + t * (road.b.X - road.a.X);
                float y = road.a.Y + t * (road.b.Y - road.a.Y);
                Point n = new Point((int)x, (int)y); // All points along the line become n
                road.pointsOnTheEdge.Add(n);
            }
        }

        public void IntersectionAlreadyExists()
        {
            intersections.Distinct();
        }
    }

    public class Road : Edge
    {
        public string type;

        public Road(int edgeweight, Point a, Point b, string name) : base(edgeweight, a, b, name)
        {
            type = "road";
            this.FindAllPointOnEdge(this);
        }
    }
}
