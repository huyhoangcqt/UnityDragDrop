using UnityEngine;

namespace MyPackage.DragDrop
{
    // Plain C# service, callable from any script. It owns the clone and the hidden original item,
    // and never reads input itself: feed it screen points from any source.
    public sealed class DragDropService
    {
        private DragOptions _options;
        private DragConstraint _constraint;
        private RectTransform _clone;
        private CanvasGroup _itemGroup;
        private bool _addedGroup;
        private float _originalAlpha;
        private bool _originalInteractable;
        private bool _originalBlocksRaycasts;
        private Vector2 _origin;
        private Vector2 _pointerOffset;
        private Vector2 _minOffset;
        private Vector2 _maxOffset;
        private Vector2 _localPosition;
        private float _localZ;

        public bool IsDragging { get; private set; }

        public RectTransform Clone => _clone;

        public bool Begin(DragOptions options, Vector2 screenPoint, Camera eventCamera)
        {
            if (IsDragging || !Validate(options))
            {
                return false;
            }

            Canvas canvas = options.Item.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("The dragged item must be inside a Canvas.", options.Item);
                return false;
            }

            if (!TryGetLocalPoint(options.Area, screenPoint, eventCamera, out Vector2 pointerLocal))
            {
                return false;
            }

            _options = options;
            _clone = CreateClone(options, canvas.rootCanvas.transform);

            Vector3 pivotLocal = options.Area.InverseTransformPoint(options.Item.position);
            _origin = pivotLocal;
            _localZ = pivotLocal.z;
            _localPosition = _origin;
            _pointerOffset = _origin - pointerLocal;
            CalculateOffsets();

            Rect bounds = options.Bounds ?? options.Area.rect;
            _constraint = new DragConstraint(options.Axis, bounds, options.LockThreshold);
            _constraint.Begin(_origin);

            HideItem(options.Item);
            IsDragging = true;
            return true;
        }

        public void Move(Vector2 screenPoint, Camera eventCamera)
        {
            if (!IsDragging || _clone == null)
            {
                return;
            }

            if (!TryGetLocalPoint(_options.Area, screenPoint, eventCamera, out Vector2 pointerLocal))
            {
                return;
            }

            _localPosition = _constraint.Apply(pointerLocal + _pointerOffset, _minOffset, _maxOffset);
            _clone.position = _options.Area.TransformPoint(new Vector3(_localPosition.x, _localPosition.y, _localZ));
        }

        public DragResult End()
        {
            if (!IsDragging)
            {
                return null;
            }

            IsDragging = false;
            var result = new DragResult(
                _options.Item,
                _clone,
                _options.Area,
                _origin,
                _localPosition,
                _itemGroup,
                _addedGroup,
                _originalAlpha,
                _originalInteractable,
                _originalBlocksRaycasts);

            _options = null;
            _clone = null;
            _itemGroup = null;
            return result;
        }

        public void Abort()
        {
            End()?.Cancel();
        }

        // The target's bounds in the area's local space, handy for building DragOptions.Bounds.
        public static Rect GetLocalRect(RectTransform area, RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Vector2 min = area.InverseTransformPoint(corners[0]);
            Vector2 max = min;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 corner = area.InverseTransformPoint(corners[i]);
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool Validate(DragOptions options)
        {
            if (options == null || options.Item == null || options.ClonePrefab == null || options.Area == null)
            {
                Debug.LogError("DragOptions needs an item, a clone prefab, and a drag area.");
                return false;
            }

            return true;
        }

        private static bool TryGetLocalPoint(RectTransform area, Vector2 screenPoint, Camera eventCamera, out Vector2 local)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, eventCamera, out local);
        }

        private static RectTransform CreateClone(DragOptions options, Transform parent)
        {
            RectTransform item = options.Item;
            RectTransform clone = Object.Instantiate(options.ClonePrefab, parent, false);

            clone.anchorMin = clone.anchorMax = new Vector2(0.5f, 0.5f);
            clone.pivot = item.pivot;
            clone.sizeDelta = item.rect.size;
            clone.position = item.position;
            clone.rotation = item.rotation;
            clone.SetAsLastSibling();

            // The clone must not block raycasts, so drop targets under it stay reachable.
            CanvasGroup group = clone.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = clone.gameObject.AddComponent<CanvasGroup>();
            }

            group.blocksRaycasts = false;
            return clone;
        }

        private void CalculateOffsets()
        {
            var corners = new Vector3[4];
            _clone.GetWorldCorners(corners);

            _minOffset = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            _maxOffset = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 corner = _options.Area.InverseTransformPoint(corners[i]);
                Vector2 offset = new Vector2(corner.x, corner.y) - _origin;
                _minOffset = Vector2.Min(_minOffset, offset);
                _maxOffset = Vector2.Max(_maxOffset, offset);
            }
        }

        private void HideItem(RectTransform item)
        {
            _itemGroup = item.GetComponent<CanvasGroup>();
            _addedGroup = _itemGroup == null;
            if (_addedGroup)
            {
                _itemGroup = item.gameObject.AddComponent<CanvasGroup>();
            }

            _originalAlpha = _itemGroup.alpha;
            _originalInteractable = _itemGroup.interactable;
            _originalBlocksRaycasts = _itemGroup.blocksRaycasts;
            _itemGroup.alpha = 0f;
            _itemGroup.interactable = false;
            _itemGroup.blocksRaycasts = false;
        }
    }
}
