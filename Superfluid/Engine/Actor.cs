using Heirloom.Drawing;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public abstract class Actor : Entity
    {
        private float _time;
        private int _index;

        private Rectangle _bounds;

        protected FaceDirection Facing;

        protected enum FaceDirection
        {
            Left,
            Right
        }

        protected Actor(Sprite sprite)
        {
            Sprite = sprite;
            SetAnimation(Sprite.DefaultAnimation.Name);

            // 
            LocalBounds = Sprite.Frames[0].Image.Bounds;
        }

        public Sprite Sprite { get; }

        public Sprite.FrameInfo Frame => Sprite.Frames[_index];

        public Sprite.Animation Animation { get; private set; }

        public Rectangle LocalBounds { get; protected set; }

        public Rectangle Bounds => _bounds;

        public void SetAnimation(string name)
        {
            // 
            Animation = Sprite.GetAnimation(name);

            // 
            _index = Animation.From;
            _time = 0;
        }

        public override void Update(float dt)
        {
            _bounds = LocalBounds;
            _bounds.Offset(Transform.Position);
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // 
            AdvanceAnimation(dt);

            // Compute flip matrix
            var flipMatrix = Matrix.Identity;
            if (Facing == FaceDirection.Left)
            {
                flipMatrix = Matrix.CreateScale(-1, 1);
            }

            // 
            gfx.DrawSprite(Sprite, _index, Transform * flipMatrix);
            gfx.DrawText($"{_index}", Transform.Position, Font.Default, 32);

            // 
            gfx.Color = Color.Green;
            gfx.DrawRectOutline(Bounds);
        }

        private void AdvanceAnimation(float dt)
        {
            // Accumulate time
            _time += dt;

            // 
            while (_time > Frame.Delay)
            {
                // Advance frame
                _time -= Frame.Delay;
                _index++;

                // Wrap frame number
                if (_index >= Animation.To) { _index = Animation.From; }
            }
        }
    }
}
