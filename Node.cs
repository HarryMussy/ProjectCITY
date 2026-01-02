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
        public Building tileData { get; set; }
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
        public Node(Point coords, Building tileData, bool near, int number, bool isRoad, bool isGrass)
        {
            this.coords = coords;
            this.tileData = tileData;
            isNearRoad = near;
            nodeNumber = number;
            this.isRoad = isRoad;
            this.isGrass = isGrass;
            IsNodeBuildable();
        }

        public void IsNodeBuildable()
        {
            if (tileData == null && isGrass && isNearRoad && !isRoad) 
            {
                isBuildable = true;
            }
        }
    }

    public class IntersectingNode
    {
        public Point coords { get; set; }
        public List<Edge> connectedEdges { get; set; } = new();

        public IntersectingNode() { }
        public IntersectingNode(Point coords)
        {
            this.coords = coords;
        }


        public bool Equals(IntersectingNode? other)
        {
            if (other is null) return false;
            return coords == other.coords;
        }

        public override bool Equals(object? obj) => Equals(obj as IntersectingNode);

        public override int GetHashCode() => coords.GetHashCode();

        public static bool operator ==(IntersectingNode? left, IntersectingNode? right) =>
            object.ReferenceEquals(left, right) ? true : (left?.Equals(right) ?? false);

        public static bool operator !=(IntersectingNode? left, IntersectingNode? right) => !(left == right);
    }
}
