using System.Linq;

using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Entities;

using Range = Heirloom.Math.Range;

namespace Superfluid.Engine
{
    public abstract class Actor : Entity
    {
        private float _time;
        private int _index;

        private Rectangle _bounds;

        private readonly StateMachine<State> _stateMachine = new StateMachine<State>();

        protected FaceDirection SpriteNeutralFacing = FaceDirection.Right;

        protected FaceDirection Facing;

        protected bool WantFallDown = false;

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

        protected State CurrentState => _stateMachine.State;

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

            // Velocity limit
            if (Velocity.Y > 15) { Velocity = (Velocity.X, 15); }

            // Integrate vertical velocity
            Transform.Position += (0, Velocity.Y);
            ComputeBounds();

            // Resolve vertical collisions
            var checkHorizontal = CollideVertical(dt);

            // Integrate horizontal velocity
            Transform.Position += (Velocity.X, 0);
            ComputeBounds();

            // Resolve horiztonal collisions
            if (checkHorizontal) { IntegrateHorizontal(dt); }

            // 
            AdvanceAnimation(dt);

            // Update state machine
            _stateMachine.Update(dt);
        }

        private void IntegrateHorizontal(float dt)
        {
            // Finds horizontal collision edges
            var range = Range.Indeterminate;
            foreach (var block in Game.QuerySpatial<Block>(Bounds))
            {
                range.Max = Calc.Max(block.Bounds.Right, range.Max);
                range.Min = Calc.Min(block.Bounds.Left, range.Min);
            }

            var penetration = 0F;
            if (range.Min < float.MaxValue && Velocity.X > 0)
            {
                // Right Collision
                penetration = Bounds.Right - range.Min + 1F;
            }
            else if (range.Max > float.MinValue && Velocity.X < 0)
            {
                // Left Collision
                penetration = Bounds.Left - range.Max - 1F;
            }

            // If penetrating a vertical surface, push out and trigger events
            if (Calc.Abs(penetration) > 0)
            {
                // Push out of surface
                Transform.Position -= (penetration, 0);

                // Update bounds
                ComputeBounds();

                // Trigger Collision Flag
                OnHorizontalCollision(Calc.Sign(penetration));

                // Stop moving horizontally
                Velocity = (0, Velocity.Y);
            }
        }

        private bool CollideVertical(float dt)
        {
            // Finds vertical collision edges
            var yRange = Range.Indeterminate;
            var maxBlock = default(Block);

            // Detect possibly colliders
            var colliders = Game.QuerySpatial<Block>(Bounds);

            if (colliders.Any())
            {
                // 
                foreach (var block in colliders)
                {
                    yRange.Min = Calc.Min(block.Bounds.Top, yRange.Min);

                    // 
                    if (block.Bounds.Bottom > yRange.Max)
                    {
                        yRange.Max = block.Bounds.Bottom;
                        maxBlock = block;
                    }
                }

                var penetration = 0F;
                if (Bounds.Top < yRange.Min && Bounds.Bottom < yRange.Max)
                {
                    // One way pass through
                    if ((maxBlock?.IsOneWay ?? false))
                    {
                        if (Velocity.Y < 0) { return false; }
                        if (WantFallDown) { return false; }
                    }

                    // Down Collision
                    penetration = Bounds.Bottom - yRange.Min + 0.2F;
                }
                else if (Bounds.Top > yRange.Min && Bounds.Bottom > yRange.Max)
                {
                    // One way pass through
                    if (maxBlock?.IsOneWay ?? false) { return false; }

                    // Up Collision
                    penetration = Bounds.Top - yRange.Max - 0.2F;
                }

                // If penetrating a vertical surface, push out and trigger events
                if (Calc.Abs(penetration) > 0)
                {
                    // Push out of surface
                    Transform.Position -= (0, penetration);

                    // Update bounds
                    ComputeBounds();

                    // Trigger Collision Flag
                    OnVerticalCollision(Calc.Sign(penetration));

                    // Stop moving vertically
                    Velocity = (Velocity.X, 0);
                }
                else
                {
                    // ...
                    return false;
                }
            }

            return true;
        }

        internal abstract void OnHorizontalCollision(int dir);

        internal virtual void OnVerticalCollision(int dir)
        {
            if (dir > 0 && CurrentState == State.Jump)
            {
                GotoState(State.Idle);
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
            if (Facing != SpriteNeutralFacing)
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
            gfx.DrawText($"State: {_stateMachine.State}\nWant Fall: {WantFallDown}", Transform.Position, Font.Default, 32);

            // Draw Bounds
            gfx.Color = Color.Green;
            gfx.DrawRectOutline(Bounds);

            // 
            foreach (var spatial in Game.Spatial.Query(Rectangle.Inflate(Bounds, 8)))
            {
                gfx.Color = Color.Orange;
                gfx.DrawRectOutline(spatial.Bounds, 3);
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
