using Godot;
using FixMath.NET;
using SharpCollisions.Sharp3D;

[Tool] [GlobalClass]
public partial class BallPhysics3D : SharpBody3D
{
    private FixVector3 Direction;

    public override void _Ready()
    {
        base._Ready();
        Direction = FixVector3.Normalize(FixVector3.Zero -  FixedPosition);
        Direction.y = Fix64.Zero;
    }


    public override void _FixedProcess(Fix64 delta)
    {
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Right) > 0) Direction.x = Fix64.NegativeOne;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Left) > 0) Direction.x = Fix64.One;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Forward) > 0) Direction.z = Fix64.NegativeOne;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Back) > 0) Direction.z = Fix64.One;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Below) > 0) Direction.y = Fix64.Abs(Direction.y) * (Fix64)0.95;
        else Direction.y -= (Fix64)9.81 * delta;

        SetLinearVelocity(Direction * Fix64.Two);
    }
}
