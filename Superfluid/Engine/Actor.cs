using System;

using Heirloom.Drawing;
using Heirloom.Math;

using Range = Heirloom.Math.Range;

namespace Superfluid.Engine
{
    public abstract class Actor : Entity
    {
        private float _time;
        private int _index;

        private Rectangle _bounds;

        private readonly StateMachine<State> _stateMachine = new StateMachine<State>();

        protected FaceDirection Facing;

        protected Actor(Sprite sprite)
        {
            Sprite = sprite;
            SetAnimation(Sprite.DefaultAnimation.Name);

            // 
            LocalBounds = Sprite.Frames[0].Image.Bounds;

            // Create state machine
            _stateMachine.Add(State.Idle, IdleEnter, IdleUpdate, null);
            _stateMachine.Add(State.Walk, WalkEnter, WalkUpdate, null);
            _stateMachine.Add(State.Jump, JumpEnter, JumpUpdate, null);
            _stateMachine.Add(State.Hurt, HurtEnter, HurtUpdate, null);

            // Goto default state
            _stateMachine.Goto(State.Idle);
        }

        public Sprite Sprite { get; }

        public Sprite.FrameInfo Frame => Sprite.Frames[_index];

        public Sprite.Animation Animation { get; private set; }

        public Rectangle LocalBounds { get; protected set; }

        public Rectangle Bounds => _bounds;

        public Vector Velocity { get; set; }

        public void SetAnimation(string name)
        {
            // 
            Animation = Sprite.GetAnimation(name);

            // 
            _index = Animation.From;
            _time = 0;
        }

        protected void GotoState(State state)
        {
            _stateMachine.Goto(state);
        }

        protected abstract void HurtUpdate(float dt);
        protected virtual void HurtEnter() { SetAnimation("hurt"); }

        protected abstract void JumpUpdate(float dt);
        protected virtual void JumpEnter() { SetAnimation("jump"); }

        protected abstract void WalkUpdate(float dt);
        protected virtual void WalkEnter() { SetAnimation("walk"); }

        protected abstract void IdleUpdate(float dt);
        protected virtual void IdleEnter() { SetAnimation("idle"); }

        public override void Update(float dt)
        {
            // Gravity
            Velocity += Vector.Down * 0.33F;

            // 
            IntegrateHorizontal();
            IntegrateVertical();

            // 
            AdvanceAnimation(dt);

            // Update state machine
            _stateMachine.Update(dt);
        }

        private void IntegrateHorizontal()
        {
            Transform.Position += (Velocity.X, 0);
            ComputeBounds();

            // Find thresholds
            var range = Range.Indeterminate;
            foreach (var block in Game.Spatial.Query(Bounds))
            {
                range.Max = Calc.Max(block.Bounds.Right, range.Max);
                range.Min = Calc.Min(block.Bounds.Left, range.Min);
            }

            if (Velocity.X > 0)
            // Moving right
            {
                // If found a penetration
                if (range.Min < float.MaxValue)
                {
                    // Push out of blocks
                    var pen = Bounds.Right - range.Min;
                    Transform.Position -= (pen + 1, 0);

                    // Stop moving horizontally
                    Velocity = (0, Velocity.Y);

                    // Update bounds
                    ComputeBounds();
                }
            }
            else
            // Moving left
            {
                // If found a penetration
                if (range.Max > float.MinValue)
                {
                    // Push out of blocks
                    var pen = Bounds.Left - range.Max;
                    Transform.Position -= (pen - 1, 0);

                    // Stop moving horizontally
                    Velocity = (0, Velocity.Y);

                    // Update bounds
                    ComputeBounds();
                }
            }
        }

        private void IntegrateVertical()
        {
            Transform.Position += (0, Velocity.Y);
            ComputeBounds();

            // Find minimal threshold
            var range = Range.Indeterminate;
            foreach (var block in Game.Spatial.Query(Bounds))
            {
                range.Max = Calc.Max(block.Bounds.Bottom, range.Max);
                range.Min = Calc.Min(block.Bounds.Top, range.Min);
            }

            if (Velocity.Y > 0)
            // Moving down
            {
                // If found a penetration
                if (range.Min < float.MaxValue)
                {
                    // Push out of blocks
                    var pen = Bounds.Bottom - range.Min;
                    Transform.Position -= (0, pen + 1);

                    // Stop moving vertically
                    Velocity = (Velocity.X, 0);

                    // Update bounds
                    ComputeBounds();
                }
            }
            else
            // Moving up
            {
                // If found a penetration
                if (range.Max > float.MinValue)
                {
                    // Push out of blocks
                    var pen = Bounds.Top - range.Max;
                    Transform.Position -= (0, pen - 1);

                    // Stop moving vertically
                    Velocity = (Velocity.X, 0);

                    // Update bounds
                    ComputeBounds();
                }
            }
        }

        private void ComputeBounds()
        {
            _bounds = LocalBounds;
            _bounds.Offset(Transform.Position);
        }

        public override void Draw(Graphics gfx, float dt)
        {
            // Compute flip matrix
            var flipMatrix = Matrix.Identity;
            if (Facing == FaceDirection.Left)
            {
                flipMatrix = Matrix.CreateScale(-1, 1);
            }

            // Draw sprite
            gfx.DrawSprite(Sprite, _index, Transform * flipMatrix);
        }

        public override void DebugDraw(Graphics gfx)
        {
            // Draw State
            gfx.Color = Color.Magenta;
            gfx.DrawText($"State: {_stateMachine.State}", Transform.Position, Font.Default, 32);

            // Draw Bounds
            gfx.Color = Color.Green;
            gfx.DrawRectOutline(Bounds);

            // 
            foreach (var block in Game.Spatial.Query(Bounds))
            {
                gfx.Color = Color.Orange;
                gfx.DrawRectOutline(block.Bounds, 3);
            }
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

        protected enum FaceDirection
        {
            Left,
            Right
        }

        protected enum State
        {
            Idle,
            Walk,
            Jump,
            Hurt
        }
    }
}
