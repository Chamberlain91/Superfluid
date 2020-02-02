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

        private bool IsBloodThirsty { get; }
        
        public Laser(bool isBloodThirsty)
        {
            Image = Assets.GetImage("particlewhite_4");
            Transform.Scale = (0.2F, 0.2F);
            IsBloodThirsty = isBloodThirsty;
        }

        public override void Update(float dt)
        {
            Transform.Position += Transform.Direction * 20;

            // 
            var circle = new Circle(Transform.Position, 10);
            var blocks = Game.QuerySpatial<Block>(circle);
            var pipes = Game.QuerySpatial<Pipe>(circle);
            var enemies = Game.FindEntities<Enemy>().Where(e => e.Bounds.Overlaps(circle));
            var color = Color.Red;
            

            // Helper function
            void AddSparks(Color color) {
                // Schedule add sparks
                for (var i = 0; i < 3; i++)
                {
                    var spark = Game.AddEntity(new Spark(color));
                    spark.Transform.Position = Transform.Position;
                }

                // Schedule remove this blob
                Game.RemoveEntity(this);
            }

            if (!IsBloodThirsty) {
                color = Color.Green;
                // Collides with pipe -> Repair
                if (pipes.Any())
                {
                    AddSparks(color);

                    foreach (Pipe p in pipes)
                    {
                        p.HealDamage();
                    }
                }
            } 
            else {
                // Colides with enemy -> damage
                if(enemies.Any())
                {
                    AddSparks(color);
                    
                    foreach (Enemy e in enemies)
                    {
                        e.TakeDamage(34);
                    }
                    
                }
            }

            // Collides with block
            if (blocks.Any())
            {
                AddSparks(color);
            }
        }

        public override void Draw(Graphics gfx, float dt)
        {
            if (IsBloodThirsty) 
            {
                gfx.Color = Color.Red;
            } 
            else
            {
                gfx.Color = Color.Green;
            } 
            
            gfx.DrawImage(Image, Transform);
        }
    }
}
