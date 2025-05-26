using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool]
    [GlobalClass]
    public partial class CapsuleCollider2D : SharpCollider2D
    {
        public Fix64 Radius;
        public Fix64 Height;
        public FixVector2 RawUpperPoint;
        public FixVector2 RawLowerPoint;
        public FixVector2 UpperPoint;
        public FixVector2 LowerPoint;

        [Export] protected int radius;
        [Export] protected int height;

        public override void Initialize()
        {
            base.Initialize();
            Radius = (Fix64)radius / SharpNode.NodeScale;
            Height = (Fix64)height / SharpNode.NodeScale;
            Shape = CollisionType2D.Capsule;
            CreateCapsulePoints();
        }

        public override bool CollisionDetection(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            if (other.Shape == CollisionType2D.AABB) return false;

            if (other.Shape == CollisionType2D.Circle)
                return CapsuleToCircleCollision(this, other as CircleCollider2D, out Normal, out Depth, out ContactPoint);
            else if (other.Shape == CollisionType2D.Capsule)
                return CapsuleToCapsuleCollision(this, other as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
            else if (other.Shape == CollisionType2D.Polygon)
            {
                PolygonCollider2D pol = other as PolygonCollider2D;
                return pol.GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
            }
            return false;
        }

        private void CreateCapsulePoints()
        {
            FixVector2 CapsuleDirection = new FixVector2(Fix64.Zero, Height - Radius);

            RawUpperPoint = CapsuleDirection;
            RawLowerPoint = -CapsuleDirection;
        }

        private void UpdateCapsulePoints(FixVector2 position, Fix64 rotation)
        {
            UpperPoint = FixVector2.Rotate(RawUpperPoint, RotationOffset);
            LowerPoint = FixVector2.Rotate(RawLowerPoint, RotationOffset);
            UpperPoint = FixVector2.Transform(UpperPoint + PositionOffset, position, rotation);
            LowerPoint = FixVector2.Transform(LowerPoint + PositionOffset, position, rotation);
        }

        public override void DebugDrawShapes(SharpBody2D reference)
        {
            if (!Active) return;
            if (!DrawDebug) return;

            Vector3 Dir = (Vector3)FixVector2.Normalize(UpperPoint - LowerPoint);

            float inflatedRadius = (float)Radius + 0.005f;

            Vector3 LineNormal = (Vector3)FixVector2.GetNormal(UpperPoint, LowerPoint);
            Vector3 LineSpacing = LineNormal * inflatedRadius;

            DebugDraw3D.DrawHalfSphereY((Vector3)UpperPoint, LineNormal, Dir, Vector3.Zero, false, inflatedRadius, debugColor);
            DebugDraw3D.DrawHalfSphereY((Vector3)LowerPoint, LineNormal, Dir, Vector3.Zero, true, inflatedRadius, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint, (Vector3)LowerPoint, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing, (Vector3)LowerPoint + LineSpacing, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing, (Vector3)LowerPoint - LineSpacing, debugColor);
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;

            Color finalColor = selected ? selectedColor : debugColor;

            float scaledHeight = (float)height / SharpNode.nodeScale;
            float scaledRadius = (float)radius / SharpNode.nodeScale;

            Vector2 scaledPosOffset = (Vector2)positionOffset / SharpNode.nodeScale;
            float scaledRotOffset = rotationOffset / SharpNode.nodeRotation;

            Vector2 upPoint = scaledPosOffset + (Vector2.Up * (scaledHeight - scaledRadius));
            Vector2 lowPoint = scaledPosOffset - (Vector2.Up * (scaledHeight - scaledRadius));

            Vector2 upperPoint0 = SharpHelpers.Rotate2D(upPoint, Mathf.DegToRad(scaledRotOffset));
            Vector2 lowerPoint0 = SharpHelpers.Rotate2D(lowPoint, Mathf.DegToRad(scaledRotOffset));
            Vector2 upperPoint = SharpHelpers.Transform2D(upperPoint0, reference.GlobalPosition, reference.GlobalRotation.Z);
            Vector2 lowerPoint = SharpHelpers.Transform2D(lowerPoint0, reference.GlobalPosition, reference.GlobalRotation.Z);

            Vector2 direction = (upperPoint - lowerPoint).Normalized();

            float inflatedRadius = scaledRadius + 0.005f;

            Vector2 nor = SharpHelpers.GetNormal2D(upperPoint, lowerPoint);
            Vector3 LineNormal = new Vector3(nor.X, nor.Y, 0);
            Vector3 Dir = new Vector3(direction.X, direction.Y, 0);
            Vector3 LineSpacing = LineNormal * inflatedRadius;
            Vector3 Up = new Vector3(upperPoint.X, upperPoint.Y, 0);
            Vector3 Low = new Vector3(lowerPoint.X, lowerPoint.Y, 0);

            DebugDraw3D.DrawHalfSphereY(Up, LineNormal, Dir, Vector3.Zero, false, inflatedRadius, finalColor);
            DebugDraw3D.DrawHalfSphereY(Low, LineNormal, Dir, Vector3.Zero, true, inflatedRadius, finalColor);
            DebugDraw3D.DrawLine(Up, Low, finalColor);
            DebugDraw3D.DrawLine(Up + LineSpacing, Low + LineSpacing, finalColor);
            DebugDraw3D.DrawLine(Up - LineSpacing, Low - LineSpacing, finalColor);
        }

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdateCapsuleBoundingBox();
        }

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            UpdateCapsulePoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }

        public override FixVector2 Support(FixVector2 direction)
        {
            FixVector2 NormalizedDirection = FixVector2.Normalize(direction);
            Fix64 Dy = FixVector2.Dot(NormalizedDirection, UpperPoint - LowerPoint);

            if (Dy == Fix64.Zero) return Center + Radius * NormalizedDirection;
            else return (Dy < Fix64.Zero ? LowerPoint : UpperPoint) + Radius * NormalizedDirection;
        }

        public FixRect UpdateCapsuleBoundingBox()
        {
            Fix64 minX = UpperPoint.x - Radius;
            Fix64 minY = UpperPoint.y - Radius;
            Fix64 maxX = UpperPoint.x + Radius;
            Fix64 maxY = UpperPoint.y + Radius;

            if (LowerPoint.x < UpperPoint.x)
                minX = LowerPoint.x - Radius;
            if (LowerPoint.x > UpperPoint.x)
                maxX = LowerPoint.x + Radius;
            if (LowerPoint.y < UpperPoint.y)
                minY = LowerPoint.y - Radius;
            if (LowerPoint.y > UpperPoint.y)
                maxY = LowerPoint.y + Radius;

            return new FixRect(minX, minY, maxX, maxY);
        }

        public bool CapsuleToCapsuleCollision(CapsuleCollider2D colliderA, CapsuleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            LineToLineDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector2 r1, out FixVector2 r2);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 distance = FixVector2.Distance(r1, r2);

            bool collision = distance <= radii;

            if (collision)
            {
                Normal = FixVector2.Normalize(r2 - r1);
                Depth = Normal * (radii - distance);
                ContactPoint = CapsuleContactPoint
                (
                    colliderA.UpperPoint, colliderA.LowerPoint,
                    colliderB.UpperPoint, colliderB.LowerPoint,
                    colliderA.Radius, colliderB.Radius, Normal
                );
            }

            return collision;
        }

        public bool CapsuleToCircleCollision(CapsuleCollider2D colliderA, CircleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector2 CapsulePoint);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 distance = FixVector2.Distance(CapsulePoint, colliderB.Center);

            bool collision = distance <= radii;

            if (collision)
            {
                Normal = FixVector2.Normalize(colliderB.Center - CapsulePoint);
                Depth = Normal * (radii - distance);
                ContactPoint = CircleContactPoint(CapsulePoint, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
            }

            return collision;
        }

        public FixVector2 CircleContactPoint(FixVector2 centerA, Fix64 radiusA, FixVector2 centerB, Fix64 radiusB, FixVector2 direction)
        {
            FixVector2 ContactA = centerA + (direction * radiusA);
            FixVector2 ContactB = centerB - (direction * radiusB);
            return (ContactA + ContactB) / Fix64.Two;
        }

        public FixVector2 CapsuleContactPoint(FixVector2 upperA, FixVector2 lowerA, FixVector2 upperB, FixVector2 lowerB, Fix64 radiusA, Fix64 radiusB, FixVector2 direction)
        {
            /*FixVector2 contact1 = FixVector2.Zero;
            FixVector2 contact2 = FixVector2.Zero;

            Fix64 minDistSq = Fix64.MaxValue;

            LineToLineDistance(upperA, lowerA, upperB, lowerB, out FixVector2 r1, out FixVector2 r2);
            Fix64 distSq = FixVector2.DistanceSq(r2, r1);

            if (Fix64.Approximate(distSq, minDistSq))
            {
                if (!FixVector2.Approximate(r1, contact1))
                {
                    contact2 = r1;
                }
            }
            else if (distSq < minDistSq)
            {
                minDistSq = distSq;
                contact1 = r1;
            }

            LineToLineDistance(lowerB, upperB, lowerA, upperA, out r1, out r2);
            distSq = FixVector2.DistanceSq(r2, r1);

            if (Fix64.Approximate(distSq, minDistSq))
            {
                if (!FixVector2.Approximate(r1, contact1))
                {
                    contact2 = r1;
                }
            }
            else if (distSq < minDistSq)
            {
                contact1 = r1;
            }

            return CircleContactPoint(contact1, radiusA, contact2, radiusB, direction);
            */

            LineToPointDistance(upperB, lowerB, upperA, out FixVector2 r1);
            LineToPointDistance(upperB, lowerB, lowerA, out FixVector2 r3);
            LineToPointDistance(upperA, lowerA, upperB, out FixVector2 r2);
            LineToPointDistance(upperA, lowerA, lowerB, out FixVector2 r4);

            FixVector2 p1 = r1 - (direction * radiusB);
            FixVector2 p2 = r2 + (direction * radiusA);
            FixVector2 p3 = r3 - (direction * radiusB);
            FixVector2 p4 = r4 + (direction * radiusA);

            FixVector2 contact1 = (p1 + p2) / Fix64.Two;
            FixVector2 contact2 = (p3 + p4) / Fix64.Two;
            return (contact1 + contact2) / Fix64.Two;
        }
    }
}
