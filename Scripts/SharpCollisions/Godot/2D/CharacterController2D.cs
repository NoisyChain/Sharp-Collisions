using FixMath.NET;
using Godot;
using System;

namespace SharpCollisions
{
    public class CharacterController2D : PhysicsBody2D
    {
        [Export(PropertyHint.Range, "0, 89")]
        public int SlopeLimit = 45;
        public bool IsOnGround => body.collider.collisionFlags.Below;
        public bool IsOnCeiling => body.collider.collisionFlags.Above;
        public bool IsOnWalls => body.collider.collisionFlags.Walls;

        public FixVector2 GroundNormal => GetGroundNormal();
        public Fix64 GroundAngle => FixVector2.AngleDegrees(GroundNormal, body.Up);
        public bool IsWalkableSlope => CompareGroundAngle((Fix64)SlopeLimit);

        private FixVector2 VerticalVelocity;
        private FixVector2 LateralVelocity;
        private FixVector2 UpVector = FixVector2.Up;

        private Spatial contactPoint;

        public override void _Ready()
        {
            base._Ready();
            contactPoint = GetNode<Spatial>("Debug/Contact");
        }

        public override void _SharpProcess(Fix64 delta)
        {
            if (IsOnGround && IsWalkableSlope)
            {
                UpVector = -GroundNormal;
                VerticalVelocity = -UpVector;
                if (Input.IsActionPressed("ui_left") && !Input.IsActionPressed("ui_right"))
                    LateralVelocity = FixVector2.Left;
                if (!Input.IsActionPressed("ui_left") && Input.IsActionPressed("ui_right"))
                    LateralVelocity = FixVector2.Right;
                if (!Input.IsActionPressed("ui_left") && !Input.IsActionPressed("ui_right"))
                    LateralVelocity = FixVector2.Zero;

                LateralVelocity = FixVector2.Normalize(FixVector2.Reject(LateralVelocity, UpVector)) * (Fix64)2;

                if (Input.IsActionPressed("ui_up"))
                {
                    UpVector = FixVector2.Up;
                    VerticalVelocity = UpVector * (Fix64)5;
                }
                contactPoint.Visible = true;
                contactPoint.GlobalTranslation = (Vector3)body.Collisions[0].ContactPoint;
            }
            else
            {
                UpVector = FixVector2.Up;
                VerticalVelocity -= UpVector * (Fix64)9.81 * delta;
                contactPoint.Visible = false;
            }
            
            FixVector2 finalVelocity = LateralVelocity + VerticalVelocity;
		    
            if (FixVector2.Distance(body.position, FixVector2.Zero) > (Fix64)10)
            { 
                LateralVelocity = FixVector2.Zero;
                VerticalVelocity = FixVector2.Zero;
                body.MoveTo(FixVector2.Zero);
            }

            /*if (Input.IsActionPressed("ui_page_up"))
            {
                body.RotateDegrees((Fix64)90 * delta);
            }
            if (Input.IsActionPressed("ui_page_down"))
            {
                body.RotateDegrees((Fix64)(-90) * delta);
            }
            Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_down", "ui_up");
		    FixVector2 FixInput = inputDir != Vector2.Zero ? FixVector2.Normalize((FixVector2)inputDir) : FixVector2.Zero;
		    FixVector2 finalVelocity = FixInput * (Fix64)2;*/

            body.SetVelocity(finalVelocity);

            //debug2.Visible = body.collider.collisionFlags.Any;
        }

        public FixVector2 GetGroundNormal()
        {
            FixVector2 Normal = FixVector2.Zero;

            if (body.Collisions.Count > 0)
            {
                if (FixVector2.Dot(body.Collisions[0].Normal, body.Down) > Fix64.ETA)
                    Normal = body.Collisions[0].Normal;

                if (body.Collisions.Count > 1)
                {
                    for (int c = 1; c < body.Collisions.Count; c++)
                    {
                        if (FixVector2.Dot(body.Collisions[c].Normal, body.Down) > Fix64.ETA && 
                            FixVector2.Dot(body.Collisions[c].Normal, FixVector2.Normalize(LateralVelocity)) > Fix64.Zero)
                        {
                            Normal = body.Collisions[c].Normal;
                        }
                    }
                }
            }

            return Normal;
        }

        public bool CompareGroundAngle(Fix64 Threshold)
        {
            Fix64 HalfThreshold = (Threshold + Fix64.One) / (Fix64) 2;
            return GroundAngle >= (Fix64)90 - HalfThreshold && GroundAngle <= (Fix64)90 + HalfThreshold;
        }

        public override void OnBeginOverlap(SharpBody2D other)
        {
            if (!body.isTrigger)
                GD.Print("Entered Collision!");
        }

        public override void OnOverlap(SharpBody2D other)
        {
            //GD.Print(body.Collisions.Count);
        }

        public override void OnEndOverlap(SharpBody2D other)
        {
            if (!body.isTrigger)
                GD.Print("Exited Collision!");
        }
    }
}
