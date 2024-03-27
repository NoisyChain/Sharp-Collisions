using Godot;
using FixMath.NET;
using System.Collections.Generic;
using System.Linq;


namespace SharpCollisions
{
	public class SharpCollider3D  : Node
	{
		[Export] public Color debugColor = new Color(0, 0, 1);

		[Export] public bool Active = true;
		[Export] protected bool DrawDebug;
		public CollisionFlags collisionFlags;
		public CollisionType3D Shape = CollisionType3D.Null;
		public FixVector3 Position;
		public FixVector3 Offset => (FixVector3)offset;
		public FixVector3 Center;
		public FixVolume BoundingBox;
		[Export] protected Vector3 offset;

		protected bool CollisionRequireUpdate = true;
		protected bool BoundingBoxRequireUpdate = true;

		protected ImmediateGeometry Draw3D;

		/*public SharpCollider2D(){}
		
		public SharpCollider2D(FixVector2 center, FixVector2 offset, FixVector2 size, FixVector2[] points, CollisionType shape)
		{
			Shape = shape;
			Position = center;
			Offset = offset;
			Radius = Fix64.Min(size.x, size.y) / Fix64.Two;
			Height = Fix64.Max(size.x, size.y) / Fix64.Two;
			Size = size;
			CreatePoints(points);
		}*/

		public override void _Ready()
		{
			Draw3D = GetNode<ImmediateGeometry>("Draw3D");
		}

		public override void _Process(float delta)
		{
			DebugDrawShapes();
		}

		public virtual void DebugDrawShapes()
		{

		}

		public bool IsOverlapping(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			if (!Active) return false;
			if (!other.Active) return false;
			
			return CollisionDetection(this, other, out Normal, out Depth, out ContactPoint);
		}

		private bool CollisionDetection(SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			if (colliderA.Shape == CollisionType3D.AABB && colliderB.Shape == CollisionType3D.AABB)
                return AABBtoAABBCollision(colliderA as AABBCollider3D, colliderB as AABBCollider3D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType3D.Sphere && colliderB.Shape == CollisionType3D.Sphere)
                return SphereToSphereCollision(colliderA as SphereCollider3D, colliderB as SphereCollider3D, out Normal, out Depth, out ContactPoint);
            else if (colliderA.Shape == CollisionType3D.Capsule && colliderB.Shape == CollisionType3D.Capsule)
                return CapsuleToCapsuleCollision(colliderA as CapsuleCollider3D, colliderB as CapsuleCollider3D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType3D.Sphere && colliderB.Shape == CollisionType3D.Capsule)
				return CapsuleToSphereCollision(colliderB as CapsuleCollider3D, colliderA as SphereCollider3D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType3D.Capsule && colliderB.Shape == CollisionType3D.Sphere)
				return CapsuleToSphereCollision(colliderA as CapsuleCollider3D, colliderB as SphereCollider3D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType3D.Polygon && colliderB.Shape == CollisionType3D.Polygon ||
					colliderA.Shape == CollisionType3D.Polygon && colliderB.Shape == CollisionType3D.Sphere ||
                    colliderA.Shape == CollisionType3D.Sphere && colliderB.Shape == CollisionType3D.Polygon ||
					colliderA.Shape == CollisionType3D.Polygon && colliderB.Shape == CollisionType3D.Capsule ||
                    colliderA.Shape == CollisionType3D.Capsule && colliderB.Shape == CollisionType3D.Polygon)
                return GJKPolygonCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
			
			return false;
		}

