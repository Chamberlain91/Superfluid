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

        public Pipe(Image image, Rectangle localBounds, IEnumerable<Vector> localOffsets, bool isGoldPipe)
        {
            Image = image;

            Connections = new HashSet<Pipe>();

            LocalBounds = localBounds;
            Bounds = localBounds;

            LocalConnectPoints = localOffsets.ToArray();
            ConnectPoints = localOffsets.ToArray();

            IsGoldPipe = isGoldPipe;

            // Draw behind ground layer
            Layer = EntityLayer.Back;
        }

        public Rectangle LocalBounds { get; }

        public readonly Vector[] LocalConnectPoints;

        public Rectangle Bounds { get; private set; }

        public readonly Vector[] ConnectPoints;

        public bool IsGoldPipe { get; }

        public HashSet<Pipe> Connections { get; }

        public override void Update(float dt)
        {
            // nada
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
            gfx.DrawImage(Image, Transform);
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
