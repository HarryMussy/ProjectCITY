namespace CitySkylines0._5alphabeta
{
    public class Edge //edges are always/ usually roads but can be power lines and water pipes
    {
        public int edgeweight; //higher edges (roads) can have more lanes for more cars
        public string name; //e.g. Benton Road or null
        public Point a;
        public Point b;
        public List<IntersectingNode> intersections;
        List<string> invalidnames = new List<string>();
        bool validity = false;
        public List<Point> pointsOnTheEdge;

        public Edge(int edgeweight, Point a, Point b, string name)
        {
            this.edgeweight = edgeweight;
            this.a = a;
            this.b = b;
            this.name = name;
            Random rng = new Random();
            intersections = new List<IntersectingNode>();
            pointsOnTheEdge = new List<Point>();
        }

        public void AddIntersection(Point p)
        {
            IntersectingNode newnode = new IntersectingNode(p);
            intersections.Add(newnode);
        }

        public void FindAllPointOnEdge(Edge road)
        {
            float edgeGradient = (road.b.Y - road.a.Y) / (road.b.X - road.a.X);
            int steps = Math.Max(Math.Abs(road.b.X - road.a.X), Math.Abs(road.b.Y - road.a.Y));
            for (int step = 0; step <= steps; step += 20)
            {
                float t = step / (float)steps; // t is a parameter from 0 to 1
                float x = road.a.X + t * (road.b.X - road.a.X);
                float y = road.a.Y + t * (road.b.Y - road.a.Y);
                Point n = new Point((int)x, (int)y); // All points along the line become n
                road.pointsOnTheEdge.Add(n);
            }
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
