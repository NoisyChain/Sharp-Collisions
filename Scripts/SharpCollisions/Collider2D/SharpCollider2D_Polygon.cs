using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions
{
	public partial class SharpCollider2D : Node
	{
		public FixVector2[] RawPoints;
		public FixVector2[] Points;

		public bool GJKPolygonCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			Simplex2D Simplex = new Simplex2D(
				new List<FixVector2>() { FixVector2.Zero, FixVector2.Zero, FixVector2.Zero }
			);

			FixVector2 supportDirection = colliderB.Center - colliderA.Center;
			FixVector2 SupportPoint;

			while (true)
			{
				SupportPoint = Support(colliderA, colliderB, supportDirection);
				if (!IsSameDirection(SupportPoint, supportDirection)) 
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

		private bool CheckSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			switch (Simplex.Size)
			{
				case 1: //Point
					return PointSimplex(ref supportDirection);
				case 2: //Line
					return LineSimplex(ref Simplex, ref supportDirection);
				case 3: //Triangle
					return TriangleSimplex(ref Simplex, ref supportDirection);
				case 4: //Tetrahedron (not used in 2D)
					break;
			}

			//Should never be here
			return false;
		}

		private bool PointSimplex(ref FixVector2 supportDirection)
		{
			supportDirection *= Fix64.NegativeOne;
			return false;
		}

		private bool LineSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			FixVector2 a = Simplex.Points[0];
			FixVector2 b = Simplex.Points[1];

			FixVector2 ab = b - a;
			FixVector2 ao = a * Fix64.NegativeOne;

			supportDirection = FixVector2.TripleProduct(ab, ao, ab);
			return false;
		}

		private bool TriangleSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			FixVector2 a = Simplex.Points[0];
			FixVector2 b = Simplex.Points[1];
			FixVector2 c = Simplex.Points[2];

			FixVector2 ab = b - a;
			FixVector2 ac = c - a;
			FixVector2 ao = a * Fix64.NegativeOne;

			FixVector2 abPerp = FixVector2.TripleProduct(ac, ab, ab);
			FixVector2 acPerp = FixVector2.TripleProduct(ab, ac, ac);

			if(IsSameDirection(abPerp, ao))
			{
                // the origin is outside line ab
                // get rid of c and add a new support in the direction of abPerp
				Simplex.Reset(new List<FixVector2>(){a, b});
                supportDirection = abPerp;
            }
            else if(IsSameDirection(acPerp, ao))
			{
                // the origin is outside line ac
                // get rid of b and add a new support in the direction of acPerp
				Simplex.Reset(new List<FixVector2>(){a, c});
                supportDirection = acPerp;
            }
            else
			{
                // the origin is inside both ab and ac,
                // so it must be inside the triangle!
                return true;
            }

			return false;
		}

		//Saving this code for the 3D version
		/*
		bool TetrahedronSimplex(ref Simplex3D Simplex, ref FixVector2 supportDirection, out FixVector2 Normal, out FixVector2 Depth)
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

			if (IsSameDirection(abc, ao))
			{
				Simplex.Reset(new List<FixVector2>(){a, b, c});
				return TriangleSimplex(ref Simplex, ref supportDirection, out Normal, out Depth);
			}
			if (IsSameDirection(acd, ao))
			{
				Simplex.Reset(new List<FixVector2>(){a, c, d});
				return TriangleSimplex(ref Simplex, ref supportDirection, out Normal, out Depth);
			}
			if (IsSameDirection(adb, ao))
			{
				Simplex.Reset(new List<FixVector2>(){a, d, b});
				return TriangleSimplex(ref Simplex, ref supportDirection, out Normal, out Depth);
			}
			return false;
		}*/

		private bool GetPolytopeDirection(List<FixVector2> polytope)
		{
			Fix64 e0 = (polytope[1].x - polytope[0].x) * (polytope[1].y + polytope[0].y);
			Fix64 e1 = (polytope[2].x - polytope[1].x) * (polytope[2].y + polytope[1].y);
			Fix64 e2 = (polytope[0].x - polytope[2].x) * (polytope[0].y + polytope[2].y);

			return e0 + e1 + e2 > Fix64.Zero;
		}

		private void EPA(Simplex2D simplex, SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out Fix64 Depth, out FixVector2 Contact)
		{
			int minIndex = 0;
			Fix64 minDistance = Fix64.MaxValue;
			FixVector2 minNormal = FixVector2.Zero;
			List<FixVector2> polytope = simplex.Points;
			bool IsClockWise = GetPolytopeDirection(polytope);

			while (minDistance == Fix64.MaxValue)
			{
				for (int i = 0; i < polytope.Count; i++)
				{
					int j = (i + 1) % polytope.Count;

					FixVector2 vertexI = polytope[i];
					FixVector2 vertexJ = polytope[j];


					FixVector2 normal = IsClockWise ?
						FixVector2.GetNormal(vertexI, vertexJ) :
						FixVector2.GetInvertedNormal(vertexI, vertexJ);

					Fix64 distance = FixVector2.Dot(normal, vertexI);

					if (distance < minDistance)
					{
						minDistance = distance;
						minNormal = normal;
						minIndex = j;
					}
				}
				FixVector2 support = Support(colliderA, colliderB, minNormal);
				Fix64 sDistance = FixVector2.Dot(minNormal, support);

				if (Fix64.Abs(sDistance - minDistance) > Fix64.ETA)
				{
					minDistance = Fix64.MaxValue;
					polytope.Insert(minIndex, support);
				}
			}

			Normal = minNormal;
			Depth = Fix64.Abs(minDistance);
			Contact = GJKGetContactPoint(colliderA, colliderB);
		}

		public FixVector2 PolygonsContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB) 
		{
			FixVector2 Contact0 = FixVector2.Zero;
			FixVector2 Contact1 = FixVector2.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector2 point = colliderA.Points[i];
				for (int j = 0; j < colliderB.Points.Length; j++)
				{
					FixVector2 va = colliderB.Points[j];
					FixVector2 vb = colliderB.Points[(j + 1) % colliderB.Points.Length];

					LineToPointDistance(va, vb, point, out FixVector2 r1);

					Fix64 dist = FixVector2.Length(r1 - point);

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
				FixVector2 point = colliderB.Points[i];
				for (int j = 0; j < colliderA.Points.Length; j++)
				{
					FixVector2 va = colliderA.Points[j];
					FixVector2 vb = colliderA.Points[(j + 1) % colliderA.Points.Length];

					LineToPointDistance(va, vb, point, out FixVector2 r1);

					Fix64 dist = FixVector2.Length(r1 - point);

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

			if (Contact1 != FixVector2.Zero)
				return (Contact0 + Contact1) / Fix64.Two;
			else
				return Contact0 ;
		}

		public FixVector2 PolygonCircleContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 Contact = FixVector2.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector2 va = colliderA.Points[i];
				FixVector2 vb = colliderA.Points[(i + 1) % colliderA.Points.Length];

				LineToPointDistance(va, vb, colliderB.Center, out FixVector2 r1);

				Fix64 dist = FixVector2.Length(colliderB.Center - r1);

				if (dist < minDist)
				{
					minDist = dist;
					Contact = r1;
				}
			}

			return Contact;
		}

		public FixVector2 PolygonCapsuleContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 Contact0 = FixVector2.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector2 va = colliderA.Points[i];
				FixVector2 vb = colliderA.Points[(i + 1) % colliderA.Points.Length];

				LineToLineDistance(va, vb, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector2 r1, out FixVector2 r2);

				Fix64 dist = FixVector2.Length(r2 - r1);

				if (dist < minDist)
				{
					minDist = dist;
					Contact0 = r1;
				}
			}

			return Contact0;
		}

		private FixVector2 Support(SharpCollider2D colliderA, SharpCollider2D colliderB, FixVector2 direction)
		{
			FixVector2 SupportPointA = FixVector2.Zero;
			FixVector2 SupportPointB = FixVector2.Zero;

			switch(colliderA.Shape)
			{
				case CollisionType.Box:
				case CollisionType.Polygon:
					SupportPointA = colliderA.PolygonSupport(direction);
					break;
				case CollisionType.Circle:
					SupportPointA = colliderA.CircleSupport(direction);
					break;
				case CollisionType.Capsule:
					SupportPointA = colliderA.CapsuleSupport(direction);
					break;
			}

			switch(colliderB.Shape)
			{
				case CollisionType.Box:
				case CollisionType.Polygon:
					SupportPointB = colliderB.PolygonSupport(-direction);
					break;
				case CollisionType.Circle:
					SupportPointB = colliderB.CircleSupport(-direction);
					break;
				case CollisionType.Capsule:
					SupportPointB = colliderB.CapsuleSupport(-direction);
					break;
			}

			return SupportPointA - SupportPointB;
		}

		private FixVector2 GJKGetContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			bool AIsPolygon = colliderA.Shape == CollisionType.Polygon || colliderA.Shape == CollisionType.Box;
			bool BIsPolygon = colliderB.Shape == CollisionType.Polygon || colliderB.Shape == CollisionType.Box;
			FixVector2 contacts = FixVector2.Zero;

			if (AIsPolygon && BIsPolygon)//Polygon/Polygon
				contacts = PolygonsContactPoint(colliderA, colliderB);
			else if (colliderA.Shape == CollisionType.Circle && BIsPolygon) //Circle/Polygon
				contacts = PolygonCircleContactPoint(colliderB, colliderA);
			else if (AIsPolygon && colliderB.Shape == CollisionType.Circle) //Polygon/Circle
				contacts = PolygonCircleContactPoint(colliderA, colliderB);
			else if (colliderA.Shape == CollisionType.Capsule && BIsPolygon) //Capsule/Polygon
				contacts = PolygonCapsuleContactPoint(colliderB, colliderA);
			else if (AIsPolygon && colliderB.Shape == CollisionType.Capsule) //Polygon/Capsule
				contacts = PolygonCapsuleContactPoint(colliderA, colliderB);

			return contacts;
		}

		public void CreatePoints(FixVector2[] NewPoints)
		{
			Fix64 two = new Fix64(2);

			if (Shape == CollisionType.Polygon)
			{
				RawPoints = NewPoints;
				for (int p = 0; p < RawPoints.Length; p++)
				{
					RawPoints[p] += Offset;
				}
			}
			else 
			{
				RawPoints = new FixVector2[]
				{
					new FixVector2(Offset.x - (Size.x / two), Offset.y + (Size.y / two)),
					new FixVector2(Offset.x - (Size.x / two), Offset.y - (Size.y / two)),
					new FixVector2(Offset.x + (Size.x / two), Offset.y -(Size.y / two)),
					new FixVector2(Offset.x + (Size.x / two), Offset.y + (Size.y / two))
				};
			}
			
			Points = new FixVector2[RawPoints.Length];

			bool isYAxis = AxisDirection != 0;

			FixVector2 CapsuleDirection = isYAxis ? 
				new FixVector2(Fix64.Zero, Height - Radius) : 
				new FixVector2(Height - Radius, Fix64.Zero);

			RawUpperPoint = Offset + CapsuleDirection;
			RawLowerPoint = Offset - CapsuleDirection;
		}

		public void UpdatePoints(SharpBody2D body)
		{
			for (int i = 0; i < RawPoints.Length; i++)
			{
				Points[i] = FixVector2.Transform(RawPoints[i], body);
			}
			Center = FixVector2.Transform(Offset, body);
			UpperPoint = FixVector2.Transform(RawUpperPoint, body);
			LowerPoint = FixVector2.Transform(RawLowerPoint, body);
			CollisionRequireUpdate = false;
		}

		public FixVector2 PolygonSupport(FixVector2 direction)
		{
			FixVector2 maxPoint = FixVector2.Zero;
			Fix64 maxDistance = Fix64.MinValue;

			for (int i = 0; i < Points.Length; i++)
			{
				Fix64 dist = FixVector2.Dot(Points[i], direction);
				if (dist > maxDistance)
				{
					maxDistance = dist;
					maxPoint = Points[i];
				}
			}
			return maxPoint;
		}

		bool IsSameDirection(FixVector2 Direction, FixVector2 AO)
		{
			return FixVector2.Dot(Direction, AO) > Fix64.Zero;
		}
	}
}