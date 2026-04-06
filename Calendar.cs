using System.Text.Json.Serialization;

namespace ProjectCity
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


        //must be serialized or time will jump after load
        public double elapsedMilliSeconds { get; set; }
        public double elapsedSeconds { get; set; }

        //configuration for visual alpha ranges
        private const int MaxAlpha = 150;

        //required for JSON
        public Calendar() { }

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

        public int GetHour() => hour; //returns the current hour

        //return a fade factor 0-1 depending on how close we are to season end
        public float GetSeasonTransitionFactor()
        {
            int monthLength = DaysInMonth(month);
            int fadeStart = monthLength - 7; //last 7 days of season is transitional period
            if (day <= fadeStart) return 0f;

            return (float)(day - fadeStart) / 7f;
        }

        //at the end of every month, this is called to find the current season
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

        //this is called when retrieving the current or next season
        public string GetCurrentSeason(int month)
        {
            if (month == 12 || month == 1 || month == 2 || month == 13) //13th month used to prevent getting Autumn when transitioning from 12th month the 13th/ 1st month
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

        //advance the in game time
        public void AdvanceTime(double elapsed)
        {
            elapsedMilliSeconds += elapsed;
            elapsedSeconds += elapsed / 1000;

            //every real second is 1 minute and 1 day
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

            if (hour >= 24) //days go forward every second or when 24 hours have passed, reset minute and hour counter
            {
                day += 1;
                hour = 0;
                minute = 0;
            }

            //increase the month when the number of days is greater than the days in the month and reset day counter
            if (day > DaysInMonth(month))
            {
                month += 1;
                UpdateCurrentSeason();
                form1PassIn.populationManager.UpdatePopulationByMonth(); //update monthly population data
                day = 1;
            }

            if (month > 12)
            {
                year += 1;
                form1PassIn.populationManager.UpdatePopulationByYear(); //update yearly population data
                month = 1;
            }
        }

        //apply a screen to the game at specific times of day
        public void TimePainter(object? sender, Graphics g)
        {
            double progress = minute / 60.0;

            int duskOpacity = 0;
            int nightOpacity = 0;

            double factorIncrease = Math.Sin(Math.PI / 2 * progress);
            double factorDecrease = Math.Cos(Math.PI / 2 * progress);

            //increase dawn and decrease night at 5 am
            if (hour == 5)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorIncrease);
                nightOpacity = (int)Math.Round(MaxAlpha * factorDecrease);
            }

            //decrease dawn factor at 6 am
            else if (hour == 6)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorDecrease);
                nightOpacity = 0;
            }

            //increase dusk factor at 8pm
            else if (hour == 20)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorIncrease);
            }

            //decrease dusk factor and increase nightly factor at 9 pm
            else if (hour == 21)
            {
                duskOpacity = (int)Math.Round(MaxAlpha * factorDecrease);
                nightOpacity = (int)Math.Round(MaxAlpha * factorIncrease);
            }

            //between the hours of 10pm and 4am, it is night
            else if (hour > 21 || hour < 5)
            {
                nightOpacity = MaxAlpha;
                duskOpacity = 0;
            }

            //between 7am and 8pm it is day
            else
            {
                duskOpacity = 0;
                nightOpacity = 0;
            }

            duskOpacity = Clamp(duskOpacity, 0, 255);
            nightOpacity = Clamp(nightOpacity, 0, 255);

            //draw dusk over the screen if the opacity is > 0
            if (duskOpacity > 0)
            {
                using Brush dusk = new SolidBrush(Color.FromArgb((int)(duskOpacity * 0.25), 180, 80, 30));
                g.FillRectangle(dusk, 0, 0, form1PassIn.ClientSize.Width, form1PassIn.ClientSize.Height);
            }

            //draw night over the screen if the opacity is > 0
            if (nightOpacity > 0)
            {
                using Brush night = new SolidBrush(Color.FromArgb(nightOpacity, 0, 0, 50));
                g.FillRectangle(night, 0, 0, form1PassIn.ClientSize.Width, form1PassIn.ClientSize.Height);
            }
        }

        //returns the number of days dependant on the month int
        private int DaysInMonth(int month)
        {
            if (month == 2) return 28;
            else if (month == 4 || month == 6 || month == 9 || month == 11) return 30;
            else return 31;
        }

        //clamps data between values
        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}