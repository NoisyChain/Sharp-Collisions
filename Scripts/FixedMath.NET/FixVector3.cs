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

		public static Fix64 LengthSq(FixVector3 v)
		{
			return v.x * v.x + v.y * v.y + v.z * v.z;
		}

		public static Fix64 DistanceSq(FixVector3 vec0, FixVector3 vec1)
		{
			Fix64 dx = vec0.x - vec1.x;
			Fix64 dy = vec0.y - vec1.y;
			Fix64 dz = vec0.z - vec1.z;
			return dx * dx + dy * dy + dz * dz;
		}

		public static FixVector3 Normalize(FixVector3 v)
		{
			Fix64 len = Length(v);
			if (len == Fix64.Zero) return Zero;
			FixVector3 nor = new FixVector3(v.x / len, v.y / len, v.z / len);
			return nor;
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

		public static FixVector3 GetLineNormal(FixVector3 line, FixVector3 arbitraryA, FixVector3 arbitraryB)
		{
			FixVector3 arbitrary = arbitraryA;
            if (Cross(line, arbitrary) == Zero)
            {
                arbitrary = arbitraryB;
            }
            FixVector3 normal = Cross(line, arbitrary);
            return Normalize(normal);
		}

		public static FixVector3 GetPlaneNormal(FixVector3 a, FixVector3 b, FixVector3 c)
		{
			FixVector3 ab = b - a;
			FixVector3 ac = c - b;
			return Normalize(Cross(ab, ac));
		}

		public static FixVector3 FindTriangleCentroid(FixVector3 a, FixVector3 b, FixVector3 c)
		{
			Fix64 centerX = (a.x + b.x + c.x) / (Fix64)3;
			Fix64 centerY = (a.y + b.y + c.y) / (Fix64)3;
			Fix64 centerZ = (a.z + b.z + c.z) / (Fix64)3;

			return new FixVector3(centerX, centerY, centerZ);
		}

		public static FixVector3 Transform(FixVector3 v, SharpCollisions.Sharp3D.FixedTransform3D body)
		{
			return Transform(v, body.FixedPosition, body.FixedRotation);
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

		/// <summary>
		/// Rotates a vector using a ZXY Euler angles
		/// </summary>
		/// <param name="v">The vector to be rotated.</param>
		/// <param name="angle">The rotation vector.</param>
		/// <returns>The rotated vector</returns>
		public static FixVector3 Rotate(FixVector3 v, FixVector3 angle)
		{
			FixVector3 RotRoll = Roll(v, angle.z);
			FixVector3 RotPitch = Pitch(RotRoll, angle.x);
			FixVector3 RotYaw = Yaw(RotPitch, angle.y);
			
			return RotYaw;
		}

		/// <summary>
		/// Rotates a vector using the Rodriguez rotation formula
		/// about an arbitrary axis.
		/// </summary>
		/// <param name="v">The vector to be rotated.</param>
		/// <param name="axis">The rotation axis.</param>
		/// <param name="angle">The rotation angle.</param>
		/// <returns>The rotated vector</returns>
		public static FixVector3 RodriguezRotate(FixVector3 v, FixVector3 axis, Fix64 angle)
		{
			FixVector3 vxp = Cross(axis, v);
			FixVector3 vxvxp = Cross(axis, vxp);
			return v + Fix64.Sin(angle) * vxp + (Fix64.One - Fix64.Cos(angle)) * vxvxp;
		}

		/// <summary>
		///Rotates the vector on the X axis
		/// </summary>
		public static FixVector3 Pitch(FixVector3 v, Fix64 angle)
		{
			FixVector3 newVector = v;
			newVector.y = v.y * Fix64.Cos(angle) - v.z * Fix64.Sin(angle);
			newVector.z = v.y * Fix64.Sin(angle) + v.z * Fix64.Cos(angle);
			return newVector;
		}

		/// <summary>
		///Rotates the vector on the Y axis
		/// </summary>
		public static FixVector3 Yaw(FixVector3 v, Fix64 angle)
		{
			FixVector3 newVector = v;
			newVector.x = v.x * Fix64.Cos(angle) + v.z * Fix64.Sin(angle);
			newVector.z = v.x * -Fix64.Sin(angle) + v.z * Fix64.Cos(angle);
			return newVector;
		}

		/// <summary>
		///Rotates the vector on the Z axis
		/// </summary>
		public static FixVector3 Roll(FixVector3 v, Fix64 angle)
		{
			FixVector3 newVector = v;
			newVector.x = v.x * Fix64.Cos(angle) - v.y * Fix64.Sin(angle);
			newVector.y = v.x * Fix64.Sin(angle) + v.y * Fix64.Cos(angle);
			return newVector;
		}

		public static Fix64 Angle(FixVector3 a, FixVector3 b)
		{
			Fix64 dot = Dot(a, b);
			Fix64 magA = Length(a);
			Fix64 magB = Length(b);
			if (magA * magB == Fix64.Zero) return Fix64.Zero;
			return Fix64.Acos(dot / (magA * magB));
		}

		public static Fix64 AngleDegrees(FixVector3 a, FixVector3 b)
		{
			return Angle(a, b) * Fix64.RadToDeg;
		}

		public static bool IsSameDirection(FixVector3 a, FixVector3 b)
		{
			return Dot(a, b) > Fix64.Zero;
		}
		
		public static bool Approximate(FixVector3 a, FixVector3 b)
		{
			return Fix64.Approximate(a.x, b.x) && Fix64.Approximate(a.y, b.y) && Fix64.Approximate(a.z, b.z);
		}

		public static FixVector3 ClampMagnitude(FixVector3 vector, Fix64 magnitude)
		{
			if (Length(vector) > magnitude)
				return Normalize(vector) * magnitude;
			return vector;
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
			return new FixVector3((Fix64)value.X, (Fix64)value.Y, (Fix64)value.Z);
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