using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class SphereCollider3D : SharpCollider3D
    {
        public Fix64 Radius => (Fix64)radius;
        [Export] protected float radius;
        
        public override void _Ready()
        {
            base._Ready();
            Shape = CollisionType3D.Sphere;
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;

            Vector3 DirX = (Vector3)ParentNode.Right;
            Vector3 DirY = (Vector3)ParentNode.Up;
            Vector3 DirZ = (Vector3)ParentNode.Forward;

            DebugDraw3D.DrawSimpleSphere((Vector3)Center, DirX, DirY, DirZ, (float)Radius + 0.005f, debugColor);
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateSphereBoundingBox();
        }

        public override void UpdatePoints(SharpBody3D body)
        {
            base.UpdatePoints(body);
        }

		public override FixVector3 Support(FixVector3 direction)
		{
			FixVector3 NormalizedDirection = FixVector3.Normalize(direction);
			return Center + Radius * NormalizedDirection;
		}

        public FixVolume UpdateSphereBoundingBox()
        {
            Fix64 minX = Center.x - Radius;
            Fix64 minY = Center.y - Radius;
            Fix64 minZ = Center.z - Radius;
            Fix64 maxX = Center.x + Radius;
            Fix64 maxY = Center.y + Radius;
            Fix64 maxZ = Center.z + Radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }
}
