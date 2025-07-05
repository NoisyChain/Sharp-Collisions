using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool]
    [GlobalClass]
    public partial class CapsuleCollider3D : SharpCollider3D
    {
        public Fix64 Radius;
        public Fix64 Height;
        public FixVector3 RawUpperPoint;
        public FixVector3 RawLowerPoint;
        public FixVector3 UpperPoint;
        public FixVector3 LowerPoint;

        [Export] protected int radius;
        [Export] protected int height;

        public override void Initialize()
        {
            base.Initialize();
            Radius = (Fix64)radius / SharpNode.NodeScale;
            Height = (Fix64)height / SharpNode.NodeScale;
            Shape = CollisionType3D.Capsule;
            CreateCapsulePoints();
        }

        public override bool CollisionDetection(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            if (other.Shape == CollisionType3D.AABB) return false;

            if (other.Shape == CollisionType3D.Sphere)
                return CapsuleToSphereCollision(this, other as SphereCollider3D, out Normal, out Depth, out ContactPoint);
            else if (other.Shape == CollisionType3D.Capsule)
                return CapsuleToCapsuleCollision(this, other as CapsuleCollider3D, out Normal, out Depth, out ContactPoint);
            else if (other.Shape == CollisionType3D.Polygon)
            {
                ConvexShapeCollider3D pol = other as ConvexShapeCollider3D;
                return pol.GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
            }
            return false;
        }

        private void CreateCapsulePoints()
        {
            FixVector3 CapsuleDirection = new FixVector3(Fix64.Zero, Height - Radius, Fix64.Zero);

            RawUpperPoint = CapsuleDirection;
            RawLowerPoint = -CapsuleDirection;
        }

        private void UpdateCapsulePoints(FixVector3 position, FixVector3 rotation)
        {
            UpperPoint = FixVector3.Rotate(RawUpperPoint, RotationOffset);
            LowerPoint = FixVector3.Rotate(RawLowerPoint, RotationOffset);
            UpperPoint = FixVector3.Transform(UpperPoint + PositionOffset, position, rotation);
            LowerPoint = FixVector3.Transform(LowerPoint + PositionOffset, position, rotation);
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!Active) return;
            if (!DrawDebug) return;

            Vector3 DirY = (Vector3)FixVector3.Normalize(UpperPoint - LowerPoint);
            Vector3 DirX = SharpHelpers.GetLineNormal3D(DirY, (Vector3)reference.Forward, (Vector3)reference.Up);//normal.Normalized();
            Vector3 DirZ = DirX.Cross(DirY);

            float inflatedRadius = (float)Radius + 0.005f;

            Vector3 LineSpacing1 = DirX * inflatedRadius;
            Vector3 LineSpacing2 = DirZ * inflatedRadius;

            DebugDraw3D.DrawHalfSphereY((Vector3)UpperPoint, DirX, DirY, DirZ, false, inflatedRadius, debugColor);
            DebugDraw3D.DrawHalfSphereY((Vector3)LowerPoint, DirX, DirY, DirZ, true, inflatedRadius, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing1, (Vector3)LowerPoint + LineSpacing1, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing1, (Vector3)LowerPoint - LineSpacing1, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing2, (Vector3)LowerPoint + LineSpacing2, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing2, (Vector3)LowerPoint - LineSpacing2, debugColor);
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;

            Color finalColor = selected ? selectedColor : debugColor;

            float scaledHeight = (float)height / SharpNode.nodeScale;
            float scaledRadius = (float)radius / SharpNode.nodeScale;

            Vector3 scaledPosOffset = (Vector3)positionOffset / SharpNode.nodeScale;
            Vector3 scaledRotOffset = (Vector3)rotationOffset / SharpNode.nodeRotation;

            Vector3 upPoint = scaledPosOffset + (Vector3.Up * (scaledHeight - scaledRadius));
            Vector3 lowPoint = scaledPosOffset - (Vector3.Up * (scaledHeight - scaledRadius));

            Vector3 upperPoint0 = SharpHelpers.RotateDeg3D(upPoint, scaledRotOffset);
            Vector3 lowerPoint0 = SharpHelpers.RotateDeg3D(lowPoint, scaledRotOffset);
            Vector3 upperPoint = SharpHelpers.Transform3D(upperPoint0, reference.GlobalPosition, reference.GlobalRotation);
            Vector3 lowerPoint = SharpHelpers.Transform3D(lowerPoint0, reference.GlobalPosition, reference.GlobalRotation);

            Vector3 DirY = (upperPoint - lowerPoint).Normalized();
            Vector3 DirX = SharpHelpers.GetLineNormal3D(DirY, reference.Basis.Z, reference.Basis.Y);//normal.Normalized();
            Vector3 DirZ = DirX.Cross(DirY);

            float inflatedRadius = scaledRadius + 0.005f;

            Vector3 LineSpacing1 = DirX * inflatedRadius;
            Vector3 LineSpacing2 = DirZ * inflatedRadius;

            DebugDraw3D.DrawHalfSphereY(upperPoint, DirX, DirY, DirZ, false, inflatedRadius, finalColor);
            DebugDraw3D.DrawHalfSphereY(lowerPoint, DirX, DirY, DirZ, true, inflatedRadius, finalColor);
            DebugDraw3D.DrawLine(upperPoint + LineSpacing1, lowerPoint + LineSpacing1, finalColor);
            DebugDraw3D.DrawLine(upperPoint - LineSpacing1, lowerPoint - LineSpacing1, finalColor);
            DebugDraw3D.DrawLine(upperPoint + LineSpacing2, lowerPoint + LineSpacing2, finalColor);
            DebugDraw3D.DrawLine(upperPoint - LineSpacing2, lowerPoint - LineSpacing2, finalColor);
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateCapsuleBoundingBox();
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            UpdateCapsulePoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }

        public override FixVector3 Support(FixVector3 direction)
        {
            FixVector3 NormalizedDirection = FixVector3.Normalize(direction);
            Fix64 Dy = FixVector3.Dot(NormalizedDirection, UpperPoint - LowerPoint);

            if (Dy == Fix64.Zero) return Center + Radius * NormalizedDirection;
            else return (Dy < Fix64.Zero ? LowerPoint : UpperPoint) + Radius * NormalizedDirection;
        }

        public FixVolume UpdateCapsuleBoundingBox()
        {
            Fix64 minX = UpperPoint.x - Radius;
            Fix64 minY = UpperPoint.y - Radius;
            Fix64 minZ = UpperPoint.z - Radius;
            Fix64 maxX = UpperPoint.x + Radius;
            Fix64 maxY = UpperPoint.y + Radius;
            Fix64 maxZ = UpperPoint.z + Radius;

            if (LowerPoint.x < UpperPoint.x)
                minX = LowerPoint.x - Radius;
            if (LowerPoint.x > UpperPoint.x)
                maxX = LowerPoint.x + Radius;
            if (LowerPoint.y < UpperPoint.y)
                minY = LowerPoint.y - Radius;
            if (LowerPoint.y > UpperPoint.y)
                maxY = LowerPoint.y + Radius;
            if (LowerPoint.z < UpperPoint.z)
                minZ = LowerPoint.z - Radius;
            if (LowerPoint.z > UpperPoint.z)
                maxZ = LowerPoint.z + Radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public bool CapsuleToCapsuleCollision(CapsuleCollider3D colliderA, CapsuleCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            LineToLineDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector3 r1, out FixVector3 r2);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
            Fix64 distance = FixVector3.DistanceSq(r1, r2);

            bool collision = distance <= radiiSq;

            if (collision)
            {
                Normal = FixVector3.Normalize(r2 - r1);
                Depth = Normal * Fix64.Abs(radiiSq - distance);
                ContactPoint = CapsuleContactPoint
                (
                    colliderA.UpperPoint, colliderA.LowerPoint,
                    colliderB.UpperPoint, colliderB.LowerPoint,
                    colliderA.Radius, colliderB.Radius, Normal
                );
            }

            return collision;
        }

        public bool CapsuleToSphereCollision(CapsuleCollider3D colliderA, SphereCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector3 CapsulePoint);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
            Fix64 distance = FixVector3.DistanceSq(CapsulePoint, colliderB.Center);

            bool collision = distance <= radiiSq;

            if (collision)
            {
                Normal = FixVector3.Normalize(colliderB.Center - CapsulePoint);
                Depth = Normal * Fix64.Abs(radiiSq - distance);
                ContactPoint = SphereContactPoint(CapsulePoint, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
            }

            return collision;
        }

        public FixVector3 SphereContactPoint(FixVector3 centerA, Fix64 radiusA, FixVector3 centerB, Fix64 radiusB, FixVector3 direction)
        {
            FixVector3 ContactA = centerA + (direction * radiusA);
            FixVector3 ContactB = centerB - (direction * radiusB);
            return (ContactA + ContactB) / Fix64.Two;
        }

        public FixVector3 CapsuleContactPoint(FixVector3 upperA, FixVector3 lowerA, FixVector3 upperB, FixVector3 lowerB, Fix64 radiusA, Fix64 radiusB, FixVector3 direction)
        {
            /*
            FixVector3 contact1 = FixVector3.Zero;
            FixVector3 contact2 = FixVector3.Zero;

            Fix64 minDistSq = Fix64.MaxValue;

            LineToLineDistance(upperA, lowerA, upperB, lowerB, out FixVector3 r1, out FixVector3 r2);
            Fix64 distSq = FixVector3.DistanceSq(r2, r1);

            if (Fix64.Approximate(distSq, minDistSq))
            {
                if (!FixVector3.Approximate(r1, contact1))
                {
                    contact2 = r1;
                }
            }
            else if (distSq < minDistSq)
            {
                minDistSq = distSq;
                contact1 = r1;
            }

            LineToLineDistance(lowerA, upperA, upperB, lowerB, out FixVector3 r3, out FixVector3 r4);
            distSq = FixVector3.DistanceSq(r2, r1);

            if (Fix64.Approximate(distSq, minDistSq))
            {
                if (!FixVector3.Approximate(r1, contact1))
                {
                    contact2 = r1;
                }
            }
            else if (distSq < minDistSq)
            {
                contact1 = r1;
            }

            return SphereContactPoint(contact1, radiusA, contact2, radiusB, direction);
            */

            LineToPointDistance(upperB, lowerB, upperA, out FixVector3 r1);
            LineToPointDistance(upperB, lowerB, lowerA, out FixVector3 r3);
            LineToPointDistance(upperA, lowerA, upperB, out FixVector3 r2);
            LineToPointDistance(upperA, lowerA, lowerB, out FixVector3 r4);

            FixVector3 p1 = r1 - (direction * radiusB);
            FixVector3 p2 = r2 + (direction * radiusA);
            FixVector3 p3 = r3 - (direction * radiusB);
            FixVector3 p4 = r4 + (direction * radiusA);

            FixVector3 contact1 = (p1 + p2) / Fix64.Two;
            FixVector3 contact2 = (p3 + p4) / Fix64.Two;
            return (contact1 + contact2) / Fix64.Two;
        }
    }
}
