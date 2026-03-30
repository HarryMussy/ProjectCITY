using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Controls;


namespace CitySkylines0._5alphabeta
{
    public class Building
    {
        public Point coords { get; set; }
        public Size size { get; set; }
        public string type { get; set; }
        public List<int> occupyingNodesIndex { get; set; } = new();
        public List<Necessity> necessities { get; set; } = new();
        public int MaxOccupants { get; set; }
        public Person[] Occupants { get; set; }

        [JsonIgnore] public float efficiency;
        public string imagePath { get; set; }

        public virtual int cost { get; set; }
        public virtual int tax { get; set; }
        public bool isInCrime { get; set; }
        public bool isOnFire { get; set; }
        public double timeUntilAbandoned { get; set; }
        public bool isAbandoned { get; set; }
        private static Random random = new Random();

        public Building() { } // required for JSON

        //building assembler
        public Building(Size size, Point coords, string type, float powerDemand, float waterDemand, int MaxOccupants)
        {
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodesIndex = new List<int>();
            this.cost = 0;
            necessities = [new Power(powerDemand), new Water(waterDemand)];
            this.MaxOccupants = MaxOccupants;
            Occupants = new Person[MaxOccupants];
            efficiency = 0;
            isInCrime = false;
            isAbandoned = false;
            timeUntilAbandoned = 0;
        }

        public Building(Size size, Point coords, string type, float powerDemand, float waterDemand, int MaxOccupants, bool b) //end bool dictates if the building needs a workforce
        {
            this.size = size;
            this.coords = coords;
            this.type = type;
            occupyingNodesIndex = new List<int>();
            this.cost = 0;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
            this.MaxOccupants = MaxOccupants;
            Occupants = new Person[MaxOccupants];
            efficiency = 0;
        }

        //assigns new occupants to the building after loading a save if Occupants wasnt saved
        public void InitializeAfterLoad()
        {
            if (Occupants == null)
            {
                Occupants = new Person[MaxOccupants];
            }
        }

        //updates the building- applies affects to it if it meets a chance
        public void UpdateBuilding(double timeElapsed)
        {
            if (this.type != "fireservice") //fireservice stations cannot be set on fire
            {
                isOnFire = random.Next(25000) == 67; //chance of being on fire
            }

            if (this.type != "policebuilding" && this.type != "hospital" && this.type != "fireservice") //essential buildings cannot be robbed
            {
                isInCrime = random.Next(12500) == 67; //chance of being robbed
            }

            bool isNecessityUnfulfilled = false;
            foreach (Necessity n in necessities)
            {
                if (n.type != "Health" && (!n.fulFilled || isInCrime))
                {
                    isNecessityUnfulfilled = true;
                    break;
                }
            }

            //if the building is on fire, the time it takes to be abandoned is reduced
            if (isOnFire && isNecessityUnfulfilled)
            {
                timeUntilAbandoned += timeElapsed * 2;
            }

            //otherwise, regular progression
            if (isNecessityUnfulfilled)
            {
                timeUntilAbandoned += timeElapsed;
            }
            
            //after 100 seconds the building is abandoned
            if (timeUntilAbandoned >= 100f)
            {
                isAbandoned = true;
            }
        }
    }

    public class House : Building
    {
        float energyDemand { get; set; }
        float waterDemand { get; set; }
        public House() { } //required for json

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
        public PowerPlant() { } //required for json
        public PowerPlant(Size size, Point coords, string type, float powerDemand, float waterDemand) : base(size, coords, type, powerDemand, waterDemand, 35, true)
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
        public WaterPump() { } //required for json
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

    public class Shop : Building
    {
        float powerDemand { get; set; }
        float waterDemand { get; set; }
        public Shop() { } //required for json
        public Shop(Size size, Point coords, string type, float powerDemand, float waterDemand) : base(size, coords, type, powerDemand, waterDemand, 5, true)
        {
            type = "shop";
            cost = 30000;
            tax = 50;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
        }
    }

    public class Factory : Building
    {
        float powerDemand { get; set; }
        float waterDemand { get; set; }
        public Factory() { } //required for json
        public Factory(Size size, Point coords, string type, float powerDemand, float waterDemand) : base(size, coords, type, powerDemand, waterDemand, 30, true)
        {
            type = "shop";
            cost = 50000;
            tax = 50;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
        }
    }

