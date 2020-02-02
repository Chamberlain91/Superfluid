﻿using System.Linq;

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

        public const float ShootRate = 0.33F;

        private float _shootTime = ShootRate;

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

        public bool InputShoot => Input.GetMouseDown(0);

        public bool HasGrab => Input.GetMouseDown(1);

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

            // Shooting input
            DetectShoot(dt);
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
                var laser = new Laser();
                laser.Transform.Position = Transform.Position;
                laser.Transform.Direction = dir;

                Game.AddEntity(laser);
            }
        }

        private void DetectPickUp() 
        {
            // TODO: UwU
        }

        protected override void WalkUpdate(float dt)
        {
            DetectJump();

            if (!DetectMovement())
            {
                GotoState(State.Idle);
            }

            // Shooting input
            DetectShoot(dt);
        }

        protected override void JumpUpdate(float dt)
        {
            WantFallDown = false;
            DetectMovement();
            DetectShoot(dt);
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
