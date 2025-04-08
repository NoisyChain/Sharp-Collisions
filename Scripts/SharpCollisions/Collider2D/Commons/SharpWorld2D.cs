using FixMath.NET;
using Godot;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
	[System.Serializable]
	public class SharpWorld2D
	{
		public List<SharpBody2D> bodies;
		
		public int BodyCount => bodies.Count;
		private uint CreatedBodies = 0;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<PossibleCollision> PossibleCollisions;
		private List<(int, int, int, int, bool)> ConfirmedCollisions;

		public const int mask = 0b_1111_1111;
		
		public SharpWorld2D()
		{
			bodies = new List<SharpBody2D>();
			PossibleCollisions = new List<PossibleCollision>();
			ConfirmedCollisions = new List<(int, int, int, int, bool)>();
		}
		
		public void AddBody(SharpBody2D newBody)
		{
			newBody.SetBodyID(CreatedBodies);
			bodies.Add(newBody);
			CreatedBodies++;
			GD.Print(BodyCount);
		}
		
		public bool RemoveBody(SharpBody2D body)
		{
			return bodies.Remove(body);
		}

		public bool GetBody(int index, out SharpBody2D body)
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
			foreach(SharpBody2D body in bodies)
				body.ResetIgnoreBodies();
		}

		private bool CompareLayers(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			/*for (int i = 0; i < 8; i++)
			{
				if ((bodyA.CollisionLayers & 1 << i) != 0)
					if ((bodyA.CollisionLayers & 1 << i) == (bodyB.CollisionLayers & 1 << i))
						return true;
			}
			return false;*/
			
			//DON'T ASK ME WHAT'S HAPPENING HERE
			return ((colliderA.CollisionMask & colliderB.CollisionLayers) & mask) != 0;
		}

		private void BroadPhase()
		{
			PossibleCollisions.Clear();
			foreach(SharpBody2D body in bodies)
			{
				body.ClearFlags();
				body.ClearCollisions();
			}

			for (int i = 0; i < bodies.Count; i++)
			{
				for (int j = i + 1; j < bodies.Count; j++)
				{
					//Check every collider in each body
					CheckColliders(i, j);
				}
			}

			//Sort the colliders so the nearest colliders are checked first
			PossibleCollisions.Sort((a, b) => b.distance.CompareTo(a.distance));
			//Sort again to reorder by bodies keeping the distance
			PossibleCollisions.Sort((a, b) => a.BodyA.CompareTo(b.BodyA));
		}

		private void CheckColliders(int indA, int indB)
		{
			SharpBody2D bodyA = bodies[indA];
			SharpBody2D bodyB = bodies[indB];

			if (!bodyA.HasColliders() || !bodyB.HasColliders()) return;

			if (!bodyA.Active || !bodyB.Active)
			{ ClearCollision(indA, 0, indB, 0); return; }
			if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2)
			{ ClearCollision(indA, 0, indB, 0); return; }
			if (bodyA.BodiesToIgnore.Contains(bodyB.GetBodyID()))
			{ ClearCollision(indA, 0, indB, 0); return; }
			if (!bodyA.BoundingBox.IsOverlapping(bodyB.BoundingBox))
			{ ClearCollision(indA, 0, indB, 0); return; }

			for (int i = 0; i < bodyA.GetColliders().Length; i++)
			{
				for (int j = 0; j < bodyB.GetColliders().Length; j++)
				{
					if (!bodyA.GetCollider(i).Active || !bodyB.GetCollider(j).Active)
					{ ClearCollision(indA, i, indB, j); continue; }
					if (!CompareLayers(bodyA.GetCollider(i), bodyB.GetCollider(j)))
					{ ClearCollision(indA, i, indB, j); continue; }
					if (!bodyA.GetCollider(i).BoundingBox.IsOverlapping(bodyB.GetCollider(j).BoundingBox))
					{ ClearCollision(indA, i, indB, j); continue; }

					PossibleCollisions.Add(new PossibleCollision(
						indA, indB, i, j,
						GetCollisionDistance(bodyA.GetCollider(i), bodyB.GetCollider(j))
					));
				}
			}
		}

		private void NarrowPhase()
		{
			for(int i = 0; i < PossibleCollisions.Count; i ++)
			{
				SharpBody2D bodyA = bodies[PossibleCollisions[i].BodyA];
				SharpBody2D bodyB = bodies[PossibleCollisions[i].BodyB];
				int colIndA = PossibleCollisions[i].ColliderA;
				int colIndB = PossibleCollisions[i].ColliderB;

				if (bodyA.GetCollider(colIndA).IsOverlapping(bodyB.GetCollider(colIndB), out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint))
				{
					if (!bodyA.GetCollider(colIndA).isTrigger && !bodyB.GetCollider(colIndB).isTrigger)
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

					bodyA.AddCollision(new CollisionManifold2D(bodyB, colIndA, colIndB, -Normal, Depth, ContactPoint));
					bodyB.AddCollision(new CollisionManifold2D(bodyA, colIndB, colIndA, Normal, Depth, ContactPoint));

					if (!bodyA.GetCollider(colIndA).isTrigger && !bodyB.GetCollider(colIndB).isTrigger)
					{
						bodyA.GetCollider(colIndA).GetCollisionFlags(-Normal, bodyA);
						bodyB.GetCollider(colIndB).GetCollisionFlags(Normal, bodyB);
						bodyA.GetCollider(colIndA).GetGlobalCollisionFlags(-Normal);
						bodyB.GetCollider(colIndB).GetGlobalCollisionFlags(Normal);
					}
					
					AddConfirmedCollision((PossibleCollisions[i].BodyA, colIndA, PossibleCollisions[i].BodyB, colIndB, true));					
					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					AddConfirmedCollision((PossibleCollisions[i].BodyA, colIndA, PossibleCollisions[i].BodyB, colIndB, false));
				}
			}
		}

		private void AddConfirmedCollision((int, int, int, int, bool) col)
		{
			if (!ConfirmedCollisions.Contains(col))
				ConfirmedCollisions.Add(col);
		}

		private void SetCollidedWith(SharpBody2D bodyA, SharpBody2D bodyB, int colA, int colB, bool hasCollided)
		{
			if (hasCollided)
			{
				CollisionManifold2D col = bodyA.GetCollision(bodyB, colB);
				if (col == null) return;

				if (!bodyA.CollidedWith.Contains((bodyB.GetBodyID(), colB)))
				{
					bodyA.OnBeginOverlap(col);
					bodyA.CollidedWith.Add((bodyB.GetBodyID(), colB));
				}
				else
					bodyA.OnOverlap(col);
			}
			else
			{
				if (bodyA.CollidedWith.Contains((bodyB.GetBodyID(), colB)))
				{
					CollisionManifold2D col = new CollisionManifold2D(bodyB, colA, colB, FixVector2.Zero, FixVector2.Zero, FixVector2.Zero);

					bodyA.OnEndOverlap(col);
					bodyA.CollidedWith.Remove((bodyB.GetBodyID(), colB));
				}
			}
		}

		private void ClearCollision(int bodyA, int colA, int bodyB, int colB)
		{
			SetCollidedWith(bodies[bodyA], bodies[bodyB], colA, colB, false);
			SetCollidedWith(bodies[bodyB], bodies[bodyA], colB, colA, false);
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
				(int, int, int, int, bool) cur = ConfirmedCollisions[i];
				SetCollidedWith(bodies[cur.Item1], bodies[cur.Item3], cur.Item2, cur.Item4, cur.Item5);
				SetCollidedWith(bodies[cur.Item3], bodies[cur.Item1], cur.Item4, cur.Item2, cur.Item5);
			}
		}

		/*void ResolvePhysics(SharpBody2D bodyA, SharpBody2D bodyB, FixVector2 normal)
		{
			FixVector2 relativeVelocity = bodyB.Velocity - bodyA.Velocity;

			if (FixVector2.IsSameDirection(relativeVelocity, normal))
			{
				return;
			}

			//Restituition (bounciness)
			Fix64 e = Fix64.Zero; //Min(bodyA.Restituition, bodyB.Restituition)

			Fix64 j = -(Fix64.One + e) * FixVector2.Dot(relativeVelocity, normal);

			j /= Fix64.Two;  //bodyA.InverseMass + bodyB.InverseMass

			FixVector2 impulse = j * normal;

			if (bodyA.BodyMode == 0) bodyA.Velocity -= impulse;
			if (bodyB.BodyMode == 0) bodyB.Velocity += impulse;
		}*/
		
		public void Simulate(int steps, int iterations)
		{
			if (BodyCount == 0) return;
			
			iterations = Mathf.Clamp(iterations, MinIterations, MaxIterations);

			ConfirmedCollisions.Clear();
			for (int it = 0; it < iterations; it++)
			{
				MoveBodies(steps, iterations);
				BroadPhase();
				NarrowPhase();
			}
			CallCollisionEvents();
		}

		public Fix64 GetCollisionDistance(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 length = colliderB.Center - colliderA.Center;

			FixVector2 newDepth = FixVector2.Zero;
			newDepth.x = (colliderA.BoundingBox.w - colliderA.Center.x) + (colliderB.BoundingBox.w - colliderB.Center.x);
			newDepth.y = (colliderA.BoundingBox.h - colliderA.Center.y) + (colliderB.BoundingBox.h - colliderB.Center.y);
			newDepth.x -= Fix64.Abs(length.x);
			newDepth.y -= Fix64.Abs(length.y);

			return FixVector2.Length(newDepth);
		}
	}
}