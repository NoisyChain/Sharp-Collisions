using Godot;
using FixMath.NET;
using SharpCollisions.Sharp3D;

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
        if (Collider.collisionFlags.Right) Direction.x = Fix64.NegativeOne;
        if (Collider.collisionFlags.Left) Direction.x = Fix64.One;
        if (Collider.collisionFlags.Forward) Direction.z = Fix64.NegativeOne;
        if (Collider.collisionFlags.Back) Direction.z = Fix64.One;
        if (Collider.collisionFlags.Below) Direction.y = Fix64.Abs(Direction.y) * (Fix64)0.95;
        else Direction.y -= (Fix64)9.81 * delta;

        SetVelocity(Direction * Fix64.Two);
    }
}
