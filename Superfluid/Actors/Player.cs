using System.Linq;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;
using Superfluid.Entities;

namespace Superfluid.Actors
{
    public class Player : Actor
    {
        public const float WalkSpeed = 4;

        public Player(Sprite sprite)
            : base(sprite)
        {
            LocalBounds = Rectangle.Inflate(LocalBounds, -10);
            LocalBounds = Rectangle.Offset(LocalBounds, (0, 10));
        }

        public bool KeyLeft => Input.GetKeyDown(Key.A);

        public bool KeyRight => Input.GetKeyDown(Key.D);

        public bool KeyJump => Input.GetKeyDown(Key.Space);

        protected override void IdleUpdate(float dt)
        {
            DetectJump();

            // Movement keys differ (ie, one is pressed)
            if (KeyLeft != KeyRight)
            {
                GotoState(State.Walk);
            }
        }

        protected override void WalkUpdate(float dt)
        {
            DetectJump();

            if (!DetectMovement())
            {
                GotoState(State.Idle);
            }
        }

        protected override void JumpUpdate(float dt)
        {
            DetectMovement();
        }

        protected override void HurtUpdate(float dt)
        {
            // 
        }

        private bool DetectMovement()
        {
            // Movement keys differ (ie, one is pressed)
            if (KeyLeft != KeyRight)
            {
                if (KeyLeft)
                {
                    Velocity = (-WalkSpeed, Velocity.Y);
                    Facing = FaceDirection.Left;
                }
                else
                {
                    Velocity = (WalkSpeed, Velocity.Y);
                    Facing = FaceDirection.Right;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void DetectJump()
        {
            if (KeyJump)
            {
                Velocity = (Velocity.X, -10);
                GotoState(State.Jump);
            }

            // Fall detection
            if (!Game.QuerySpatial<Block>(Rectangle.Inflate(Bounds, 0.5F)).Any())
            {
                GotoState(State.Jump);
            }
        }

        internal override void OnHorizontalCollision(int dir)
        {
            // 
        }

        internal override void OnVerticalCollision(int dir)
        {
            if (dir > 0 && CurrentState == State.Jump)
            {
                GotoState(State.Idle);
            }
        }
    }
}
