using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

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
            newbutton.Font = new System.Drawing.Font("Segoe UI", 6, FontStyle.Bold);
            newbutton.Size = size;
            newbutton.Location = loc;
            newbutton.BackColor = Color.FromArgb(60, 60, 60);
            newbutton.ForeColor = Color.White;
            newbutton.FlatStyle = FlatStyle.Flat;
            newbutton.Cursor = Cursors.Hand;
            newbutton.FlatAppearance.BorderSize = 2;
            newbutton.FlatAppearance.BorderColor = Color.LightBlue;
            Objects.Add(newbutton);
            form.Controls.Add(newbutton);
            this.form = form;
            return newbutton;
        }
        public Button CreateButton(Point loc, Size size, Form form, int fontsize, System.Drawing.Image img)
        {
            Button newbutton = new Button();
            newbutton.Size = size;
            newbutton.Location = loc;
            newbutton.Image = img;
            newbutton.BackColor = Color.FromArgb(60, 60, 60);
            newbutton.ForeColor = Color.White;
            newbutton.FlatStyle = FlatStyle.Flat;
            newbutton.Cursor = Cursors.Hand;
            newbutton.FlatAppearance.BorderSize = 2;
            newbutton.FlatAppearance.BorderColor = Color.LightBlue;
            Objects.Add(newbutton);
            form.Controls.Add(newbutton);
            this.form = form;
            return newbutton;
        }

        public TrackBar CreateSlider(string name, Point loc, Size size, Form form, int fontsize)
        {
            TrackBar slider = new TrackBar();

            // These properties are standard for TrackBar
            slider.BackColor = Color.White;
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

            slider.BackColor = Color.FromArgb(60, 60, 60);
            slider.ForeColor = Color.White;
            //slider.FlatStyle = FlatStyle.Flat;
            slider.Cursor = Cursors.Hand;
            //slider.FlatAppearance.BorderSize = 2;
            //slider.FlatAppearance.BorderColor = Color.LightBlue;

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