    public class Hospital : Building
    {
        Ambulance[] ambulances; //available ambulances

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
            //create 3 ambulances per hospital
            ambulances =
            [
                new Ambulance(null, 6f, null, path, "ambulance", null),
                new Ambulance(null, 6f, null, path, "ambulance", null),
                new Ambulance(null, 6f, null, path, "ambulance", null)
            ];
            ambulances[0].hasPriority = true;
            ambulances[1].hasPriority = true;
            ambulances[2].hasPriority = true;
        }

        //upon reloading a save, reassign [JSONIGNORE] properties and remake ambulances
        public void Reconnect(Grid gridIn, CarManager carManagerIn)
        {
            grid = gridIn;
            carManager = carManagerIn;

            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string path = Path.Combine(root, "gameAssets", "gameArt", "Service Vehicles", "ambulance.png");
            ambulances =
            [
                new Ambulance(null, 6f, null, path, "ambulance", null),
                new Ambulance(null, 6f, null, path, "ambulance", null),
                new Ambulance(null, 6f, null, path, "ambulance", null)
            ];
            ambulances[0].hasPriority = true;
            ambulances[1].hasPriority = true;
            ambulances[2].hasPriority = true;
        }

        //called every tick to update every hospital
        public void UpdateHospital()
        {
            //for every house that has occupants that arent healthy
            foreach (Building h in grid.buildings.Where(b => b.type == "house")) 
            {
                foreach (Person p in h.Occupants.Where(p => p != null)) 
                {
                    if (p.IsHealthy == false) 
                    {
                        for (int i = 0; i < ambulances.Length; i++)
                        {
                            //find an ambulance that is not in service and send it to a house
                            if (!ambulances[i].inService && ambulances.Where(e => e.destBuilding == h).Count() == 0)
                            {
                                ambulances[i].speed = 6f;
                                SendAmbulanceToBuilding(ambulances[i], h);
                                ambulances[i].inService = true;
                                ambulances[i].destBuilding = h;
                                break;
                            }
                        }
                    }
                }
            }

            foreach (EmergencyServiceVehicle a in ambulances)
            {
                if (a.inService && !a.isMoving)
                {
                    SendAmbulanceToBuilding(a, this); //send ambulance to house
                    //make everyone in the ill house healthy
                    foreach (Person p in a.destBuilding.Occupants.Where(p => p != null))
                    {
                        p.IsHealthy = true;
                    }
                    a.inService = false;
                }

                //bring ambulance back
                if (!a.inService && !a.isMoving && a.destBuilding != null)
                {
                    a.speed = 3f;
                    a.isMoving = true;
                    BringAmbulanceFromBuilding(a, a.destBuilding);
                }
            }
        }
        public void SendAmbulanceToBuilding(EmergencyServiceVehicle ambulance, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(ambulance, this, building);
        }

        public void BringAmbulanceFromBuilding(EmergencyServiceVehicle ambulance, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(ambulance, building, this);
        }
    }

    //police
    public class PoliceBuilding : Building
    {
        PoliceCar[] PoliceCars;

        [JsonIgnore] Grid grid;
        [JsonIgnore] CarManager carManager;
        float powerDemand { get; set; }
        float waterDemand { get; set; }

        [JsonIgnore] Random random = new Random();

        public PoliceBuilding() { }
        public PoliceBuilding(Size size, Point coords, string type, float powerDemand, float waterDemand, Grid gridIn, CarManager carManagerIn) : base(size, coords, type, powerDemand, waterDemand, 50, true)
        {
            type = "policebuilding";
            cost = 100000;
            tax = -10;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
            grid = gridIn;
            carManager = carManagerIn;
            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string path = Path.Combine(root, "gameAssets", "gameArt", "Service Vehicles", "policecar.png");
            PoliceCars =
            [
                new PoliceCar(null, 6f, null, path, "policecar", null),
                new PoliceCar(null, 6f, null, path, "policecar", null)
            ];
            PoliceCars[0].hasPriority = true;
            PoliceCars[1].hasPriority = true;
        }

        public void Reconnect(Grid gridIn, CarManager carManagerIn)
        {
            grid = gridIn;
            carManager = carManagerIn;

            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string path = Path.Combine(root, "gameAssets", "gameArt", "Service Vehicles", "policecar.png");
            PoliceCars =
            [
                new PoliceCar(null, 6f, null, path, "policecar", null),
                new PoliceCar(null, 6f, null, path, "policecar", null)
            ];
            PoliceCars[0].hasPriority = true;
            PoliceCars[1].hasPriority = true;
        }

