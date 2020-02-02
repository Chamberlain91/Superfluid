using Heirloom.Drawing;

using Superfluid.Engine;

namespace Superfluid.Actors
{
    public abstract class Enemy : Actor
    {
        public float CurrHealth = 0
        ;
        protected Enemy(Sprite sprite, float maxHealth) 
            : base(sprite)
        {
            SpriteNeutralFacing = FaceDirection.Left;
            MaxHealth = maxHealth;
            CurrHealth = maxHealth;
        }

        public float MaxHealth { get; }
        private bool _damageTaken = false;

        public void TakeDamage(float damage)
        {
            CurrHealth -= damage;
            _damageTaken = true;
        }

        protected override void HurtUpdate(float dt) 
        {
            if (_damageTaken)
            {
                if (CurrHealth <= 0)
                {
                    // TODO: some death animation
                    
                    Game.RemoveEntity(this);
                }

                else 
                {
                    // TODO: Damage animation
                }

                _damageTaken = false;
            }
        }
    }
}
