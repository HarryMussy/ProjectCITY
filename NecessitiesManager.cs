using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public class NecessitiesManager
    {
        public int globalElectricitySupply;
        public int globalWaterSupply;

        public int globalElectricityDemand;
        public int globalWaterDemand;

        public string globalElectricityStatus;
        public string globalWaterStatus;

        public Grid grid;

        public NecessitiesManager(Grid grid)
        {
            globalElectricityDemand = 0;
            globalWaterDemand = 0;
            globalElectricitySupply = 0;
            globalWaterSupply = 0;

            globalElectricityStatus = $"{globalElectricitySupply} / {globalElectricityDemand}MW";
            globalWaterStatus = $"{globalWaterSupply} / {globalWaterDemand}L";

            this.grid = grid;   
        }

        public void UpdateGlobalNecessities()
        {
            foreach (Building b in grid.buildings)
            {
                foreach (Necessity necessity in b.necessities)
                {
                    if (necessity.name is "Energy")
                    {
                        if (b.type == "windfarm") //if it's a windfarm
                        {
                            globalElectricitySupply += (int)-necessity.demand;
                            necessity.fulFilled = true;
                        }
                        else
                        {
                            globalElectricityDemand += (int)necessity.demand;
                        }
                    }
                    else if (necessity.name is "Water")
                    {
                        if (b.type == "waterpump") //if it's a water pump
                        {
                            globalWaterSupply += (int)(-necessity.demand);
                        }
                        else
                        {
                            globalWaterDemand += (int)necessity.demand;
                        }

                    }
                }
            }

            foreach (Building b in grid.buildings)
            {
                foreach (Necessity necessity in b.necessities)
                {
                    if (necessity.name is "Energy")
                    {
                        if (globalElectricitySupply < globalElectricityDemand && b.type != "windfarm")
                        {
                            necessity.fulFilled = false;
                        }
                        else
                        {
                            necessity.fulFilled = true;
                        }
                    }
                    else if (necessity.name is "Water")
                    {

                        if (globalWaterSupply < globalWaterDemand)
                        {
                            necessity.fulFilled = false;
                        }
                        else
                        {
                            necessity.fulFilled = true;
                        }
                    }
                }
            }

            globalElectricityStatus = $"{globalElectricitySupply} / {globalElectricityDemand}MW";
            globalWaterStatus = $"{globalWaterSupply} / {globalWaterDemand}L";
        }
            
    }

    public class Necessity
    {
        public string name { get; set; }
        public float value { get; private set; }
        public float demand { get; private set; }
        public float decayRate { get; private set; } //the time it takes (in seconds) for the building to be abandoned
        public bool fulFilled;
        public Necessity() { }

        public Necessity(string nameIN, float initialValueIN, float decayRateIN, float demandIN)
        {
            name = nameIN;
            value = initialValueIN;
            decayRate = decayRateIN;
            demand = demandIN;
        }
    }

    public class Electricity : Necessity
    {
        public Electricity() { }
        public Electricity(float demandIN) : base("Energy", 0, 120, demandIN) { }
    }

    public class Water : Necessity
    {
        public Water() { }
        public Water(float demandIN) : base("Water", 0, 60, demandIN) { }
    }

}
