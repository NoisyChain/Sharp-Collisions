using System;
namespace FixMath.NET
{
    [Serializable]
    public struct FixVolume
    {
        public Fix64 x, y, z, w, h, d;

        public FixVolume(Fix64 _x, Fix64 _y, Fix64 _z, Fix64 _w, Fix64 _h, Fix64 _d)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
            h = _h;
            d = _d;
        }

        public bool IsOverlapping(FixVolume other)
        {
			bool collisionX = x < other.w && w > other.x;
			bool collisionY = y < other.h && h > other.y;
            bool collisionZ = z < other.d && d > other.z;

            return collisionX && collisionY && collisionZ;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {x}, {w}, {h}, {d})";
 
        }
    }
}
