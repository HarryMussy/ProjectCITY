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
        public int Gender { get; set; }
        public bool IsAlive { get; set; }
        public bool IsHealthy { get; set; }
        public Building Residence {  get; set; }
        public Building WorkPlace { get; set; }
        public Person() { }

        public Person(Building b)
        {
            Age = new Random().Next(1, 100);
            Residence = b;
            WorkPlace = null;
        }
    }

    public class PopulationManager
    {
        public List<Person> Population { get; set; }

        [JsonIgnore] public Grid grid;
        [JsonIgnore] List<Building> possibleWorkplaces = new List<Building>();

        public PopulationManager() { }
        public PopulationManager(Grid grid)
        {
            this.grid = grid;
            Population = new List<Person>();
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
