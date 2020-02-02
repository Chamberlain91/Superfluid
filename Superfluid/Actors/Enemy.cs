using Heirloom.Drawing;

using Superfluid.Engine;
using Superfluid.Entities;

namespace Superfluid.Actors
{
    public abstract class Enemy : Actor
    {
        private bool _wasDamagedFlag = false;

        protected Enemy(Sprite sprite, float maxHealth)
            : base(sprite)
        {
            SpriteNeutralFacing = FaceDirection.Left;

            CurrentHealth = maxHealth;
            MaxHealth = maxHealth;

            // 
            AttackTimer = AttackDuration;
        }

        public float MaxHealth { get; }

        public float CurrentHealth { get; private set; }

        public float AttackDuration { get; protected set; } = 0.5F;

        public float AttackTimer { get; set; }

        public float AttackPower { get; set; } = 5;

        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            _wasDamagedFlag = true;

            if (CurrentHealth <= 0)
            {
                Game.RemoveEntity(this);
            }
        }

        /// <summary>
        /// Attacks the given pipe, assumes this pipe is valid.
        /// </summary>
        protected void AttackPipe(float dt, Pipe pipe)
        {
            if (AdvanceAttackTimer(dt))
            {
                // SMASH!
                pipe.TakeDamage(AttackPower);
            }
        }

        protected override void HurtUpdate(float dt)
        {
            if (_wasDamagedFlag)
            {
                if (CurrentHealth <= 0)
                {
                    // TODO: some death animation
                }
                else
                {
                    // TODO: Damage animation
                }

                _wasDamagedFlag = false;
            }
        }

        private bool AdvanceAttackTimer(float dt)
        {
            AttackTimer -= dt;

            if (AttackTimer <= 0)
            {
                AttackTimer = AttackDuration;
                return true;
            }

            return false;
        }
    }
}
