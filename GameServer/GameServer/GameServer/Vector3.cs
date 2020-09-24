using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
   public class Vector3
    {

        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static  Vector3 Transform(Vector3 inVector, Quaternion rot)
        {
            return new Vector3(0, 0, 0); //Skip this logic 
        }
    }
}
