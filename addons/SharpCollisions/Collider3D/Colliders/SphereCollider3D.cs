using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class SphereCollider3D : SharpCollider3D
    {
        [Export] private float _radius
        {
            get =>(float)Fix64.FromRaw(raw_Radius);
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Radius = ((Fix64)((decimal)value)).RawValue;
                    Radius = Fix64.FromRaw(raw_Radius);
                }
            }
        }

		[ExportSubgroup("Raw Values")]
        [Export] private long raw_radius
        {
            get => raw_Radius;
            set
            {
                raw_Radius = value;
                Radius = Fix64.FromRaw(raw_Radius);
            }
        }

        private long raw_Radius;

        public Fix64 Radius = new Fix64();
        
        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType3D.Sphere;
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Vector3 DirX = (Vector3)reference.Right;
            Vector3 DirY = (Vector3)reference.Up;
            Vector3 DirZ = (Vector3)reference.Forward;

            CustomDebugDraw.DrawSimpleSphere((Vector3)Center, DirX, DirY, DirZ, (float)Radius + 0.005f, DebugShapeColor);
        }

        public override void DebugDrawShapesEditor(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebugShape) return;

            Color finalColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector3 DirX = (Vector3)reference.Right;
            Vector3 DirY = (Vector3)reference.Up;
            Vector3 DirZ = (Vector3)reference.Forward;
            Vector3 pos = _positionOffset;
            Vector3 newPos = SharpHelpers.Transform3D(pos, (Vector3)reference.FixedPosition, (Vector3)reference.FixedRotation);

            CustomDebugDraw.DrawSimpleSphere(newPos, DirX, DirY, DirZ, (float)Radius + 0.005f, finalColor);
        }

        public override void UpdateBoundingBox()
        {
            BoundingBox = CollisionMath3D.UpdateSphereBoundingBox(Center, Radius);
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            base.UpdatePoints(position, rotation);
        }
    }
}
