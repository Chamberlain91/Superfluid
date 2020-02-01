using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{
    public class Block : Entity, ISpatialObject
    {
        public Block(Rectangle bounds, bool isOneWay)
        {
            IsOneWay = isOneWay;
            Bounds = bounds;
        }

        public Rectangle Bounds { get; }

        public bool IsOneWay { get; }

        public override void Update(float dt)
        {
            // nothing
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // nothing
        }
    }
}
