using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{
    public class Spark : Entity
    {
        public Image Image { get; }

        private float _time;

        private Color _color;
        private float _speed;
        private Vector _direction;

        public Spark(Color color)
        {
            Image = Assets.GetImage("particlewhite_2");

            _color = color;

            // Randomized direction and speed
            _speed = Calc.Random.NextFloat(3F, 7F);
            _direction = Vector.FromAngle(Calc.Random.NextFloat(0, Calc.TwoPi));

            // Randomize kill time
            _time = Calc.Random.NextFloat(0.2F, 0.8F);

            // Randomize scale
            var scale = Calc.Random.NextFloat(0.1F, 0.2F);
            Transform.Scale = (scale, scale);
        }

        public override void Update(float dt)
        {
            Transform.Position += _direction * _speed;
            Transform.Rotation += 1F / _speed;

            _speed += -dt * 5; // slow down
            if (_speed < 1) { _speed = 1F; }

            // Count down
            _time -= dt;

            // Particle is expired
            if (_time < 0)
            {
                Game.RemoveEntity(this);
            }
        }

        public override void Draw(Graphics gfx, float dt)
        {
            var color = _color;
            color.A = 1F - Calc.Pow(1F - _time, 2);

            gfx.Color = color;
            gfx.DrawImage(Image, Transform);
        }
    }
}
