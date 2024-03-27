using System;
using Godot;
using SharpCollisions;

namespace FixMath.NET 
{
	[Serializable]
	public struct FixVector3
	{
		public Fix64 x;
		public Fix64 y;
        public Fix64 z;

		public FixVector3(Fix64 x, Fix64 y, Fix64 z)
		{
			this.x = x;
			this.y = y;
            this.z = z;
		}

		public static readonly FixVector3 Zero = new FixVector3();
		
		public static readonly FixVector3 One = new FixVector3(Fix64.One, Fix64.One, Fix64.One);
		
		public static readonly FixVector3 Up = new FixVector3(Fix64.Zero, Fix64.One, Fix64.Zero);
		public static readonly FixVector3 Down = new FixVector3(Fix64.Zero, Fix64.NegativeOne, Fix64.Zero);
		public static readonly FixVector3 Right = new FixVector3(Fix64.One, Fix64.Zero, Fix64.Zero);
		public static readonly FixVector3 Left = new FixVector3(Fix64.NegativeOne, Fix64.Zero, Fix64.Zero);
        public static readonly FixVector3 Forward = new FixVector3(Fix64.Zero, Fix64.Zero, Fix64.One);
		public static readonly FixVector3 Back = new FixVector3(Fix64.Zero, Fix64.Zero, Fix64.NegativeOne);
		
		public static Fix64 Length(FixVector3 v)
		{
			return Fix64.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
		}

