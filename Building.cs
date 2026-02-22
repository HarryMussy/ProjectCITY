using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.IO;


namespace CitySkylines0._5alphabeta
{
    public class Building
    {
        public Point coords { get; set; }
        public Size size { get; set; }
        public string type { get; set; }
        public List<Node> occupyingNodes { get; set; } = new();
        public List<Necessity> necessities { get; set; } = new();
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
        EmergencyServiceVehicle[] ambulances;

        [JsonIgnore] Grid grid;
        [JsonIgnore] CarManager carManager;
        float powerDemand { get; set; }
        float waterDemand { get; set; }

        public Hospital() { }
        public Hospital(Size size, Point coords, string type, float powerDemand, float waterDemand, Grid gridIn, CarManager carManagerIn) : base(size, coords, type, powerDemand, waterDemand, 50, true)
        {
            type = "hospital";
            cost = 50000;
            tax = -10;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
            grid = gridIn;
            carManager = carManagerIn;
            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string path = Path.Combine(root, "gameAssets", "gameArt", "Service Vehicles", "ambulance.png");
            ambulances = 
            [
                new EmergencyServiceVehicle(null, 6, null, path, "ambulance", null), 
                new EmergencyServiceVehicle(null, 6, null, path, "ambulance", null), 
                new EmergencyServiceVehicle(null, 6, null, path, "ambulance", null)
            ];
        }

        public void UpdateHospital()
        {
            foreach (House h in grid.buildings.Where(b => b.type == "house"))
            {
                foreach (Person p in h.Occupants.Where(p => p != null))
                {
                    if (p.IsHealthy == false)
                    {
                        for (int i = 0; i < ambulances.Length; i++)
                        {
                            if (!ambulances[i].inService)
                            {
                                SendAmbulanceToBuilding(ambulances[i], h);
                                ambulances[i].inService = true;
                                ambulances[i].destBuilding = h;
                            }
                        }
                    }
                }
            }

            foreach (EmergencyServiceVehicle a in ambulances)
            {
                if (a.inService && !a.isMoving)
                {
                    SendAmbulanceToBuilding(a, this);
                    foreach (Person p in a.destBuilding.Occupants.Where(p => p != null))
                    {
                        p.IsHealthy = true;
                    }
                }
            }
        }
        public void SendAmbulanceToBuilding(EmergencyServiceVehicle ambulance, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(ambulance, this, building);
        }
    }

}
