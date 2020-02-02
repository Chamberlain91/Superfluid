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

        public static Player Player { get; private set; }

        public static BoundingTreeSpatialCollection<ISpatialObject> Spatial { get; private set; }

        public static Matrix ScreenToWorld { get; private set; }

        public static AudioSource Music;

        public static Color BackgroundColor = Color.Parse("#95A5A6");

        public static Image KillCursor;

        public static Image HealCursor;

        public static Image Background;

        private static HashSet<Entity> _addEntities, _removeEntities;
        private static TypeDictionary<Entity> _entities;

        private static float _elapsedTime;
        private static Vector _cameraPos;

        public static int StageIndex;
        public static string[] StageNames =
        {
            "stage4", // eileen be mean
            "stage1", // intro
            "stage2", // more
            "stage3", // tall
        };

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
                _removeEntities = new HashSet<Entity>();
                _addEntities = new HashSet<Entity>();

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

                // Load game assets
                Assets.LoadDatabase();
                Assets.PackAtlas();

                // Center origins on assets prefixed by string given
                Assets.SetImagesCenterOrigin("crosshair102", "particle");
                Assets.SetImagesCenterOrigin("alien", "slime"); // alienpink_walk1, etc

                // Setup the cursors
                var cursorImage = Assets.GetImage("crosshair102");
                HealCursor = CreateImageClone(cursorImage, Color.Green);
                KillCursor = CreateImageClone(cursorImage, Color.Red);
                Window.SetCursor(KillCursor); // ...

                // Load the background image
                Background = Assets.GetImage("colored_desert");

                // 
                _elapsedTime = 0;

                // Create the player actor
                Player = AddEntity(new Player());
                Player.Transform.Position = (200, 300);

                // Load the test map
                LoadMap(StageNames[StageIndex]);

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

            _removeEntities.Remove(entity);
            _addEntities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Schedules to remove an entity from the stage next frame.
        /// </summary>
        public static void RemoveEntity(Entity entity)
        {
            if (!_entities.Contains(entity) && !_addEntities.Contains(entity))
            {
                throw new InvalidOperationException($"Entity does not exist in scene.");
            }

            _addEntities.Remove(entity);
            _removeEntities.Add(entity);
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

        #region Load Map

        private static void LoadMap(string name)
        {
            // == Purge Existing Stage

            Spatial.Clear();
            Pipes.Clear();

            // Kill entities generated by loading map
            foreach (var o in FindEntities<Spawner>()) { RemoveEntity(o); }
            foreach (var o in FindEntities<Block>()) { RemoveEntity(o); }
            foreach (var o in FindEntities<Pipe>()) { RemoveEntity(o); }

            // == Load Phase

            // Get map data
            Map = Assets.GetMap(name);

            // Get Layers
            var groundLayer = Map.GetLayer("ground");
            var spawnLayer = Map.GetLayer("spawn");
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

                var spawnTile = spawnLayer.GetTile(x, y);
                if (spawnTile != null)
                {
                    var position = new Vector(x, y) * 70 + (35, 15);

                    if (spawnTile.Id == 80)
                    {
                        // Move player to start position
                        Player.Transform.Position = position;
                    }
                    else
                    {
                        // Create spawner
                        var spawner = AddEntity(new Spawner(count: 2, period: 6, () => new Slime()));
                        spawner.Transform.Position = position;
                    }
                }
            }

            // Detect initial pipe configuration
            Pipes.EvaluatePipeConfiguration();
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
                    tile.Id == 34 || tile.Id == 35 || tile.Id == 36)
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
                var isGreyPipe = !isGoldPipe && (tile.Id > 99); // THIS IS BAD BUT FUNCTIONAL

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
                var pipe = AddEntity(new Pipe(tile.Image, bounds, points, isGreyPipe, isGoldPipe));
                pipe.Transform.Position = position;
                pipe.ComputeWorldSpace();

                Spatial.Add(pipe, pipe.Bounds);
                Pipes.Add(pipe);
            }
        }

        #endregion

        private static Image CreateImageClone(Image image, Color blend)
        {
            var clone = new Image(image.Size);

            // Copy pixels
            foreach (var (x, y) in Rasterizer.Rectangle(image.Size))
            {
                clone.SetPixel(x, y, (Color) image.GetPixel(x, y) * blend);
            }

            // Copy origin
            clone.Origin = image.Origin;
            return clone;
        }

        public static IEnumerable<T> QuerySpatial<T>(IShape shape)
            where T : ISpatialObject
        {
            return Spatial.Query(shape)
                          .Where(obj => obj is T)
                          .Cast<T>();
        }

        public static T[] FindEntities<T>() where T : Entity
        {
            var items = _entities.GetItemsByType<T>();

            if (_addEntities.Count > 0)
            {
                items = _addEntities.Where(x => x is T)
                                    .Cast<T>()
                                    .Concat(items);
            }

            return items.ToArray();
        }

        public static void PlaySound(string name)
        {
            var src = new AudioSource(Assets.GetAudioClip(name));
            src.Play();
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            // Go to next stage, etc
            ProcessStageFlow(dt);

            // Update entities
            UpdateEntities(dt);

            // Draw everything
            Draw(gfx, dt);
        }

        private static void UpdateEntities(float dt)
        {
            // Add/Remove entities
            foreach (var e in _removeEntities) { _entities.Remove(e); }
            foreach (var e in _addEntities) { _entities.Add(e); }
            _removeEntities.Clear(); _addEntities.Clear();

            // Update Entities
            foreach (var entity in _entities)
            {
                entity.Update(dt);
            }
        }

        private static void ProcessStageFlow(float dt)
        {
            if (Pipes.IsComplete)
            {
                // Kill enemies and spawners
                foreach (var o in FindEntities<Spawner>()) { RemoveEntity(o); }
                foreach (var o in FindEntities<Enemy>()) { RemoveEntity(o); }

                // 
                if (Input.GetKeyDown(Key.Enter))
                {
                    StageIndex++;
                    if (StageIndex >= StageNames.Length)
                    {
                        // TODO: remove this demo style reset
                        _elapsedTime = 0;
                        StageIndex = 0;
                    }

                    // Load next stage
                    LoadMap(StageNames[StageIndex]);
                }
            }
            else
            {
                // Accumulate time only when stage is "active"
                _elapsedTime += dt;
            }
        }

        private static void Draw(Graphics gfx, float dt)
        {
            gfx.PushState();
            {
                // 
                ComputeAndApplyCamera(gfx, dt);

                // Draws the background image and frame
                DrawBackground(gfx);

                // Draws each map layer
                var foregroundLayer = Map.GetLayer("foreground");
                var backgroundLayer = Map.GetLayer("background");
                var groundLayer = Map.GetLayer("ground");
                var spawnLayer = Map.GetLayer("spawn");

                // Draw background
                backgroundLayer.Draw(gfx);

                // Draw Entity Back (ie. Pipes...)
                DrawEntities(gfx, dt, EntityLayer.Back);

                // Draw spawn layer
                spawnLayer.Draw(gfx);

                // Draw ground
                groundLayer.Draw(gfx);

                // Draws Entity Front (ie, Player, Sparks...)
                DrawEntities(gfx, dt, EntityLayer.Front);

                // Draw foreground
                foregroundLayer.Draw(gfx);
            }
            gfx.PopState();

            // Draw HUD
            var timeStr = Time.GetEnglishTime(_elapsedTime);
            if (Pipes.IsComplete) { timeStr += " [Press Enter To Go Next Stage]"; }
            DrawLabel(gfx, timeStr, (10, 10));

            gfx.Color = Color.Black;
            var credit = "Music is \"Pixelland\" by Kevin MacLeod (https://incompetech.com)";
            gfx.DrawText(credit, (gfx.Surface.Width - 10, gfx.Surface.Height - 20), Font.Default, 16, TextAlign.Right);
        }

        private static void ComputeAndApplyCamera(Graphics gfx, float dt)
        {
            // Animate camera smoothly to player position
            _cameraPos = Vector.Lerp(_cameraPos, Player.Bounds.Center, 5 * dt);
            _cameraPos = Vector.Round(_cameraPos);

            // Compute and set the "Camera"
            var cameraCenterOffset = ((Vector) gfx.Surface.Size) / 2F;
            var cameraMatrix = Matrix.CreateTranslation((IntVector) cameraCenterOffset - _cameraPos);
            ScreenToWorld = Matrix.Inverse(cameraMatrix);
            gfx.GlobalTransform = cameraMatrix;
        }

        private static void DrawLabel(Graphics gfx, string str, Vector pos)
        {
            // Computes bounding rectangle for label background
            var rect = TextLayout.Measure(str, Font.Default, 32);
            rect.Offset(pos);
            rect.Inflate(8);

            // Draw label background
            gfx.Color = FlatColors.MidnightBlue;
            gfx.DrawRect(rect);

            // Draw text
            gfx.Color = FlatColors.Emerald;
            gfx.DrawText(str, pos, Font.Default, 32);
        }

        private static void DrawBackground(Graphics gfx)
        {
            var stageHeight = Map.Height * Map.TileSize.Height;
            var stageWidth = Map.Width * Map.TileSize.Width;

            gfx.PushState();
            {
                // Draw background (skybox)
                var stageAspect = stageWidth / (float) stageHeight;

                // Computes a paralax...?
                var ox = _cameraPos.X / stageWidth;
                var oy = _cameraPos.Y / stageHeight;
                var backgroundOffset = new Vector(ox, oy) * 60 - (30, 30);

                float backgroundScaling;
                if (stageAspect > 1) { backgroundScaling = stageWidth / (float) Background.Width; }
                else { backgroundScaling = stageHeight / (float) Background.Height; }

                gfx.DrawImage(Background, Matrix.CreateTransform(backgroundOffset, 0, backgroundScaling));

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
