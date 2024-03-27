using FixMath.NET;
using Godot;


namespace SharpCollisions
{
    public class CharacterController3D : SharpBody3D
    {
        public override void _FixedProcess(Fix64 delta)
        {
            FixVector3 AddDegrees = new FixVector3(Fix64.Zero, (Fix64)90, Fix64.Zero);
            if (Input.IsActionPressed("ui_home"))
            {
                RotateDegrees(AddDegrees * delta);
            }
            if (Input.IsActionPressed("ui_end"))
            {
                RotateDegrees(-AddDegrees * delta);
            }

            Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		    FixVector2 FixInput = inputDir != Vector2.Zero ? 
                        FixVector2.Normalize((FixVector2)inputDir) : FixVector2.Zero;
		    
            FixVector3 Input3D = new FixVector3(FixInput.x, Fix64.Zero, FixInput.y);
            if (Input.IsActionPressed("ui_page_up"))
                Input3D.y = Fix64.One;
            else if (Input.IsActionPressed("ui_page_down"))
                Input3D.y = Fix64.NegativeOne;
            
            FixVector3 finalVelocity = Input3D * (Fix64)2;

            //SetRotation(correctedRotation);
            SetVelocity(finalVelocity);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            //foreach (CollisionManifold2D col in Collisions)
                //DebugDrawCS.DrawSphere((Vector3)col.ContactPoint, 0.05f, new Color(1,1,0));
        }
    }
}
