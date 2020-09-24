using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public class Quaternion
    {
       public float X;
       public float Y;
       public float Z;
       public float W;
       public static Quaternion Identity = new Quaternion(0,0,0,1);

        public Quaternion (float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
