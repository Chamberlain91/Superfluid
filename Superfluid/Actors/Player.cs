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
        public const float PickupRadius = 170;

        public const float WalkSpeed = 4;

        public const float ShootRate = 0.33F;

        private float _shootTime = ShootRate;

        private bool _killingIntent = true;

        public Player()
            : base(CreatePlayerSprite())
        {
            LocalBounds = Rectangle.Inflate(LocalBounds, -10);
            LocalBounds = Rectangle.Offset(LocalBounds, (0, 10));
        }

        public bool InputLeft => Input.GetKeyDown(Key.A);

        public bool InputRight => Input.GetKeyDown(Key.D);

        public bool InputJump => Input.GetKeyDown(Key.Space);

        public bool InputDown => Input.GetKeyDown(Key.S);

        public bool InputKill => Input.GetKeyDown(Key.Q);

        public bool InputHeal => Input.GetKeyDown(Key.E);

        public bool InputShoot => Input.GetMouseDown(0);

        public bool InputGrab => Input.GetMouseDown(1);

        private bool _canGrab = true;

        // used to hold pipe when "picked up"
        public Pipe Pocket = null;

        protected override void IdleUpdate(float dt)
        {
            // 
            DetectJump();

            // Movement keys differ (ie, one is pressed)
            if (InputLeft != InputRight)
            {
                GotoState(State.Walk);
            }

            // Stop horizontal motion
            Velocity = (0, Velocity.Y);

            // Gun Mode input
            DetectGunMode();

            // Shooting input
            DetectShoot(dt);

            // Pickup input
            DetectPickUp();
        }

        private void DetectShoot(float dt)
        {
            _shootTime -= dt;

            if (InputShoot && _shootTime <= 0F)
            {
                _shootTime = ShootRate;

                var mouseWorld = Game.ScreenToWorld * Input.MousePosition;
                var dir = (mouseWorld - Transform.Position).Normalized;

                // 
                var laser = new Laser(_killingIntent);
                laser.Transform.Position = Transform.Position;
                laser.Transform.Direction = dir;

                Game.AddEntity(laser);
            }
        }

        private void DetectPickUp()
        {
            // Pressing grab and can grab
            if (InputGrab && _canGrab)
            {
                var mouseWorld = Game.ScreenToWorld * Input.MousePosition;

                // Test grab distance, if further than ~2x tiles away, reject
                if (Vector.Distance(Bounds.Center, mouseWorld) < PickupRadius)
                {
                    Game.Pipes.Pickup(mouseWorld, ref Pocket);
                }

                _canGrab = false;
            }
            // Not pressing grab
            else if (!InputGrab)
            {
                // Allow grabbing again 
                _canGrab = true;
            }
        }

        private void DetectGunMode()
        {
            if (InputKill)
            {
                // Sets the cursor
                Game.Window.SetCursor(Game.KillCursor);
                _killingIntent = true;
            }

            if (InputHeal)
            {
                Game.Window.SetCursor(Game.HealCursor);
                _killingIntent = false;
            }
        }

        protected override void WalkUpdate(float dt)
        {
            DetectJump();

            if (!DetectMovement())
            {
                GotoState(State.Idle);
            }

            // Gun Mode input
            DetectGunMode();

            // Shooting input
            DetectShoot(dt);

            // Pickup input 
            DetectPickUp();
        }

        protected override void JumpUpdate(float dt)
        {
            WantFallDown = false;
            DetectMovement();
            DetectGunMode();
            DetectShoot(dt);
            DetectPickUp();
        }

        protected override void HurtUpdate(float dt)
        {
            // 
        }

        private bool DetectMovement()
        {
            // Movement keys differ (ie, one is pressed)
            if (InputLeft != InputRight)
            {
                if (InputLeft)
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
                // Stop horizontal motion
                Velocity = (0, Velocity.Y);

                return false;
            }
        }

        private void DetectJump()
        {
            // 
            WantFallDown = false;

            // 
            if (InputJump)
            {
                Velocity = (Velocity.X, -10);
                GotoState(State.Jump);
            }

            // 
            if (InputDown)
            {
                // GotoState(State.Jump);
                WantFallDown = true;
            }

            // Fall detection
            if (!Game.QuerySpatial<Block>(Rectangle.Inflate(Bounds, 0.5F)).Any())
            {
                GotoState(State.Jump);
                WantFallDown = false;
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

        public override void Draw(Graphics gfx, float dt)
        {
            base.Draw(gfx, dt);

            // 
            if (Pocket != null)
            {
                var mouseGrid = Input.GetGridMousePosition();
                var mouseTest = mouseGrid + (35, 35);

                var alpha = 0.40F;
                if (Vector.Distance(Bounds.Center, mouseTest) > PickupRadius)
                {
                    alpha = 0.15F;
                }

                // Pocket shadow
                gfx.Color = new Color(alpha, alpha, alpha, alpha);
                gfx.DrawImage(Pocket.Image, Matrix.CreateTranslation(mouseGrid));
            }
        }

        private static Sprite CreatePlayerSprite()
        {
            var builder = new SpriteBuilder
            {
                { "walk", 0.1F, Assets.GetImages("alienpink_walk1_crop", "alienpink_walk2_crop") },
                { "jump", 0.1F, Assets.GetImages("alienpink_jump_crop") },
                { "idle", 5F, Assets.GetImages("alienpink_stand_crop", "alienpink_front_crop") },
                { "hurt", 1.0F, Assets.GetImages("alienpink_hit_crop") }
            };

            return builder.CreateSprite();
        }
    }
}
