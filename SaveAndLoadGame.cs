using System;
using System.Collections.Generic;

namespace CitySkylines0._5alphabeta
{
    public class SaveData
    {
        public float Cash { get; set; }
        public List<SaveEdge> Edges { get; set; }
        public List<SaveBuilding> Buildings { get; set; }
        public SaveCalendar Calendar { get; set; }
    }

    public class SaveEdge
    {
        public int Weight { get; set; }
        public string Name { get; set; }
        public Point A { get; set; }
        public Point B { get; set; }
    }

    public class SaveBuilding
    {
        public string Type { get; set; }
        public Point Coords { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class SaveCalendar
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
    }


}
