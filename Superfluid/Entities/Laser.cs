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
            Image = Assets.GetImage("particlewhite_4");
            Transform.Scale = (0.2F, 0.2F);
        }

        public override void Update(float dt)
        {
            Transform.Position += Transform.Direction * 20;

            // 
            var circle = new Circle(Transform.Position, 10);
            var blocks = Game.QuerySpatial<Block>(circle);
            if (blocks.Any())
            {
                // Schedule add sparks
                for (var i = 0; i < 3; i++)
                {
                    var spark = Game.AddEntity(new Spark(Color.Red));
                    spark.Transform.Position = Transform.Position;
                }

                // Schedule remove this laser
                Game.RemoveEntity(this);
            }
        }

        public override void Draw(Graphics gfx, float dt)
        {
            gfx.Color = Color.Red;
            gfx.DrawImage(Image, Transform);
        }
    }
}
