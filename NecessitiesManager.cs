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
            b.necessities.ForEach(n =>
            {
                if (n.name == "Electricity")
                {
                    globalElectricityDemand += (int)n.demand;
                }
                else if (n.name == "Water")
                {
                    globalWaterDemand += (int)n.demand;
                }
            });
        }
    }

    public class Necessity
    {
        public string name { get; private set; }
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
        public Electricity(float demandIN) : base("Electricity", 0, 120, demandIN) { }
    }

    public class Water : Necessity
    {
        public Water(float demandIN) : base("Water", 0, 60, demandIN) { }
    }
}
