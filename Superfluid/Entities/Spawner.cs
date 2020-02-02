using System;

using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Actors;
using Superfluid.Engine;

namespace Superfluid.Entities
{
    public sealed class Spawner : Entity
    {
        private readonly Func<Enemy> _createEnemy;
        private float _time;
        private int _count;

        public Spawner(int count, float period, Func<Enemy> createEnemy)
        {
            _createEnemy = createEnemy;

            // 
            _time = period / 2 + Calc.Random.NextFloat(0, period / 2);

            SpawnPeriod = period;
            SpawnCount = count;
        }

        public float SpawnPeriod { get; }

        public int SpawnCount { get; }

        public override void Update(float dt)
        {
            // 
            _time -= dt;

            // 
            if (_time <= 0)
            {
                _time = SpawnPeriod;

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
