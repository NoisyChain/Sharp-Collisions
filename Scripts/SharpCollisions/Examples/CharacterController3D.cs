using FixMath.NET;
using Godot;

namespace SharpCollisions.Sharp3D
{
    [Tool]
    public partial class CharacterController3D : SharpBody3D
    {
        [Export(PropertyHint.Range, "0,89")]
        public int SlopeLimit = 45;
        [Export(PropertyHint.Range, "0,89")]
        public int CeilingAngleLimit = 45;
        [Export] private bool KeepVelocityOnSlopes = true;
        [Export] private bool KeepSlopeVelocityOnJump = true;
        [Export] private bool StopAirVelocityOnCeiling = true;
        [Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
		public int FloorLayers = 1;

        public FixVector3 GroundNormal => GetGround().Normal;
        public Fix64 GroundAngle => FixVector3.AngleDegrees(GroundNormal, Up);
        public Fix64 CeilingAngle => FixVector3.AngleDegrees(GetCeiling().Normal, Up);

        private FixVector3 VerticalVelocity;
        private FixVector3 LateralVelocity;
        private FixVector3 UpVector = FixVector3.Up;

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
            return Colliders[0].collisionFlags.Below;
        }
        public bool IsOnCeiling()
        {
            if (!HasColliders()) return false;
            return Colliders[0].collisionFlags.Above;
        }
        public bool IsOnWalls()
        {
            if (!HasColliders()) return false;
            return Colliders[0].collisionFlags.Walls;
        }

        public override void _Instance()
        {
            base._Instance();
        }

        public override void _FixedProcess(Fix64 delta)
        {
            /*FixVector3 AddDegrees = new FixVector3(Fix64.Zero, Fix64.Zero, (Fix64)90);

            
            if (Input.IsActionPressed("ui_home"))
            {
                RotateDegrees(AddDegrees * delta);
            }
            if (Input.IsActionPressed("ui_end"))
            {
                RotateDegrees(-AddDegrees * delta);
            }
            if (Input.IsActionPressed("ui_page_up"))
                Input3D.y = Fix64.One;
            else if (Input.IsActionPressed("ui_page_down"))
                Input3D.y = Fix64.NegativeOne;
            */

            if (IsOnGround() && IsValidFloor() && IsWalkableSlope(GroundAngle))
            {
                UpVector = GroundNormal;
                VerticalVelocity = -UpVector;
                if (Input.IsActionPressed("left"))
                    LateralVelocity.x = Fix64.NegativeOne;
                else if (Input.IsActionPressed("right"))
                    LateralVelocity.x = Fix64.One;
                else
                    LateralVelocity.x = Fix64.Zero;
                
                if (Input.IsActionPressed("up"))
                    LateralVelocity.z = Fix64.NegativeOne;
                else if (Input.IsActionPressed("down"))
                    LateralVelocity.z = Fix64.One;
                else
                    LateralVelocity.z = Fix64.Zero;
                LateralVelocity = FixVector3.Normalize(LateralVelocity);

                //LateralVelocity = FixVector3.Reject(LateralVelocity, UpVector);
                //if (KeepVelocityOnSlopes)
                    //LateralVelocity = FixVector3.Normalize(LateralVelocity);
                
                LateralVelocity *= speed;

                if (Input.IsActionPressed("jump"))
                {
                    UpVector = FixVector3.Up;
                    if (!KeepSlopeVelocityOnJump)
                    {
                        LateralVelocity = FixVector3.Reject(LateralVelocity, UpVector);
                        LateralVelocity = FixVector3.Normalize(LateralVelocity);
                        LateralVelocity *= speed;
                    }

                    VerticalVelocity = UpVector * jumpSpeed;
                }

                //DebugDraw3D.DrawSphere(GlobalPosition, 0.1f, new Color(0f, 1f, 1f));
            }
            else
            {
                UpVector = FixVector3.Up;

                if (StopAirVelocityOnCeiling && GetCeiling() != null && !IsAngularCeiling())
                    //Add a bit of extra force or else the body gets stuck on the ceiling for some reason
                    VerticalVelocity = -UpVector * ceilingUnstickForce;
                
                VerticalVelocity -= UpVector * gravity * delta;
                
                if (VerticalVelocity.y < -fallSpeedLimit)
                    VerticalVelocity = -UpVector * fallSpeedLimit;
            }
            
            FixVector3 finalVelocity = LateralVelocity + VerticalVelocity;
		    
            if (FixVector3.Distance(FixedPosition, FixVector3.Zero) > worldBounds)
            { 
                LateralVelocity = FixVector3.Zero;
                VerticalVelocity = FixVector3.Zero;
                MoveTo(FixVector3.Up);
            }

            //SetRotation(correctedRotation);
            SetVelocity(finalVelocity);
            string groundAngle = IsOnGround() ? GroundAngle.ToString() : "No Ground";
            if (debug != null && HasColliders()) debugText = "Normal: " + UpVector.ToString() + 
                "\nFlags: " + Colliders[0].collisionFlags.ToString() + 
                "\nCollisions: " + Collisions.Count + 
                "\nContact Point: " + (Collisions.Count > 0 ? Collisions[0].ContactPoint : 0) + 
                "\nFloor angle: " + groundAngle;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            debug.Text = debugText;
        }

        public override void RenderNode()
        {
            base.RenderNode();
            foreach(CollisionManifold3D col in Collisions)
                DebugDraw3D.DrawSimpleSphere((Vector3)col.ContactPoint, Vector3.Right, Vector3.Up, Vector3.Forward, 0.1f, new Color(0f, 1f, 1f));
        }


        public CollisionManifold3D GetGround()
        { 
            CollisionManifold3D Ground = null;

            if (Collisions.Count > 0)
            {
                Ground = Collisions[0];

                if (Collisions.Count > 1)
                {
                    for (int c = 1; c < Collisions.Count; c++)
                    {
                        if (IsWalkableSlope(FixVector3.AngleDegrees(Collisions[c].Normal, Up)) ||
                            !IsWalkableSlope(FixVector3.AngleDegrees(Ground.Normal, Up)))
                            Ground = Collisions[c];
                    }
                }
            }

            return Ground;
        }

        public CollisionManifold3D GetCeiling()
        { 
            CollisionManifold3D Ceiling = null;

            if (IsOnCeiling() && Collisions.Count > 0)
            {
                Ceiling = Collisions[0];

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
            return ((FloorLayers & GetGround().CollidedWith.Colliders[0].CollisionLayers) & SharpWorld3D.mask) != 0;
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

        public override void OnBeginOverlap(SharpBody3D other)
        {
            base.OnBeginOverlap(other);
            //CollisionManifold3D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
                //GD.Print(collision.CollidedWith.GetBodyID());
            //}
        }

        public override void OnOverlap(SharpBody3D other)
        {
            base.OnOverlap(other);
            //CollisionManifold3D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
        }

        public override void OnEndOverlap(SharpBody3D other)
        {
            base.OnEndOverlap(other);
            //CollisionManifold3D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
        }
    }
}