		public static Fix64 Distance(FixVector3 vec0, FixVector3 vec1)
		{
			Fix64 dx = vec0.x - vec1.x;
			Fix64 dy = vec0.y - vec1.y;
			Fix64 dz = vec0.z - vec1.z;
			return Fix64.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static FixVector3 Normalize(FixVector3 v)
		{
			Fix64 len = Length(v);
			if (len == Fix64.Zero) return Zero;
			return new FixVector3(v.x / len, v.y / len, v.z / len);
		}
        public static Fix64 Dot(FixVector3 a, FixVector3 b)
		{
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public static FixVector3 Cross(FixVector3 a, FixVector3 b)
		{
			return new FixVector3(
				(a.y * b.z) - (a.z * b.y),
				(a.z * b.x) - (a.x * b.z),
				(a.x * b.y) - (a.y * b.x));
		}

		public static FixVector3 TripleProduct(FixVector3 a, FixVector3 b, FixVector3 c)
		{
			return Cross(Cross(a, b), c);
		}

		public static FixVector3 Transform(FixVector3 v, FixedTransform3D body)
		{
			FixVector3 r = Rotate(v, body.FixedRotation);

			Fix64 tx = r.x + body.FixedPosition.x;
			Fix64 ty = r.y + body.FixedPosition.y;
			Fix64 tz = r.z + body.FixedPosition.z;

			return new FixVector3(tx, ty, tz);
		}

		public static FixVector3 Transform(FixVector3 v, FixVector3 refPosition, FixVector3 refRotation)
		{
			FixVector3 r = Rotate(v, refRotation);

			Fix64 tx = r.x + refPosition.x;
			Fix64 ty = r.y + refPosition.y;
			Fix64 tz = r.z + refPosition.z;

			return new FixVector3(tx, ty, tz);
		}

		public static FixVector3 Project(FixVector3 u, FixVector3 v)
		{
			if (u == Zero) return Zero;
			if (v == Zero) return Zero;

			return Dot(v, u) / Dot(v, v) * v;
		}
		public static FixVector3 Reject(FixVector3 u, FixVector3 v)
		{
			return u - Project(u, v);
		}

		public static FixVector3 Rotate(FixVector3 v, FixVector3 angle)
		{
			Fix64 rx = v.x * Fix64.Cos(angle.y) * Fix64.Cos(angle.z) - v.y * (Fix64.Sin(angle.x) * Fix64.Sin(angle.y) * Fix64.Cos(angle.z) + Fix64.Cos(angle.x) * Fix64.Sin(angle.z)) + v.z * (Fix64.Cos(angle.x) * Fix64.Sin(angle.y) * Fix64.Cos(angle.z) - Fix64.Sin(angle.x) * Fix64.Sin(angle.z));
    		Fix64 ry = v.x * Fix64.Cos(angle.y) * Fix64.Sin(angle.z) + v.y * (Fix64.Sin(angle.x) * Fix64.Sin(angle.y) * Fix64.Sin(angle.z) + Fix64.Cos(angle.x) * Fix64.Cos(angle.z)) + v.z * (Fix64.Cos(angle.x) * Fix64.Sin(angle.y) * Fix64.Sin(angle.z) + Fix64.Sin(angle.x) * Fix64.Cos(angle.z));
    		Fix64 rz = v.x * Fix64.Sin(angle.y) + v.y * (-Fix64.Sin(angle.x) * Fix64.Cos(angle.y)) + v.z * (Fix64.Cos(angle.x) * Fix64.Cos(angle.y));
			
			//FixVector2 rxy = FixVector2.Rotate(new FixVector2(v.x, v.y), angle.z);
			//FixVector2 rxz = FixVector2.Rotate(new FixVector2(v.x, v.z), angle.y);
			//FixVector2 ryz = FixVector2.Rotate(new FixVector2(v.y, v.z), angle.x);
			
			return new FixVector3(rx, ry, rz);
		}

		public static Fix64 Angle(FixVector3 a, FixVector3 b)
		{
			return Fix64.Atan2(b.y - a.y, b.x - a.x);
		}

		public static Fix64 AngleDegrees(FixVector3 a, FixVector3 b)
		{
			return Angle(a, b) * Fix64.RadToDeg;
		}

		public static bool IsSameDirection(FixVector3 a, FixVector3 b)
		{
			return Dot(a, b) > Fix64.Zero;
		}
		
		public static bool IsExactDirection(FixVector3 a, FixVector3 b)
		{
			return Dot(a, b) > (Fix64)0.9;
		}

        public static FixVector3 operator +(FixVector3 a, FixVector3 b) {
			return new FixVector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static FixVector3 operator -(FixVector3 a, FixVector3 b) {
			return new FixVector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static FixVector3 operator -(FixVector3 a) {
			return new FixVector3(-a.x, -a.y, -a.z);
		}

		public static FixVector3 operator *(FixVector3 a, FixVector3 b) {
			return new FixVector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
		
		public static FixVector3 operator *(Fix64 a, FixVector3 b) {
			return new FixVector3(a * b.x, a * b.y, a * b.z);
		}
		
		public static FixVector3 operator *(FixVector3 a, Fix64 b) {
			return new FixVector3(a.x * b, a.y * b, a.z * b);
		}

		public static FixVector3 operator /(FixVector3 a, FixVector3 b) {
			return new FixVector3(a.x / b.x, a.y / b.y, a.z / b.z);
		}
		
		public static FixVector3 operator /(FixVector3 a, Fix64 b) {
			return new FixVector3(a.x / b, a.y / b, a.z / b);
		}
		
		public static FixVector3 operator /(Fix64 a, FixVector3 b) {
			return new FixVector3(a / b.x, a / b.y, a / b.z);
		}

		public static bool operator ==(FixVector3 a, FixVector3 b)
		{
			return a.x == b.x && a.y == b.y && a.z == b.z;
		}

		public static bool operator !=(FixVector3 a, FixVector3 b)
		{
			return a.x != b.x || a.y != b.y || a.z != b.z;
		}

		public static bool operator >(FixVector3 a, FixVector3 b)
		{
			return a.x > b.x || a.y > b.y || a.z > b.z;
		}

		public static bool operator <(FixVector3 a, FixVector3 b)
		{
			return a.x < b.x || a.y < b.y || a.z < b.z;
		}

		public static bool operator >=(FixVector3 a, FixVector3 b)
		{
			return a.x >= b.x || a.y >= b.y || a.z >= b.z;
		}

		public static bool operator <=(FixVector3 a, FixVector3 b)
		{
			return a.x <= b.x || a.y <= b.y || a.z <= b.z;
		}

		public static explicit operator Vector2(FixVector3 value)
		{
			return new Vector2((float)value.x, (float)value.y);
		}

		public static explicit operator FixVector2(FixVector3 value) {
			return new FixVector2(value.x, value.y);
		}

        public static explicit operator FixVector3(FixVector2 value) {
			return new FixVector3(value.x, value.y, Fix64.Zero);
		}

		public static explicit operator Vector3(FixVector3 value) {
			return new Vector3((float)value.x, (float)value.y, (float)value.z);
		}

		public static explicit operator FixVector3(Vector3 value) {
			return new FixVector3((Fix64)value.x, (Fix64)value.y, (Fix64)value.z);
		}

		public override bool Equals(object obj)
		{
			return ((FixVector3)obj).x.RawValue == x.RawValue &&
				((FixVector3)obj).y.RawValue == y.RawValue &&
				((FixVector3)obj).z.RawValue == z.RawValue;
		}

		public override int GetHashCode()
		{
			return x.m_rawValue.GetHashCode() + y.m_rawValue.GetHashCode() + z.m_rawValue.GetHashCode();
		}

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public bool Equals(FixVector3 other)
		{
			return x == other.x && y == other.y && z == other.z;
		}
    }
}