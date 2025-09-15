using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public class Car
    {
        public PointF Position;
        public Edge startEdge;
        public Point startPoint;
        public float Progress; // 0=start, 1=end
        public float Speed; // units per tick
        public Point destinationPoint;
        public Edge destinationEdge;

        public Car(Edge edge, Point spawnIN, float speed, Point destinationIN, Edge edgeDestinationIN)
        {
            startEdge = edge;
            Progress = 0f;
            Speed = speed;
            Position = new PointF(startPoint.X, startPoint.Y);
            startPoint = spawnIN;
            destinationPoint = destinationIN;
            destinationEdge = edgeDestinationIN;
        }

        public void Update()
        {
            Progress += Speed;
            if (Progress > 1f) { Progress = 1f; }
            Position = new PointF( startPoint.X + (destinationPoint.X - startPoint.X) * Progress, startPoint.Y + (destinationPoint.Y - startPoint.Y) * Progress);
        }

        public bool HasReachedEnd() => Progress >= 1f;
    }
}
