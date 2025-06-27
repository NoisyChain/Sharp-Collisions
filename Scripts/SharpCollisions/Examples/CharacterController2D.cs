using FixMath.NET;
using Godot;

namespace SharpCollisions.Sharp2D
{
    [Tool]
    public partial class CharacterController2D : SharpBody2D
    {
        [Export(PropertyHint.Range, "0,89")]
        public int SlopeLimit = 45;
        [Export(PropertyHint.Range, "0,89")]
        public int CeilingAngleLimit = 45;
        [Export] private bool KeepVelocityOnSlopes = true;
        [Export] private bool KeepSlopeVelocityOnJump = true;
        [Export] private bool StopAirVelocityOnCeiling = true;
        [Export(PropertyHint.Layers2DPhysics)]
		public int FloorLayers = 1;

        public FixVector2 GroundNormal => GetGround().Normal;
        public Fix64 GroundAngle => FixVector2.AngleDegrees(GroundNormal, Up);
        public Fix64 CeilingAngle => FixVector2.AngleDegrees(GetCeiling().Normal, Up);

        private FixVector2 VerticalVelocity;
        private FixVector2 LateralVelocity;
        private FixVector2 UpVector = FixVector2.Up;

        private Fix64 speed = Fix64.Two;
        private Fix64 jumpSpeed = (Fix64)5;
        private Fix64 gravity = (Fix64)9.81f;
        private Fix64 fallSpeedLimit = (Fix64)10;
        private Fix64 ceilingUnstickForce = (Fix64)1.25f;
        private Fix64 worldBounds = (Fix64)10;

        [Export] private Label debug;
        private string debugText;

        public bool IsOnGround() 
        {
            if (!HasColliders()) return false;
            return GetCollider(0).collisionFlags.Below;
        }
        public bool IsOnCeiling()
        {
            if (!HasColliders()) return false;
            return GetCollider(0).collisionFlags.Above;
        }
        public bool IsOnWalls()
        {
            if (!HasColliders()) return false;
            return GetCollider(0).collisionFlags.Walls;
        }

        public override void _Instance()
        {
            base._Instance();
        }

