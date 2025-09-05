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

        public NecessitiesManager()
        {
            globalElectricityDemand = 0;
            globalWaterDemand = 0;
            globalElectricitySupply = 0;
            globalWaterSupply = 0;

            globalElectricityStatus = $"{globalElectricitySupply} / {globalElectricityDemand}MW";
            globalWaterStatus = $"{globalWaterSupply} / {globalWaterDemand}L";
        }

        public void UpdateGlobalNecessities(Building b)
        {
            foreach (Necessity necessity in b.necessities)
            {
                if(necessity.name is "Energy")
                {
                    globalElectricityDemand += (int)necessity.demand;
                }
                else if(necessity.name is "Water")
                {
                    globalWaterDemand += (int)necessity.demand;
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
        public Electricity(float demandIN) : base("Energy", 0, 120, demandIN) { }
    }

    public class Water : Necessity
    {
        public Water(float demandIN) : base("Water", 0, 60, demandIN) { }
    }
}
