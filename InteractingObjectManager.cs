using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    public class InteractingObjectManager
    {
        public List<object> Objects { get; set; }
        Form form;
        public InteractingObjectManager()
        {
            Objects = new List<object>();
        }
        public Button CreateButton(string name, Point loc, Size size, Form form, int fontsize)
        {
            Button newbutton = new Button();
            newbutton.Text = name;
            newbutton.Font = new Font("Comic Sans", fontsize);
            newbutton.Size = size;
            newbutton.Location = loc;
            Objects.Add(newbutton);
            form.Controls.Add(newbutton);
            this.form = form;
            return newbutton;
        }

        public TrackBar CreateSlider(string name, Point loc, Size size, Form form, int fontsize)
        {
            TrackBar slider = new TrackBar();

            // These properties are standard for TrackBar
            slider.Minimum = 0;
            slider.Maximum = 100;
            slider.Value = 0; // Default to max volume, for example
            slider.TickFrequency = 10;

            // Add controls
            Objects.Add(slider); // Optional if you want to track it
            form.Controls.Add(slider);

            // Set slider position and size after label
            slider.Location = loc;
            slider.Size = size;

            this.form = form;
            return slider;
        }


        public void RemoveButtons()
        {
            foreach (object obj in Objects)
            {
                if (obj is Control control)
                {
                    form.Controls.Remove(control);
                }
            }
            Objects.Clear();
        }
    }
}
