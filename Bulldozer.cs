using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CitySkylines0._5alphabeta
{
    public class Bulldozer
    {
        public Grid gridRef;
        public CarManager carManager;
        public Form1 form1;
        private Road road; //the road that is being selected
        private Building building; //the building that is being selected
        private Car car; //the car that is being selected


        // visible semi-transparent red
        private SolidBrush redBrush = new SolidBrush(Color.FromArgb(120, 255, 0, 0));

        public Bulldozer(Grid grid, Form1 form1)
        {
            gridRef = grid;
            this.form1 = form1;
            carManager = form1.carManager;
        }

        public void RemoveBuilding(Building b)
        {
            if (b == null) return;

            //reset the tile data that the building occupies
            foreach (int index in b.occupyingNodesIndex)
            {
                Node n = gridRef.nodes.FirstOrDefault(node => node.nodeNumber == index);
                n.hasTileData = false;
                n.isBuildable = true;
                n.IsNodeBuildable();
            }

            //remove the building
            gridRef.buildings.Remove(b);
            gridRef.FindRoadTilesAndAdjacentRoadTiles();
            gridRef.cash += b.cost / 2; //give the player half of the cost of the building back
        }

        public void RemoveCar(Car c)
        {
            if (c.type == "car") { carManager.DespawnCar(c); } //remove the car fully
            else { carManager.DespawnEmergencyServiceVehicle(c); } //otherwise despawn the emergency vehicle: safe so it can be reused
        }

        public void RemoveRoad(Road r)
        {
            if (r == null) return;

            if (gridRef.roads.Contains(r)) { gridRef.roads.Remove(r); } //remove the road

            //reset all nodes tile data
            foreach (Node n in gridRef.nodes)
            {
                n.isBuildable = false;
                n.isRoad = false;
                n.isNearRoad = false;
            }

            //reconstruct tile data for roads
            gridRef.FindRoadTilesAndAdjacentRoadTiles();

            foreach (Road road in gridRef.roads)
            {
                road.lane1.occupyingNodesIndex = gridRef.FindRoadTilesForSpecificEdge(road.lane1, 0);
                road.lane2.occupyingNodesIndex = gridRef.FindRoadTilesForSpecificEdge(road.lane2, 1);
            }

            //rebuilding adjacency for car navigation for roads
            gridRef.RebuildEntireRoadGraph();
        }

        public void BulldozerPainter(object? sender, Graphics g)
        {
            int tileW = form1.rectSize;
            if (car != null) //highlight a car
            {
                g.FillRectangle(redBrush, car.currentPosition.X - 8, car.currentPosition.Y - 8, tileW, tileW);
            }

            if (road != null) //highlight every node that a road occupies
            {
                foreach (int index in road.lane1.occupyingNodesIndex)
                {
                    Node n = gridRef.nodes.Where(node => node.nodeNumber == index).FirstOrDefault();
                    g.FillRectangle(redBrush, n.coords.X, n.coords.Y, tileW, tileW);
                }

                foreach (int index in road.lane2.occupyingNodesIndex)
                {
                    Node n = gridRef.nodes.Where(node => node.nodeNumber == index).FirstOrDefault();
                    g.FillRectangle(redBrush, n.coords.X, n.coords.Y, tileW, tileW);
                }
            }

            if (building != null) //highlight every node that a building occupies
            {
                foreach (int index in building.occupyingNodesIndex)
                {
                    Node n = gridRef.nodes.FirstOrDefault(n => n.nodeNumber == index);
                    g.FillRectangle(redBrush, n.coords.X, n.coords.Y, tileW, tileW);
                }
            }
        }

        public void Bulldozing(object? sender, Point mousePos, bool click, MouseEventArgs m)
        {
            if (!form1.selectingBulldozing) { return; }

            road = null;
            building = null;
            car = null;

            Point worldMousePos = ((Form1)sender).Mouse_Pos(sender, m); //find the world mouse position

            foreach (Car c in carManager.cars) //check if the mouse is over a car
            {
                int cx = (int)c.currentPosition.X;
                int cy = (int)c.currentPosition.Y;

                if (worldMousePos.X >= cx - 8 && worldMousePos.X <= cx + 8 && worldMousePos.Y >= cy - 8 && worldMousePos.Y <= cy + 8)
                {
                    car = c;
                    break;
                }
            }

            if (car == null) //dont check to see if highlighting a road if a car is being selected
            {
                //find edge by checking points on the edge (tolerance matches node size)
                foreach (Road r in gridRef.roads)
                {
                    foreach (Point p in r.lane1.pointsOnTheEdge)
                    {
                        if (worldMousePos.X >= p.X - form1.rectSize / 2 && worldMousePos.X <= p.X + form1.rectSize / 2 && worldMousePos.Y >= p.Y - form1.rectSize / 2 && worldMousePos.Y <= p.Y + form1.rectSize / 2)
                        {
                            road = r;
                            break;
                        }
                    }
                    foreach (Point p in r.lane2.pointsOnTheEdge)
                    {
                        if (worldMousePos.X >= p.X - form1.rectSize / 2 && worldMousePos.X <= p.X + form1.rectSize / 2 && worldMousePos.Y >= p.Y - form1.rectSize / 2 && worldMousePos.Y <= p.Y + form1.rectSize / 2)
                        {
                            road = r;
                            break;
                        }
                    }
                    if (road != null) { break; }
                }
            }
            
            if (car == null && road == null) //dont check to see if highlighting a building if selecting a car or road
            {
                //find building (account for tile -> pixel size)
                foreach (Building b in gridRef.buildings)
                {
                    int bx = b.coords.X;
                    int by = b.coords.Y;
                    int bw = Math.Max(1, b.size.Width * form1.rectSize);
                    int bh = Math.Max(1, b.size.Height * form1.rectSize);

                    if (worldMousePos.X >= bx && worldMousePos.X <= bx + bw && worldMousePos.Y >= by && worldMousePos.Y <= by + bh)
                    {
                        building = b;
                        break;
                    }
                }
            }

            //now we are allowed to delete if clicked over the highlighted object
            if (click)
            {
                if (road != null) { RemoveRoad(road); }
                if (building != null) { RemoveBuilding(building); }
                if (car != null) { RemoveCar(car); }
            }
        }
    }
}