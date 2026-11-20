using Godot;
using FixMath.NET;
using SharpCollisions.Sharp2D;

[Tool] [GlobalClass]
public partial class BallPhysics2D : SharpBody2D
{
    private FixVector2 Direction;

    public override void _Ready()
    {
        base._Ready();
        if (!Engine.IsEditorHint()) ProcessMode = ProcessModeEnum.Disabled;
        Direction = FixVector2.Normalize(FixVector2.Zero - FixedPosition);
    }

    public override void OnOverlap(CollisionResult2D collision)
    {
        //Direction = -collision.Normal;
    }


    public override void _FixedProcess(Fix64 delta)
    {
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Right) > 0) Direction.x = Fix64.NegativeOne;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Left) > 0) Direction.x = Fix64.One;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Above) > 0) Direction.y = Fix64.NegativeOne;
        if ((GetCollider(0).collisionFlags & SharpCollisions.CollisionFlags.Below) > 0) Direction.y = Fix64.One;

        SetLinearVelocity(Direction * Fix64.Two);
    }
}