		public CollisionFlags GetCollisionFlags(CollisionManifold3D collisiondData, SharpBody3D body)
		{
			CollisionFlags flag = collisionFlags;

			if (FixVector3.Dot(collisiondData.Normal, body.Down) > Fix64.ETA)
				flag.Below = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Up) > Fix64.ETA)
				flag.Above = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Right) > Fix64.ETA)
				flag.Right = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Left) > Fix64.ETA)
				flag.Left = true;
            if (FixVector3.Dot(collisiondData.Normal, body.Forward) > Fix64.ETA)
				flag.Forward = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Back) > Fix64.ETA)
				flag.Back = true;
			
			return flag;
		}

		private FixVector3 SupportFunction(SharpCollider3D colliderA, SharpCollider3D colliderB, FixVector3 direction)
		{
            FixVector3 SupportPointA = colliderA.Support(direction);
            FixVector3 SupportPointB = colliderB.Support(-direction);

			return SupportPointA - SupportPointB;
		}

		public FixVector3 FindAABBNormals(AABBCollider3D colliderA, AABBCollider3D colliderB)
        {
            FixVector3 finalNormal;
            FixVector3 length = colliderB.Center - colliderA.Center;

            Fix64 ExtentsX = colliderB.Extents.x + colliderA.Extents.x;
            Fix64 ExtentsY = colliderB.Extents.y + colliderA.Extents.y;
            Fix64 ExtentsZ = colliderB.Extents.z + colliderA.Extents.z;

            // calculate normal of collided surface
			if (Fix64.Abs(length.x) + ExtentsY > Fix64.Abs(length.y) + ExtentsX || 
				Fix64.Abs(length.z) + ExtentsY > Fix64.Abs(length.y) + ExtentsZ)
			{
				if (Fix64.Abs(length.x) + ExtentsZ > Fix64.Abs(length.z) + ExtentsX)
				{
					
					if (colliderA.Center.x < colliderB.Center.x)
					{
						finalNormal = FixVector3.Right;
					} 
					else
					{
						finalNormal = FixVector3.Left;
					}
				}
				else
				{
					if (colliderA.Center.z < colliderB.Center.z)
					{
						finalNormal = FixVector3.Forward;
					} 
					else
					{
						finalNormal = FixVector3.Back;
					}
				}
			}
			else
			{
				if (colliderA.Center.y < colliderB.Center.y)
				{
					finalNormal = FixVector3.Up;
				} 
				else
				{
					finalNormal = FixVector3.Down;
				}
			}
            return finalNormal;
        }

		public bool AABBtoAABBCollision(AABBCollider3D colliderA, AABBCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            bool collisionX = colliderA.Center.x - colliderA.Extents.x <= colliderB.Center.x + colliderB.Extents.x &&
                colliderA.Center.x + colliderA.Extents.x >= colliderB.Center.x - colliderB.Extents.x;

            bool collisionY = colliderA.Center.y - colliderA.Extents.y <= colliderB.Center.y + colliderB.Extents.y &&
                colliderA.Center.y + colliderA.Extents.y >= colliderB.Center.y - colliderB.Extents.y;
            
            bool collisionZ = colliderA.Center.z - colliderA.Extents.z <= colliderB.Center.z + colliderB.Extents.z &&
                colliderA.Center.z + colliderA.Extents.z >= colliderB.Center.z - colliderB.Extents.z;


            if (collisionX && collisionY && collisionZ)
            {
                ContactPoint = AABBContactPoint(colliderA, colliderB);

                FixVector3 length = colliderB.Center - colliderA.Center;

                FixVector3 newDepth = FixVector3.Zero;
                newDepth.x = colliderA.Extents.x + colliderB.Extents.x;
                newDepth.y = colliderA.Extents.y + colliderB.Extents.y;
                newDepth.z = colliderA.Extents.z + colliderB.Extents.z;
                newDepth.x -= Fix64.Abs(length.x);
                newDepth.y -= Fix64.Abs(length.y);
                newDepth.z -= Fix64.Abs(length.z);
                Normal = FindAABBNormals(colliderA, colliderB);
                Depth = Normal * newDepth;
            }

            return collisionX && collisionY && collisionZ;
        }

		public bool SphereToSphereCollision(SphereCollider3D colliderA, SphereCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			Fix64 distance = FixVector3.Distance(colliderA.Center, colliderB.Center);
			Fix64 radii = colliderA.Radius + colliderB.Radius;
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(colliderB.Center - colliderA.Center);
				Depth = Normal * (radii - distance);
				ContactPoint = SphereContactPoint(colliderA, colliderB);
			}
			
			return collision;
		}

		public bool CapsuleToCapsuleCollision(CapsuleCollider3D colliderA, CapsuleCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			LineToLineDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector3 r1, out FixVector3 r2);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector3.Distance(r1, r2);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(r2 - r1);
				Depth = Normal * (radii - distance);
				ContactPoint = CapsuleContactPoint(r1, colliderA.Radius, r2, colliderB.Radius, Normal);
