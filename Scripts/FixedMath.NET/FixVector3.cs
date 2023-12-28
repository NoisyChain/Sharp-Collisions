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
            return $"({(float)x}, {(float)y}, {(float)z}))";
        }

        public bool Equals(FixVector3 other)
		{
			return x == other.x && y == other.y && z == other.z;
		}
    }
}