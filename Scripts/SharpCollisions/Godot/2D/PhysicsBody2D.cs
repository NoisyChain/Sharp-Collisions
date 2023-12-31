using FixMath.NET;
using Godot;
using System;

/// <summary>
/// Class to expose SharpBodies in the editor
/// </summary>

/*namespace SharpCollisions
{
    public class PhysicsBody2D : Spatial
    {
        public SharpBody2D body;

        [Export] private string DebugShape = ""; 
        [Export] private CollisionType collisionType;
        [Export] private Vector2 position;
        [Export] private float angle;
        [Export] private Vector2 size;
        [Export] private Vector2 offset;
        [Export] private float height;
        [Export] private float radius;
        [Export] private Vector2[] vertices;
        [Export] private bool isStatic;
        [Export] private bool isPushable = true;
        [Export] private bool isTrigger;
        [Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
        private int CollisionLayers = 1;
        [Export] private bool debugDraw;

        private FixVector2[] newPoints = new FixVector2[1] { FixVector2.Zero};

        protected Spatial debug;
        protected MeshInstance debug2;

        public bool isColliding => body.Collisions.Count > 0;

        public override void _Ready()
        {
            Fix64 convertedAngle = (Fix64)angle * Fix64.DegToRad;
            if (vertices != null && vertices.Length > 0)
            {
                newPoints = new FixVector2[vertices.Length];
                for (int p = 0; p < vertices.Length; p++)
                {
                    newPoints[p] = (FixVector2)vertices[p];
                }
            }
            
            body = new SharpBody2D
            (
                (FixVector2)position, (FixVector2)offset, (FixVector2)size, 
                newPoints, convertedAngle, collisionType, CollisionLayers,
                isStatic, isPushable, isTrigger
            );
            debug = GetNode<Spatial>("Debug");
            debug2 = GetNode<MeshInstance>(DebugShape);
            GetTree().Root.GetNode<PhysicsManager2D>("Main/PhysicsManager").AddBody(this);
            body.BeginOverlap = OnBeginOverlap;
            body.DuringOverlap = OnOverlap;
            body.EndOverlap = OnEndOverlap;

            GD.Print(body.CollisionLayers);
        }

        public override void _Process(float delta)
        {
            Translation = (Vector3)body.position;
            Rotation = new Vector3(Rotation.x, Rotation.y, (float)body.rotation);
            if (debug != null)
            {
                debug.Visible = debugDraw;
                debug.Translation = (Vector3)body.collider.Offset;
                if (debug.Visible)
                    DebugSelect();
            }
        }

        public virtual void _SharpProcess(Fix64 delta)
        {
            body.SetVelocity(FixVector2.Zero);
        }

        public void _Destroy()
        {
            GetTree().Root.GetNode<PhysicsManager2D>("Main/PhysicsManager").RemoveBody(this);
            QueueFree();
        }

        private void DebugSelect()
        {
            switch (collisionType)
            {
                case CollisionType.AABB:
                case CollisionType.Box:
                    debug.Scale = new Vector3(size.x, size.y, 0.1f);
                    break;
                case CollisionType.Circle:
                    float debugRadius = Mathf.Min(size.x, size.y);
                    debug.Scale = new Vector3(debugRadius, debugRadius, debugRadius);
                    break;
                 case CollisionType.Capsule:
                    debug.Scale = new Vector3(size.x, size.y / 2, size.x);
                    break;
            }

            Color debugColor = isColliding ? new Color(1, 0, 0, 0.7f) : new Color(0, 0, 1, 0.7f);
            debug2.GetActiveMaterial(0).Set("albedo_color", debugColor);
        }

        public virtual void OnBeginOverlap(SharpBody2D other)
        {
            //GD.Print(other.GetHashCode());
        }

        public virtual void OnOverlap(SharpBody2D other)
        {
            //GD.Print(other.GetHashCode());
        }

        public virtual void OnEndOverlap(SharpBody2D other)
        {
            //GD.Print(other.GetHashCode());
        }
    }
}*/
