using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool]
    [GlobalClass]
    public partial class BoxCollider2D : PolygonCollider2D
    {
        public FixVector2 Extents;

        [Export] private Vector2I extents = Vector2I.One;

        public override void Initialize()
        {
            base.Initialize();
            Extents = new FixVector2(
                (Fix64)extents.X / SharpNode.NodeScale,
                (Fix64)extents.Y / SharpNode.NodeScale
            );
            CreateBoxPoints();
        }

        private void CreateBoxPoints()
        {
            RawPoints = new FixVector2[]
            {
                new FixVector2(-Extents.x, Extents.y),
                new FixVector2(-Extents.x, -Extents.y),
                new FixVector2(Extents.x, -Extents.y),
                new FixVector2(Extents.x, Extents.y)
            };

            Points = new FixVector2[RawPoints.Length];
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;

            Color finalColor = selected ? selectedColor : debugColor;

            Vector3 scaledPosOffset = new Vector3(positionOffset.X, positionOffset.Y, 0) / SharpNode.nodeScale;
            Vector3 scaledRotOffset = new Vector3(0, 0, rotationOffset) / SharpNode.nodeRotation;
            Vector3 scaledExtents = new Vector3(extents.X * 2, extents.Y * 2, 0.1f) / SharpNode.nodeScale;

            Vector3 rotPos = SharpHelpers.RotateDeg3D(scaledPosOffset, scaledRotOffset);
            Vector3 newPos = SharpHelpers.Transform3D(rotPos, reference.GlobalPosition, reference.GlobalRotation);

            DebugDraw3D.DrawBox(newPos, Quaternion.FromEuler(reference.GlobalRotation + SharpHelpers.VectorDegToRad(scaledRotOffset)), scaledExtents, finalColor, true);

            if (selected) DebugDraw3D.DrawGizmo(reference.Transform, finalColor, true);
        }
    }
}
