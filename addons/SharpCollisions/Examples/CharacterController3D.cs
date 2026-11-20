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
        [Export] private bool RotateOnMovingPlatformer = true;
        [Export(PropertyHint.Layers3DPhysics)]
		public int FloorLayers = 1;

        private SurfaceData3D GroundData;
        private SurfaceData3D WallData;
        private SurfaceData3D CeilingData;

        public FixVector3 GroundNormal => GroundData.Normal;
        public Fix64 GroundAngle => FixVector3.AngleDegrees(GroundNormal, Up);
        public FixVector3 WallNormal => WallData.Normal;
        public Fix64 WallAngle => FixVector3.AngleDegrees(WallNormal, Up);
        public FixVector3 CeilingNormal => CeilingData.Normal;
        public Fix64 CeilingAngle => FixVector3.AngleDegrees(CeilingNormal, Up);

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
            return (GetCollider(0).collisionFlags & CollisionFlags.Below) > 0;
        }
        public bool IsOnCeiling()
        {
            if (!HasColliders()) return false;
            return (GetCollider(0).collisionFlags & CollisionFlags.Above) > 0;
        }
        public bool IsOnWalls()
        {
            if (!HasColliders()) return false;
            return (GetCollider(0).collisionFlags & CollisionFlags.Walls) > 0;
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

            if (IsOnGround() && GroundData.ID > -1)
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

                //if (StopAirVelocityOnCeiling && GetCeiling() != null && !IsAngularCeiling())
                    //Add a bit of extra force or else the body gets stuck on the ceiling for some reason
                    //VerticalVelocity = -UpVector * ceilingUnstickForce;
                
                VerticalVelocity -= UpVector * gravity * delta;
                
                if (VerticalVelocity.y < -fallSpeedLimit)
                    VerticalVelocity = -UpVector * fallSpeedLimit;
            }
            
            FixVector3 finalVelocity = LateralVelocity + VerticalVelocity;
		    
            if (FixVector3.Distance(FixedPosition, FixVector3.Zero) > worldBounds)
            { 
                LateralVelocity = FixVector3.Zero;
                VerticalVelocity = FixVector3.Zero;
                SetPosition(FixVector3.Up);
            }

            //SetRotation(correctedRotation);
            SetLinearVelocity(finalVelocity);
            string groundAngle = IsOnGround() ? GroundAngle.ToString() : "No Ground";
            if (debug != null && HasColliders()) debugText = "Position: " + FixedPosition +
                "\nRotation: " + FixedRotation +
                "\nVelocity: " + LinearVelocity +
                "\nNormal: " + UpVector.ToString() + 
                "\nFlags: " + GetCollider(0).collisionFlags.ToString() + 
                "\nCollisions: " + ConfirmedCollisionCount() + 
                //"\nContact Point: " + (GetCollisions().Count > 0 ? GetCollision(0).ContactPoint : 0) + 
                "\nFloor angle: " + groundAngle;
            
            GroundData.ID = -1;
            GroundData.Normal = FixVector3.Up;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            debug.Text = debugText;
        }

        public void GetSurfaceData(CollisionResult3D surface)
        {
            //
            if (!IsValidFloor(surface.CollidedWith.CollisionLayers)) return;
            Fix64 err = (Fix64)0.1;
            Fix64 surfaceDot = FixVector3.Dot(surface.Normal, Up);
            Fix64 surfaceAngle = FixVector3.AngleDegrees(surface.Normal, Up);
            //Check floor
            if (IsOnGround() && surfaceDot > err)
            {
                if (IsOnWalls() && surfaceAngle > (Fix64)(SlopeLimit + 1)) return;

                GroundData.ID = (int)surface.CollidedWith.GetBodyID();
                GroundData.Normal = surface.Normal;
                return;
            }
            //Checl ceiling
            else if (IsOnCeiling() && surfaceDot < -err)
            {
                CeilingData.ID = (int)surface.CollidedWith.GetBodyID();
                CeilingData.Normal = surface.Normal;
                return;
            }
            //Check wall
            else if (IsOnWalls() && Fix64.Abs(surfaceDot) < err)
            {
                WallData.ID = (int)surface.CollidedWith.GetBodyID();
                WallData.Normal = surface.Normal;
                return;
            }
        }

        public bool IsValidFloor(int colliderLayer)
        {
            return ((FloorLayers & colliderLayer) & SharpHelpers.mask) != 0;
        }

        public bool IsWalkableSlope()
        {
            return Fix64.Abs(GroundAngle) < (Fix64)SlopeLimit;
        }

        public bool IsAngularCeiling(Fix64 angle)
        {
            return Fix64.Abs(angle) >= (Fix64)CeilingAngleLimit + Fix64.One;
        }

        public override void OnBeginOverlap(CollisionResult3D collision)
        {
            base.OnBeginOverlap(collision);
            GD.Print(collision.CollidedWith.GetBodyID());
        }

        public override void OnOverlap(CollisionResult3D collision)
        {
            base.OnOverlap(collision);
            GetSurfaceData(collision);
        }

        public override void OnEndOverlap(CollisionResult3D collision)
        {
            base.OnEndOverlap(collision);
            GD.Print(collision.CollidedWith.GetBodyID());
        }
    }
}
