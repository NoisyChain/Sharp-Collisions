using FixMath.NET;
using Godot;

namespace SharpCollisions
{
    [Tool]
    public class CharacterController2D : SharpBody2D
    {
        [Export(PropertyHint.Range, "0,89")]
        public int SlopeLimit = 45;
        [Export(PropertyHint.Range, "0,89")]
        public int CeilingAngleLimit = 45;
        [Export] private bool KeepVelocityOnSlopes;
        [Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
		public int FloorLayers = 1;
        public bool IsOnGround => Collider.collisionFlags.Below;
        public bool IsOnCeiling => Collider.collisionFlags.Above;
        public bool IsOnWalls => Collider.collisionFlags.Walls;

        public FixVector2 GroundNormal => GetGround().Normal;
        public Fix64 GroundAngle => FixVector2.AngleDegrees(GroundNormal, Up);

        private FixVector2 VerticalVelocity;
        private FixVector2 LateralVelocity;
        private FixVector2 UpVector = FixVector2.Up;

        public override void _FixedProcess(Fix64 delta)
        {
            if (IsOnGround && IsValidFloor() && IsWalkableSlope(GroundAngle))
            {
                UpVector = -GroundNormal;
                VerticalVelocity = -UpVector;
                if (Input.IsActionPressed("ui_left") && !Input.IsActionPressed("ui_right"))
                    LateralVelocity = FixVector2.Left;
                if (!Input.IsActionPressed("ui_left") && Input.IsActionPressed("ui_right"))
                    LateralVelocity = FixVector2.Right;
                if (!Input.IsActionPressed("ui_left") && !Input.IsActionPressed("ui_right"))
                    LateralVelocity = FixVector2.Zero;

                LateralVelocity = FixVector2.Reject(LateralVelocity, UpVector);
                if (KeepVelocityOnSlopes)
                    LateralVelocity = FixVector2.Normalize(LateralVelocity);
                LateralVelocity *= (Fix64)2;

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
                if (VerticalVelocity.y < -(Fix64)10)
                    VerticalVelocity = -UpVector * (Fix64)10;
            }
            
            FixVector2 finalVelocity = LateralVelocity + VerticalVelocity;
		    
            if (FixVector2.Distance(FixedPosition, FixVector2.Zero) > (Fix64)10)
            { 
                LateralVelocity = FixVector2.Zero;
                VerticalVelocity = FixVector2.Zero;
                MoveTo(FixVector2.Zero);
            }

            SetVelocity(finalVelocity);

            //GD.Print(Collisions.Count);
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

        //TODO: Select the slope relative to movement direction
        public CollisionManifold2D GetGround()
        { 
            CollisionManifold2D Ground = null;

            if (Collisions.Count > 0)
            {
                Ground = Collisions[0];

                if (Collisions.Count > 1)
                {
                    for (int c = 1; c < Collisions.Count; c++)
                    {
                        GD.Print(FixVector2.IsExactDirection(Collisions[c].Normal, LateralVelocity));
                        if ((IsWalkableSlope(FixVector2.AngleDegrees(Collisions[c].Normal, Up)) ||
                            !IsWalkableSlope(FixVector2.AngleDegrees(Ground.Normal, Up))) &&
                            FixVector2.IsExactDirection(Collisions[c].Normal, LateralVelocity))
                            Ground = Collisions[c];
                    }
                }
            }

            return Ground;
        }

        public bool IsValidFloor()
        {
            return ((FloorLayers & GetGround().CollidedWith.CollisionLayers) & SharpWorld2D.mask) != 0;
        }

        public bool IsWalkableSlope(Fix64 angle)
        {
            Fix64 HalfThreshold = ((Fix64)SlopeLimit + Fix64.One) / (Fix64) 2;
            return angle >= (Fix64)90 - HalfThreshold && angle <= (Fix64)90 + HalfThreshold;
        }

        public override void OnBeginOverlap(SharpBody2D other)
        {
            if (!isTrigger)
                GD.Print("Entered Collision!");
        }

        public override void OnOverlap(SharpBody2D other)
        {
            //GD.Print("Still colliding...");
        }

        public override void OnEndOverlap(SharpBody2D other)
        {
            if (!isTrigger)
                GD.Print("Exited Collision!");
        }
    }
}
