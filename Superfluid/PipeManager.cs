using System.Collections.Generic;
using System.Linq;
using Heirloom.Math;

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

        /// <summary>
        /// Evaluates the pipes to update their connections with each other.
        /// </summary>
        public void DetectPipeConnections()
        {
            // Reset previously known connections
            foreach (var pipe in _pipes)
            {
                pipe.Connections.Clear();
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
        public bool CheckCompleteConnection()
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
            return false;
        }

        public bool Pickup(Vector position, ref Pipe pocketPipe)
        {
            var fieldPipe = Game.GetPipe(position);

            // Both locations are empty, can't do anything
            if (fieldPipe == null && pocketPipe == null) { return false; }

            // todo: check exchange is valid
            // todo: remove pipe from spatial/structure
            // todo: exchange pipes (assign to ref)
            // todo: add pipe to spatial/structure

            // false negative, just to compile
            // todo: implement for real
            return false;
        }
    }
}
