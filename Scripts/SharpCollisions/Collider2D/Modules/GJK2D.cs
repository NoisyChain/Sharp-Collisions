using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp2D.GJK
{
    public partial class GJK2D
    {
        private bool AllowDraw = false;
		public const int MAX_GJK_ITERATIONS = 32;
		public const int MAX_EPA_ITERATIONS = 32;

        public GJK2D() {}
        public GJK2D(bool draw) { AllowDraw = draw; }

		private FixVector2 SupportFunction(SharpCollider2D colliderA, SharpCollider2D colliderB, FixVector2 direction)
		{
            FixVector2 SupportPointA = colliderA.Support(direction);
            FixVector2 SupportPointB = colliderB.Support(-direction);

			return SupportPointA - SupportPointB;
		}

        public bool PolygonCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			int maxIterations = 0;
			
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
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_GJK_ITERATIONS) return false;

				SupportPoint = SupportFunction(colliderA, colliderB, supportDirection);
				if (!FixVector2.IsSameDirection(SupportPoint, supportDirection))
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

			if(FixVector2.IsSameDirection(abPerp, ao))
			{
                // the origin is outside line ab
                // get rid of c and add a new support in the direction of abPerp
				Simplex.Reset(new List<FixVector2>(){a, b});
                supportDirection = abPerp;
            }
            else if(FixVector2.IsSameDirection(acPerp, ao))
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

		private bool GetPolytopeDirection(List<FixVector2> polytope)
		{
			Fix64 e0 = (polytope[1].x - polytope[0].x) * (polytope[1].y + polytope[0].y);
			Fix64 e1 = (polytope[2].x - polytope[1].x) * (polytope[2].y + polytope[1].y);
			Fix64 e2 = (polytope[0].x - polytope[2].x) * (polytope[0].y + polytope[2].y);

			return e0 + e1 + e2 > Fix64.Zero;
		}

		private void EPA(Simplex2D simplex, SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out Fix64 Depth, out FixVector2 Contact)
		{
			int maxIterations = 0;

			int minIndex = 0;
			Fix64 minDistance = Fix64.MaxValue;
			FixVector2 minNormal = FixVector2.Zero;
			List<FixVector2> polytope = simplex.Points;
			bool IsClockWise = GetPolytopeDirection(polytope);

			while (minDistance == Fix64.MaxValue)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_EPA_ITERATIONS) break;

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
				FixVector2 support = SupportFunction(colliderA, colliderB, minNormal);
				Fix64 sDistance = FixVector2.Dot(minNormal, support);

				if (Fix64.Abs(sDistance - minDistance) > Fix64.ETA)
				{
					minDistance = Fix64.MaxValue;
					polytope.Insert(minIndex, support);
				}
			}
			DrawPolytope(polytope);

			Normal = minNormal;
			Depth = Fix64.Abs(minDistance) + Fix64.ETA;
			Contact = GetContactPoint(colliderA, colliderB);
		}

		private void DrawPolytope(List<FixVector2> polytope)
		{
			if (!AllowDraw) return;

			DebugDraw3D.DrawSphere(Vector3.Zero, 0.03f, new Color(0f, 0f, 0f));
			
			for (int i = 0; i < polytope.Count; i++)
			{
				Vector3 a = (Vector3)polytope[i];
				Vector3 b = (Vector3)polytope[(i + 1) % polytope.Count];
				DebugDraw3D.DrawLine(a, b);
			}
		}

        public FixVector2 GetContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 contacts = FixVector2.Zero;

			return contacts;
		}
    }
}
