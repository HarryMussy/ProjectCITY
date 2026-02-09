using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public class Node
    {
        public Point coords { get; set; }
        public Car OccupyingCar { get; set; } = null;
        public int laneIndex { get; set; } = 0; //0 is one way 1 is the other
        public Edge parentEdge { get; set; }
        public HashSet<Point> allowedDirs { get; set; } = new();
        public bool hasTileData { get; set; }
        public bool isNearRoad { get; set; }
        public int nodeNumber { get; set; }
        public bool isRoad { get; set; }
        public bool isGrass { get; set; }
        public bool isBuildable { get; set; }
        public float gCost { get; set; }
        public float hCost { get; set; }
        public float fCost => gCost + hCost;
        public Node parent { get; set; }
        public string imageKey { get; set; }

        public Node() { } // required
        public Node(Point coords, bool isTileData, bool near, int number, bool isRoad, bool isGrass)
        {
            this.coords = coords;
            this.hasTileData = isTileData;
            isNearRoad = near;
            nodeNumber = number;
            this.isRoad = isRoad;
            this.isGrass = isGrass;
            IsNodeBuildable();
        }

        public void IsNodeBuildable()
        {
            if (!hasTileData && isGrass && isNearRoad && !isRoad) 
            {
                isBuildable = true;
            }
        }
    }

   /* public class IntersectionNode
    {
        public Point coords { get; set; }
        public Size size { get; set; }
        public List<Edge> connectedEdges { get; set; } = new();

        public IntersectionNode() { }
        public IntersectionNode(Point coords, Size size)
        {
            this.coords = coords;
            this.size = size;
        }

        public bool Equals(IntersectionNode? other)
        {
            if (other is null) return false;
            return coords == other.coords;
        }

        public override bool Equals(object? obj) => Equals(obj as IntersectionNode);

        public override int GetHashCode() => coords.GetHashCode();

        public static bool operator ==(IntersectionNode? left, IntersectionNode? right) => object.ReferenceEquals(left, right) ? true : (left?.Equals(right) ?? false);

        public static bool operator !=(IntersectionNode? left, IntersectionNode? right) => !(left == right);
    }*/
}