        public override void _FixedProcess(Fix64 delta)
        {
            if (IsOnGround() && IsValidFloor() && IsWalkableSlope(GroundAngle))
            {
                UpVector = GroundNormal;
                VerticalVelocity = -UpVector;
                if (Input.IsActionPressed("left") && !Input.IsActionPressed("right"))
                    LateralVelocity = FixVector2.Left;
                if (!Input.IsActionPressed("left") && Input.IsActionPressed("right"))
                    LateralVelocity = FixVector2.Right;
                if (!Input.IsActionPressed("left") && !Input.IsActionPressed("right"))
                    LateralVelocity = FixVector2.Zero;

                LateralVelocity = FixVector2.Reject(LateralVelocity, UpVector);
                if (KeepVelocityOnSlopes)
                    LateralVelocity = FixVector2.Normalize(LateralVelocity);
                LateralVelocity *= speed;

                if (Input.IsActionPressed("jump"))
                {
                    UpVector = FixVector2.Up;
                    if (!KeepSlopeVelocityOnJump)
                    {
                        LateralVelocity = FixVector2.Reject(LateralVelocity, UpVector);
                        LateralVelocity = FixVector2.Normalize(LateralVelocity);
                        LateralVelocity *= speed;
                    }

                    VerticalVelocity = UpVector * jumpSpeed;
                }
            }
            else
            {
                UpVector = FixVector2.Up;

                if (StopAirVelocityOnCeiling && GetCeiling() != null && !IsAngularCeiling())
                    //Add a bit of extra force or else the body gets stuck on the ceiling for some reason
                    VerticalVelocity = -UpVector * ceilingUnstickForce;
                
                VerticalVelocity -= UpVector * gravity * delta;
                
                if (VerticalVelocity.y < -fallSpeedLimit)
                    VerticalVelocity = -UpVector * fallSpeedLimit;
            }

            VerticalVelocity -= UpVector * gravity * delta;
                
                if (VerticalVelocity.y < -fallSpeedLimit)
                    VerticalVelocity = -UpVector * fallSpeedLimit;
            
            FixVector2 finalVelocity = LateralVelocity + VerticalVelocity;
		    
            if (FixVector2.Distance(FixedPosition, FixVector2.Zero) > worldBounds)
            { 
                LateralVelocity = FixVector2.Zero;
                VerticalVelocity = FixVector2.Zero;
                MoveTo(FixVector2.Zero);
            }

            SetLinearVelocity(finalVelocity);

            string groundAngle = IsOnGround() ? GroundAngle.ToString() : "No Ground";
            if (debug != null && HasColliders()) debugText = "Normal: " + UpVector.ToString() + 
                "\nFlags: " + GetCollider(0).collisionFlags.ToString() + 
                "\nCollisions:" + GetCollisions().Count + 
                "\nFloor angle: " + groundAngle;

            //GD.Print(Collisions.Count);

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

        public override void _Process(double delta)
        {
            base._Process(delta);
            debug.Text = debugText;
        }
        
        public override void RenderNode(bool debug)
        {
            base.RenderNode(debug);

            if (!debug) return;
            
            foreach(CollisionManifold2D col in GetCollisions())
                DebugDraw3D.DrawSimpleSphere((Vector3)col.ContactPoint, Vector3.Right, Vector3.Up, Vector3.Forward, 0.1f, new Color(0f, 1f, 1f));
        }

        public CollisionManifold2D GetGround()
        { 
            CollisionManifold2D Ground = null;

            if (GetCollisions().Count > 0)
            {
                Ground = GetCollision(0);

                if (GetCollisions().Count > 1)
                {
                    for (int c = 1; c < GetCollisions().Count; c++)
                    {
                        if (IsWalkableSlope(FixVector2.AngleDegrees(GetCollision(c).Normal, Up)) ||
                            !IsWalkableSlope(FixVector2.AngleDegrees(Ground.Normal, Up)))
                            Ground = GetCollision(c);
                    }
                }
            }

            return Ground;
        }

        public CollisionManifold2D GetCeiling()
        { 
            CollisionManifold2D Ceiling = null;

            if (IsOnCeiling() && GetCollisions().Count > 0)
            {
                Ceiling = GetCollision(0);

                /*if (Collisions.Count > 1)
                {
                    for (int c = 1; c < Collisions.Count; c++)
                    {
                        if ((IsWalkableSlope(FixVector2.AngleDegrees(Collisions[c].Normal, Down)) ||
                            !IsWalkableSlope(FixVector2.AngleDegrees(Ceiling.Normal, Down))) &&
                            FixVector2.IsExactDirection(Collisions[c].Normal, LateralVelocity))
                            Ceiling = Collisions[c];
                    }
                }*/
            }

            return Ceiling;
        }

        public bool IsValidFloor()
        {
            return ((FloorLayers & GetGround().CollidedWith.CollisionLayers) & SharpWorld2D.mask) != 0;
        }

        public bool IsWalkableSlope(Fix64 angle)
        {
            return Fix64.Abs(angle) <= (Fix64)SlopeLimit + Fix64.One;
        }

        public bool IsAngularCeiling()
        {
            //GD.Print(CeilingAngle);
            Fix64 HalfThreshold = ((Fix64)CeilingAngleLimit + Fix64.One) / Fix64.Two;
            return CeilingAngle >= (Fix64)90 - HalfThreshold && CeilingAngle <= (Fix64)90 + HalfThreshold;
        }

        public override void OnBeginOverlap(CollisionManifold2D collision)
        {
            base.OnBeginOverlap(collision);
            GD.Print(collision.CollidedWith.GetBodyID());
        }

        public override void OnOverlap(CollisionManifold2D collision)
        {
            base.OnOverlap(collision);
            
        }

        public override void OnEndOverlap(CollisionManifold2D collision)
        {
            base.OnEndOverlap(collision);
            GD.Print(collision.CollidedWith.GetBodyID());
        }
    }
}
