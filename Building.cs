using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.Json.Serialization;

namespace CitySkylines0._5alphabeta
{
    public abstract class Building
    {
        public Point coords { get; set; }
        public Size size { get; set; }
        public string type { get; set; }
        [JsonIgnore] public List<Node> occupyingNodes { get; set; } = new();
        [JsonIgnore] public List<Necessity> necessities { get; set; } = new();
        public int MaxOccupants { get; set; }
        public Person[] Occupants { get; set; }

        [JsonIgnore] public float efficiency;

        public virtual int cost { get; set; }
        public virtual int tax { get; set; }

        public Building() { } // required for JSON

        public Building(Size size, Point coords, string type, float powerDemand, float waterDemand, int MaxOccupants)
        {
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodes = new List<Node>();
            this.cost = 0;
            necessities = [new Power(powerDemand), new Water(waterDemand)];
            this.MaxOccupants = MaxOccupants;
            Occupants = new Person[MaxOccupants];
            efficiency = 0;
        }

        public Building(Size size, Point coords, string type, float powerDemand, float waterDemand, int MaxOccupants, bool b) //end bool dictates if the building needs a workforce
        {
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodes = new List<Node>();
            this.cost = 0;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
            this.MaxOccupants = MaxOccupants;
            Occupants = new Person[MaxOccupants];
            efficiency = 0;
        }
    }

    public class House : Building
    {
        float energyDemand { get; set; }
        float waterDemand { get; set; }
        public House() { } //required

        public House(Size size, Point coords, string type, float energyDemand, float waterDemand) : base(size, coords, type, energyDemand, waterDemand, 5)
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
        public PowerPlant(Size size, Point coords, string type, float powerDemand, float waterDemand) : base(size, coords, type, powerDemand, waterDemand, 75, true)
        {
            type = "powerplant";
            cost = 50000;
            tax = 20;
            this.energyDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
        }
    }

    public class WaterPump : Building
    {
        float powerDemand { get; set; }
        float waterDemand { get; set; }
        public WaterPump() { } //required
        public WaterPump(Size size, Point coords, string type, float powerDemand, float waterDemand) : base(size, coords, type, powerDemand, waterDemand, 25, true)
        {
            type = "waterpump";
            cost = 20000;
            tax = 20;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
        }
    }

    public class Hospital : Building
    {
        Car ambulance { get; set; }
        float powerDemand { get; set; }
        float waterDemand { get; set; }

        public Hospital() { }
        public Hospital(Size size, Point coords, string type, float powerDemand, float waterDemand) : base(size, coords, type, powerDemand, waterDemand, 50, true)
        {
            type = "hospital";
            cost = 50000;
            tax = -10;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
        }
    }
}
