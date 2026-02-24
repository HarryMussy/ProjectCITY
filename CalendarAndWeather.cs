using System.Text.Json.Serialization;

namespace CitySkylines0._5alphabeta
{
    public class Calendar
    {
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        [JsonIgnore]
        public string date => day + "/" + month + "/" + year;

        public int hour { get; set; }
        public int minute { get; set; }

        [JsonIgnore]
        public string time
        {
            get
            {
                if (minute < 10) return hour + ":0" + minute;
                else return hour + ":" + minute;
            }
        }

        [JsonIgnore] public Form1 form1PassIn;
        [JsonIgnore] private double timeToAdvanceMinute = 100; //in milliseconds
        [JsonIgnore] private double timeToAdvanceDay = 1; //in seconds
        [JsonIgnore] private int advanceMinutes = 1; //how much to advance minutes by

        [JsonIgnore]
        public string CurrentSeason;


        // Must be serialized or time will jump after load
        public double elapsedMilliSeconds { get; set; }
        public double elapsedSeconds { get; set; }

        // configuration for visual alpha ranges
        private const int MaxAlpha = 150;

        // REQUIRED for JSON
        public Calendar() { }

        // Your existing constructor
        public Calendar(int dayIn, int monthIn, int yearIn, int hourIn, int minuteIn, Form1 form1)
        {
            day = dayIn;
            month = monthIn;
            year = yearIn;
            hour = hourIn;
            minute = minuteIn;
            elapsedMilliSeconds = 0;
            elapsedSeconds = 0;
            form1PassIn = form1;
        }

        public int GetHour() => hour;

        // Return a fade factor 0-1 depending on how close we are to season end
        public float GetSeasonTransitionFactor()
        {
            int monthLength = DaysInMonth(month);
            int fadeStart = monthLength - 7; // last 7 days of season
            if (day <= fadeStart) return 0f;

            return (float)(day - fadeStart) / 7f;
        }

        public void UpdateCurrentSeason()
        {
            if (month == 12 || month == 1 || month == 2)
            {
                CurrentSeason = "Winter";
            }
            else if (month >= 3 && month <= 5)
            {
                CurrentSeason = "Spring";
            }
            else if (month >= 6 && month <= 8)
            {
                CurrentSeason = "Summer";
            }
            else
            {
                CurrentSeason = "Autumn";
            }
        }

        public string GetCurrentSeason(int month)
        {
            if (month == 12 || month == 1 || month == 2)
            {
                return "Winter";
            }
            else if (month >= 3 && month <= 5)
            {
                return "Spring";
            }
            else if (month >= 6 && month <= 8)
            {
                return "Summer";
            }
            else
            {
                return "Autumn";
            }
        }

        public void AdvanceTime(double elapsed)
        {
            elapsedMilliSeconds += elapsed;
            elapsedSeconds += elapsed / 1000;

            if (elapsedMilliSeconds >= timeToAdvanceMinute)
            {
                minute += advanceMinutes;

                if (minute >= 60)
                {
                    hour += 1;
                    minute = 0;
                }

                elapsedMilliSeconds = 0;
            }

            if (elapsedSeconds >= timeToAdvanceDay)
            {
                day++;
                elapsedSeconds = 0;
            }

            if (hour >= 24) //days go forward every second or when 1 second has passed
            {
                day += 1;
                hour = 0;
                minute = 0;
            }

            if (day > DaysInMonth(month))
            {
                month += 1;
                UpdateCurrentSeason();
                form1PassIn.populationManager.UpdatePopulationByMonth();
                day = 1;
            }

            if (month > 12)
            {
                year += 1;
                form1PassIn.populationManager.UpdatePopulationByYear();
                month = 1;
            }
        }

        public void TimePainter(object? sender, Graphics g)
        {
            double progress = minute / 60.0;

            int duskOpacity = 0;
            int nightOpacity = 0;

            double factorIncrease = Math.Sin(Math.PI / 2 * progress);
            double factorDecrease = Math.Cos(Math.PI / 2 * progress);

            if (hour == 5)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorIncrease);
                nightOpacity = (int)Math.Round(MaxAlpha * factorDecrease);
            }
            else if (hour == 6)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorDecrease);
                nightOpacity = 0;
            }
            else if (hour == 20)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorIncrease);
            }
            else if (hour == 21)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorDecrease);
                nightOpacity = (int)Math.Round(MaxAlpha * factorIncrease);
            }
            else if (hour > 21 || hour < 5)
            {
                nightOpacity = MaxAlpha;
                duskOpacity = 0;
            }
            else
            {
                duskOpacity = 0;
                nightOpacity = 0;
            }

            duskOpacity = Clamp(duskOpacity, 0, 255);
            nightOpacity = Clamp(nightOpacity, 0, 255);

            if (duskOpacity > 0)
            {
                using Brush dusk = new SolidBrush(Color.FromArgb((int)(duskOpacity * 0.25), 180, 80, 30));
                g.FillRectangle(dusk, 0, 0, form1PassIn.ClientSize.Width, form1PassIn.ClientSize.Height);
            }

            if (nightOpacity > 0)
            {
                using Brush night = new SolidBrush(Color.FromArgb(nightOpacity, 0, 0, 50));
                g.FillRectangle(night, 0, 0, form1PassIn.ClientSize.Width, form1PassIn.ClientSize.Height);
            }
        }

        private int DaysInMonth(int month)
        {
            if (month == 2) return 28;
            else if (month == 4 || month == 6 || month == 9 || month == 11) return 30;
            else return 31;
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}