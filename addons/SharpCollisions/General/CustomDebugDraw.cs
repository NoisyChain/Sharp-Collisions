using Godot;
using System;

namespace SharpCollisions
{
	public static class CustomDebugDraw
	{
		//--------------------------Custom Functions-----------------------------------
		public static void DrawSimpleSphere(Vector3 origin, Vector3 X, Vector3 Y, Vector3 Z, 
											float radius = 0.5f, Color? color = null, float duration = 0f)
		{
			DrawArc(origin, X, Y, radius, color, duration);
			DrawArc(origin, X, Z, radius, color, duration);
			DrawArc(origin, Y, Z, radius, color, duration);
			DrawArc(origin, X, -Y, radius, color, duration);
			DrawArc(origin, -X, Z, radius, color, duration);
			DrawArc(origin, Y, -Z, radius, color, duration);
			DrawArc(origin, -X, -Y, radius, color, duration);
			DrawArc(origin, -X, -Z, radius, color, duration);
			DrawArc(origin, -Y, -Z, radius, color, duration);
			DrawArc(origin, -X, Y, radius, color, duration);
			DrawArc(origin, X, -Z, radius, color, duration);
			DrawArc(origin, -Y, Z, radius, color, duration);
		}

		public static void DrawHalfSphereX(Vector3 origin, Vector3 X, Vector3 Y, Vector3 Z, bool inverse = false,  
											float radius = 0.5f, Color? color = null, float duration = 0f)
		{
			DrawArc(origin, Y, Z, radius, color, duration);
			DrawArc(origin, Y, -Z, radius, color, duration);
			DrawArc(origin, -Y, Z, radius, color, duration);
			DrawArc(origin, -Y, -Z, radius, color, duration);

			if (inverse)
			{
					DrawArc(origin, -X, Y, radius, color, duration);
					DrawArc(origin, -X, Z, radius, color, duration);
					DrawArc(origin, -X, -Y, radius, color, duration);
					DrawArc(origin, -X, -Z, radius, color, duration);
			}
			else
			{
					DrawArc(origin, X, Y, radius, color, duration);
					DrawArc(origin, X, Z, radius, color, duration);
					DrawArc(origin, X, -Y, radius, color, duration);
					DrawArc(origin, X, -Z, radius, color, duration);
			}
		}

		public static void DrawHalfSphereY(Vector3 origin, Vector3 X, Vector3 Y, Vector3 Z, bool inverse = false,  
											float radius = 0.5f, Color? color = null, float duration = 0f)
		{
			DrawArc(origin, X, Z, radius, color, duration);
			DrawArc(origin, -X, Z, radius, color, duration);
			DrawArc(origin, X, -Z, radius, color, duration);
			DrawArc(origin, -X, -Z, radius, color, duration);

			if (inverse)
			{
					DrawArc(origin, X, -Y, radius, color, duration);
					DrawArc(origin, -Y, Z, radius, color, duration);
					DrawArc(origin, -X, -Y, radius, color, duration);
					DrawArc(origin, -Y, -Z, radius, color, duration);
			}
			else
			{
					DrawArc(origin, X, Y, radius, color, duration);
					DrawArc(origin, Y, Z, radius, color, duration);
					DrawArc(origin, -X, Y, radius, color, duration);
					DrawArc(origin, Y, -Z, radius, color, duration);
			}
		}

		public static void DrawHalfSphereZ(Vector3 origin, Vector3 X, Vector3 Y, Vector3 Z, bool inverse = false,  
											float radius = 0.5f, Color? color = null, float duration = 0f)
		{
			DrawArc(origin, X, Y, radius, color, duration);
			DrawArc(origin, X, -Y, radius, color, duration);
			DrawArc(origin, -X, Y, radius, color, duration);
			DrawArc(origin, -X, -Y, radius, color, duration);
			
			if (inverse)
			{
					DrawArc(origin, X, Z, radius, color, duration);
					DrawArc(origin, Y, Z, radius, color, duration);
					DrawArc(origin, -X, Z, radius, color, duration);
					DrawArc(origin, -Y, Z, radius, color, duration);
			}
			else
			{
					DrawArc(origin, Y, -Z, radius, color, duration);
					DrawArc(origin, -X, -Z, radius, color, duration);
					DrawArc(origin, -Y, -Z, radius, color, duration);
					DrawArc(origin, X, -Z, radius, color, duration);
			}
		}

		public static void DrawCircle(Vector3 origin, Vector3 X, Vector3 Z,  
									float radius = 0.5f, Color? color = null, float duration = 0f)
		{
			DrawArc(origin, X, Z, radius, color, duration);
			DrawArc(origin, -X, Z, radius, color, duration);
			DrawArc(origin, X, -Z, radius, color, duration);
			DrawArc(origin, -X, -Z, radius, color, duration);
		}

		public static void DrawArc(Vector3 origin, Vector3 dirA, Vector3 dirB, float radius = 0.5f, Color? color = null, float duration = 0f)
		{
			Vector3 pointA = origin + (dirA * radius);
			Vector3 pointB = origin + (dirB * radius);
			Vector3 pointAB = origin + ((dirA + dirB).Normalized() * radius);
			Vector3 pointAAB = origin + ((dirA + (dirA + dirB).Normalized()).Normalized() * radius);
			Vector3 pointABB = origin + (((dirA + dirB).Normalized() + dirB).Normalized() * radius);
			DebugDraw3D.DrawLine(pointA, pointAAB, color, duration);
			DebugDraw3D.DrawLine(pointAAB, pointAB, color, duration);
			DebugDraw3D.DrawLine(pointAB, pointABB, color, duration);
			DebugDraw3D.DrawLine(pointABB, pointB, color, duration);
		}
	//--------------------------------------------------------------------------
	}
}
