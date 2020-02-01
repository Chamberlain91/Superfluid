using System.Diagnostics;

using Heirloom.Drawing;

namespace Superfluid.Engine
{
    public abstract class Entity
    {
        public Transform Transform { get; }

        public EntityLayer Layer { get; set; }

        protected Entity()
        {
            Transform = new Transform();
        }

        public abstract void Update(float dt);

        public abstract void Draw(Graphics gfx, float dt);

        [Conditional("DEBUG")]
        public virtual void DebugDraw(Graphics gfx) { }
    }
}
