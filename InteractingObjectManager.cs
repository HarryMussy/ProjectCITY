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

        //creates a text button, adds it to the form and registers it in the objects list
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

        //creates an icon button using an image instead of a text label
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

        //removes all tracked buttons from the form and clears the objects list
        //called by UIManager when the window is resized so buttons can be repositioned
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