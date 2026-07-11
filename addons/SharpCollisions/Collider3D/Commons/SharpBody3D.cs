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

		[Export] public int Priority = 0;
		[Export] private SharpCollider3D[] Colliders;
		[Export] private SharpBody3D[] Attachments;
		[Export(PropertyHint.Enum, "Dynamic,Kinematic,Static")]
		public int BodyMode = 0;
		[Export(PropertyHint.Layers3DPhysics)]
		public int CollisionLayers = 1;
		[Export(PropertyHint.Layers3DPhysics)]
		public int CollisionMask = 1;
		
		private SharpBody3D AttachedTo;
		private List<CollisionManifold3D> Collisions = new List<CollisionManifold3D>();
		private List<(uint, int)> CollidedWith = new List<(uint, int)>();
		private List<uint> BodiesToIgnore = new List<uint>();

		public void SetAttachment(SharpBody3D parent) { AttachedTo = parent; }
		public bool HasAttachments() => Attachments != null && Attachments.Length > 0;
		public bool IsAttached() => AttachedTo != null;

		public SharpCollider3D[] GetColliders() => Colliders;
		public SharpCollider3D GetCollider(int index) => Colliders[index];

		private bool IsStationary => FixVector3.LengthSq(LinearVelocity) == Fix64.Zero && FixVector3.LengthSq(AngularVelocity) == Fix64.Zero;

		private bool UpdatePositions;
		private bool UpdateRotations;
		private bool UpdateBoundingBoxes;

		public override void _Instance()
		{
			base._Instance();
			if (SharpManager.Instance != null)
				SharpManager.Instance.AddBody(this);
			else
                GD.PrintErr("SharpManager not found. Make sure to add a SharpManager to the scene");

			if (HasColliders())
				foreach (SharpCollider3D col in Colliders)
					col.Initialize();

			if (HasAttachments())
			{
				foreach (SharpBody3D child in Attachments)
				{
					child.SetAttachment(this);
					child._Instance();
				}
			}

			UpdatePositions = true;
			UpdateRotations = true;
			UpdateBoundingBoxes = true;
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
			if (!Engine.IsEditorHint()) return;
			if (!HasColliders()) return;

			var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();

			foreach (SharpCollider3D col in Colliders)
				if (col != null) col.DebugDrawShapesEditor(this, selected.Contains(this));
			
			//if (selected.Contains(this)) DebugDraw3D.DrawGizmo(transform, new Color(1, 0.6f, 0.1f), true);
		}

		public void DrawColliders()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider3D col in Colliders)
			{
				if (col != null)
				{
					col.DebugDrawShapes(this);
					col.DebugDrawBoundingBox();
				}
			}
		}

		public override void _Destroy()
        {
			if (SharpManager.Instance == null)
			{
				QueueFree();
				GD.PrintErr("SharpManager not found. Make sure to add a SharpManager to the scene");
				return;
			}

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
		
		public void SetLinearVelocity(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			if (IsAttached()) return;
			
			LinearVelocity = newVelocity;
		}

		public void SetAngularVelocity(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			if (IsAttached()) return;

			AngularVelocity = newVelocity;
		}

		public void SetAngularVelocityDegrees(FixVector3 newVelocity)
		{
			if (BodyMode == 2) return;
			if (IsAttached()) return;

			SetAngularVelocity(newVelocity * Fix64.DegToRad);
		}
		
		public void Move()
		{
			if (FixVector3.Length(LinearVelocity) == Fix64.Zero) return;

			FixedPosition += LinearVelocity * SharpTime.SubDelta;
			UpdatePositions = true;
			UpdateBoundingBoxes = true;
		}

		public void Rotate()
		{
			if (FixVector3.Length(AngularVelocity) == Fix64.Zero) return;

			FixedRotation += AngularVelocity * SharpTime.SubDelta;
			UpdateRotations = true;
			UpdateBoundingBoxes = true;
		}

		public void UpdateBody()
		{
			UpdatePositions = false;
			UpdateRotations = false;
			UpdateBoundingBoxes = false;
			if (BodyMode == 2) return;
			if (IsAttached()) return;

			Rotate();
			Move();
			UpdateColliders();
		}

		public void SetPosition(FixVector3 destination)
		{
			FixedPosition = destination;
			UpdatePositions = true;
			UpdateBoundingBoxes = true;
		}

		public void SetRotation(FixVector3 angle)
		{
			FixedRotation = angle;
			UpdateRotations = true;
			UpdateBoundingBoxes = true;
		}

		public void SetRotationDegrees(FixVector3 angle)
		{
			SetRotation(angle * Fix64.DegToRad);
		}

		public void PushAway(FixVector3 direction)
		{
			if (BodyMode == 2) return;
			if (IsAttached()) return;

			FixedPosition += direction;
			UpdatePositions = true;
			UpdateRotations = true;
			if (IsStationary) UpdateBoundingBoxes = true;
			UpdateColliders();
		}
		
		

		public void UpdateAttachments()
		{
			if (!HasAttachments()) return;

			foreach (SharpBody3D child in Attachments)
			{
				child.SetRotation(FixedRotation);
				child.SetPosition(FixedPosition);
			}
		}
		
		public void UpdateColliders()
		{
			if (!HasColliders())
			{
				GD.Print("There is no collider attached to this body. No collision will happen.");
				return;
			}

			foreach (SharpCollider3D col in Colliders)
			{
				if (UpdatePositions || UpdateRotations)
					col.UpdatePoints(FixedPosition, FixedRotation);
				if (UpdateBoundingBoxes)
					col.UpdateBoundingBox();
			}

			UpdateAttachments();
			UpdatePositions = false;
			UpdateRotations = false;
			UpdateBoundingBoxes = false;
		}

		public void ClearFlags()
		{
			if (!HasColliders()) return;
			
			foreach(SharpCollider3D col in Colliders)
			{
				col.collisionFlags = CollisionFlags.Empty;
				col.globalCollisionFlags = CollisionFlags.Empty;
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
