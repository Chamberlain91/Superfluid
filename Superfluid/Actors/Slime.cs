using System.Linq;

using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;
using Superfluid.Entities;

namespace Superfluid.Actors
{
    public class Slime : Enemy
    {
        private static readonly Sprite _sprite = CreateSprite();

        public Slime()
            : base(_sprite, maxHealth: 100)
        {
            // Shrink bounds a little
            LocalBounds = Rectangle.Inflate(LocalBounds, -4);
            LocalBounds = Rectangle.Offset(LocalBounds, (0, 4));

            // Start in walking state
            GotoState(State.Walk);
        }

        private static Sprite CreateSprite()
        {
            var builder = new SpriteBuilder
            {
                { "walk", 0.5F, Assets.GetImages("slime", "slime_walk") },
                { "jump", 1.0F, Assets.GetImages("slime") },
                { "idle", 1.0F, Assets.GetImages("slime") },
                { "hurt", 1.0F, Assets.GetImages("slime_hit") }
            };

            return builder.CreateSprite();
        }

        protected override void JumpUpdate(float dt)
        {
            // nada
        }

        protected override void WalkUpdate(float dt)
        {
            if (Facing == FaceDirection.Left) { Velocity = (-1, Velocity.Y); }
            else { Velocity = (+1, Velocity.Y); }

            // Near/Touching a pipe
            if (Game.QuerySpatial<Pipe>(Bounds).Where(p => p.Health > 0 && !p.IsGoldPipe).Any())
            {
                GotoState(State.Idle);
            }
        }

        protected override void IdleUpdate(float dt)
        {
            Velocity = (0, Velocity.Y);

            var pipes = Game.QuerySpatial<Pipe>(Bounds).Where(p => p.Health > 0);

            // Not near a pipe
            if (!pipes.Any())
            {
                GotoState(State.Walk);
            }
            else
            {
                AttackPipe(dt, pipes.First());
            }
        }

        internal override void OnHorizontalCollision(int dir)
        {
            if (dir == -1 && Facing == FaceDirection.Left) { Facing = FaceDirection.Right; }
            if (dir == +1 && Facing == FaceDirection.Right) { Facing = FaceDirection.Left; }
        }

        internal override void OnVerticalCollision(int dir)
        {
            // nada
        }
    }
}
