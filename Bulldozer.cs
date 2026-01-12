using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CitySkylines0._5alphabeta
{
    public class Bulldozer
    {
        public Grid gridRef;
        public Form1 form1;
        private Edge edge;
        private Building building;
        private SolidBrush redBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 50));

        public Bulldozer(Grid grid, Form1 form1)
        {
            gridRef = grid;
            this.form1 = form1;
        }

        public void RemoveBuilding(Building b)
        {
            gridRef.buildings.Remove(b);
        }

        public void RemoveEdge(Edge e)
        {
            gridRef.edges.Remove(e);
        }

        public void BulldozerPainter(object? sender, Graphics g)
        {
            if (edge != null)
            {
                g.FillRectangle(redBrush, edge.a.X - 8, edge.a.Y - 8, edge.b.X - edge.a.X, edge.b.Y - edge.a.Y);
            }

            if (building != null)
            {
                g.FillRectangle(redBrush, building.coords.X, building.coords.Y, building.size.Width, building.size.Height);
            }
        }

        public void Bulldozing(Point mousePos, bool click)
        {
            if (!form1.selectingBulldozing)
                return;

            edge = null;
            building = null;

            int x = (int)((mousePos.X - form1.screencentre.X) / form1.zoomLevel + form1.screencentre.X + form1.camera.X);
            int y = (int)((mousePos.Y - form1.screencentre.Y) / form1.zoomLevel + form1.screencentre.Y + form1.camera.Y);

            Point worldMousePos = new Point(x, y);

            // find edge
            foreach (Edge e in gridRef.edges)
            {
                foreach (Point p in e.pointsOnTheEdge)
                {
                    if (worldMousePos.X >= p.X - 8 && worldMousePos.X <= p.X + 8 &&
                        worldMousePos.Y >= p.Y - 8 && worldMousePos.Y <= p.Y + 8)
                    {
                        edge = e;
                        break;
                    }
                }
                if (edge != null)
                    break;
            }

            // find building
            foreach (Building b in gridRef.buildings)
            {
                if (worldMousePos.X >= b.coords.X &&
                    worldMousePos.X <= b.coords.X + b.size.Width &&
                    worldMousePos.Y >= b.coords.Y &&
                    worldMousePos.Y <= b.coords.Y + b.size.Height)
                {
                    building = b;
                    break;
                }
            }

            // now we are allowed to delete
            if (click)
            {
                if (edge != null)
                    RemoveEdge(edge);

                if (building != null)
                    RemoveBuilding(building);
            }
        }
    }
}