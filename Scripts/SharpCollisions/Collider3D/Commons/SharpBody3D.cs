using Godot;
using FixMath.NET;
using System.Collections.Generic;
using System.Linq;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpBody3D : FixedTransform3D
	{
		public delegate void OnOverlapDelegate(SharpBody3D other);
		public OnOverlapDelegate BeginOverlap;
		public OnOverlapDelegate DuringOverlap;
		public OnOverlapDelegate EndOverlap;

		protected uint ID; 
		public FixVector3 Velocity;

		[Export] public SharpCollider3D[] Colliders;
		public List<CollisionManifold3D> Collisions = new List<CollisionManifold3D>();
		public List<(uint, int)> CollidedWith = new List<(uint, int)>();
		public List<uint> BodiesToIgnore = new List<uint>();
		public FixVolume BoundingBox = new FixVolume();

		[Export(PropertyHint.Enum, "Dynamic,Kinematic,Static")]
		public int BodyMode = 0;
		
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
			
			UpdateColliders();
			BeginOverlap = OnBeginOverlap;
			DuringOverlap = OnOverlap;
			EndOverlap = OnEndOverlap;
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			PreviewColliders();
		}

		public override void RenderNode()
        {
			base.RenderNode();
            DrawColliders();
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
		
		public void SetVelocity(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			
			Velocity = newVelocity;
		}
		
		public void Move(int delta, int iterations)
		{
			if (BodyMode == 2) return;
			if (FixVector3.Length(Velocity) == Fix64.Zero) return;
			
			Fix64 fDelta = (Fix64)delta;
			Fix64 fIterations = (Fix64)iterations;

			Fix64 finalDelta = Fix64.One / (fDelta * fIterations);

			FixedPosition += Velocity * finalDelta;
			UpdateColliders();
		}

		public void Rotate(FixVector3 angle)
		{
			if (BodyMode == 2) return;

			FixedRotation += angle;
			UpdateColliders();
		}

		public void RotateDegrees(FixVector3 angle)
		{
			Rotate(angle * Fix64.DegToRad);
		}

		public void SetRotation(FixVector3 angle)
		{
			FixedRotation = angle;
			UpdateColliders();
		}

		public void PushAway(FixVector3 direction)
		{
			FixedPosition += direction;
			UpdateColliders();
		}
		
		public void MoveTo(FixVector3 destination)
		{
			FixedPosition = destination;
			UpdateColliders();
		}
		
		public void UpdateColliders()
		{
			//UpdateParenting();

			if (!HasColliders())
			{
				GD.Print("There is no collider attached to this body. No collision will happen.");
				return;
			}

			foreach(SharpCollider3D col in Colliders)
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
			
			foreach(SharpCollider3D col in Colliders)
			{
				col.collisionFlags.Clear();
				col.globalCollisionFlags.Clear();
			}
		}

		protected CollisionManifold3D GetCollision(SharpBody3D otherBody)
		{
			CollisionManifold3D ret = null;
			for (int i = 0; i < Collisions.Count; i++)
			{
				if (Collisions[i].CollidedWith == otherBody)
					ret = Collisions[i];
			}

			return ret;
		}

		public virtual void OnBeginOverlap(SharpBody3D other)
		{
			//CollisionManifold3D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
			//GD.Print("Entered Collision!");
		}

		public virtual void OnOverlap(SharpBody3D other)
		{
			//CollisionManifold3D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
			//GD.Print("Still colliding...");
		}

		public virtual void OnEndOverlap(SharpBody3D other)
		{
			//CollisionManifold3D collision = GetCollision(other);
            //if (collision != null)
            //{
                //Execute action here
            //}
			//GD.Print("Exited Collision!");
		}
	}
}
