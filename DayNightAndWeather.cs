using System;
using System.Drawing;
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

        // Must be serialized or time will jump after load
        public double elapsedTotal { get; set; }

        // configuration for visual alpha ranges
        private const int MaxAlpha = 150;

        // REQUIRED for JSON
        public Calendar() { }

        // Your existing constructor
        public Calendar(int dayIn, int monthIn, int yearIn, int hourIn, int minuteIn)
        {
            day = dayIn;
            month = monthIn;
            year = yearIn;
            hour = hourIn;
            minute = minuteIn;
            elapsedTotal = 0;
        }

        public int GetHour() => hour;

        public void AdvanceTime(double elapsed)
        {
            double timeToAdvance = 1000; // 1 second real time
            int advanceMinutes = 1;

            elapsedTotal += elapsed;

            if (elapsedTotal >= timeToAdvance)
            {
                minute += advanceMinutes;

                if (minute >= 60)
                {
                    hour += 1;
                    minute = 0;
                }

                elapsedTotal = 0;
            }

            if (hour >= 24)
            {
                day += 1;
                hour = 0;
                minute = 0;
            }

            if (day > DaysInMonth(month))
            {
                month += 1;
                day = 1;
            }

            if (month > 12)
            {
                year += 1;
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
                g.FillRectangle(dusk, 0, 0, 1920, 1080);
            }

            if (nightOpacity > 0)
            {
                using Brush night = new SolidBrush(Color.FromArgb(nightOpacity, 0, 0, 50));
                g.FillRectangle(night, 0, 0, 1920, 1080);
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