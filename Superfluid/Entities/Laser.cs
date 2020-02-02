using System.Linq;

using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;
using Superfluid.Actors;

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
            var pipes = Game.QuerySpatial<Pipe>(circle);
            var enemies = Game.FindEntities<Enemy>().Where(e => e.Bounds.Overlaps(circle));

            // Helper function
            void AddSparks() {
                // Schedule add sparks
                for (var i = 0; i < 3; i++)
                {
                    var spark = Game.AddEntity(new Spark(Color.Red));
                    spark.Transform.Position = Transform.Position;
                }

                // Schedule remove this blob
                Game.RemoveEntity(this);
            }

            // Collides with block
            if (blocks.Any())
            {
                AddSparks();
            }

            // Collides with pipe -> Repair
            if (pipes.Any())
            {
                // TODO: 
                AddSparks();

                foreach (Pipe p in pipes)
                {
                    p.HealDamage();
                }
            }

            // Colides with enemy -> damage
            if(enemies.Any())
            {
                // TODO: Damage enemies
                AddSparks();
                
                foreach (Enemy e in enemies)
                {
                    e.TakeDamage(34);
                }
                
            }


        }

        public override void Draw(Graphics gfx, float dt)
        {
            gfx.Color = Color.Red;
            gfx.DrawImage(Image, Transform);
        }
    }
}
