using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;using System.Diagnostics;
using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class Node
    {
        public Point coords { get; set; }
        [JsonIgnore] public Car OccupyingCar { get; set; } = null;
        public int laneIndex { get; set; } = 0; //0 is one way 1 is the other
/*        [JsonIgnore] public Edge parentEdge { get; set; }*/
        [JsonIgnore] public HashSet<Node> neighbors { get; set; } = new();
        public bool hasTileData { get; set; }
        public int nodeNumber { get; set; }
        public bool isGrass { get; set; }

        public List<Point> allowedDirs = new();
        public bool isNearRoad { get; set; }
        public bool isRoad { get; set; }
        public bool isBuildable { get; set; }

        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
        [JsonIgnore] public Node parent { get; set; }
        public Dictionary<string, string> seasonalImagePaths { get; set; } = new();

        public string imagePath { get; set; }

        public Node() { } // required
        public Node(Point coords, bool isTileData, bool near, bool isRoad, bool isGrass, int nodeNumberIn)
        {
            this.coords = coords;
            this.hasTileData = isTileData;
            isNearRoad = near;
            this.isRoad = isRoad;
            this.isGrass = isGrass;
            nodeNumber = nodeNumberIn;
            IsNodeBuildable();
        }

        public void IsNodeBuildable()
        {
            if (!hasTileData && isGrass && isNearRoad && !isRoad) 
            {
                isBuildable = true;
            }
        }

        public Point Center(int rectSize)
        {
            return new Point(coords.X + rectSize / 2, coords.Y + rectSize / 2);
        }
    }
}