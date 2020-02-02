using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{
    public class Pipe : Entity, ISpatialObject
    {
        public Image Image;

        public Pipe(Image image, Rectangle localBounds,
                    IntVector offset1, IntVector offset2,
                    bool isGoldPipe)
        {
            Image = image;
            LocalBounds = localBounds;
            Offset1 = offset1;
            Offset2 = offset2;
            IsGoldPipe = isGoldPipe;

            // Draw behind ground layer
            Layer = EntityLayer.Back;
        }

        public Rectangle LocalBounds { get; }

        public Rectangle Bounds { get; private set; }

        public IntVector Offset1 { get; }

        public IntVector Offset2 { get; }

        public bool IsGoldPipe { get; }

        public override void Update(float dt)
        {
            // nada
        }

        public void ComputeWorldBounds()
        {
            // Computes world bounds
            var bounds = LocalBounds;
            bounds.Position += Transform.Position;
            Bounds = bounds;
        }

        public override void Draw(Graphics gfx, float dt)
        {
            gfx.DrawImage(Image, Transform);
        }

        public override void DebugDraw(Graphics gfx)
        {
            gfx.Color = Color.Black;
            gfx.DrawRectOutline(Bounds);

            gfx.Color = Color.Pink;
            var a = Transform.Position + (Offset1 * 70) + (35, 35);
            var b = Transform.Position + (Offset2 * 70) + (35, 35);
            gfx.DrawCross(a, 24, 4);
            gfx.DrawCross(b, 24, 4);

            gfx.Color = Color.Orange;
            gfx.DrawLine(a, b, 2);
        }
    }
}