        public void UpdatePoliceBuilding()
        {
            foreach (Building h in grid.buildings.Where(b => b.type == "house"))
            {
                if (h.isInCrime == true)
                {
                    for (int i = 0; i < PoliceCars.Length; i++)
                    {
                        if (!PoliceCars[i].inService && PoliceCars.Where(e => e.destBuilding == h).Count() == 0)
                        {
                            PoliceCars[i].speed = 6f;
                            SendPoliceCarToBuilding(PoliceCars[i], h);
                            PoliceCars[i].inService = true;
                            PoliceCars[i].destBuilding = h;
                            break;
                        }
                    }
                }
            }

            foreach (PoliceCar pc in PoliceCars)
            {
                if (pc.inService && !pc.isMoving)
                {
                    SendPoliceCarToBuilding(pc, this);
                    pc.destBuilding.isInCrime = false;
                    pc.inService = false;
                }

                if (!pc.inService && !pc.isMoving && pc.destBuilding != null)
                {
                    pc.speed = 3f;
                    pc.isMoving = true;
                    BringPoliceCarFromBuilding(pc, pc.destBuilding);
                }
            }
        }
        public void SendPoliceCarToBuilding(EmergencyServiceVehicle policeCar, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(policeCar, this, building);
        }

        public void BringPoliceCarFromBuilding(EmergencyServiceVehicle policeCar, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(policeCar, building, this);
        }
    }

    //fire service
    public class FireService : Building
    {
        FireTruck[] fireTrucks;
        [JsonIgnore] Random random = new Random();
        [JsonIgnore] Grid grid;
        [JsonIgnore] CarManager carManager;
        float powerDemand { get; set; }
        float waterDemand { get; set; }

        public FireService() { }
        public FireService(Size size, Point coords, string type, float powerDemand, float waterDemand, Grid gridIn, CarManager carManagerIn) : base(size, coords, type, powerDemand, waterDemand, 50, true)
        {
            type = "fireservice";
            cost = 75000;
            tax = -15;
            this.powerDemand = powerDemand;
            this.waterDemand = waterDemand;
            necessities = [new Power(powerDemand), new Water(waterDemand), new Workers(0)];
            grid = gridIn;
            carManager = carManagerIn;
            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string path = Path.Combine(root, "gameAssets", "gameArt", "Service Vehicles", "firetruck.png");
            fireTrucks =
            [
                new FireTruck(null, 6f, null, path, "firetruck", null),
                new FireTruck(null, 6f, null, path, "firetruck", null)
            ];
            fireTrucks[0].hasPriority = true;
            fireTrucks[1].hasPriority = true;
        }

        public void Reconnect(Grid gridIn, CarManager carManagerIn)
        {
            grid = gridIn;
            carManager = carManagerIn;

            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string path = Path.Combine(root, "gameAssets", "gameArt", "Service Vehicles", "firetruck.png");
            fireTrucks =
            [
                new FireTruck(null, 6f, null, path, "firetruck", null),
                new FireTruck(null, 6f, null, path, "firetruck", null)
            ];
            fireTrucks[0].hasPriority = true;
            fireTrucks[1].hasPriority = true;
        }

        public void UpdateFireService()
        {
            foreach (Building h in grid.buildings.Where(b => b.type == "house"))
            {
                if (h.isOnFire)
                {
                    for (int i = 0; i < fireTrucks.Length; i++)
                    {
                        if (!fireTrucks[i].inService && fireTrucks.Where(e => e.destBuilding == h).Count() == 0)
                        {
                            fireTrucks[i].speed = 6f;
                            SendFireTruckToBuilding(fireTrucks[i], h);
                            fireTrucks[i].inService = true;
                            fireTrucks[i].destBuilding = h;
                            break;
                        }
                    }
                } 
            }

            foreach (FireTruck ft in fireTrucks)
            {
                if (ft.inService && !ft.isMoving)
                {
                    SendFireTruckToBuilding(ft, this);
                    ft.destBuilding.isOnFire = false;
                    ft.inService = false;
                }

                if (!ft.inService && !ft.isMoving && ft.destBuilding != null)
                {
                    ft.speed = 3f;
                    ft.isMoving = true;
                    BringFireTruckFromBuilding(ft, ft.destBuilding);
                }
            }
        }
        public void SendFireTruckToBuilding(EmergencyServiceVehicle fireTruck, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(fireTruck, this, building);
        }

        public void BringFireTruckFromBuilding(EmergencyServiceVehicle fireTruck, Building building)
        {
            carManager.SendSpecificCarToAndFromSpecificBuilding(fireTruck, building, this);
        }
    }

}
