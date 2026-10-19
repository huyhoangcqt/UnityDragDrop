using UnityEngine;

namespace MyPackage.DragDrop
{
    // Pure math: no input, no scene objects. All values share one 2D space (the drag area's local space).
    public sealed class DragConstraint
    {
        private readonly DragAxis _axis;
        private readonly Rect? _bounds;
        private readonly float _lockThreshold;

        private Vector2 _origin;
        private DragAxis _resolvedAxis;

        public DragConstraint(DragAxis axis, Rect? bounds, float lockThreshold = 5f)
        {
            _axis = axis;
            _bounds = bounds;
            _lockThreshold = Mathf.Max(0f, lockThreshold);
            Begin(Vector2.zero);
        }

        public DragAxis ResolvedAxis => _resolvedAxis;

        public void Begin(Vector2 origin)
        {
            _origin = origin;
            _resolvedAxis = _axis;
        }

        // minOffset/maxOffset: the dragged object's extents relative to its position.
        public Vector2 Apply(Vector2 desired, Vector2 minOffset, Vector2 maxOffset)
        {
            Vector2 position = ApplyAxis(desired);

            if (_bounds.HasValue)
            {
                position = ClampToBounds(position, _bounds.Value, minOffset, maxOffset);
            }

            return position;
        }

        public bool IsOutside(Vector2 position, Vector2 minOffset, Vector2 maxOffset)
        {
            return _bounds.HasValue && ClampToBounds(position, _bounds.Value, minOffset, maxOffset) != position;
        }

        private Vector2 ApplyAxis(Vector2 desired)
        {
            if (_resolvedAxis == DragAxis.Auto)
            {
                Vector2 delta = desired - _origin;
                if (Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) < _lockThreshold)
                {
                    return _origin;
                }

                _resolvedAxis = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? DragAxis.Horizontal : DragAxis.Vertical;
            }

            switch (_resolvedAxis)
            {
                case DragAxis.Horizontal:
                    desired.y = _origin.y;
                    break;
                case DragAxis.Vertical:
                    desired.x = _origin.x;
                    break;
            }

            return desired;
        }

        private static Vector2 ClampToBounds(Vector2 position, Rect bounds, Vector2 minOffset, Vector2 maxOffset)
        {
            float minX = bounds.xMin - minOffset.x;
            float maxX = bounds.xMax - maxOffset.x;
            float minY = bounds.yMin - minOffset.y;
            float maxY = bounds.yMax - maxOffset.y;

            // The object is larger than the bounds on this axis: center it.
            position.x = minX > maxX ? bounds.center.x : Mathf.Clamp(position.x, minX, maxX);
            position.y = minY > maxY ? bounds.center.y : Mathf.Clamp(position.y, minY, maxY);
            return position;
        }
    }
}
