using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public class Node
    {
        public Point coords;
        public int rnd;
        public Building tiledata; //if it has a building on it
        public bool isNearRoad;
        public int nodeNumber;
        public bool isBuildable; //if it is buildable or not
        public bool isGrass;

        public Node(Point coordsin, Building tiledatain, bool isNearRoadIn, int nodeNumber)
        {
            coords = coordsin;
            tiledata = tiledatain;
            isNearRoad = isNearRoadIn;
            this.nodeNumber = nodeNumber;
            IsNodeBuildable();
        }
        //2 constructors cuz why not
        public Node(int xcor, int ycor, Building tiledatain, bool isNearRoadIn, int nodeNumber, bool isGrass)
        {
            coords = new Point(xcor, ycor);
            tiledata = tiledatain;
            this.nodeNumber = nodeNumber;
            isNearRoad = isNearRoadIn;
            this.isGrass = isGrass;
            IsNodeBuildable();
        }

        public void IsNodeBuildable()
        {
            if (tiledata == null && isGrass && isNearRoad) 
            {
                isBuildable = true;
            }
        }
    }

    public class IntersectingNode
    {
        public Point coords;
        public List<Edge> connectedEdges = new List<Edge>();
        public IntersectingNode(Point coordsIn)
        {
            coords = coordsIn;
        }
    }
}
