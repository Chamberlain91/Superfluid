using System.Linq;

using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{
    public class Laser : Entity
    {
        public Image Image { get; }

        public Laser()
        {
            Image = Assets.GetImage("particleblue_5");
        }

        public override void Update(float dt)
        {
            Transform.Position += Transform.Direction;

            // 
            var circle = new Circle(Transform.Position, 40);
            var blocks = Game.QuerySpatial<Block>(circle);
            if (blocks.Any())
            {
                Game.RemoveEntity(this);
            }
        }

        public override void Draw(Graphics gfx, float dt)
        {
            gfx.DrawImage(Image, Transform);
        }
    }
}
