using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Heirloom.Collections;
using Heirloom.Collections.Spatial;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Drawing.Extras;
using Heirloom.Math;

using Superfluid.Actors;
using Superfluid.Engine;
using Superfluid.Entities;

namespace Superfluid
{
    public static class Game
    {
        public static Window Window { get; private set; }

        public static RenderLoop Loop { get; private set; }

        public static TileMap Map;

        public static Image Background;

        public static BoundingTreeSpatialCollection<ISpatialObject> Spatial { get; private set; }

        public static Color BackgroundColor = Color.Parse("#95A5A6");

        private static HashSet<Entity> _addSet, _remSet;
        private static TypeDictionary<Entity> _entities;

        private static void Main(string[] args)
        {
            Application.Run(() =>
            {
                // Create spatial collection
                Spatial = new BoundingTreeSpatialCollection<ISpatialObject>();

                // Create entities collection
                _entities = new TypeDictionary<Entity>();

                // 
                _remSet = new HashSet<Entity>();
                _addSet = new HashSet<Entity>();

                // Create the game window
                Window = new Window("Superfluid!");
                Window.Graphics.EnableFPSOverlay = true;
                Window.Maximize();

                // Bind Input
                Input.AttachToWindow(Window);

                // Load game assets
                Assets.LoadDatabase();
                Assets.PackAtlas();

                // Center origins on assets prefixed by string given
                Assets.SetImagesCenterOrigin("crosshair102");
                Assets.SetImagesCenterOrigin("alien"); // alienpink_walk1, etc

                // Sets the cursor
                SetCursor("crosshair102", Color.Pink);

                // Load the background image
                Background = Assets.GetImage("colored_desert");

                // Load the test map
                LoadMap("testmap");

                // Create the player actor
                var player = new Player(LoadPlayerSprite());
                player.Transform.Position = (200, 300);
                _entities.Add(player);

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
        }

        public static void AddEntity(Entity entity)
        {
            _remSet.Remove(entity);
            _addSet.Add(entity);
        }

        public static void RemoveEntity(Entity entity)
        {
            _addSet.Remove(entity);
            _remSet.Add(entity);
        }

        private static void LoadMap(string name)
        {
            Spatial.Clear();

            // Load map data (load phase)
            Map = Assets.GetMap(name);

            var tileset = Assets.GetTileSet("industrial");

            // Scan map data (generate phase)
            var groundLayer = Map.GetLayer("ground");
            foreach (var (x, y) in Rasterizer.Rectangle(Map.Size))
            {
                var tile = groundLayer.GetTile(x, y);
                if (tile == null) { continue; }
                else
                {
                    // Compute block position
                    var pos = new Vector(x, y) * (Vector) Map.TileSize;
                    var rec = new Rectangle(pos, Map.TileSize);

                    var soft = false;

                    if (tile.TileSet == tileset)
                    {
                        // 
                        if (tile.Id == 65 || tile.Id == 63 ||
                            tile.Id == 48 || tile.Id == 49 || tile.Id == 50)
                        {
                            rec.Height = 30;
                            soft = true;
                        }

                        // 
                        if (tile.Id == 64 || tile.Id == 62 ||
                            tile.Id == 34 || tile.Id == 35 || tile.Id == 37)
                        {
                            rec.Height = 20;
                            soft = true;
                        }
                    }

                    var block = new Block(rec, soft);

                    // 
                    Spatial.Add(block, block.Bounds);
                    _entities.Add(block);
                }
            }
        }

        private static void SetCursor(string name, Color color)
        {
            // Clone image
            var image = Assets.GetImage(name);
            var cursor = new Image(image.Size);

            // Copy pixels
            foreach (var (x, y) in Rasterizer.Rectangle(cursor.Size))
            {
                cursor.SetPixel(x, y, (Color) image.GetPixel(x, y) * color);
            }

            // Center of image
            cursor.Origin = image.Origin;
            Window.SetCursor(cursor);
        }

        public static Sprite LoadPlayerSprite()
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

        public static IEnumerable<T> QuerySpatial<T>(IShape shape)
            where T : ISpatialObject
        {
            return Spatial.Query(shape)
                          .Where(obj => obj is T)
                          .Cast<T>();
        }

        public static IEnumerable<T> FindEntities<T>() where T : Entity
        {
            return _entities.GetItemsByType<T>();
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            // Add/Remove entities
            foreach (var e in _remSet) { _entities.Remove(e); }
            foreach (var e in _addSet) { _entities.Add(e); }
            _remSet.Clear(); _addSet.Clear();

            // Update Entities
            foreach (var entity in _entities)
            {
                entity.Update(dt);
            }

            // Draw everything
            Draw(gfx, dt);
        }

        private static void Draw(Graphics gfx, float dt)
        {
            var stageHeight = Map.Height * Map.TileSize.Height;
            var stageWidth = Map.Width * Map.TileSize.Width;

            // "Camera"
            var offset = ((Vector) gfx.Surface.Size - (stageWidth, stageHeight)) / 2F;
            gfx.GlobalTransform = Matrix.CreateTranslation((IntVector) offset);

            // Draws the background image and frame
            DrawBackground(gfx);

            // Draws each map layer
            for (var i = 0; i < Map.LayerCount; i++)
            {
                var layer = Map.GetLayer(i);
                layer.Draw(gfx);
            }

            // Draws each entity
            DrawEntities(gfx, dt);
        }

        private static void DrawBackground(Graphics gfx)
        {
            var stageHeight = Map.Height * Map.TileSize.Height;
            var stageWidth = Map.Width * Map.TileSize.Width;

            gfx.PushState();
            {
                // Draw background (skybox)
                var backgroundRatio = Background.Width / (float) stageWidth;
                gfx.DrawImage(Background, Matrix.CreateScale(backgroundRatio));

                // Draw background (frame)
                gfx.Color = BackgroundColor;
                gfx.DrawRect((-stageWidth + 35, -stageHeight, stageWidth, stageHeight * 3));
                gfx.DrawRect((0, -stageHeight + 35, stageWidth, stageHeight));
                gfx.DrawRect((stageWidth - 35, -stageHeight, stageWidth, stageHeight * 3));
                gfx.DrawRect((0, stageHeight - 35, stageWidth, stageHeight));
            }
            gfx.PopState();
        }

        private static void DrawEntities(Graphics gfx, float dt)
        {
            // Draw Entities
            foreach (var entity in _entities)
            {
                gfx.PushState();
                entity.Draw(gfx, dt);
                gfx.PopState();
            }

            // Draw debug information
            DebugDrawEntities(gfx);
        }

        [Conditional("DEBUG")]
        private static void DebugDrawEntities(Graphics gfx)
        {
            // Debug Drawing for Entities
            foreach (var entity in _entities)
            {
                gfx.PushState();
                entity.DebugDraw(gfx);
                gfx.PopState();
            }
        }
    }
}
