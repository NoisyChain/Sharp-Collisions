using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions
{
	[System.Serializable]
	public class SharpBody2D
	{
		public delegate void OnOverlapDelegate(SharpBody2D other);
		public OnOverlapDelegate BeginOverlap;
		public OnOverlapDelegate DuringOverlap;
		public OnOverlapDelegate EndOverlap;

		public FixVector2 position;
		public FixVector2 velocity;
		public Fix64 rotation;
		public SharpCollider2D collider;
		public List<CollisionManifold2D> Collisions;
		public List<SharpBody2D> CollidedWith;
		public List<SharpBody2D> BodiesToIgnore;

		public int CollisionLayers;

		public bool isStatic;
		public bool isPushable;
		public bool isTrigger;

		public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, rotation);
		public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, rotation);
		public FixVector2 Left => -Right;
		public FixVector2 Down => -Up;
		
		public SharpBody2D() {}
		
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
			if (isStatic) return;
			
			velocity = newVelocity;
		}
		
		public void Move(int delta, int iterations)
		{
			if (isStatic) return;
			
			Fix64 fDelta = (Fix64)delta;
			Fix64 fIterations = (Fix64)iterations;

			Fix64 finalDelta = fDelta * fIterations;

			position.x += velocity.x / finalDelta;
			position.y += velocity.y / finalDelta;
			UpdateCollider();
		}

		public void Rotate(Fix64 angle)
		{
			if (isStatic) return;

			rotation += angle;
			UpdateCollider();
		}

		public void RotateDegrees(Fix64 angle)
		{
			if (isStatic) return;

			rotation += angle * Fix64.DegToRad;
			UpdateCollider();
		}

		public void SetRotation(Fix64 angle)
		{
			rotation = angle;
			UpdateCollider();
		}

		public void PushAway(FixVector2 direction)
		{
			position -= direction;
			UpdateCollider();
		}
		
		public void MoveTo(FixVector2 destination)
		{
			position = destination;
			UpdateCollider();
		}
		
		public void UpdateCollider()
		{
			collider.Position = position;
			collider.UpdatePoints(this);
			collider.UpdateBoundingBox();
		}
	}
}
