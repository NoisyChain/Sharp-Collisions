using Godot;
using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpBody3D : FixedTransform3D
	{
		private uint ID; 
		public FixVector3 LinearVelocity;
		public FixVector3 AngularVelocity;

		[Export] private SharpCollider3D[] Colliders;
		private List<CollisionManifold3D> Collisions = new List<CollisionManifold3D>();
		private List<(uint, int)> CollidedWith = new List<(uint, int)>();
		private List<uint> BodiesToIgnore = new List<uint>();
		public FixVolume BoundingBox = new FixVolume();

		[Export(PropertyHint.Enum, "Dynamic,Kinematic,Static")]
		public int BodyMode = 0;

		public SharpCollider3D[] GetColliders() => Colliders;
		public SharpCollider3D GetCollider(int index) => Colliders[index];

		private bool collidersRequireUpdate;
		
		/*public SharpBody3D() {}
		
		public SharpBody3D(FixVector3 origin, FixVector3 offset, FixVector3 size, 
			FixVector3[] points, Fix64 angle, CollisionType shape, int layers,
			bool staticBody, bool pushableBody, bool trigger)
		{
			position = origin;
			rotation = angle;
			velocity = FixVector3.Zero;
			isStatic = staticBody;
			isPushable = pushableBody;
			isTrigger = trigger;
			CollisionLayers = layers;
			collider = new SharpCollider3D(position, offset, size, points, shape);
			UpdateCollider();
			Collisions = new List<CollisionManifold3D>();
			CollidedWith = new List<int>();
			BodiesToIgnore = new List<int>();
		}*/

		public override void _Instance()
		{
			base._Instance();
			SharpManager.Instance.AddBody(this);
			
			if (HasColliders())
				foreach(SharpCollider3D col in Colliders)
					col.Initialize();
			
			collidersRequireUpdate = true;
			UpdateColliders();
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			PreviewColliders();
		}

		public override void RenderNode(bool debug)
        {
			base.RenderNode(debug);
            if (debug) DrawColliders();
        }

		public void PreviewColliders()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider3D col in Colliders)
				if (col != null) col.DebugDrawShapesEditor(this);
		}

		public void DrawColliders()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider3D col in Colliders)
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

		public void IgnoreBody(SharpBody3D bodyToIgnore, bool ignore)
		{
			if (ignore)
			{
				if (!IsIgnoringBody(bodyToIgnore))  AddBodyToIgnore(bodyToIgnore.ID);
				if (!bodyToIgnore.IsIgnoringBody(this))	bodyToIgnore.AddBodyToIgnore(ID);
			}
			else
			{
				if (IsIgnoringBody(bodyToIgnore))	RemoveBodyToIgnore(bodyToIgnore.ID);
				if (bodyToIgnore.IsIgnoringBody(this))	bodyToIgnore.RemoveBodyToIgnore(ID);
			}
		}

		public bool IsIgnoringBody(SharpBody3D body)
		{
			return BodiesToIgnore.Contains(body.ID);
		}

		public void AddBodyToIgnore(uint bodyID)
		{
			BodiesToIgnore.Add(bodyID);
		}

		public void RemoveBodyToIgnore(uint bodyID)
		{
			BodiesToIgnore.Remove(bodyID);
		}

		public void ResetIgnoreBodies()
		{
			BodiesToIgnore.Clear();
		}

		public bool HasCollidedWith((uint, int) col)
		{
			return CollidedWith.Contains(col);
		}

		public void ConfirmCollision((uint, int) col)
		{
			CollidedWith.Add(col);
		}

		public void RemoveCollision((uint, int) col)
		{
			CollidedWith.Remove(col);
		}

		private void UpdateBoundingBox()
		{
			Fix64 minX = Fix64.MaxValue;
            Fix64 minY = Fix64.MaxValue;
			Fix64 minZ = Fix64.MaxValue;
            Fix64 maxX = Fix64.MinValue;
            Fix64 maxY = Fix64.MinValue;
			Fix64 maxZ = Fix64.MinValue;

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
				if (Colliders[i].BoundingBox.z < minZ)
					minZ = Colliders[i].BoundingBox.z;
				if (Colliders[i].BoundingBox.d > maxZ)
					maxZ = Colliders[i].BoundingBox.d;

			}
            
            BoundingBox = new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
		}
		
		public void SetLinearVelocity(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			
			LinearVelocity = newVelocity;
		}

		public void SetAngularVelocity(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			
			AngularVelocity = newVelocity;
		}

		public void SetAngularVelocityDegrees(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			
			AngularVelocity = newVelocity * Fix64.DegToRad;
		}
		
		public void Move()
		{
			//if (BodyMode == 2) return;
			if (FixVector3.Length(LinearVelocity) == Fix64.Zero) return;

			FixedPosition += LinearVelocity * SharpTime.SubDelta;
			collidersRequireUpdate = true;
			//UpdateColliders();
		}

		public void Rotate()
		{
			//if (BodyMode == 2) return;
			if (FixVector3.Length(AngularVelocity) == Fix64.Zero) return;

			FixedRotation += AngularVelocity * SharpTime.SubDelta;
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

		public void SetRotation(FixVector3 angle)
		{
			FixedRotation = angle;
			collidersRequireUpdate = true;
			UpdateColliders();
		}

		public void SetRotationDegrees(FixVector3 angle)
		{
			SetRotation(angle * Fix64.DegToRad);
		}

		public void PushAway(FixVector3 direction)
		{
			FixedPosition += direction;
			collidersRequireUpdate = true;
			UpdateColliders();
		}
		
		public void MoveTo(FixVector3 destination)
		{
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

			foreach(SharpCollider3D col in Colliders)
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
			
			foreach(SharpCollider3D col in Colliders)
			{
				col.collisionFlags.Clear();
				col.globalCollisionFlags.Clear();
			}
		}
		public List<CollisionManifold3D> GetCollisions() => Collisions;
		public CollisionManifold3D GetCollision(int index) => Collisions[index];
		public CollisionManifold3D GetCollision(SharpBody3D otherBody)
		{
			CollisionManifold3D ret = null;
			for (int i = 0; i < Collisions.Count; i++)
			{
				if (Collisions[i].CollidedWith == otherBody)
					ret = Collisions[i];
			}

			return ret;
		}
		public CollisionManifold3D GetCollision(SharpBody3D otherBody, int otherCollider)
		{
			CollisionManifold3D ret = null;
			for (int i = 0; i < Collisions.Count; i++)
			{
				if (Collisions[i].Collider != otherBody.GetCollider(otherCollider)) continue;
				
				ret = Collisions[i];
			}

			return ret;
		}
		public void AddCollision(CollisionManifold3D col) => Collisions.Add(col);
		public void ClearCollisions() => Collisions.Clear();

		public virtual void OnBeginOverlap(CollisionManifold3D collision)
		{
			//GD.Print("Entered Collision!");
		}

		public virtual void OnOverlap(CollisionManifold3D collision)
		{
			//GD.Print("Still colliding...");
		}

		public virtual void OnEndOverlap(CollisionManifold3D collision)
		{
			//GD.Print("Exited Collision!");
		}
	}
}
