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
            if (b == null) return;

            foreach (Node n in b.occupyingNodes)
            {
                n.tileData = null;
                n.isBuildable = true;
                n.IsNodeBuildable();
            }

            gridRef.buildings.Remove(b);
            gridRef.FindRoadTilesAndAdjacentRoadTiles();
        }

        public void RemoveEdge(Edge e)
        {
            if (e == null) return;

            if (gridRef.edges.Contains(e))
                gridRef.edges.Remove(e);

            if (e.occupyingNodes != null)
            {
                foreach (Node n in e.occupyingNodes)
                {
                    n.imageKey = null;
                    n.isRoad = false;
                    n.isNearRoad = false;
                    n.IsNodeBuildable();
                }
            }

            if (e.intersections != null)
            {
                foreach (var inter in e.intersections.ToList())
                {
                    if (inter.connectedEdges != null)
                        inter.connectedEdges.Remove(e);

                    if (inter.connectedEdges == null || inter.connectedEdges.Count == 0)
                    {
                        gridRef.roadIntersections.Remove(inter);
                    }
                }
            }

            gridRef.FindRoadTilesAndAdjacentRoadTiles();
        }

        public void BulldozerPainter(object? sender, Graphics g)
        {
            int tileW = form1.rectSize;
            if (edge != null)
            {
                foreach (Node n in edge.occupyingNodes)
                {
                    g.FillRectangle(redBrush, n.coords.X, n.coords.Y, tileW, tileW);
                }
            }

            if (building != null)
            {
                foreach (Node n in building.occupyingNodes)
                {
                    g.FillRectangle(redBrush, n.coords.X, n.coords.Y, tileW, tileW);
                }
            }
        }

        public void Bulldozing(object? sender, Point mousePos, bool click, MouseEventArgs m)
        {
            if (!form1.selectingBulldozing)
                return;

            edge = null;
            building = null;

            Point worldMousePos = ((Form1)sender).Mouse_Pos(sender, m);

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