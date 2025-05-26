using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class AABBCollider2D : SharpCollider2D
    {
        public FixVector2 Extents;

        [Export] private Vector2I extents = Vector2I.One;

        public override void Initialize()
        {
            base.Initialize();
            Extents = new FixVector2(
                (Fix64)extents.X / SharpNode.NodeScale,
                (Fix64)extents.Y / SharpNode.NodeScale
            );
            Shape = CollisionType2D.AABB;
        }

        public override bool CollisionDetection(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			if (other.Shape == CollisionType2D.AABB)
                return AABBtoAABBCollision(this, other as AABBCollider2D, out Normal, out Depth, out ContactPoint);
            
            return false;
		}

        public override void DebugDrawShapes(SharpBody2D reference)
        {
            if (!Active) return;
            if (!DrawDebug) return;
            
            Vector2 fCenter = (Vector2)Center;
            Vector2 fExtents = (Vector2)Extents;

            float minX = fCenter.X - fExtents.X;
            float minY = fCenter.Y - fExtents.Y;
            float maxX = fCenter.X + fExtents.X;
            float maxY = fCenter.Y + fExtents.Y;

            Vector3 point1 = new Vector3(minX, minY, 0);
            Vector3 point2 = new Vector3(maxX, minY, 0);
            Vector3 point3 = new Vector3(maxX, maxY, 0);
            Vector3 point4 = new Vector3(minX, maxY, 0);

            DebugDraw3D.DrawLine(point1, point2, debugColor);
            DebugDraw3D.DrawLine(point2, point3, debugColor);
            DebugDraw3D.DrawLine(point3, point4, debugColor);
            DebugDraw3D.DrawLine(point4, point1, debugColor);
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;

            Color finalColor = selected ? selectedColor : debugColor;

            Vector3 scaledPosOffset = new Vector3(positionOffset.X, positionOffset.Y, 0) / SharpNode.nodeScale;
            Vector3 scaledRotOffset = new Vector3(0, 0, rotationOffset) / SharpNode.nodeRotation;
            Vector3 scaledExtents = new Vector3(extents.X * 2, extents.Y * 2, 0.1f) / SharpNode.nodeScale;

            Vector3 rotPos = SharpHelpers.RotateDeg3D(scaledPosOffset, scaledRotOffset);
            Vector3 newPos = SharpHelpers.Transform3D(rotPos, reference.GlobalPosition, reference.GlobalRotation);

            DebugDraw3D.DrawBox(newPos, Quaternion.Identity, scaledExtents, finalColor, true);
            
            if (selected) DebugDraw3D.DrawGizmo(reference.Transform, finalColor, true);
        }

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdateAABBBoundingBox();
        }

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            base.UpdatePoints(position, rotation);
        }

        public FixRect UpdateAABBBoundingBox()
        {
            Fix64 minX = Center.x - Extents.x;
            Fix64 minY = Center.y - Extents.y;
            Fix64 maxX = Center.x + Extents.x;
            Fix64 maxY = Center.y + Extents.y;

            return new FixRect(minX, minY, maxX, maxY);
        }

        public FixVector2 FindAABBNormals(AABBCollider2D colliderA, AABBCollider2D colliderB)
        {
            FixVector2 finalNormal;
            FixVector2 length = colliderB.Center - colliderA.Center;

            Fix64 ExtentsX = colliderB.Extents.x + colliderA.Extents.x;
            Fix64 ExtentsY = colliderB.Extents.y + colliderA.Extents.y;

            // calculate normal of collided surface
            if (Fix64.Abs(length.x) + ExtentsY > Fix64.Abs(length.y) + ExtentsX)
            {
                if (colliderA.Center.x < colliderB.Center.x)
                {
                    finalNormal = FixVector2.Right;
                } 
                else
                {
                    finalNormal = FixVector2.Left;
                }
            }
            else
            {
                if (colliderA.Center.y < colliderB.Center.y)
                {
                    finalNormal = FixVector2.Up;
                }
                else
                {
                    finalNormal = FixVector2.Down;
                }
            }
            return finalNormal;
        }

		public bool AABBtoAABBCollision(AABBCollider2D colliderA, AABBCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            bool collisionX = colliderA.Center.x - colliderA.Extents.x <= colliderB.Center.x + colliderB.Extents.x &&
                colliderA.Center.x + colliderA.Extents.x >= colliderB.Center.x - colliderB.Extents.x;

            bool collisionY = colliderA.Center.y - colliderA.Extents.y <= colliderB.Center.y + colliderB.Extents.y &&
                colliderA.Center.y + colliderA.Extents.y >= colliderB.Center.y - colliderB.Extents.y;

            if (collisionX && collisionY)
            {
                ContactPoint = AABBContactPoint(colliderA, colliderB);

                FixVector2 length = colliderB.Center - colliderA.Center;

                FixVector2 newDepth = FixVector2.Zero;
                newDepth.x = colliderA.Extents.x + colliderB.Extents.x;
                newDepth.y = colliderA.Extents.y + colliderB.Extents.y;
                newDepth.x -= Fix64.Abs(length.x);
                newDepth.y -= Fix64.Abs(length.y);
                Normal = FindAABBNormals(colliderA, colliderB);
                Depth = Normal * newDepth;
            }

            return collisionX && collisionY;
        }

        public FixVector2 AABBContactPoint(AABBCollider2D A, AABBCollider2D B)
        {
            Fix64 minPointX = Fix64.Min(A.Center.x + A.Extents.x, B.Center.x + B.Extents.x);
            Fix64 maxPointX = Fix64.Max(A.Center.x - A.Extents.x, B.Center.x - B.Extents.x);
            Fix64 minPointY = Fix64.Min(A.Center.y + A.Extents.y, B.Center.y + B.Extents.y);
            Fix64 maxPointY = Fix64.Max(A.Center.y - A.Extents.y, B.Center.y - B.Extents.y);
            Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
            Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
            return new FixVector2(mediantX, mediantY);
        }
    }
}
