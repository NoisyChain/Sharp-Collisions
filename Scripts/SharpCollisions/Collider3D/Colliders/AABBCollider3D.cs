using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class AABBCollider3D : SharpCollider3D
    {
        public FixVector3 Extents;

        [Export] private Vector3I extents = Vector3I.One;

        public override void Initialize()
        {
            base.Initialize();
            Extents = new FixVector3(
                (Fix64)extents.X / SharpNode.NodeScale,
                (Fix64)extents.Y / SharpNode.NodeScale,
                (Fix64)extents.Z / SharpNode.NodeScale
            );
            Shape = CollisionType3D.AABB;
        }

        public override bool CollisionDetection(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			if (other.Shape == CollisionType3D.AABB)
                return AABBtoAABBCollision(this, other as AABBCollider3D, out Normal, out Depth, out ContactPoint);
            
            return false;
		}

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!Active) return;
            if (!DrawDebug) return;

            DebugDraw3D.DrawBox((Vector3)Center, Quaternion.Identity, (Vector3)Extents * 2, debugColor, true);
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;

            Color finalColor = selected ? selectedColor : debugColor;

            Vector3 pos = (Vector3)positionOffset / SharpNode.nodeScale;
            Vector3 newPos = SharpHelpers.Transform3D(pos, reference.GlobalPosition, reference.GlobalRotation);

            DebugDraw3D.DrawBox(newPos, Quaternion.Identity, ((Vector3)extents / SharpNode.nodeScale) * 2, finalColor, true);
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateAABBBoundingBox();
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            base.UpdatePoints(position, rotation);
        }

        public FixVolume UpdateAABBBoundingBox()
        {
            Fix64 minX = Center.x - Extents.x;
            Fix64 minY = Center.y - Extents.y;
            Fix64 minZ = Center.z - Extents.z;
            Fix64 maxX = Center.x + Extents.x;
            Fix64 maxY = Center.y + Extents.y;
            Fix64 maxZ = Center.z + Extents.z;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public FixVector3 FindAABBNormals(AABBCollider3D colliderA, AABBCollider3D colliderB)
        {
            FixVector3 finalNormal;
            FixVector3 length = colliderB.Center - colliderA.Center;

            Fix64 ExtentsX = colliderB.Extents.x + colliderA.Extents.x;
            Fix64 ExtentsY = colliderB.Extents.y + colliderA.Extents.y;
            Fix64 ExtentsZ = colliderB.Extents.z + colliderA.Extents.z;

            // calculate normal of collided surface
			if (Fix64.Abs(length.x) + ExtentsY > Fix64.Abs(length.y) + ExtentsX || 
				Fix64.Abs(length.z) + ExtentsY > Fix64.Abs(length.y) + ExtentsZ)
			{
				if (Fix64.Abs(length.x) + ExtentsZ > Fix64.Abs(length.z) + ExtentsX)
				{
					
					if (colliderA.Center.x < colliderB.Center.x)
					{
						finalNormal = FixVector3.Right;
					} 
					else
					{
						finalNormal = FixVector3.Left;
					}
				}
				else
				{
					if (colliderA.Center.z < colliderB.Center.z)
					{
						finalNormal = FixVector3.Forward;
					} 
					else
					{
						finalNormal = FixVector3.Back;
					}
				}
			}
			else
			{
				if (colliderA.Center.y < colliderB.Center.y)
				{
					finalNormal = FixVector3.Up;
				} 
				else
				{
					finalNormal = FixVector3.Down;
				}
			}
            return finalNormal;
        }

		public bool AABBtoAABBCollision(AABBCollider3D colliderA, AABBCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            bool collisionX = colliderA.Center.x - colliderA.Extents.x <= colliderB.Center.x + colliderB.Extents.x &&
                colliderA.Center.x + colliderA.Extents.x >= colliderB.Center.x - colliderB.Extents.x;

            bool collisionY = colliderA.Center.y - colliderA.Extents.y <= colliderB.Center.y + colliderB.Extents.y &&
                colliderA.Center.y + colliderA.Extents.y >= colliderB.Center.y - colliderB.Extents.y;
            
            bool collisionZ = colliderA.Center.z - colliderA.Extents.z <= colliderB.Center.z + colliderB.Extents.z &&
                colliderA.Center.z + colliderA.Extents.z >= colliderB.Center.z - colliderB.Extents.z;


            if (collisionX && collisionY && collisionZ)
            {
                ContactPoint = AABBContactPoint(colliderA, colliderB);

                FixVector3 length = colliderB.Center - colliderA.Center;

                FixVector3 newDepth = FixVector3.Zero;
                newDepth.x = colliderA.Extents.x + colliderB.Extents.x;
                newDepth.y = colliderA.Extents.y + colliderB.Extents.y;
                newDepth.z = colliderA.Extents.z + colliderB.Extents.z;
                newDepth.x -= Fix64.Abs(length.x);
                newDepth.y -= Fix64.Abs(length.y);
                newDepth.z -= Fix64.Abs(length.z);
                Normal = FindAABBNormals(colliderA, colliderB);
                Depth = Normal * newDepth;
            }

            return collisionX && collisionY && collisionZ;
        }

        public FixVector3 AABBContactPoint(AABBCollider3D A, AABBCollider3D B)
        {
            Fix64 minPointX = Fix64.Min(A.Center.x + A.Extents.x, B.Center.x + B.Extents.x);
            Fix64 maxPointX = Fix64.Max(A.Center.x - A.Extents.x, B.Center.x - B.Extents.x);
            Fix64 minPointY = Fix64.Min(A.Center.y + A.Extents.y, B.Center.y + B.Extents.y);
            Fix64 maxPointY = Fix64.Max(A.Center.y - A.Extents.y, B.Center.y - B.Extents.y);
            Fix64 minPointZ = Fix64.Min(A.Center.z + A.Extents.z, B.Center.z + B.Extents.z);
            Fix64 maxPointZ = Fix64.Max(A.Center.z - A.Extents.z, B.Center.z - B.Extents.z);
            Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
            Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
            Fix64 mediantZ = (minPointZ + maxPointZ) / Fix64.Two;
            return new FixVector3(mediantX, mediantY, mediantZ);
        }
    }
}
