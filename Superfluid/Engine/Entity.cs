using Heirloom.Drawing;

namespace Superfluid.Engine
{
    public abstract class Entity
    {
        public Transform Transform { get; }

        protected Entity()
        {
            Transform = new Transform();
        }

        public abstract void Update(float dt);

        public abstract void Draw(Graphics gfx, float dt);
    }
}
