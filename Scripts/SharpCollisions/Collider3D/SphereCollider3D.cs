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
            Transform3D debugTransform = new(ParentNode.GlobalBasis, (Vector3)Center);
            DebugDraw.Sphere(debugTransform, (float)Radius, debugColor);
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
