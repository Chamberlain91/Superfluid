using System.Collections.Generic;
using System.Linq;

using Heirloom.Math;

using Superfluid.Engine;
using Superfluid.Entities;

namespace Superfluid
{
    public class PipeManager
    {
        private readonly List<Pipe> _pipes = new List<Pipe>();

        public void Clear()
        {
            _pipes.Clear();
        }

        public void Add(Pipe pipe)
        {
            _pipes.Add(pipe);
        }

        public void Remove(Pipe pipe)
        {
            _pipes.Remove(pipe);
        }

        public void EvaluatePipeConfiguration()
        {
            // 
            DetectPipeConnections();

            // 
            if (CheckCompleteConnection(out var pipes))
            {
                foreach (var pipe in pipes)
                {
                    pipe.IsFunctional = true;
                }
            }
        }

        /// <summary>
        /// Evaluates the pipes to update their connections with each other.
        /// </summary>
        private void DetectPipeConnections()
        {
            // Reset previously known connections
            foreach (var pipe in _pipes)
            {
                pipe.Connections.Clear();
                pipe.IsFunctional = false;
            }

            // For each pipe
            for (var i = 0; i < _pipes.Count; i++)
            {
                var a = _pipes[i];

                // For each other pipe
                for (var j = i + 1; j < _pipes.Count; j++)
                {
                    var b = _pipes[j];

                    // Test first pipe connections inside second
                    if (CheckConnection(a, b) && CheckConnection(b, a))
                    {
                        a.Connections.Add(b);
                        b.Connections.Add(a);
                    }
                }
            }

            static bool CheckConnection(Pipe source, Pipe target)
            {
                // Broken pipes, no good
                if (source.Health < 1) { return false; }
                if (target.Health < 1) { return false; }

                // 
                foreach (var connectionPoint in source.GetValidConnectionPoints())
                {
                    if (target.Bounds.ContainsPoint(connectionPoint))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Checks if the golden pipes can see each other (thus a completed connection).
        /// </summary>
        private bool CheckCompleteConnection(out IEnumerable<Pipe> pipes)
        {
            var gold = _pipes.First(p => p.IsGoldPipe);

            // can make static for better memory use
            var visited = new HashSet<Pipe>();
            var frontier = new Queue<Pipe>();
            frontier.Enqueue(gold);

            // While we have more in the frontier
            while (frontier.Count > 0)
            {
                var pipe = frontier.Dequeue();

                // Is this the destination?
                if (pipe.IsGoldPipe && pipe != gold)
                {
                    // gold can see gold
                    pipes = visited;
                    return true;
                }
                else
                {
                    // For each neighbor
                    foreach (var neighbor in pipe.Connections)
                    {
                        // Add to visited set, if newly visited...
                        if (visited.Add(neighbor))
                        {
                            // ...add to frontier
                            frontier.Enqueue(neighbor);
                        }
                    }
                }
            }

            // Was not able to see another golden pipe
            pipes = null;
            return false;
        }

        public bool Pickup(Vector position, ref Pipe pocketPipe)
        {
            var fieldPipe = Game.GetPipe(position);

            // Both locations are empty, can't do anything
            if (fieldPipe == null && pocketPipe == null) { return false; }

            // Field was not null (pickup)
            if (fieldPipe != null)
            {
                // Pocket is full, unable to pick up
                if (pocketPipe != null) { return false; }

                // Can't swap grey or gold pipes
                if (fieldPipe.IsGreyPipe) { return false; }
                if (fieldPipe.IsGoldPipe) { return false; }

                // Remove it from the stage
                Game.Spatial.Remove(fieldPipe);
                Game.RemoveEntity(fieldPipe);
            }

            // Swap...
            Calc.Swap(ref fieldPipe, ref pocketPipe);

            // Field can be null on pickup vs deposit
            if (fieldPipe != null)
            {
                // Position and update world information
                fieldPipe.Transform.Position = Input.GetGridMousePosition();
                fieldPipe.ComputeWorldSpace();

                // Insert into stage
                Game.Spatial.Add(fieldPipe, fieldPipe.Bounds);
                Game.AddEntity(fieldPipe);
            }

            EvaluatePipeConfiguration();

            // Pickup complete
            return true;
        }
    }
}
