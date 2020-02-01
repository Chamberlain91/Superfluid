using System.Collections.Generic;

using Heirloom.Math;

namespace Superfluid.Engine
{
    public sealed class Transform
    {
        // Components in local space
        private Vector _position, _scale = Vector.One;
        private Vector _direction;
        private float _rotation;

        // Matrices
        private Matrix _localToWorld; // local -> world
        private Matrix _worldToLocal; // world -> local

        // Dirty flags
        private bool _dirtyWorldToLocal = true;
        private bool _dirtyLocalToWorld = true;
        private bool _dirtyDirection = true;

        #region Constructor

        /// <summary>
        /// Constructs a new transform (identity).
        /// </summary>
        public Transform()
            : this(Vector.Zero, 0, Vector.One)
        { }

        /// <summary>
        /// Constructs a new transform.
        /// </summary>
        public Transform(Vector position)
            : this(position, 0, Vector.One)
        { }

        /// <summary>
        /// Constructs a new transform.
        /// </summary>
        public Transform(Vector position, float rotation)
            : this(position, rotation, Vector.One)
        { }

        /// <summary>
        /// Constructs a new transform.
        /// </summary>
        public Transform(Vector position, Vector scale)
            : this(position, 0, scale)
        { }

        /// <summary>
        /// Constructs a new transform.
        /// </summary>
        public Transform(Vector position, float rotation, Vector scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the local to world matrix.
        /// </summary>
        public Matrix Matrix
        {
            get
            {
                if (_dirtyLocalToWorld)
                {
                    // Compute local transform
                    _localToWorld = Matrix.CreateTransform(Position, Rotation, Scale);

                    // We are no longer dirty (local to world)
                    _dirtyLocalToWorld = false;
                }

                return _localToWorld;
            }
        }

        /// <summary>
        /// Gets the world to local matrix.
        /// </summary>
        public Matrix InverseMatrix
        {
            get
            {
                if (_dirtyWorldToLocal)
                {
                    // Computes the inverse of the local to world matrix
                    // Note: Using the property form here is important for order of operations
                    _worldToLocal = Matrix.Inverse(Matrix);

                    // We are no longer dirty (world to local)
                    _dirtyWorldToLocal = false;
                }

                return _worldToLocal;
            }
        }

        /// <summary>
        /// Gets or sets the position in local space.
        /// </summary>
        public Vector Position
        {
            get => _position;

            set
            {
                _position = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// Gets or sets the rotation in local space.
        /// </summary>
        public float Rotation
        {
            get => _rotation;

            set
            {
                _rotation = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// Gets or sets the direction vector in local space.
        /// </summary>
        public Vector Direction
        {
            get
            {
                if (_dirtyDirection)
                {
                    _direction = Vector.FromAngle(Rotation);
                    _dirtyDirection = false;
                }

                return _direction;
            }

            set => Rotation = value.Angle;
        }

        /// <summary>
        /// Gets or sets the scale in local space.
        /// </summary>
        public Vector Scale
        {
            get => _scale;

            set
            {
                _scale = value;
                MarkDirty();
            }
        }

        #endregion

        private void MarkDirty()
        {
            _dirtyLocalToWorld = true;
            _dirtyWorldToLocal = true;
            _dirtyDirection = true;
        }

        #region Conversion Operators

        public static implicit operator Matrix(Transform transform)
        {
            return transform.Matrix;
        }

        #endregion
    }
}
