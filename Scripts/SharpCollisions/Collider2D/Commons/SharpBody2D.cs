using Godot;
using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpBody2D : FixedTransform2D
	{
		protected uint ID;
		public FixVector2 LinearVelocity;
		public Fix64 AngularVelocity;

		[Export] private SharpCollider2D[] Colliders;
		private List<CollisionManifold2D> Collisions = new List<CollisionManifold2D>();
		public List<(uint, int)> CollidedWith = new List<(uint, int)>();
		public List<uint> BodiesToIgnore = new List<uint>();
		public FixRect BoundingBox = new FixRect();

		[Export(PropertyHint.Enum, "Dynamic,Kinematic,Static")]
		public int BodyMode = 0;
		
		public SharpCollider2D[] GetColliders() => Colliders;
		public SharpCollider2D GetCollider(int index) => Colliders[index];

		private bool collidersRequireUpdate;
		
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
			
			collidersRequireUpdate = true;
			UpdateColliders();
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
		}

		public override void RenderNode(bool debug)
        {
			base.RenderNode(debug);
            if (debug) DrawColliders();
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
		
		public void SetLinearVelocity(FixVector2 newVelocity)
		{
			if (BodyMode == 2) return;
			
			LinearVelocity = newVelocity;
		}

		public void SetAngularVelocity(Fix64 newVelocity)
		{
			if (BodyMode == 2) return;
			
			AngularVelocity = newVelocity;
		}

		public void SetAngularVelocityDegrees(Fix64 newVelocity)
		{
			if (BodyMode == 2) return;
			
			AngularVelocity = newVelocity * Fix64.DegToRad;
		}
		
		public void Move()
		{
			//if (BodyMode == 2) return;
			if (FixVector2.Length(LinearVelocity) == Fix64.Zero) return;

			FixedPosition += LinearVelocity * SharpTime.TimeScale * SharpTime.SubDelta;
			collidersRequireUpdate = true;
			//UpdateColliders();
		}

		public void Rotate()
		{
			//if (BodyMode == 2) return;
			if (AngularVelocity == Fix64.Zero) return;

			FixedRotation += AngularVelocity * SharpTime.TimeScale * SharpTime.SubDelta;
			collidersRequireUpdate = true;
			//UpdateColliders();
		}

		public void UpdateBody()
		{
			if (BodyMode == 2) return;

			Rotate();
			Move();
			UpdateColliders();
		}

		public void SetRotation(Fix64 angle)
		{
			if (BodyMode == 2) return;

			FixedRotation = angle;
			collidersRequireUpdate = true;
			UpdateColliders();
		}

		public void SetRotationDegrees(Fix64 angle)
		{
			SetRotation(angle * Fix64.DegToRad);
		}

		public void PushAway(FixVector2 direction)
		{
			if (BodyMode == 2) return;

			FixedPosition += direction;
			collidersRequireUpdate = true;
			UpdateColliders();
		}
		
		public void MoveTo(FixVector2 destination)
		{
			if (BodyMode == 2) return;
			
			FixedPosition = destination;
			collidersRequireUpdate = true;
			UpdateColliders();
		}
		
		public void UpdateColliders()
		{
			if (!HasColliders())
			{
				GD.Print("There is no collider attached to this body. No collision will happen.");
				return;
			}

			if (!collidersRequireUpdate) return;

			foreach(SharpCollider2D col in Colliders)
			{
				col.Position = FixedPosition;
				col.UpdatePoints(FixedPosition, FixedRotation);
				col.UpdateBoundingBox();
			}

			UpdateBoundingBox();
			collidersRequireUpdate = false;
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

		public List<CollisionManifold2D> GetCollisions() => Collisions;
		public CollisionManifold2D GetCollision(int index) => Collisions[index];
		public CollisionManifold2D GetCollision(SharpBody2D otherBody)
		{
			CollisionManifold2D ret = null;
			for (int i = 0; i < Collisions.Count; i++)
			{
				if (Collisions[i].CollidedWith == otherBody)
					ret = Collisions[i];
			}

			return ret;
		}
		public CollisionManifold2D GetCollision(SharpBody2D otherBody, int otherCollider)
		{
			CollisionManifold2D ret = null;
			for (int i = 0; i < Collisions.Count; i++)
			{
				if (Collisions[i].Collider != otherBody.GetCollider(otherCollider)) continue;
				
				ret = Collisions[i];
			}

			return ret;
		}
		public void AddCollision(CollisionManifold2D col) => Collisions.Add(col);
		public void ClearCollisions() => Collisions.Clear();

		public virtual void OnBeginOverlap(CollisionManifold2D collision)
		{
			//GD.Print("Entered Collision!");
		}

		public virtual void OnOverlap(CollisionManifold2D collision)
		{
			//GD.Print("Still colliding...");
		}

		public virtual void OnEndOverlap(CollisionManifold2D collision)
		{
			//GD.Print("Exited Collision!");
		}
	}
}
