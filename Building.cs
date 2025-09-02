using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public abstract class Building
    {
        public virtual int powerusage { get; } //in kWh
        public Size size;
        public Point coords;
        public string type; //e.g. factory, house
        public List<Node> occupyingNodes;
        public virtual int cost { get; } = 0;
        public virtual int tax { get; } = 0;

        public Building(int powerusage, Size size, Point coords, string type)
        {
            this.powerusage = powerusage;
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodes = new List<Node>();
            this.cost = 0;
        }
    }

    public class House : Building
    {
        public override int powerusage { get; } = 15;
        public override int cost { get; } = 10000;
        public override int tax { get; } = 5;
        public House(int powerusage, Size size, Point coords, string type) : base(powerusage, size, coords, type)
        {
            this.type = "house";
        }
    }

    public class LoggingFactory : Building
    {
        public override int powerusage { get; } = 150;
        public LoggingFactory(int powerusage, Size size, Point coords, string type) : base(powerusage, size, coords, type)
        {
        }
    }

}
