using System.Collections.Generic;
using System.Linq;

using Heirloom.Drawing;
using Heirloom.Math;

using Superfluid.Engine;

namespace Superfluid.Entities
{
    public class Pipe : Entity, ISpatialObject
    {
        public Image Image;

        private const float MaxHealth = 100;
        
        private const int MaxHearts = 3;

        public Pipe(Image image, Rectangle localBounds, IEnumerable<Vector> localOffsets, bool isGreyPipe, bool isGoldPipe)
        {
            Image = image;

            Connections = new HashSet<Pipe>();

            LocalBounds = localBounds;
            Bounds = localBounds;

            LocalConnectPoints = localOffsets.ToArray();
            ConnectPoints = localOffsets.ToArray();

            IsGreyPipe = isGreyPipe;
            IsGoldPipe = isGoldPipe;

            // Draw behind ground layer
            Layer = EntityLayer.Back;
        }

        public Rectangle LocalBounds { get; }

        public readonly Vector[] LocalConnectPoints;

        public Rectangle Bounds { get; private set; }

        public readonly Vector[] ConnectPoints;

        public bool IsGreyPipe { get; }

        public bool IsGoldPipe { get; }

        public bool IsFunctional { get; set; }

        public HashSet<Pipe> Connections { get; }

        public float Health { get; private set; } = MaxHealth;

        private static Image _heartEmpty = Assets.GetImage("hud_heartempty");
        private static Image _heartHalf = Assets.GetImage("hud_hearthalf");
        private static Image _heartFull = Assets.GetImage("hud_heartfull");

        public override void Update(float dt)
        {
            // nada
        }

        // Called when enemies hit it
        public void TakeDamage(float damage)
        {
            if (IsGoldPipe) { return; }

            if (Health > 0)
            {
                // Log.Info($"Pipe Health = {Health} (Damage {damage})");
                Health -= damage;
            }

            // Clamp at 0
            if (Health < 0) { Health = 0; }
            Game.Pipes.EvaluatePipeConfiguration();
        }

        public void HealDamage(float amount = 20)
        {
            if (Health < MaxHealth)
            {
                // Log.Info($"Pipe Health = {Health} (Heal {amount})");
                Health += amount;
            }

            // Clamp at max
            if (Health > MaxHealth) { Health = MaxHealth; }
            Game.Pipes.EvaluatePipeConfiguration();
        }

        public void ComputeWorldSpace()
        {
            // Computes world bounds
            var bounds = LocalBounds;
            bounds.Position += Transform.Position;
            Bounds = bounds;

            // Compute connection offsets
            ConnectPoints[0] = LocalConnectPoints[0] + Transform.Position;
            ConnectPoints[1] = LocalConnectPoints[1] + Transform.Position;
        }

        public IEnumerable<Vector> GetValidConnectionPoints()
        {
            var worldBounds = new Rectangle(Vector.Zero, Game.Map.Size * Game.Map.TileSize);

            // Emits coordinates inside the world
            foreach (var pt in ConnectPoints)
            {
                if (worldBounds.ContainsPoint(pt))
                {
                    yield return pt;
                }
            }
        }

        public override void Draw(Graphics gfx, float dt)
        {
            if (IsFunctional)
            {
                var px = Calc.Random.Choose(-1, 0, +1);
                var py = Calc.Random.Choose(-1, 0, +1);
                var wobble = Matrix.CreateTranslation(px, py);
                gfx.DrawImage(Image, Transform * wobble);
            }
            else
            {
                gfx.DrawImage(Image, Transform);
            }

            // Draw damage
            if (Health < MaxHealth)
            {
                var hearts = Calc.Ceil(Health / MaxHealth * MaxHearts * 2) / 2F;

                var scale = new Vector(0.6F, 0.6F);
                var w = (_heartEmpty.Width / 2) + 8;
                var h = _heartEmpty.Height / 4;

                // 
                for (var i = 0; i < MaxHearts; i++)
                {
                    var pos = Bounds.Center + ((i * w) - w * MaxHearts / 2, -h);

                    // Full Heart
                    if (i < Calc.Floor(hearts)) { gfx.DrawImage(_heartFull, pos, 0, scale); }
                    // Half Heart
                    else if (i < Calc.Ceil(hearts)) { gfx.DrawImage(_heartHalf, pos, 0, scale); }
                    // Empty Heart
                    else { gfx.DrawImage(_heartEmpty, pos, 0, scale); }
                }
            }
        }

        public override void DebugDraw(Graphics gfx)
        {
            gfx.Color = Color.Black;
            gfx.DrawRectOutline(Bounds);

            // 
            gfx.Color = Color.Pink;
            foreach (var pt in GetValidConnectionPoints())
            {
                gfx.DrawCross(pt, 8, 2);
            }

            foreach (var con in Connections)
            {
                gfx.DrawLine(Bounds.Center, con.Bounds.Center);
            }
        }
    }
}
