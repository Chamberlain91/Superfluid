using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Heirloom.Collections;
using Heirloom.Collections.Spatial;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Drawing.Extras;
using Heirloom.IO;
using Heirloom.Math;
using Heirloom.Sound;
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

        public static Matrix ScreenToWorld;

        public static Color BackgroundColor = Color.Parse("#95A5A6");

        public static AudioSource BackgroundMusic;

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

                // Load BGM
                BackgroundMusic = new AudioSource(Files.OpenStream("assets/music/4222-pixelland-by-kevin-macleod.mp3"));
                BackgroundMusic.IsLooping = true;
                BackgroundMusic.Play();

                /*
                 * Music from https://filmmusic.io
                 * "Pixelland" by Kevin MacLeod(https://incompetech.com)
                 * License: CC BY(http://creativecommons.org/licenses/by/4.0/)
                 */

                // Load game assets
                Assets.LoadDatabase();
                Assets.PackAtlas();

                // Center origins on assets prefixed by string given
                Assets.SetImagesCenterOrigin("crosshair102", "particle");
                Assets.SetImagesCenterOrigin("alien", "slime"); // alienpink_walk1, etc

                // Sets the cursor
                SetCursor("crosshair102", Color.Pink);

                // Load the background image
                Background = Assets.GetImage("colored_desert");

                // Load the test map
                LoadMap("testmap");

                // Create the player actor
                var player = AddEntity(new Player());
                player.Transform.Position = (200, 300);

                // Create a test slime
                var slime = AddEntity(new Slime());
                slime.Transform.Position = (800, 300);

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
        }

        public static T AddEntity<T>(T entity) where T : Entity
        {
            _remSet.Remove(entity);
            _addSet.Add(entity);
            return entity;
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

            // Load TileSets
            var industrialTileset = Assets.GetTileSet("industrial");
            var pipeTileset = Assets.GetTileSet("pipes");

            // Load Layers
            var groundLayer = Map.GetLayer("ground");
            var pipeLayer = Map.GetLayer("pipes");

            // Scan ground layer map data (generate phase)
            foreach (var (x, y) in Rasterizer.Rectangle(Map.Size))
            {
                var tile = groundLayer.GetTile(x, y);
                if (tile == null) { continue; }
                else
                {
                    // Compute block position
                    var pos = new Vector(x, y) * (Vector) Map.TileSize;
                    var rec = new Rectangle(pos, Map.TileSize);

                    var isOneWay = false;

                    if (tile.TileSet == industrialTileset)
                    {
                        // 
                        if (tile.Id == 65 || tile.Id == 63 ||
                            tile.Id == 48 || tile.Id == 49 || tile.Id == 50)
                        {
                            rec.Height = 30;
                            isOneWay = true;
                        }

                        // 
                        if (tile.Id == 64 || tile.Id == 62 ||
                            tile.Id == 34 || tile.Id == 35 || tile.Id == 37)
                        {
                            rec.Height = 20;
                            isOneWay = true;
                        }
                    }

                    var block = AddEntity(new Block(rec, isOneWay));
                    Spatial.Add(block, block.Bounds);
                }
            }

            // Scan ground layer map data (generate phase)
            foreach (var (x, y) in Rasterizer.Rectangle(Map.Size))
            {
                var tile = pipeLayer.GetTile(x, y);
                if (tile == null)
                { continue; }
                else
                {
                    var rect = new Rectangle(Vector.Zero, Map.TileSize);

                    // Offsets for pipe openings
                    var off1 = new IntVector();
                    var off2 = new IntVector();
                    var gotOffset = false; // don't judge me lol
                    var gold = false;

                    if (tile.TileSet == pipeTileset)
                    {
                        // vertical pipe
                        if (tile.Id == 88 || tile.Id == 100 || tile.Id == 106)
                        {
                            off1.Set(0, -2); // bottom 
                            off2.Set(0, 1);  // top
                            gotOffset = true;

                            // vertical pipes are 1x2
                            rect.Size = (Size) ((Vector) rect.Size * (1, 2));

                            if (tile.Id == 106)
                            {
                                gold = true;
                            }
                        }

                        // horizontal pipe
                        if (tile.Id == 89 || tile.Id == 101 || tile.Id == 107)
                        {
                            off1.Set(-1, 0); // left
                            off2.Set(2, 0);  // right
                            gotOffset = true;

                            // vertical pipes are 1x2
                            rect.Size = (Size) ((Vector) rect.Size * (2, 1));

                            if (tile.Id == 106)
                            {
                                gold = true;
                            }
                        }

                        // == Curved Pipes

                        var TR = tile.Id == 90 || tile.Id == 102;
                        var TL = tile.Id == 91 || tile.Id == 103;
                        var BL = tile.Id == 92 || tile.Id == 104;
                        var BR = tile.Id == 93 || tile.Id == 105;

                        if (TR)
                        {
                            // Top Left to Bottom Right
                            off1.Set(-1, -1);
                            off2.Set(1, 1);
                            gotOffset = true;
                        }

                        if (TL)
                        {
                            // Bottom Left to Top Right (upside down L)
                            off1.Set(0, 1);
                            off2.Set(2, -1);
                            gotOffset = true;
                        }

                        if (BL)
                        {
                            // Top Left to bottom Right (L)
                            off1.Set(0, -2);
                            off2.Set(2, 0);
                            gotOffset = true;
                        }

                        if (BR)
                        {
                            // Bottom Left to Top Right (Backwards L)
                            off1.Set(-1, 0);
                            off2.Set(1, -2);
                            gotOffset = true;
                        }

                        // Corner pipes are 2x2
                        if (TR || TL || BL || BR)
                        {
                            rect.Size *= 2;
                        }
                    }

                    if (gotOffset)
                    {
                        // Compute Block Position
                        var pos = new Vector(x, y) * (Vector) Map.TileSize;
                        pos.Y += Map.TileSize.Height - tile.Image.Height;

                        var pipe = AddEntity(new Pipe(tile.Image, rect, off1, off2, gold));
                        pipe.Transform.Position = pos;
                        pipe.ComputeWorldBounds();

                        Spatial.Add(pipe, pipe.Bounds);
                    }
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

            // Compute and set the "Camera"
            var cameraCenterOffset = ((Vector) gfx.Surface.Size - (stageWidth, stageHeight)) / 2F;
            var cameraMatrix = Matrix.CreateTranslation((IntVector) cameraCenterOffset);
            ScreenToWorld = Matrix.Inverse(cameraMatrix);
            gfx.GlobalTransform = cameraMatrix;

            // Draws the background image and frame
            DrawBackground(gfx);

            // Draws each map layer
            var foregroundLayer = Map.GetLayer("foreground");
            var backgroundLayer = Map.GetLayer("background");
            var groundLayer = Map.GetLayer("ground");

            // Draw background
            backgroundLayer.Draw(gfx);

            // Draw Entity Back (ie. Pipes...)
            DrawEntities(gfx, dt, EntityLayer.Back);
            // Map.GetLayer("pipes").Draw(gfx); // todo: remove when the entities exist

            // Draw ground
            groundLayer.Draw(gfx);

            // Draws Entity Front (ie, Player, Sparks...)
            DrawEntities(gfx, dt, EntityLayer.Front);

            // Draw foreground
            foregroundLayer.Draw(gfx);
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

        private static void DrawEntities(Graphics gfx, float dt, EntityLayer layer)
        {
            // Draw Entities
            foreach (var entity in _entities.Where(e => e.Layer == layer))
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
