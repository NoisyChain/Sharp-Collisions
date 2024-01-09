using FixMath.NET;
using Godot;

namespace SharpCollisions
{
    [Tool]
    public class CharacterController2D : SharpBody2D
    {
        [Export(PropertyHint.Range, "0, 89")]
        public int SlopeLimit = 45;
        public bool IsOnGround => Collider.collisionFlags.Below;
        public bool IsOnCeiling => Collider.collisionFlags.Above;
        public bool IsOnWalls => Collider.collisionFlags.Walls;

        public FixVector2 GroundNormal => GetGroundNormal();
        public Fix64 GroundAngle => FixVector2.AngleDegrees(GroundNormal, Up);

        private FixVector2 VerticalVelocity;
        private FixVector2 LateralVelocity;
        private FixVector2 UpVector = FixVector2.Up;

        public override void _FixedProcess(Fix64 delta)
        {
            if (IsOnGround && IsWalkableSlope())
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
            }
            else
            {
                UpVector = FixVector2.Up;
                VerticalVelocity -= UpVector * (Fix64)9.81 * delta;
            }
            
            FixVector2 finalVelocity = LateralVelocity + VerticalVelocity;
		    
            if (FixVector2.Distance(Position, FixVector2.Zero) > (Fix64)10)
            { 
                LateralVelocity = FixVector2.Zero;
                VerticalVelocity = FixVector2.Zero;
                MoveTo(FixVector2.Zero);
            }

            SetVelocity(finalVelocity);

            //GD.Print(IsOnGround && IsWalkableSlope());
            foreach (CollisionManifold2D col in Collisions)
                DebugDrawCS.DrawSphere((Vector3)col.ContactPoint, 0.05f, new Color(1,1,0));

            /*if (Input.IsActionPressed("ui_page_up"))
            {
                RotateDegrees((Fix64)90 * delta);
            }
            if (Input.IsActionPressed("ui_page_down"))
            {
                RotateDegrees((Fix64)(-90) * delta);
            }
            Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_down", "ui_up");
		    FixVector2 FixInput = inputDir != Vector2.Zero ? 
                        FixVector2.Normalize((FixVector2)inputDir) : FixVector2.Zero;
            FixVector2 finalVelocity = FixInput * (Fix64)2;
            if (Input.IsActionPressed("ui_left"))
                RotateDegrees((Fix64)90 * delta);
            if (Input.IsActionPressed("ui_right"))
                RotateDegrees((Fix64)(-90) * delta);

            FixVector2 targetPos = Manager.GetBodyByIndex(1).Position;
            FixVector2 directionToTarget = targetPos - Position;

            Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_down", "ui_up");
		    FixVector2 FixInput = inputDir != Vector2.Zero ? 
                        FixVector2.Normalize((FixVector2)inputDir) : FixVector2.Zero;

            Fix64 relativeRotation = FixVector2.Angle(directionToTarget + FixVector2.Up, FixVector2.Up);
            Fix64 correctedRotation = relativeRotation + Fix64.PiOver2;

            FixVector2 rotatedInput = FixVector2.Rotate(FixInput, correctedRotation);
		    
            FixVector2 finalVelocity = rotatedInput * (Fix64)2;

            SetRotation(correctedRotation);
            SetVelocity(finalVelocity);
            
            GD.Print(FixVector2.Length(directionToTarget));*/
        }

        public FixVector2 GetGroundNormal()
        {
            FixVector2 Normal = FixVector2.Down;

            if (Collisions.Count > 0)
            {
                for (int c = 0; c < Collisions.Count; c++)
                {
                    if (FixVector2.Dot(Collisions[c].Normal, Down) > Fix64.ETA)
                    {
                        Normal = Collisions[c].Normal;
                    }
                }
            }

            return Normal;
        }

        public bool IsWalkableSlope()
        {
            Fix64 HalfThreshold = ((Fix64)SlopeLimit + Fix64.One) / (Fix64) 2;
            return GroundAngle >= (Fix64)90 - HalfThreshold && GroundAngle <= (Fix64)90 + HalfThreshold;
        }

        public override void OnBeginOverlap(SharpBody2D other)
        {
            if (!isTrigger)
                GD.Print("Entered Collision!");
        }

        public override void OnOverlap(SharpBody2D other)
        {
            //GD.Print(body.Collisions.Count);
        }

        public override void OnEndOverlap(SharpBody2D other)
        {
            if (!isTrigger)
                GD.Print("Exited Collision!");
        }
    }
}
