using FixMath.NET;
using Godot;
using System.Collections.Generic;

namespace SharpCollisions.Sharp3D
{
	[System.Serializable]
	public class SharpWorld3D
	{
		public List<SharpBody3D> bodies;
		
		public int BodyCount => bodies.Count;
		private uint CreatedBodies = 0;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<PossibleCollision> PossibleCollisions;
		private List<(int, int, bool)> ConfirmedCollisions;

		public const int mask = 0b_1111_1111;
		
		public SharpWorld3D()
		{
			bodies = new List<SharpBody3D>();
			PossibleCollisions = new List<PossibleCollision>();
			ConfirmedCollisions = new List<(int, int, bool)>();
		}
		
		public void AddBody(SharpBody3D newBody)
		{
			newBody.SetBodyID(CreatedBodies);
			bodies.Add(newBody);
			CreatedBodies++;
			GD.Print(BodyCount);
		}
		
		public bool RemoveBody(SharpBody3D body)
		{
			return bodies.Remove(body);
		}

		public bool GetBody(int index, out SharpBody3D body)
		{
			body = null;

			if(index < 0 || index >= bodies.Count)
			{
				return false;
			}

			body = bodies[index];
			return true;
		}

		public void ResetAllIgnoreStates()
		{
			foreach(SharpBody3D body in bodies)
				body.ResetIgnoreBodies();
		}

		private bool CompareLayers(SharpBody3D bodyA, SharpBody3D bodyB)
		{
			/*for (int i = 0; i < 8; i++)
			{
				if ((bodyA.CollisionLayers & 1 << i) != 0)
					if ((bodyA.CollisionLayers & 1 << i) == (bodyB.CollisionLayers & 1 << i))
						return true;
			}
			return false;*/
			
			//DON'T ASK ME WHAT'S HAPPENING HERE
			return ((bodyA.CollisionMask & bodyB.CollisionLayers) & mask) != 0;
		}

		private void BroadPhase()
		{
			PossibleCollisions.Clear();

			for (int i = 0; i < bodies.Count; i++)
			{
				SharpBody3D bodyA = bodies[i];
				bodyA.Collider.collisionFlags.Clear();
				bodyA.Collider.globalCollisionFlags.Clear();

				for (int j = i + 1; j < bodies.Count; j++)
				{
					SharpBody3D bodyB = bodies[j];
					
					if (!bodyA.Visible || !bodyB.Visible)
					{ ClearCollision(bodyA, bodyB); continue; }
					if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2)
					{ ClearCollision(bodyA, bodyB); continue; }
					if (bodyA.BodiesToIgnore.Contains(bodyB.GetBodyID()))
					{ ClearCollision(bodyA, bodyB); continue; }
					if (!CompareLayers(bodyA, bodyB))
					{ ClearCollision(bodyA, bodyB); continue; }

					if (!bodyA.Collider.BoundingBox.IsOverlapping(bodyB.Collider.BoundingBox))
					{ ClearCollision(bodyA, bodyB); continue; }

					PossibleCollisions.Add(new PossibleCollision(
						i, j,
						GetCollisionDistance(bodyA.Collider, bodyB.Collider)
					));

					bodyA.Collisions.Clear();
					bodyB.Collisions.Clear();
				}
			}

			//Sort the colliders so the nearest colliders are checked first
			PossibleCollisions.Sort((a, b) => b.distance.CompareTo(a.distance));
			//Sort again to reorder by bodies keeping the distance
			PossibleCollisions.Sort((a, b) => a.BodyA.CompareTo(b.BodyA));
		}

		private void NarrowPhase()
		{
			for(int i = 0; i < PossibleCollisions.Count; i ++)
			{
				SharpBody3D bodyA = bodies[PossibleCollisions[i].BodyA];
				SharpBody3D bodyB = bodies[PossibleCollisions[i].BodyB];

				if (bodyA.Collider.IsOverlapping(bodyB.Collider, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint))
				{
					if (!bodyA.isTrigger && !bodyB.isTrigger)
					{
						if (bodyA.BodyMode == 1 || bodyB.BodyMode == 1)
						{
							if (bodyA.BodyMode == 1)
								bodyA.PushAway(-Depth);
							else if (bodyB.BodyMode == 1)
								bodyB.PushAway(Depth);
						}
						else if (bodyA.BodyMode == 2)
							bodyB.PushAway(Depth);
						else if (bodyB.BodyMode == 2)
							bodyA.PushAway(-Depth);
						else if (bodyA.BodyMode == 0 && bodyB.BodyMode == 0)
						{
							bodyA.PushAway(-Depth / Fix64.Two);
							bodyB.PushAway(Depth / Fix64.Two);
						}

						//ResolvePhysics(bodyA, bodyB, Normal);
					}
					CollisionManifold3D collisionA = new CollisionManifold3D
					(
						bodyB, -Normal, Depth, ContactPoint
					);
					CollisionManifold3D collisionB = new CollisionManifold3D
					(
						bodyA, Normal, Depth, ContactPoint
					);
					bodyA.Collisions.Add(collisionA);
					bodyB.Collisions.Add(collisionB);

					if (!bodyA.isTrigger && !bodyB.isTrigger)
					{
						bodyA.Collider.collisionFlags = bodyA.Collider.GetCollisionFlags(collisionA, bodyA);
						bodyB.Collider.collisionFlags = bodyB.Collider.GetCollisionFlags(collisionB, bodyB);
						bodyA.Collider.globalCollisionFlags = bodyA.Collider.GetGlobalCollisionFlags(collisionA);
						bodyB.Collider.globalCollisionFlags = bodyB.Collider.GetGlobalCollisionFlags(collisionB);
					}
					
					if (!ConfirmedCollisions.Contains((PossibleCollisions[i].BodyA, PossibleCollisions[i].BodyB, true)))
						ConfirmedCollisions.Add((PossibleCollisions[i].BodyA, PossibleCollisions[i].BodyB, true));
					
					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					if (!ConfirmedCollisions.Contains((PossibleCollisions[i].BodyA, PossibleCollisions[i].BodyB, false)))
						ConfirmedCollisions.Add((PossibleCollisions[i].BodyA, PossibleCollisions[i].BodyB, false));
				}
			}
		}

