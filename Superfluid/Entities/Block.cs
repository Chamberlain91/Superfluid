using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{
    public class Block : Entity
    {
        public Block(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public Rectangle Bounds { get; }

        public override void Update(float dt)
        {
            // nothing to see here...
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // nothing to see here...
        }

        public override void DebugDraw(Graphics gfx)
        {
            gfx.Color = Color.Orange;
            gfx.DrawRectOutline(Bounds);
        }
    }
}
