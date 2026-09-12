using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [System.Serializable]
    public class CollisionResult3D
    {
        public SharpBody3D CollidedWith;
        public int ColliderA;
        public int ColliderB;
        public FixVector3 Normal;
        public FixVector3 Depth;
        public FixVector3 ContactPoint;

        public SharpCollider3D Collider => CollidedWith.GetCollider(ColliderB);

        public CollisionResult3D() {}

        public CollisionResult3D(SharpBody3D body, int colA, int colB, FixVector3 normal, FixVector3 depth, FixVector3 contact)
        {
            CollidedWith = body;
            ColliderA = colA;
            ColliderB = colB;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
    }
}
