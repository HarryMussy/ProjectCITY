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
