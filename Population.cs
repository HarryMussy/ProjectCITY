using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public class Person
    {
        public int Age {  get; set; }
        public bool IsAlive { get; set; }
        public bool IsHealthy { get; set; }
        public bool IsPregnant { get; set; }
        public int MonthTimer { get; set; }
        public bool IsMale { get; set; }
        public float WellBeing { get; set; } = 100f;   // NEW
        public List<string> UnmetDesires { get; set; } = new(); // NEW
        [JsonIgnore] public Building Residence {  get; set; }
        [JsonIgnore] public Building WorkPlace { get; set; }
        public Person() { }

        public Person(Building b)
        {
            Age = new Random().Next(1, 100);
            IsMale = new Random().Next(2) == 1;
            Residence = b;
            WorkPlace = null;
            IsHealthy = true;
            IsPregnant = false;
            IsAlive = true;
        }

        public Person(Building b, int Age) //babies
        {
            this.Age = Age;
            IsMale = new Random().Next(2) == 1;
            Residence = b;
            WorkPlace = null;
            IsHealthy = true;
            IsPregnant = false;
            IsAlive = true;
        }
    }

    public class PopulationManager
    {
        public List<Person> Population { get; set; }

        [JsonIgnore] public float AverageWellBeing;
        [JsonIgnore] public Dictionary<string, int> GlobalDesires = new();
        [JsonIgnore] public Grid grid;
        [JsonIgnore] List<Building> possibleWorkplaces = new List<Building>();
        [JsonIgnore] Random rng;

        public PopulationManager() { }
        public PopulationManager(Grid grid)
        {
            this.grid = grid;
            rng = new Random();
            Population = new List<Person>();
        }

        public void UpdateWellBeing()
        {
            if (Population.Count == 0) return;

            GlobalDesires.Clear();
            float total = 0f;

            bool hospitalExists = grid.buildings.Any(b => b.type == "hospital");

            foreach (Person p in Population)
            {
                if (!p.IsAlive) continue;

                float well = 100f;
                p.UnmetDesires.Clear();

                //check house necessities
                if (p.Residence != null)
                {
                    foreach (Necessity n in p.Residence.necessities)
                    {
                        if (!n.fulFilled)
                        {
                            well -= 20f;
                            p.UnmetDesires.Add(n.GetType().Name);
                        }
                    }
                }

                if (!p.IsHealthy)
                {
                    well -= 15f;
                    p.UnmetDesires.Add("Health");
                }

                if (p.WorkPlace == null && p.Age >= 18)
                {
                    well -= 10f;
                    p.UnmetDesires.Add("Job");
                }

                well = Math.Clamp(well, 0f, 100f);
                p.WellBeing = well;
                total += well;

                //track global desires
                foreach (string desire in p.UnmetDesires)
                {
                    if (!GlobalDesires.ContainsKey(desire))
                        GlobalDesires[desire] = 0;

                    GlobalDesires[desire]++;
                }
            }

            AverageWellBeing = total / Population.Count;
        }

        public void MakePregnant(Person person)
        {
            person.IsPregnant = true;
            person.MonthTimer = 0;
        }

        public void UpdatePopulationByMonth() //called when one month has passed, for events
        {
            foreach (Person person in Population.Where(p => p.Age > 65 || !p.IsHealthy)) //anyone over the age of 65 or is ill has a 1% chance of dying
            {
                person.IsAlive = rng.Next(100) == 67;
            }

            Person male = new();
            Person female = new();
            foreach (Building b in grid.buildings.Where(h => h.type == "house"))
            {
                if (b.Occupants.Count() > 0)
                {
                    foreach (Person p in b.Occupants)//all houses with a male and a female over 18 have a chance of the female becoming pregnant
                    {
                        if (p == null) { return; }
                        if (p.Age >= 18 && p.IsMale && male == null) { male = p; }
                        if (p.Age >= 18 && !p.IsMale && female == null) { female = p; }
                    }

                    if (male != null && female != null)
                    {
                        if (rng.Next(10) <= 7)
                        {
                            MakePregnant(female);
                        }
                    }
                } 
            }

            foreach (Person pregnantWoman in Population.Where(p => p.IsPregnant && p.MonthTimer == 9)) //make all pregnant women at 9 months give birth
            {
                pregnantWoman.IsPregnant = false;
                pregnantWoman.MonthTimer = 0;

                Person baby = new Person(pregnantWoman.Residence, 0);
                Population.Add(baby);
            }
        }

        public void UpdatePopulationByYear() //called once a year, only used to age up
        {
            foreach (Person person in Population)
            {
                person.Age++;
            }
        }

        public void UpdatePopulation()
        {
            possibleWorkplaces.Clear();
            Random rng = new Random();

            //populate houses with people
            foreach (Building b in grid.buildings)
            {
                if (b.type is "house" && b.Occupants.Count(p => p != null) == 0)
                {
                    int addToPop = rng.Next(1, b.MaxOccupants);
                    for (int i = 0; i < addToPop; i++)
                    {
                        Person newPerson = new Person(b);
                        b.Occupants[i] = newPerson;
                        Population.Add(newPerson);
                    }
                }

                foreach (Person p in b.Occupants) //1 in a hundred thou chance of being unhealthy
                {
                    if (p != null && p.IsHealthy)
                    {
                        p.IsHealthy = !(rng.Next(100000) == 1);
                    }
                }
            }

            //add to possible workplaces
            foreach (Building b in grid.buildings) 
            {
                if (b.type != "house" && b.Occupants.Count(p => p != null) < b.MaxOccupants) 
                {
                    possibleWorkplaces.Add(b);
                }
            }

            //assign over 18's jobs
            foreach (Person p in Population)
            {
                if (p.WorkPlace == null && p.Age >= 18 && possibleWorkplaces.Count > 0)
                {
                    Building job = possibleWorkplaces[rng.Next(possibleWorkplaces.Count)];
                    p.WorkPlace = job;

                    for (int i = 0; i < job.Occupants.Length; i++)
                    {
                        if (job.Occupants[i] == null)
                        {
                            job.Occupants[i] = p;
                            break;
                        }
                    }
                }
            }
        }
    }
}
