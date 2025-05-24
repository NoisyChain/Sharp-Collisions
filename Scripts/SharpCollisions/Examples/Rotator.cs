using Godot;
using FixMath.NET;
using SharpCollisions.Sharp3D;

[Tool] [GlobalClass]
public partial class Rotator : SharpBody3D
{
    private FixVector3 _rotationSpeed => (FixVector3)RotationSpeed;
    [Export] private Vector3 RotationSpeed;
    public override void _FixedProcess(Fix64 delta)
    {
        SetAngularVelocity(_rotationSpeed * delta);
    }
}
