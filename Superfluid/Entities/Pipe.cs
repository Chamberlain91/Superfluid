using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{

    public class Pipe : Entity, ISpatialObject
    {

        public Pipe(Rectangle bounds,  IntVector offset1, 
                    IntVector offset2, bool isGoldPipe)
        {
            Bounds = bounds;
            Offset1 = offset1;
            Offset2 = offset2;
            IsGoldPipe = IsGoldPipe;
        }

        public Rectangle Bounds { get; }

        public IntVector Offset1 { get; }

        public IntVector Offset2 { get; }

        public bool IsGoldPipe { get; }

        public override void Update(float dt)
        {
            // nothing (for now)
        }

        public override void Draw(Graphics gfx, float dt)
        {
            gfx.DrawRectOutline(Bounds);
            gfx.DrawCross(Bounds.Position + (Offset1 * 70) + (35, 35), 35, 8);
            gfx.Color.Equals(Color.Magenta);
            gfx.DrawCross(Bounds.Position + (Offset2 * 70) + (35, 35), 35, 8);
        }
    }
}