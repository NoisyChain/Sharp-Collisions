using Godot;

namespace SharpCollisions
{
    public static class SharpHelpers
    {
        public static Vector3 GetNormal3D(Vector3 a, Vector3 b)
        {
            Vector3 edge = b - a;
            Vector3 direction = edge.Normalized();
            Vector3 axis = new Vector3(-direction.Y, direction.X, direction.Z);
            return axis.Normalized();
        }

        public static Vector3 Transform3D(Vector3 v, Vector3 refPosition, Vector3 refRotation)
        {
            Vector3 r = Rotate3D(v, refRotation);

            float tx = r.X + refPosition.X;
            float ty = r.Y + refPosition.Y;
            float tz = r.Z + refPosition.Z;

            return new Vector3(tx, ty, tz);
        }

        public static Vector3 Rotate3D(Vector3 v, Vector3 angle)
        {
            Vector3 RotRoll = Roll(v, angle.Z);
            Vector3 RotPitch = Pitch(RotRoll, angle.X);
            Vector3 RotYaw = Yaw(RotPitch, angle.Y);

            return RotYaw;
        }
        public static Vector3 RotateDeg3D(Vector3 v, Vector3 angle)
        {
            Vector3 RotRoll = Roll(v, Mathf.DegToRad(angle.Z));
            Vector3 RotPitch = Pitch(RotRoll, Mathf.DegToRad(angle.X));
            Vector3 RotYaw = Yaw(RotPitch, Mathf.DegToRad(angle.Y));

            return RotYaw;
        }
        /// <summary>
        ///Rotates the vector on the X axis
        /// </summary>
        public static Vector3 Pitch(Vector3 v, float angle)
        {
            Vector3 newVector = v;
            newVector.Y = v.Y * Mathf.Cos(angle) - v.Z * Mathf.Sin(angle);
            newVector.Z = v.Y * Mathf.Sin(angle) + v.Z * Mathf.Cos(angle);
            return newVector;
        }
        /// <summary>
        ///Rotates the vector on the Y axis
        /// </summary>
        public static Vector3 Yaw(Vector3 v, float angle)
        {
            Vector3 newVector = v;
            newVector.X = v.X * Mathf.Cos(angle) + v.Z * Mathf.Sin(angle);
            newVector.Z = v.X * -Mathf.Sin(angle) + v.Z * Mathf.Cos(angle);
            return newVector;
        }
        /// <summary>
        ///Rotates the vector on the Z axis
        /// </summary>
        public static Vector3 Roll(Vector3 v, float angle)
        {
            Vector3 newVector = v;
            newVector.X = v.X * Mathf.Cos(angle) - v.Y * Mathf.Sin(angle);
            newVector.Y = v.X * Mathf.Sin(angle) + v.Y * Mathf.Cos(angle);
            return newVector;
        }
        public static Vector3 VectorDegToRad(Vector3 angleDegrees)
        {
            return new Vector3(
                Mathf.DegToRad(angleDegrees.X),
                Mathf.DegToRad(angleDegrees.Y),
                Mathf.DegToRad(angleDegrees.Z)
            );
        }
    }
}