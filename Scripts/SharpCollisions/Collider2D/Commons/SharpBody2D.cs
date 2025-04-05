using Godot;
using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpBody2D : FixedTransform2D
	{
		public delegate void OnOverlapDelegate(SharpBody2D other);
		public OnOverlapDelegate BeginOverlap;
		public OnOverlapDelegate DuringOverlap;
		public OnOverlapDelegate EndOverlap;

		protected uint ID;
		public FixVector2 Velocity;

		[Export] public SharpCollider2D[] Colliders;
		public List<CollisionManifold2D> Collisions = new List<CollisionManifold2D>();
		public List<uint> CollidedWith = new List<uint>();
		public List<uint> BodiesToIgnore = new List<uint>();
		public FixRect BoundingBox = new FixRect();

		[Export(PropertyHint.Enum, "Dynamic,Kinematic,Static")]
		public int BodyMode = 0;
		
		
		
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

		public override void _Instance()
		{
			base._Instance();
			SharpManager.Instance.AddBody(this);

			if (HasColliders())
				foreach(SharpCollider2D col in Colliders)
					col.Initialize();
			
			UpdateColliders();
			BeginOverlap = OnBeginOverlap;
			DuringOverlap = OnOverlap;
			EndOverlap = OnEndOverlap;
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
		}

		public override void RenderNode()
        {
			base.RenderNode();
            DrawColliders();
        }

		public void PreviewColliders()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider2D col in Colliders)
				if (col != null) col.DebugDrawShapesEditor(this);
		}

		public void DrawColliders()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider2D col in Colliders)
				if (col != null) col.DebugDrawShapes(this);
		}

		public override void _Destroy()
        {
            if (SharpManager.Instance.RemoveNode(this) && SharpManager.Instance.RemoveBody(this))
                QueueFree();
        }

		public void SetBodyID(uint value)
		{
			ID = value;
		}

		public uint GetBodyID()
		{
			return ID;
		}

		public bool HasColliders()
		{
			return Colliders != null && Colliders.Length > 0;
		}

		public void IgnoreBody(SharpBody2D bodyToIgnore, bool ignore)
		{
			if (ignore)
			{
				BodiesToIgnore.Add(bodyToIgnore.ID);
				bodyToIgnore.BodiesToIgnore.Add(ID);
			}
			else
			{
				if (BodiesToIgnore.Contains(bodyToIgnore.ID))	BodiesToIgnore.Remove(bodyToIgnore.ID);
				if (bodyToIgnore.BodiesToIgnore.Contains(ID))	bodyToIgnore.BodiesToIgnore.Remove(ID);
			}
		}

		public void ResetIgnoreBodies()
		{
			BodiesToIgnore.Clear();
		}

		private void UpdateBoundingBox()
		{
			Fix64 minX = Fix64.MaxValue;
            Fix64 minY = Fix64.MaxValue;
            Fix64 maxX = Fix64.MinValue;
            Fix64 maxY = Fix64.MinValue;

			for (int i = 0; i < Colliders.Length; i++)
			{
				if (Colliders[i].BoundingBox.x < minX)
					minX = Colliders[i].BoundingBox.x;
				if (Colliders[i].BoundingBox.w > maxX)
					maxX = Colliders[i].BoundingBox.w;
				if (Colliders[i].BoundingBox.y < minY)
					minY = Colliders[i].BoundingBox.y;
				if (Colliders[i].BoundingBox.h > maxY)
					maxY = Colliders[i].BoundingBox.h;

			}
            
            BoundingBox = new FixRect(minX, minY, maxX, maxY);
		}
		
		public void SetVelocity(FixVector2 newVelocity)
		{
			if (BodyMode == 2) return;
			
			Velocity = newVelocity;
		}
		
		public void Move(int delta, int iterations)
		{
			if (BodyMode == 2) return;
			if (FixVector2.Length(Velocity) == Fix64.Zero) return;
			
			Fix64 fDelta = (Fix64)delta;
			Fix64 fIterations = (Fix64)iterations;

			Fix64 finalDelta = Fix64.One / (fDelta * fIterations);

			FixedPosition += Velocity * finalDelta;
			UpdateColliders();
		}

		public void Rotate(Fix64 angle)
		{
			if (BodyMode == 2) return;

			FixedRotation += angle;
			UpdateColliders();
		}

		public void RotateDegrees(Fix64 angle)
		{
			Rotate(angle * Fix64.DegToRad);
		}

		public void SetRotation(Fix64 angle)
		{
			if (BodyMode == 2) return;

			FixedRotation = angle;
			UpdateColliders();
		}

		public void PushAway(FixVector2 direction)
		{
			if (BodyMode == 2) return;

			FixedPosition += direction;
			UpdateColliders();
		}
		
		public void MoveTo(FixVector2 destination)
		{
			if (BodyMode == 2) return;
			
			FixedPosition = destination;
			UpdateColliders();
		}
		
		public void UpdateColliders()
		{
			if (!HasColliders())
			{
				GD.Print("There is no collider attached to this body. No collision will happen.");
				return;
			}

			foreach(SharpCollider2D col in Colliders)
			{
				col.Position = FixedPosition;
				col.UpdatePoints(FixedPosition, FixedRotation);
				col.UpdateBoundingBox();
			}

			UpdateBoundingBox();
		}

		public void ClearFlags()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider2D col in Colliders)
			{
				col.collisionFlags.Clear();
				col.globalCollisionFlags.Clear();
			}
		}

		protected CollisionManifold2D GetCollision(SharpBody2D otherBody)
		{
			CollisionManifold2D ret = null;
			for (int i = 0; i < Collisions.Count; i++)
			{
				if (Collisions[i].CollidedWith == otherBody)
					ret = Collisions[i];
			}

			return ret;
		}

		public virtual void OnBeginOverlap(SharpBody2D other)
		{
			//CollisionManifold2D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
			//GD.Print("Entered Collision!");
		}

		public virtual void OnOverlap(SharpBody2D other)
		{
			//CollisionManifold2D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
			//GD.Print("Still colliding...");
		}

		public virtual void OnEndOverlap(SharpBody2D other)
		{
			//CollisionManifold2D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
			//GD.Print("Exited Collision!");
		}
	}
}
