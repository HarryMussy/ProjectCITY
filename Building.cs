using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CitySkylines0._5alphabeta
{
    public abstract class Building
    {
        public Point coords { get; set; }
        public Size size { get; set; }
        public string type { get; set; }
        public List<Node> occupyingNodes { get; set; } = new();
        public List<Necessity> necessities { get; set; } = new();

        public virtual int cost { get; set; }
        public virtual int tax { get; set; }

        public Building() { } // required for JSON

        public Building(Size size, Point coords, string type, float energyDemand, float waterDemand)
        {
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodes = new List<Node>();
            this.cost = 0;
            necessities = [new Electricity(energyDemand), new Water(waterDemand)];
        }
    }

    public class House : Building
    {
        float energyDemand { get; set; }
        float waterDemand { get; set; }
        public House() { } //required
        public House(Size size, Point coords, string type, float energyDemand, float waterDemand) : base(size, coords, type, energyDemand, waterDemand)
        {
            type = "house";
            cost = 10000;
            tax = 5;
            this.energyDemand = energyDemand;
            this.waterDemand = waterDemand;
        }
    }

    public class PowerPlant : Building
    {
        float energyDemand { get; set; }
        float waterDemand { get; set; }
        public PowerPlant() { } //required
        public PowerPlant(Size size, Point coords, string type, float energyDemand, float waterDemand) : base(size, coords, type, energyDemand, waterDemand)
        {
            type = "powerplant";
            cost = 50000;
            tax = 20;
            this.energyDemand = energyDemand;
            this.waterDemand = waterDemand;
        }
    }

    public class WaterPump : Building
    {
        float energyDemand { get; set; }
        float waterDemand { get; set; }
        public WaterPump() { } //required
        public WaterPump(Size size, Point coords, string type, float energyDemand, float waterDemand) : base(size, coords, type, energyDemand, waterDemand)
        {
            type = "waterpump";
            cost = 20000;
            tax = 20;
            this.energyDemand = energyDemand;
            this.waterDemand = waterDemand;
        }
    }
}
