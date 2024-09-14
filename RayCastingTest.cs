using Godot;

[Tool]
public partial class RayCastingTest : Node3D
{
	Vector3 a = new Vector3(-5f, -3, -5f);
	Vector3 b = new Vector3(0, -3, 5f);
	Vector3 c = new Vector3(5f, -3, -5f);
	Vector3 RayOrigin = new Vector3(2f, 10f, 0f);
	Vector3 RayEnd = new Vector3(0f, -5f, 2f);

	Vector3 Intersection;
	Vector3 LinePoint;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector3 AB = b - a;
		Vector3 AC = c - a;
		Vector3 Normal = AB.Cross(AC);
		Vector3 RayNormal = (RayEnd - RayOrigin).Normalized();
		Intersection = LinePlaneIntersection(RayOrigin, RayNormal, Normal, a);

		LineToPointDistance(RayOrigin, RayEnd, Intersection, out LinePoint);
		if (LinePoint.DistanceTo(Intersection) < 0.001f)
			GD.Print(Intersection);
	}

	public override void _Process(double delta)
	{
		DebugDraw3D.DrawLine(a, b, new Color(1, 1, 1));
		DebugDraw3D.DrawLine(b, c, new Color(1, 1, 1));
		DebugDraw3D.DrawLine(c, a, new Color(1, 1, 1));
		DebugDraw3D.DrawLine(RayOrigin, RayEnd, new Color(0, 1, 0));
		if (LinePoint.DistanceTo(Intersection) < 0.001f)
			DebugDraw3D.DrawSphere(Intersection, 0.25f, new Color(1, 0, 0));
	}

	public static Vector3 LinePlaneIntersection(Vector3 rayOrigin, Vector3 ray, Vector3 normal, Vector3 coord)
	{
		// get d value
		float d = normal.Dot(coord);

		//Avoid divisions by zero
		if (normal.Dot(ray) == 0)
		{
			return Vector3.Zero; // No intersection, the line is parallel to the plane
		}

		// Compute the X value for the directed line ray intersecting the plane
		float x = (d - normal.Dot(rayOrigin)) / normal.Dot(ray);

		// output contact point
		return  rayOrigin + ray.Normalized() * x; //Make sure your ray vector is normalized
	}

	public void LineToPointDistance(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 r1)
	{
		Vector3 ab = p2 - p1;
		float length = (p3 - p1).Dot(ab);
		if (length <= 0.001f) 
		{
			r1 = p1;
		} 
		else 
		{
			float denom = ab.Dot(ab);
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