		private void SetCollidedWith(SharpBody3D bodyA, SharpBody3D bodyB, bool hasCollided)
		{
			if (hasCollided)
			{
				if (!bodyA.CollidedWith.Contains(bodyB.GetBodyID()))
				{
					bodyA.BeginOverlap(bodyB);
					bodyA.CollidedWith.Add(bodyB.GetBodyID());
				}
				else
					bodyA.DuringOverlap(bodyB);
			}
			else
			{
				if (bodyA.CollidedWith.Contains(bodyB.GetBodyID()))
				{
					bodyA.EndOverlap(bodyB);
					bodyA.CollidedWith.Remove(bodyB.GetBodyID());
				}
			}
		}

		private void ClearCollision(SharpBody3D bodyA, SharpBody3D bodyB)
		{
			SetCollidedWith(bodyA, bodyB, false);
			SetCollidedWith(bodyB, bodyA, false);
			bodyA.Collisions.Clear();
			//bodyB.Collisions.Clear();
		}


		private void MoveBodies(int steps, int iterations)
		{
			for (int i = 0; i < bodies.Count; i++)
				bodies[i].Move(steps, iterations);
		}

		private void CallCollisionEvents()
		{
			for (int i = 0; i < ConfirmedCollisions.Count; i++)
			{
				(int, int, bool) cur = ConfirmedCollisions[i];
				SetCollidedWith(bodies[cur.Item1], bodies[cur.Item2], cur.Item3);
				SetCollidedWith(bodies[cur.Item2], bodies[cur.Item1], cur.Item3);
			}
			ConfirmedCollisions.Clear();
		}

		/*void ResolvePhysics(SharpBody3D bodyA, SharpBody3D bodyB, FixVector3 normal)
		{
			FixVector3 relativeVelocity = bodyB.Velocity - bodyA.Velocity;

			if (FixVector3.IsSameDirection(relativeVelocity, normal))
			{
				return;
			}

			//Restituition (bounciness)
			Fix64 e = Fix64.Zero; //Min(bodyA.Restituition, bodyB.Restituition)

			Fix64 j = -(Fix64.One + e) * FixVector3.Dot(relativeVelocity, normal);

			j /= Fix64.Two; //bodyA.InverseMass + bodyB.InverseMass

			FixVector3 impulse = j * normal;

			if (bodyA.BodyMode == 0) bodyA.Velocity -= impulse;
			if (bodyB.BodyMode == 0) bodyB.Velocity += impulse;
		}*/
		
		public void Simulate(int steps, int iterations)
		{
			if (BodyCount == 0) return;
			iterations = Mathf.Clamp(iterations, MinIterations, MaxIterations);
			
			for (int it = 0; it < iterations; it++)
			{
				MoveBodies(steps, iterations);
				BroadPhase();
				NarrowPhase();
			}
			CallCollisionEvents();
		}

		public Fix64 GetCollisionDistance(SharpCollider3D colliderA, SharpCollider3D colliderB)
		{
			FixVector3 length = colliderB.Center - colliderA.Center;

			FixVector3 newDepth = FixVector3.Zero;
			newDepth.x = (colliderA.BoundingBox.w - colliderA.Center.x) + (colliderB.BoundingBox.w - colliderB.Center.x);
			newDepth.y = (colliderA.BoundingBox.h - colliderA.Center.y) + (colliderB.BoundingBox.h - colliderB.Center.y);
			newDepth.z = (colliderA.BoundingBox.d - colliderA.Center.z) + (colliderB.BoundingBox.d - colliderB.Center.z);
			newDepth.x -= Fix64.Abs(length.x);
			newDepth.y -= Fix64.Abs(length.y);
			newDepth.z -= Fix64.Abs(length.z);

			return FixVector3.Length(newDepth);
		}
	}
}