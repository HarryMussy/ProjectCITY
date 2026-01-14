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
        // visible semi-transparent red
        private SolidBrush redBrush = new SolidBrush(Color.FromArgb(120, 255, 0, 0));

        public Bulldozer(Grid grid, Form1 form1)
        {
            gridRef = grid;
            this.form1 = form1;
        }

        public void RemoveBuilding(Building b)
        {
            foreach (Node x in b.occupyingNodes.SelectMany(n => gridRef.nodes.Where(m => n.coords == m.coords)))
            {
                x.tileData = null;
            }
            gridRef.FindRoadTilesAndAdjacentRoadTiles();
            gridRef.buildings.Remove(b);
        }

        public void RemoveEdge(Edge e)
        {
            foreach (Node n in e.occupyingNodes)
            {
                if (gridRef.roadNodes.Contains(n)) { gridRef.roadNodes.Remove(n); }
            }
            gridRef.FindRoadTilesAndAdjacentRoadTiles();
            gridRef.edges.Remove(e);
        }

        public void BulldozerPainter(object? sender, Graphics g)
        {
            // Draw edge highlight using normalized rectangle (top-left + positive size)
            if (edge != null)
            {
                int x = Math.Min(edge.a.X - 8, edge.b.X - 8);
                int y = Math.Min(edge.a.Y - 8, edge.b.Y - 8);
                int w = Math.Max(1, Math.Abs(edge.b.X - edge.a.X) + 8);
                int h = Math.Max(1, Math.Abs(edge.b.Y - edge.a.Y) + 8);

                g.FillRectangle(redBrush, x, y, w, h);
            }

            // Draw building highlight using pixel size (size.Width/Height are in tiles)
            if (building != null)
            {
                int tileW = form1.rectSize;
                int x = building.coords.X;
                int y = building.coords.Y;
                int w = Math.Max(1, building.size.Width * tileW);
                int h = Math.Max(1, building.size.Height * tileW);

                g.FillRectangle(redBrush, x, y, w, h);
            }
        }

        public void Bulldozing(Point mousePos, bool click)
        {
            if (!form1.selectingBulldozing)
                return;

            edge = null;
            building = null;

            // Convert screen mouse position to world coordinates using the same logic as Form1.Mouse_Pos
            // world = (screen - (screencentre - camera)) / zoom + screencentre
            int worldX = (int)((mousePos.X - (form1.screencentre.X - form1.camera.X)) / form1.zoomLevel + form1.screencentre.X);
            int worldY = (int)((mousePos.Y - (form1.screencentre.Y - form1.camera.Y)) / form1.zoomLevel + form1.screencentre.Y);
            Point worldMousePos = new Point(worldX, worldY);

            // find edge by checking points on the edge (tolerance matches node size)
            foreach (Edge e in gridRef.edges)
            {
                foreach (Point p in e.pointsOnTheEdge)
                {
                    if (worldMousePos.X >= p.X - form1.rectSize/2 && worldMousePos.X <= p.X + form1.rectSize/2 &&
                        worldMousePos.Y >= p.Y - form1.rectSize/2 && worldMousePos.Y <= p.Y + form1.rectSize/2)
                    {
                        edge = e;
                        break;
                    }
                }
                if (edge != null)
                    break;
            }

            // find building (account for tile -> pixel size)
            foreach (Building b in gridRef.buildings)
            {
                int bx = b.coords.X;
                int by = b.coords.Y;
                int bw = Math.Max(1, b.size.Width * form1.rectSize);
                int bh = Math.Max(1, b.size.Height * form1.rectSize);

                if (worldMousePos.X >= bx &&
                    worldMousePos.X <= bx + bw &&
                    worldMousePos.Y >= by &&
                    worldMousePos.Y <= by + bh)
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