;
			}
			
			return collision;
		}

		public bool CapsuleToSphereCollision(CapsuleCollider3D colliderA, SphereCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector3 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector3.Distance(CapsulePoint, colliderB.Center);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(colliderB.Center - CapsulePoint);
				Depth = Normal * (radii - distance);
				ContactPoint = CapsuleContactPoint(CapsulePoint, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
			}
			
			return collision;
		}

		public bool GJKPolygonCollision(SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			Simplex3D Simplex = new Simplex3D(
				new List<FixVector3>() { FixVector3.Zero, FixVector3.Zero, FixVector3.Zero, FixVector3.Zero }
			);

			FixVector3 supportDirection = colliderB.Center - colliderA.Center;
			FixVector3 SupportPoint;

			while (true)
			{
				SupportPoint = SupportFunction(colliderA, colliderB, supportDirection);
				if (!FixVector3.IsSameDirection(SupportPoint, supportDirection))
					return false;

				Simplex.MoveForward(SupportPoint);

				if (CheckSimplex(ref Simplex, ref supportDirection))
				{
					EPA(Simplex, colliderA, colliderB, out Normal, out Fix64 newDepth, out ContactPoint);
					Depth = Normal * newDepth;
					return true;
				}
			}
		}

		private bool CheckSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			switch (Simplex.Size)
			{
				case 1: //Point
					return PointSimplex(ref supportDirection);
				case 2: //Line
					return LineSimplex(ref Simplex, ref supportDirection);
				case 3: //Triangle
					return TriangleSimplex(ref Simplex, ref supportDirection);
				case 4:
                    return TetrahedronSimplex(ref Simplex, ref supportDirection);
			}

			//Should never be here
			return false;
		}

		private bool PointSimplex(ref FixVector3 supportDirection)
		{
			supportDirection *= Fix64.NegativeOne;
			return false;
		}

		private bool LineSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			FixVector3 a = Simplex.Points[0];
			FixVector3 b = Simplex.Points[1];

			FixVector3 ab = b - a;
			FixVector3 ao = a * Fix64.NegativeOne;

			supportDirection = FixVector3.TripleProduct(ab, ao, ab);
			return false;
		}

		private bool TriangleSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			FixVector3 a = Simplex.Points[0];
			FixVector3 b = Simplex.Points[1];
			FixVector3 c = Simplex.Points[2];

			FixVector3 ab = b - a;
			FixVector3 ac = c - a;
			FixVector3 ao = a * Fix64.NegativeOne;

			FixVector3 abc = FixVector3.Cross(ab, ac);

			if (FixVector3.IsSameDirection(FixVector3.Cross(abc, ac), ao))
			{
				if (FixVector3.IsSameDirection(ac, ao))
				{
					Simplex.Reset(new List<FixVector3>(){ a, c });
					supportDirection = FixVector3.TripleProduct(ac, ao, ac);
				}
				else
				{
					Simplex.Reset(new List<FixVector3>(){ a, b });
					return LineSimplex(ref Simplex, ref supportDirection);
				}
			}
			else
			{
				if (FixVector3.IsSameDirection(FixVector3.Cross(ab, abc), ao))
				{
					Simplex.Reset(new List<FixVector3>(){ a, b });
					return LineSimplex(ref Simplex, ref supportDirection);
				}
				else
				{
					if (FixVector3.IsSameDirection(abc, ao))
					{
						supportDirection = abc;
					}
					else
					{
						Simplex.Reset(new List<FixVector3>(){ a, c, b });
						supportDirection = -abc;
					}
				}
			}

			/*if(FixVector3.IsSameDirection(abPerp, ao))
			{
                // the origin is outside line ab
                // get rid of c and add a new support in the direction of abPerp
				Simplex.Reset(new List<FixVector3>(){a, b});
                supportDirection = abPerp;
            }
            else if(FixVector3.IsSameDirection(acPerp, ao))
			{
                // the origin is outside line ac
                // get rid of b and add a new support in the direction of acPerp
				Simplex.Reset(new List<FixVector3>(){a, c});
                supportDirection = acPerp;
            }
            else
			{
                // the origin is inside both ab and ac,
                // so it must be inside the triangle!
                return true;
            }*/

			return false;
		}
		bool TetrahedronSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			FixVector3 a = Simplex.Points[0];
			FixVector3 b = Simplex.Points[1];
			FixVector3 c = Simplex.Points[2];
			FixVector3 d = Simplex.Points[3];

			FixVector3 ab = b - a;
			FixVector3 ac = c - a;
			FixVector3 ad = d - a;
			FixVector3 ao = a * Fix64.NegativeOne;

			FixVector3 abc = FixVector3.Cross(ab, ac);
			FixVector3 acd = FixVector3.Cross(ac, ad);
			FixVector3 adb = FixVector3.Cross(ad, ab);

			if (FixVector3.IsSameDirection(abc, ao))
			{
				Simplex.Reset(new List<FixVector3>(){a, b, c});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
			if (FixVector3.IsSameDirection(acd, ao))
			{
				Simplex.Reset(new List<FixVector3>(){a, c, d});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
			if (FixVector3.IsSameDirection(adb, ao))
			{
				Simplex.Reset(new List<FixVector3>(){a, d, b});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
			return false;
		}

		private bool GetPolytopeDirection(List<FixVector3> polytope)
		{
			Fix64 e0 = (polytope[1].x - polytope[0].x) * (polytope[1].y + polytope[0].y);
			Fix64 e1 = (polytope[2].x - polytope[1].x) * (polytope[2].y + polytope[1].y);
			Fix64 e2 = (polytope[0].x - polytope[2].x) * (polytope[0].y + polytope[2].y);

			return e0 + e1 + e2 > Fix64.Zero;
		}

		private void GetFaceNormals(ref List<FixVector3> polytope, ref List<int> faces, out List<FixVector3> normals, out List<Fix64> distances, out int minFace)
		{
			normals = new List<FixVector3>();
			distances = new List<Fix64>();
			minFace = 0;
			Fix64 minDistance = Fix64.MaxValue;

			for (int i = 0; i < faces.Count; i += 3)
			{
				FixVector3 a = polytope[faces[i]];
				FixVector3 b = polytope[faces[i + 1]];
				FixVector3 c = polytope[faces[i + 2]];

				FixVector3 normal = FixVector3.Normalize(FixVector3.Cross(b - a, c - a));
				Fix64 distance = FixVector3.Dot(normal, a);

				if (distance < Fix64.Zero)
				{
					normal *= Fix64.NegativeOne;
					distance *= Fix64.NegativeOne;
				}

				normals.Add(normal);
				distances.Add(distance);

				if (distance < minDistance)
				{
					minFace = i / 3;
					minDistance = distance;
				}
			}
		}

		private void AddIfUniqueEdge(ref List<(int, int)> edges, List<int> faces, int a, int b)
		{
			//      0--<--3
			//     / \ B /   A: 2-0
			//    / A \ /    B: 0-2
			//   1-->--2

			var reverse = edges.Find(x => x == (faces[b], faces[a]));
		
			if (reverse != edges[edges.Count - 1])
			{
				edges.Remove(reverse);
			}
			else
			{
				edges.Add((faces[a], faces[b]));
			}
		}

		private void EPA(Simplex3D simplex, SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out Fix64 Depth, out FixVector3 Contact)
		{
			List<FixVector3> polytope = simplex.Points;
			List<int> faces  = new List<int>()
			{
				0, 1, 2,
				0, 3, 1,
				0, 2, 3,
				1, 3, 2
			};

			GetFaceNormals(ref polytope, ref faces, out List<FixVector3> normals, out List<Fix64> distances, out int minFace);

			Fix64 minDistance = Fix64.MaxValue;
			FixVector3 minNormal = FixVector3.Zero;

			while (minDistance == Fix64.MaxValue)
			{
				minNormal   = normals[minFace];
				minDistance = distances[minFace];
		
				FixVector3 support = SupportFunction(colliderA, colliderB, minNormal);
				Fix64 sDistance = FixVector3.Dot(minNormal, support);
		
				if (Fix64.Abs(sDistance - minDistance) > Fix64.ETA)
				{
					minDistance = Fix64.MaxValue;
				
					//std::vector<std::pair<size_t, size_t>> uniqueEdges;
					List<(int, int)> uniqueEdges = new List<(int, int)>();

					for (int i = 0; i < normals.Count; i++)
					{
						if (FixVector3.IsSameDirection(normals[i], support))
						{
							int f = i * 3;

							AddIfUniqueEdge(ref uniqueEdges, faces, f, f + 1);
							AddIfUniqueEdge(ref uniqueEdges, faces, f + 1, f + 2);
							AddIfUniqueEdge(ref uniqueEdges, faces, f + 2, f);

							faces[f + 2] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);
							faces[f + 1] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);
							faces[f    ] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);

							normals[i] = normals[normals.Count - 1];
							normals.RemoveAt(normals.Count - 1);
							distances[i] = distances[distances.Count - 1];
							distances.RemoveAt(distances.Count - 1);

							i--;
						}
					}

					//std::vector<size_t> newFaces;
					List<int> newFaces = new List<int>();
					foreach ((int, int) edges in uniqueEdges)
					{
						newFaces.AddRange(new List<int>() { edges.Item1, edges.Item2, polytope.Count} );
					}
			 
					polytope.Add(support);

					GetFaceNormals(ref polytope, ref newFaces, out List<FixVector3> newNormals, out List<Fix64> newDistances, out int newMinFace);

					Fix64 oldMinDistance = Fix64.MaxValue;
					for (int i = 0; i < distances.Count; i++)
					{
						if (distances[i] < oldMinDistance)
						{
							oldMinDistance = distances[i];
							minFace = i;
						}
					}
 
					if (newDistances[newMinFace] < oldMinDistance)
					{
						minFace = newMinFace + distances.Count;
					}

					faces.AddRange(newFaces);
					normals.AddRange(newNormals);
					distances.AddRange(newDistances);
				}
			}

			Normal = minNormal;
			Depth = Fix64.Abs(minDistance);
			Contact = GJKGetContactPoint(colliderA, colliderB);
		}

		public FixVector3 AABBContactPoint(AABBCollider3D A, AABBCollider3D B)
        {
            Fix64 minPointX = Fix64.Min(A.Center.x + A.Extents.x, B.Center.x + B.Extents.x);
            Fix64 maxPointX = Fix64.Max(A.Center.x - A.Extents.x, B.Center.x - B.Extents.x);
            Fix64 minPointY = Fix64.Min(A.Center.y + A.Extents.y, B.Center.y + B.Extents.y);
            Fix64 maxPointY = Fix64.Max(A.Center.y - A.Extents.y, B.Center.y - B.Extents.y);
            Fix64 minPointZ = Fix64.Min(A.Center.z + A.Extents.z, B.Center.z + B.Extents.z);
            Fix64 maxPointZ = Fix64.Max(A.Center.z - A.Extents.z, B.Center.z - B.Extents.z);
            Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
            Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
            Fix64 mediantZ = (minPointZ + maxPointZ) / Fix64.Two;
            return new FixVector3(mediantX, mediantY, mediantZ);
        }

		public FixVector3 SphereContactPoint(SphereCollider3D A, SphereCollider3D B)
		{
			FixVector3 Direction = FixVector3.Normalize(B.Center - A.Center);
			FixVector3 ContactA = A.Center + (A.Radius * Direction);
			FixVector3 ContactB = B.Center - (B.Radius * Direction);

			return (ContactA + ContactB) / Fix64.Two;
		}

		public FixVector3 CapsuleContactPoint(FixVector3 centerA, Fix64 radiusA, FixVector3 centerB, Fix64 radiusB, FixVector3 direction)
		{
			FixVector3 ContactA = centerA + (direction * radiusA);
            FixVector3 ContactB = centerB - (direction * radiusB);
			return (ContactA + ContactB) / Fix64.Two;
		}

		public FixVector3 GJKGetContactPoint(SharpCollider3D colliderA, SharpCollider3D colliderB)
		{
			FixVector3 contacts = FixVector3.Zero;

			if (colliderA.Shape == CollisionType3D.Polygon && colliderB.Shape == CollisionType3D.Polygon)//Polygon/Polygon
				contacts = PolygonsContactPoint(colliderA as PolygonCollider3D, colliderB as PolygonCollider3D);
			else if (colliderA.Shape == CollisionType3D.Sphere && colliderB.Shape == CollisionType3D.Polygon) //Circle/Polygon
				contacts = PolygonSphereContactPoint(colliderB as PolygonCollider3D, colliderA as SphereCollider3D);
			else if (colliderA.Shape == CollisionType3D.Polygon && colliderB.Shape == CollisionType3D.Sphere) //Polygon/Circle
				contacts = PolygonSphereContactPoint(colliderA as PolygonCollider3D, colliderB as SphereCollider3D);
			else if (colliderA.Shape == CollisionType3D.Capsule && colliderB.Shape == CollisionType3D.Polygon) //Capsule/Polygon
				contacts = PolygonCapsuleContactPoint(colliderB as PolygonCollider3D, colliderA as CapsuleCollider3D);
			else if (colliderA.Shape == CollisionType3D.Polygon && colliderB.Shape == CollisionType3D.Capsule) //Polygon/Capsule
				contacts = PolygonCapsuleContactPoint(colliderA as PolygonCollider3D, colliderB as CapsuleCollider3D);

			return contacts;
		}

		public FixVector3 PolygonsContactPoint(PolygonCollider3D colliderA, PolygonCollider3D colliderB) 
		{
			FixVector3 Contact0 = FixVector3.Zero;
			FixVector3 Contact1 = FixVector3.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector3 point = colliderA.Points[i];
				for (int j = 0; j < colliderB.Points.Length; j++)
				{
					FixVector3 va = colliderB.Points[j];
					FixVector3 vb = colliderB.Points[(j + 1) % colliderB.Points.Length];

					LineToPointDistance(va, vb, point, out FixVector3 r1);

					Fix64 dist = FixVector3.Length(r1 - point);

					if (dist == minDist)
					{
						if (r1 != Contact0)
						{
							minDist = dist;
							Contact1 = r1;
						}
					}
					else if (dist < minDist)
					{
						minDist = dist;
						Contact0 = r1;
					}
				}
			}

			for (int i = 0; i < colliderB.Points.Length; i++)
			{
				FixVector3 point = colliderB.Points[i];
				for (int j = 0; j < colliderA.Points.Length; j++)
				{
					FixVector3 va = colliderA.Points[j];
					FixVector3 vb = colliderA.Points[(j + 1) % colliderA.Points.Length];

					LineToPointDistance(va, vb, point, out FixVector3 r1);

					Fix64 dist = FixVector3.Length(r1 - point);

					if (dist == minDist)
					{
						if (r1 != Contact0)
						{
							minDist = dist;
							Contact1 = r1;
						}
					}
					else if (dist < minDist)
					{
						minDist = dist;
						Contact0 = r1;
					}
				}
			}

			if (Contact1 != FixVector3.Zero)
				return (Contact0 + Contact1) / Fix64.Two;
			else
				return Contact0;
		}

		public FixVector3 PolygonSphereContactPoint(PolygonCollider3D colliderA, SphereCollider3D colliderB)
		{
			FixVector3 Contact = FixVector3.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector3 va = colliderA.Points[i];
				FixVector3 vb = colliderA.Points[(i + 1) % colliderA.Points.Length];

				LineToPointDistance(va, vb, colliderB.Center, out FixVector3 r1);

				Fix64 dist = FixVector3.Length(colliderB.Center - r1);

				if (dist < minDist)
				{
					minDist = dist;
					Contact = r1;
				}
			}

			return Contact;
		}

		public FixVector3 PolygonCapsuleContactPoint(PolygonCollider3D colliderA, CapsuleCollider3D colliderB)
		{
			FixVector3 Contact0 = FixVector3.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector3 va = colliderA.Points[i];
				FixVector3 vb = colliderA.Points[(i + 1) % colliderA.Points.Length];

				LineToLineDistance(va, vb, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector3 r1, out FixVector3 r2);

				Fix64 dist = FixVector3.Length(r2 - r1);
				FixVector3 dir = FixVector3.Normalize(r2 - r1);

				if (dist < minDist)
				{
					minDist = dist;
					Contact0 = r1;
				}
			}

			return Contact0;
		}

		public virtual FixVector3 Support(FixVector3 direction)
		{
			return direction;
		}
		
		public void UpdateBoundingBox()
		{
			BoundingBox = GetBoundingBoxPoints();
			BoundingBoxRequireUpdate = false;
		}

		protected virtual FixVolume GetBoundingBoxPoints()
		{
			return new FixVolume();
		}

		public virtual void UpdatePoints(SharpBody3D body)
		{
			Center = FixVector3.Transform(Offset, body);
			CollisionRequireUpdate = false;
		}

		public void LineToLineDistance(FixVector3 p1, FixVector3 p2, FixVector3 p3, FixVector3 p4, out FixVector3 r1, out FixVector3 r2)
		{
			FixVector3 r = p3 - p1;
			FixVector3 u = p2 - p1;
			FixVector3 v = p4 - p3;
			Fix64 ru = FixVector3.Dot(r, u);
			Fix64 rv = FixVector3.Dot(r, v);
			Fix64 uu = FixVector3.Dot(u, u);
			Fix64 uv = FixVector3.Dot(u, v);
			Fix64 vv = FixVector3.Dot(v, v);
			Fix64 det = uu * vv - uv * uv;

			Fix64 s, t;
			if (det < Fix64.ETA * uu * vv)
			{
				s = Fix64.Clamp01(ru / uu);
				t = Fix64.Zero;
			} 
			else
			{
				s = Fix64.Clamp01((ru * vv - rv * uv) / det);
				t = Fix64.Clamp01((ru * uv - rv * uu) / det);
			}

			Fix64 S = Fix64.Clamp01((t * uv + ru) / uu);
			Fix64 T = Fix64.Clamp01((s * uv - rv) / vv);

			r1 = p1 + S * u;
			r2 = p3 + T * v;
		}

		//Line to point collision code taken from Noah Zuo's Blog
		//https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
		public void LineToPointDistance(FixVector3 p1, FixVector3 p2, FixVector3 p3, out FixVector3 r1)
		{
			FixVector3 ab = p2 - p1;
			Fix64 length = FixVector3.Dot(p3 - p1, ab);
			if (length <= Fix64.ETA) 
			{
				r1 = p1;
			} 
			else 
			{
				Fix64 denom = FixVector3.Dot(ab, ab);
				if (length >= denom) 
				{
					r1 = p2;
				} 
				else 
				{
					length = length / denom;
					r1 = p1 + length * ab;
				}
			}
		}
	}
}

public enum CollisionType3D
{
	Null = -1,
	AABB = 0,
	Sphere = 1,
	Capsule = 2,
	Cylinder = 3,
	Polygon = 4,
}
