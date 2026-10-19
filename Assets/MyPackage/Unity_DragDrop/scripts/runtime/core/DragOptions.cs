using UnityEngine;

namespace MyPackage.DragDrop
{
    public sealed class DragOptions
    {
        public RectTransform Item;
        public RectTransform ClonePrefab;
        public RectTransform Area;
        public DragAxis Axis = DragAxis.Free;

        // Allowed range for the clone, in Area local space. Null uses the whole Area.
        public Rect? Bounds;

        public float LockThreshold = 5f;
    }
}
