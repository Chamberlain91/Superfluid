using System;

using Heirloom.Drawing;

using Superfluid.Actors;
using Superfluid.Engine;

namespace Superfluid.Entities
{
    public sealed class Spawner : Entity
    {
        private readonly Func<Enemy> _createEnemy;
        private float _timer;
        private int _count;

        public Spawner(int count, float period, Func<Enemy> createEnemy)
        {
            _createEnemy = createEnemy;

            SpawnPeriod = period;
            SpawnCount = count;
        }

        public float SpawnPeriod { get; }

        public int SpawnCount { get; }

        public override void Update(float dt)
        {
            // 
            _timer -= dt;

            // 
            if (_timer <= 0)
            {
                _timer = SpawnPeriod;

                // 
                if (_count < SpawnCount)
                {
                    _count++;

                    // Create and position enemy
                    var en = Game.AddEntity(_createEnemy());
                    en.Transform.Position = Transform.Position;
                }
            }
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // 
        }
    }
}
