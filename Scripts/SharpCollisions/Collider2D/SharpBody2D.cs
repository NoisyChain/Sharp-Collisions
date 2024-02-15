using Godot;
using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions
{
	[Tool]
	public class SharpBody2D : FixedTransform2D
	{
		public delegate void OnOverlapDelegate(SharpBody2D other);
		public OnOverlapDelegate BeginOverlap;
		public OnOverlapDelegate DuringOverlap;
		public OnOverlapDelegate EndOverlap;

		public FixVector2 Velocity;

		public SharpCollider2D Collider;
		public List<CollisionManifold2D> Collisions = new List<CollisionManifold2D>();
		public List<SharpBody2D> CollidedWith = new List<SharpBody2D>();
		public List<SharpBody2D> BodiesToIgnore = new List<SharpBody2D>();

		[Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
		public int CollisionLayers = 1;
		[Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
		public int CollisionMask = 1;

		[Export(PropertyHint.Enum, "Dynamic,Kinematic,Static")]
		public int BodyMode = 0;
		//[Export] public bool isStatic = false;
		//[Export] public bool isPushable = true;
		[Export] public bool isTrigger = false;
		
		/*public SharpBody2D() {}
		
		public SharpBody2D(FixVector2 origin, FixVector2 offset, FixVector2 size, 
			FixVector2[] points, Fix64 angle, CollisionType shape, int layers,
			bool staticBody, bool pushableBody, bool trigger)
		{
			position = origin;
			rotation = angle;
			velocity = FixVector2.Zero;
			isStatic = staticBody;
			isPushable = pushableBody;
			isTrigger = trigger;
			CollisionLayers = layers;
			collider = new SharpCollider2D(position, offset, size, points, shape);
			UpdateCollider();
			Collisions = new List<CollisionManifold2D>();
			CollidedWith = new List<SharpBody2D>();
			BodiesToIgnore = new List<SharpBody2D>();
		}*/

		public override void _Ready()
		{
			if (Engine.EditorHint) return;

			base._Ready();
			Manager.AddBody(this);
			Collider = GetNode<SharpCollider2D>("Collider");
			UpdateCollider();
			BeginOverlap = OnBeginOverlap;
			DuringOverlap = OnOverlap;
			EndOverlap = OnEndOverlap;
		}

		public override void _Destroy()
        {
            if (Manager.RemoveTransform(this) && Manager.RemoveBody(this))
                QueueFree();
        }

		public void IgnoreBody(SharpBody2D bodyToIgnore, bool ignore)
		{
			if (ignore)
			{
				BodiesToIgnore.Add(bodyToIgnore);
				bodyToIgnore.BodiesToIgnore.Add(this);
			}
			else
			{
				if (BodiesToIgnore.Contains(bodyToIgnore))	BodiesToIgnore.Remove(bodyToIgnore);
				if (bodyToIgnore.BodiesToIgnore.Contains(this))	bodyToIgnore.BodiesToIgnore.Remove(this);
			}
		}

		public void ResetIgnoreBodies()
		{
			BodiesToIgnore.Clear();
		}
		
		public void SetVelocity(FixVector2 newVelocity)
		{
			if (BodyMode == 2) return;
			
			Velocity = newVelocity;
		}
		
		public void Move(int delta, int iterations)
		{
			if (BodyMode == 2) return;
			
			Fix64 fDelta = (Fix64)delta;
			Fix64 fIterations = (Fix64)iterations;

			Fix64 finalDelta = fDelta * fIterations;

			FixedPosition.x += Velocity.x / finalDelta;
			FixedPosition.y += Velocity.y / finalDelta;
			UpdateCollider();
		}

		public void Rotate(Fix64 angle)
		{
			if (BodyMode == 2) return;

			FixedRotation += angle;
			UpdateCollider();
		}

		public void RotateDegrees(Fix64 angle)
		{
			if (BodyMode == 2) return;

			FixedRotation += angle * Fix64.DegToRad;
			UpdateCollider();
		}

		public void SetRotation(Fix64 angle)
		{
			FixedRotation = angle;
			UpdateCollider();
		}

		public void PushAway(FixVector2 direction)
		{
			FixedPosition -= direction;
			UpdateCollider();
		}
		
		public void MoveTo(FixVector2 destination)
		{
			FixedPosition = destination;
			UpdateCollider();
		}
		
		public void UpdateCollider()
		{
			Collider.Position = FixedPosition;
			Collider.UpdatePoints(this);
			Collider.UpdateBoundingBox();
		}

		public override void _FixedProcess(Fix64 delta)
		{
			SetVelocity(FixVector2.Zero);
		}

		public virtual void OnBeginOverlap(SharpBody2D other)
		{
			//GD.Print("Entered Collision!");
		}

		public virtual void OnOverlap(SharpBody2D other)
		{
			//GD.Print("Still colliding...");
		}

		public virtual void OnEndOverlap(SharpBody2D other)
		{
			//GD.Print("Exited Collision!");
		}
	}
}
