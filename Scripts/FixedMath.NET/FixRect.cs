using System;
namespace FixMath.NET
{
    [Serializable]
    public struct FixRect
    {
        public Fix64 x, y, w, h;

        public FixRect(Fix64 _x, Fix64 _y, Fix64 _w, Fix64 _h)
        {
            x = _x;
            y = _y;
            w = _w;
            h = _h;
        }

        public bool IsOverlapping(FixRect other)
        {
			bool collisionX = x < other.w && w > other.x;
			bool collisionY = y < other.h && h > other.y;

            return collisionX && collisionY;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {w}, {h})";
 
        }
    }
}
