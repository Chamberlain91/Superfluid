using System.Collections.Generic;

using Heirloom.Math;

namespace Superfluid.Engine
{
    public sealed class Transform
    {
        // Components in local space
        private Vector _localPosition, _localScale = Vector.One;
        private Vector _localDirection;
        private float _localRotation;

        // Components in world space
        private Vector _worldPosition; // no world scale
        private Vector _worldDirection;
        private float _worldRotation;

        // Matrices
        private Matrix _localToWorld; // local -> world
        private Matrix _worldToLocal; // world -> local

        // Parent hierarchy
        private readonly List<Transform> _children = new List<Transform>();
        private Transform _parent;

        // Dirty flags
        private bool _dirtyWorldToLocal = true;
        private bool _dirtyLocalToWorld = true;
        private bool _dirtyWorldPosition = true;
        private bool _dirtyWorldRotation = true;
        private bool _dirtyLocalDirection = true;
        private bool _dirtyWorldDirection = true;

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
            _localPosition = position;
            _localRotation = rotation;
            _localScale = scale;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Associated children transform objects.
        /// </summary>
        public IReadOnlyList<Transform> Children => _children;

        /// <summary>
        /// Does this transform have an assigned parent?
        /// </summary>
        public bool HasParent => _parent != null;

        /// <summary>
        /// Gets the parent of this transform.
        /// </summary>
        public Transform Parent => _parent;

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
                    _localToWorld = Matrix.CreateTransform(LocalPosition, LocalRotation, LocalScale);

                    // If we have a parent, combine with parent transform
                    if (HasParent) { _localToWorld *= _parent.Matrix; }

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

                    // ...? Move back into space relative to parent...?
                    if (HasParent) { _worldToLocal *= _parent.Matrix; }

                    // We are no longer dirty (world to local)
                    _dirtyWorldToLocal = false;
                }

                return _worldToLocal;
            }
        }

        /// <summary>
        /// Gets or sets the position in world space.
        /// </summary>
        public Vector WorldPosition
        {
            get
            {
                if (_dirtyWorldPosition)
                {
                    // Compute world position
                    if (HasParent) { _worldPosition = Matrix * _localPosition; }
                    else { _worldPosition = _localPosition; }

                    // 
                    _dirtyWorldPosition = false;
                }

                return _worldPosition;
            }

            set
            {
                if (HasParent) { LocalPosition = InverseMatrix * value; }
                else { LocalPosition = value; }
            }
        }

        /// <summary>
        /// Gets or sets the rotation in world space.
        /// </summary>
        public float WorldRotation
        {
            get
            {
                if (_dirtyWorldRotation)
                {
                    // Compute world rotation
                    if (HasParent) { _worldRotation = _parent.WorldRotation + _localRotation; }
                    else { _worldRotation = _localRotation; }

                    // 
                    _dirtyWorldRotation = false;
                }

                return _worldRotation;
            }

            set
            {
                if (HasParent) { LocalRotation = _parent.WorldRotation + value; }
                else { LocalRotation = value; }
            }
        }

        /// <summary>
        /// Gets or sets the direction vector in world space.
        /// </summary>
        public Vector WorldDirection
        {
            get
            {
                if (_dirtyWorldDirection)
                {
                    _worldDirection = Vector.FromAngle(WorldRotation);
                    _dirtyWorldDirection = false;
                }

                return _worldDirection;
            }

            set => WorldRotation = value.Angle;
        }

        /// <summary>
        /// Gets or sets the position in local space.
        /// </summary>
        public Vector LocalPosition
        {
            get => _localPosition;

            set
            {
                _localPosition = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// Gets or sets the rotation in local space.
        /// </summary>
        public float LocalRotation
        {
            get => _localRotation;

            set
            {
                _localRotation = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// Gets or sets the direction vector in local space.
        /// </summary>
        public Vector LocalDirection
        {
            get
            {
                if (_dirtyLocalDirection)
                {
                    _localDirection = Vector.FromAngle(LocalRotation);
                    _dirtyLocalDirection = false;
                }

                return _localDirection;
            }

            set
            {
                LocalRotation = value.Angle;
            }
        }

        /// <summary>
        /// Gets or sets the scale in local space.
        /// </summary>
        public Vector LocalScale
        {
            get => _localScale;

            set
            {
                _localScale = value;
                MarkDirty();
            }
        }

        #endregion

        /// <summary>
        /// Assign (or remove) the parent object.
        /// </summary>
        public void SetParent(Transform parent)
        {
            // Different parent than before
            if (_parent != null && parent != _parent)
            {
                // Remove this transform from the parent children
                _parent._children.Remove(this);
            }

            // Record the new parent
            _parent = parent;

            // If a valid parent, add this transform to parent children
            _parent?._children.Add(this);

            // We need to recompute because we have a new basis
            MarkDirty();
        }

        private void MarkDirty()
        {
            // Mark this transform as dirty
            _dirtyLocalDirection = true;
            _dirtyWorldDirection = true;
            _dirtyWorldPosition = true;
            _dirtyWorldRotation = true;
            _dirtyLocalToWorld = true;
            _dirtyWorldToLocal = true;

            // Mark all children as dirty (recursively)
            foreach (var child in _children)
            {
                child.MarkDirty();
            }
        }

        #region Conversion Operators

        public static implicit operator Matrix(Transform transform)
        {
            return transform.Matrix;
        }

        #endregion
    }
}
