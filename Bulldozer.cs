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

            // Clear tile data from occupying nodes
            foreach (Node n in b.occupyingNodes)
            {
                // use the node's field name used across your codebase
                n.tileData = null;
                /*foreach (Node node where gridRef.nodes.All(m => m.coords = n.coords))*/
                // recompute buildable flag from current node state
                n.isBuildable = false;
                n.IsNodeBuildable();
            }

            // Remove building from grid then recompute dependent collections
            gridRef.buildings.Remove(b);
            gridRef.FindRoadTilesAndAdjacentRoadTiles();
        }

        public void RemoveEdge(Edge e)
        {
            if (e == null) return;

            // 1) Remove the edge from the grid first so subsequent recompute won't see it
            if (gridRef.edges.Contains(e))
                gridRef.edges.Remove(e);

            // 2) Clear road state on nodes that belonged to this edge
            if (e.occupyingNodes != null)
            {
                foreach (Node n in e.occupyingNodes)
                {
                    // clear any rendering hints
                    n.imageKey = null;
                    // mark not a road (grid recompute will set correct values)
                    n.isRoad = false;
                    n.isNearRoad = false;
                    n.IsNodeBuildable();
                }
            }

            // 3) Remove intersections that referenced this edge (if any)
            if (e.intersections != null)
            {
                foreach (var inter in e.intersections.ToList())
                {
                    // remove the edge reference from the intersection node
                    if (inter.connectedEdges != null)
                        inter.connectedEdges.Remove(e);

                    // if intersection has no connected edges anymore, remove it from grid collections
                    if (inter.connectedEdges == null || inter.connectedEdges.Count == 0)
                    {
                        gridRef.roadIntersections.Remove(inter);
                    }
                }
            }

            // 4) Recompute road tiles / adjacent info after we've actually removed the edge
            gridRef.FindRoadTilesAndAdjacentRoadTiles();
        }

        public void BulldozerPainter(object? sender, Graphics g)
        {
            // Draw edge highlight using normalized rectangle (top-left + positive size)
            if (edge != null)
            {
                foreach (Node n in edge.occupyingNodes)
                {
                    g.FillRectangle(redBrush, n.coords.X, n.coords.Y, n.Width, n.height);
                }
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