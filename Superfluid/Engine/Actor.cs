using Heirloom.Drawing;

namespace Superfluid.Engine
{
    public abstract class Actor : Entity
    {
        private Sprite _sprite;

        private float _time;
        private int _index;

        protected Actor(Sprite sprite)
        {
            Sprite = sprite;
        }

        public Sprite Sprite
        {
            get => _sprite;

            set
            {
                _sprite = value;
                SetAnimation(_sprite.DefaultAnimation.Name);
            }
        }

        public Sprite.FrameInfo Frame => Sprite.Frames[_index];

        public Sprite.Animation Animation { get; private set; }

        public void SetAnimation(string name)
        {
            // 
            Animation = Sprite.GetAnimation(name);

            // 
            _index = Animation.From;
            _time = 0;
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // 
            AdvanceAnimation(dt);

            // 
            gfx.DrawSprite(Sprite, _index, Transform);
            gfx.DrawText($"{_index}", Transform.Position, Font.Default, 32);
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
