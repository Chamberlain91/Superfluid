using System;
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

        public static PipeManager Pipes { get; private set; }

        public static TileMap Map { get; private set; }

        public static BoundingTreeSpatialCollection<ISpatialObject> Spatial { get; private set; }

        public static Matrix ScreenToWorld { get; private set; }

        public static AudioSource Music;

        public static Color BackgroundColor = Color.Parse("#95A5A6");

        public static Image Background;

        private static HashSet<Entity> _addSet, _remSet;
        private static TypeDictionary<Entity> _entities;

        private static void Main(string[] args)
        {
            Application.Run(() =>
            {
                // Create spatial collection
                Spatial = new BoundingTreeSpatialCollection<ISpatialObject>();

                // 
                Pipes = new PipeManager();

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
                Music = new AudioSource(Files.OpenStream("assets/music/4222-pixelland-by-kevin-macleod.mp3"));
                Music.IsLooping = true;
                Music.Play();

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

        /// <summary>
        /// Schedules to insert an entity into the stage next frame.
        /// </summary>
        public static T AddEntity<T>(T entity) where T : Entity
        {
            if (_entities.Contains(entity))
            {
                throw new InvalidOperationException($"Entity already exists in scene.");
            }

            _remSet.Remove(entity);
            _addSet.Add(entity);
            return entity;
        }

        /// <summary>
        /// Schedules to remove an entity from the stage next frame.
        /// </summary>
        public static void RemoveEntity(Entity entity)
        {
            if (!_entities.Contains(entity))
            {
                throw new InvalidOperationException($"Entity does not exist in scene.");
            }

            _addSet.Remove(entity);
            _remSet.Add(entity);
        }

        /// <summary>
        /// Gets the pipe at the specified location.
        /// </summary>
        public static Pipe GetPipe(Vector position)
        {
            var circle = new Circle(position, 10);
            return Game.QuerySpatial<Pipe>(circle)
                       .FirstOrDefault();
        }

        private static void LoadMap(string name)
        {
            // == Purge Existing Stage

            Spatial.Clear();
            Pipes.Clear();

            // == Load Phase

            // Get map data
            Map = Assets.GetMap(name);

            // Get Layers
            var groundLayer = Map.GetLayer("ground");
            var pipeLayer = Map.GetLayer("pipes");

            // Scan map data (generate phase)
            foreach (var (x, y) in Rasterizer.Rectangle(Map.Size))
            {
                var groundTile = groundLayer.GetTile(x, y);
                if (groundTile != null)
                {
                    LoadMapProcessGroundTiles(x, y, groundTile);
                }

                var pipeTile = pipeLayer.GetTile(x, y);
                if (pipeTile != null)
                {
                    LoadMapProcessPipesTiles(x, y, pipeTile);
                }
            }

            // Detect initial pipe configuration
            Pipes.DetectPipeConnections();
        }

        private static void LoadMapProcessGroundTiles(int x, int y, Tile tile)
        {
            // Compute block position
            var position = new Vector(x, y) * (Vector) Map.TileSize;
            position.Y += Map.TileSize.Height - tile.Image.Height; // weird tiled offset thing

            var bounds = new Rectangle(position, Map.TileSize);

            var isOneWay = false;

            // Is this tile a industrial tile?
            if (tile.TileSet == Assets.GetTileSet("industrial"))
            {
                // One way thick
                if (tile.Id == 65 || tile.Id == 63 ||
                    tile.Id == 48 || tile.Id == 49 || tile.Id == 50)
                {
                    bounds.Height = 30;
                    isOneWay = true;
                }

                // One way thin
                if (tile.Id == 64 || tile.Id == 62 ||
                    tile.Id == 34 || tile.Id == 35 || tile.Id == 37)
                {
                    bounds.Height = 20;
                    isOneWay = true;
                }
            }

            // Generate block
            var block = AddEntity(new Block(bounds, isOneWay));
            Spatial.Add(block, block.Bounds);
        }

        private static void LoadMapProcessPipesTiles(int x, int y, Tile tile)
        {
            // Is this a pipes tile?
            if (tile.TileSet == Assets.GetTileSet("pipes"))
            {
                // Offsets for pipe openings
                var offset1 = new Vector();
                var offset2 = new Vector();

                var bounds = new Rectangle(Vector.Zero, Map.TileSize);

                // == Straight Pipes

                var VP = tile.Id == 88 || tile.Id == 100;
                var VG = tile.Id == 106; // vertical gold

                var HP = tile.Id == 89 || tile.Id == 101;
                var HG = tile.Id == 107; // horizontal gold

                // Vertical pipe
                if (VP || VG)
                {
                    offset1.Set(0, -1);
                    offset2.Set(0, +2);

                    // vertical pipes are 1x2
                    bounds.Size = (Size) ((Vector) bounds.Size * (1, 2));
                }

                // Horizontal pipe
                if (HP || HG)
                {
                    offset1.Set(-1, 0);
                    offset2.Set(+2, 0);

                    // vertical pipes are 1x2
                    bounds.Size = (Size) ((Vector) bounds.Size * (2, 1));
                }

                // Is this a gold (input/output) pipe?
                var isGoldPipe = HG || VG;

                // == Corner Pipes

                var TR = tile.Id == 90 || tile.Id == 102;
                var TL = tile.Id == 91 || tile.Id == 103;
                var BL = tile.Id == 92 || tile.Id == 104;
                var BR = tile.Id == 93 || tile.Id == 105;

                if (TR)
                {
                    // Corner elbow is top-right
                    offset1.Set(-1, 0);
                    offset2.Set(+1, 2);
                }

                if (TL)
                {
                    // Corner elbow is top-left
                    offset1.Set(0, 2);
                    offset2.Set(2, 0);
                }

                if (BL)
                {
                    // Corner elbow is bottom-left
                    offset1.Set(0, -1);
                    offset2.Set(2, +1);
                }

                if (BR)
                {
                    // Corner elbow is bottom-right
                    offset1.Set(-1, +1);
                    offset2.Set(+1, -1);
                }

                // Corner pipes are 2x2
                if (TR || TL || BL || BR)
                {
                    bounds.Size *= 2;
                }

                // Compute Block Position
                var position = new Vector(x, y) * (Vector) Map.TileSize;
                position.Y += Map.TileSize.Height - tile.Image.Height; // weird tiled offset thing

                // Compute connection points in world space
                var points = new[] { offset1, offset2 }.Select(s => (35, 35) + (s * 70));

                // Generate pipe
                var pipe = AddEntity(new Pipe(tile.Image, bounds, points, isGoldPipe));
                pipe.Transform.Position = position;
                pipe.ComputeWorldSpace();

                Spatial.Add(pipe, pipe.Bounds);
                Pipes.Add(pipe);
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
