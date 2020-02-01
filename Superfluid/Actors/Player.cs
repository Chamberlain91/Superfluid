using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Actors
{
    public class Player : Actor
    {
        public const float WalkSpeed = 4;

        private readonly StateMachine<State> _stateMachine = new StateMachine<State>();

        public Player(Sprite sprite)
            : base(sprite)
        {
            // Create state machine
            _stateMachine.Add(State.Idle, IdleEnter, IdleUpdate, null);
            _stateMachine.Add(State.Walk, WalkEnter, WalkUpdate, null);
            _stateMachine.Add(State.Jump, null, null, null);
            _stateMachine.Add(State.Hurt, null, null, null);

            // Goto default state
            _stateMachine.Goto(State.Idle);
        }

        #region Idle

        private void IdleEnter()
        {
            SetAnimation("idle");
        }

        private void IdleUpdate(float dt)
        {
            if (Input.GetKeyDown(Key.A) || Input.GetKeyDown(Key.D))
            {
                _stateMachine.Goto(State.Walk);
            }
        }

        #endregion

        #region Walk

        private void WalkEnter()
        {
            SetAnimation("walk");
        }

        private void WalkUpdate(float dt)
        {
            var keyLeft = Input.GetKeyDown(Key.A);
            var keyRight = Input.GetKeyDown(Key.D);

            if (!(keyLeft || keyRight) || (keyLeft && keyRight))
            {
                _stateMachine.Goto(State.Idle);
            }
            else
            {
                if (keyLeft)
                {
                    Transform.Position += Vector.Left * WalkSpeed;
                    Facing = FaceDirection.Left;
                }

                if (keyRight)
                {
                    Transform.Position += Vector.Right * WalkSpeed;
                    Facing = FaceDirection.Right;
                }
            }
        }

        #endregion

        public override void Update(float dt)
        {
            // Update actor basics
            base.Update(dt);

            // Update state machine
            _stateMachine.Update(dt);
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // Draw actor
            base.Draw(gfx, dt);

            // Draw meta data
            gfx.Color = Color.Magenta;
            gfx.DrawText($"State: {_stateMachine.State}", Transform.Position, Font.Default, 32);
        }

        private enum State
        {
            Idle,
            Walk,
            Jump,
            Hurt
        }
    }
}
