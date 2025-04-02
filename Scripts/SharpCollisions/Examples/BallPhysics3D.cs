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
        if (Colliders[0].collisionFlags.Right) Direction.x = Fix64.NegativeOne;
        if (Colliders[0].collisionFlags.Left) Direction.x = Fix64.One;
        if (Colliders[0].collisionFlags.Forward) Direction.z = Fix64.NegativeOne;
        if (Colliders[0].collisionFlags.Back) Direction.z = Fix64.One;
        if (Colliders[0].collisionFlags.Below) Direction.y = Fix64.Abs(Direction.y) * (Fix64)0.95;
        else Direction.y -= (Fix64)9.81 * delta;

        SetVelocity(Direction * Fix64.Two);
    }
}
