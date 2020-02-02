using Heirloom.Drawing;

using Superfluid.Engine;

namespace Superfluid.Actors
{
    public abstract class Enemy : Actor
    {
        protected Enemy(Sprite sprite)
            : base(sprite)
        {
            SpriteNeutralFacing = FaceDirection.Left;
        }
    }
}
