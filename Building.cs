using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public abstract class Building
    {
        public List<Necessity> necessities;
        public Size size;
        public Point coords;
        public string type; //e.g. factory, house
        public List<Node> occupyingNodes;
        public virtual int cost { get; } = 0;
        public virtual int tax { get; } = 0;

        public Building(Size size, Point coords, string type, int energyDemand, int waterDemand)
        {
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodes = new List<Node>();
            this.cost = 0;
            necessities = [ new Electricity(energyDemand), new Water(waterDemand) ];
        }
    }

    public class House : Building
    {
        public override int cost { get; } = 10000;
        public override int tax { get; } = 5;
        public House(Size size, Point coords, string type) : base(size, coords, type, 2, 150)
        {
            this.type = "house";
        }
    }
}
