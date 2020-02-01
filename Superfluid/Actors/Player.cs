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
            // 
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
                    Transform.Position += Vector.Left * WalkSpeed;
                    Facing = FaceDirection.Left;
                }
                else
                {
                    Transform.Position += Vector.Right * WalkSpeed;
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
