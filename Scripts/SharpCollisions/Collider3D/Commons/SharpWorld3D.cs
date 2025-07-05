using FixMath.NET;
using Godot;
using SharpCollisions.Sharp3D.Octree;
using System.Collections.Generic;

namespace SharpCollisions.Sharp3D
{
	[System.Serializable]
	public class SharpWorld3D
	{
		public List<SharpBody3D> bodies;
		private OcTree oTree;
		
		public int BodyCount => bodies.Count;
		private uint CreatedBodies = 0;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<PossibleCollision> PossibleCollisions;
		private List<(int, int, int, int, bool)> ConfirmedCollisions;

		public const int mask = 0b_1111_1111;

		public SharpWorld3D(int qtSize, int qtLimit)
		{
			bodies = new List<SharpBody3D>();
			PossibleCollisions = new List<PossibleCollision>();
			ConfirmedCollisions = new List<(int, int, int, int, bool)>();
			if (qtSize > 0)
			{
				Fix64 octreeSize = new Fix64(qtSize);
				oTree = new OcTree(
					new FixVolume(-octreeSize, -octreeSize, -octreeSize, octreeSize, octreeSize, octreeSize), qtLimit
				);
			}
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

		private bool CompareLayers(SharpBody3D colliderA, SharpBody3D colliderB)
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
			for (int i = 0; i < bodies.Count; i++)
			{
				bodies[i].ClearFlags();
				bodies[i].ClearCollisions();
			}

			if (oTree == null) //Brute force the broad phase if no quadtree was created
			{
				for (int i = 0; i < bodies.Count; i++)
				{
					for (int j = i + 1; j < bodies.Count; j++)
					{
						//Check every collider in each body
						CheckColliders(i, j);
					}
				}
			}
			else //Use the quadtree otherwise
			{
				List<IntPack2> collisionQueries = new List<IntPack2>();
				oTree.Compute(bodies);
				oTree.CapturePossibleCollisions(ref collisionQueries);
				
				GD.Print($"Registered {collisionQueries.Count} collision queries");
				
				foreach (IntPack2 q in collisionQueries)
					CheckColliders(q.a, q.b);
			}

			//Sort the colliders so the nearest colliders are checked first
			PossibleCollisions.Sort((a, b) => b.distance.CompareTo(a.distance));
			//Sort again to reorder by bodies keeping the distance
			PossibleCollisions.Sort((a, b) => a.BodyA.CompareTo(b.BodyA));
			//Sort again by priority
			PossibleCollisions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
		}

		private void CheckColliders(int indA, int indB)
		{
			SharpBody3D bodyA = bodies[indA];
			SharpBody3D bodyB = bodies[indB];
			
			if (!bodyA.HasColliders() || !bodyB.HasColliders()) return;
			if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2) return; 
			
			if (!bodyA.Active || !bodyB.Active)
			{ ClearCollision(indA, 0, indB, 0); return; }
			if (bodyA.IsIgnoringBody(bodyB))
			{ ClearCollision(indA, 0, indB, 0); return; }
			if (!CompareLayers(bodyA, bodyB))
			{ ClearCollision(indA, 0, indB, 0); return; }
			if (!bodyA.BoundingBox.IsOverlapping(bodyB.BoundingBox))
			{ ClearCollision(indA, 0, indB, 0); return; }

			for (int i = 0; i < bodyA.GetColliders().Length; i++)
			{
				for (int j = 0; j < bodyB.GetColliders().Length; j++)
				{
					if (!bodyA.GetCollider(i).Active || !bodyB.GetCollider(j).Active)
					{ ClearCollision(indA, i, indB, j);  continue; }
					if (!bodyA.GetCollider(i).BoundingBox.IsOverlapping(bodyB.GetCollider(j).BoundingBox))
					{ ClearCollision(indA, i, indB, j);  continue; }

					PossibleCollisions.Add(new PossibleCollision(
						indA, indB, i, j, Mathf.Max(bodyA.Priority, bodyB.Priority),
						GetCollisionDistance(bodyA.GetCollider(i), bodyB.GetCollider(j))
					));
				}
			}
		}

		private void NarrowPhase()
		{
			for(int i = 0; i < PossibleCollisions.Count; i ++)
			{
				SharpBody3D bodyA = bodies[PossibleCollisions[i].BodyA];
				SharpBody3D bodyB = bodies[PossibleCollisions[i].BodyB];
				int colIndA = PossibleCollisions[i].ColliderA;
				int colIndB = PossibleCollisions[i].ColliderB;

				if (bodyA.GetCollider(colIndA).IsOverlapping(bodyB.GetCollider(colIndB), out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint))
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

					bodyA.AddCollision(new CollisionManifold3D(bodyB, colIndA, colIndB, -Normal, Depth, ContactPoint));
					bodyB.AddCollision(new CollisionManifold3D(bodyA, colIndB, colIndA, Normal, Depth, ContactPoint));

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

		private void SetCollidedWith(SharpBody3D bodyA, SharpBody3D bodyB, int colA, int colB, bool hasCollided)
		{
			if (hasCollided)
			{
				CollisionManifold3D col = bodyA.GetCollision(bodyB, colB);
				if (col == null) return;
				
				if (!bodyA.HasCollidedWith((bodyB.GetBodyID(), colB)))
				{
					bodyA.OnBeginOverlap(col);
					bodyA.ConfirmCollision((bodyB.GetBodyID(), colB));
				}
				else
					bodyA.OnOverlap(col);
			}
			else
			{
				if (bodyA.HasCollidedWith((bodyB.GetBodyID(), colB)))
				{
					CollisionManifold3D col = new CollisionManifold3D(bodyB, colA, colB, FixVector3.Zero, FixVector3.Zero, FixVector3.Zero);
					bodyA.OnEndOverlap(col);
					bodyA.RemoveCollision((bodyB.GetBodyID(), colB));
				}
			}
		}

		private void ClearCollision(int bodyA, int colA, int bodyB, int colB)
		{
			SetCollidedWith(bodies[bodyA], bodies[bodyB], colA, colB, false);
			SetCollidedWith(bodies[bodyB], bodies[bodyA], colB, colA, false);
		}

		private void MoveBodies()
		{
			for (int i = 0; i < bodies.Count; i++)
				bodies[i].UpdateBody();
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
		
		public void Simulate()
		{
			if (BodyCount == 0) return;
			int iterations = Mathf.Clamp(SharpTime.Substeps, MinIterations, MaxIterations);
			
			ConfirmedCollisions.Clear();
			for (int it = 0; it < iterations; it++)
			{
				MoveBodies();
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

			return FixVector3.LengthSq(newDepth);
		}
	}
}