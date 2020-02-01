using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Actors
{
    public class Player : Actor
    {
        public const float WalkSpeed = 4;

        public Player(Sprite sprite)
            : base(sprite)
        {
            LocalBounds = Rectangle.Inflate(LocalBounds, -8);
            LocalBounds = Rectangle.Offset(LocalBounds, (0, 8));
        }

        public override void Update(float dt)
        {
            base.Update(dt);


            if (Input.GetKeyDown(Key.P))
            {
                Transform.Position = (300, 300);
            }
        }

        protected override void IdleUpdate(float dt)
        {
            var keyLeft = Input.GetKeyDown(Key.A);
            var keyRight = Input.GetKeyDown(Key.D);

            // Movement keys differ (ie, one is pressed)
            if (keyLeft != keyRight)
            {
                GotoState(State.Walk);
            }
        }

        protected override void WalkUpdate(float dt)
        {
            var keyLeft = Input.GetKeyDown(Key.A);
            var keyRight = Input.GetKeyDown(Key.D);

            // Movement keys differ (ie, one is pressed)
            if (keyLeft != keyRight)
            {
                if (keyLeft)
                {
                    Velocity = (-WalkSpeed, Velocity.Y);
                    Facing = FaceDirection.Left;
                }
                else
                {
                    Velocity = (WalkSpeed, Velocity.Y);
                    Facing = FaceDirection.Right;
                }
            }
            else
            {
                GotoState(State.Idle);
            }
        }

        protected override void JumpUpdate(float dt)
        {
            // 
        }

        protected override void HurtUpdate(float dt)
        {
            // 
        }
    }
}
