using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace TPSyntheseRT
{
   public class Ray
    {
        private Position startPosition;
        private Direction direction;
        private Direction invDirection;
        
        public Direction Direction { get => direction; set => direction = value; }
        public Direction InvDirection { get => invDirection; set => invDirection = value; }
        public Position StartPosition { get => startPosition; set => startPosition = value; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="direction">La direction sera toujours normaliser</param>
        public Ray(Position startPosition, Direction direction)
        {
            this.startPosition = startPosition;
            direction.Dir = Vector3.Normalize(direction.Dir);
            this.direction = direction;

            this.invDirection = new Direction(Direction.Invert(this.direction.Dir));
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
        private  Vector3 direction;

        public Vector3 Dir { get => direction; set => direction = value; }

        public Direction(Vector3 direction)
        {
            this.direction = direction;
        }

        /// <summary>
        /// Inverts a scale vector by dividing 1 by each component
        /// </summary>
        public static Vector3 Invert(Vector3 vec)
        {
            return new Vector3(1 / vec.X, 1 / vec.Y, 1 / vec.Z);
        }
    }
}
