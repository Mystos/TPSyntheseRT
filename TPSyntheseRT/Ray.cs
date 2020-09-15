using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace TPSyntheseRT
{
    class Ray
    {
        private Position startPosition;
        private Direction direction;

        public Direction Direction { get => direction; set => direction = value; }
        public Position StartPosition { get => startPosition; set => startPosition = value; }

        public Ray(Position startPosition, Direction direction)
        {
            this.startPosition = startPosition;
            this.direction = direction;
        }
    }

    public class Position
    {
        private Vector3 origin;

        public Vector3 Origin { get => origin; set => origin = value; }

        public Position(Vector3 origin)
        {
            this.origin = origin;
        }
    }
    public class Direction
    {
        private Vector3 direction;

        public Vector3 Dir { get => direction; set => direction = value; }

        public Direction(Vector3 direction)
        {
            this.direction = direction;
        }
    }
}
