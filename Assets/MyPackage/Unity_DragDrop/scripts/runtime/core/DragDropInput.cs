using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MyPackage.DragDrop
{
    // Input adapter only: forwards EventSystem drag events to DragDropService.
    // Game-specific rules go through OptionsProvider (what to drag, axis, bounds) and Dropped (accept or cancel).
    public sealed class DragDropInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform item;
        [SerializeField] private RectTransform clonePrefab;
        [SerializeField] private RectTransform dragArea;
        [SerializeField] private DragAxis axis = DragAxis.Free;
        [SerializeField] private float lockThreshold = 5f;

        private readonly DragDropService _service = new DragDropService();

        // Called at drag start; when null, options are built from the serialized fields.
        public Func<DragOptions> OptionsProvider { get; set; }

        public event Action<DragOptions> DragStarted;
        public event Action<DragResult> Dropped;

        public DragDropService Service => _service;

        public void Configure(RectTransform dragItem, RectTransform prefab, RectTransform area)
        {
            item = dragItem;
            clonePrefab = prefab;
            dragArea = area;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            DragOptions options = OptionsProvider != null ? OptionsProvider() : BuildDefaultOptions();
            if (_service.Begin(options, eventData.position, eventData.pressEventCamera))
            {
                DragStarted?.Invoke(options);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            _service.Move(eventData.position, eventData.pressEventCamera);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragResult result = _service.End();
            if (result == null)
            {
                return;
            }

            Dropped?.Invoke(result);
            result.Cancel();
        }

        private void OnDisable()
        {
            _service.Abort();
        }

        private DragOptions BuildDefaultOptions()
        {
            RectTransform dragItem = item != null ? item : transform as RectTransform;
            RectTransform area = dragArea != null
                ? dragArea
                : dragItem != null ? dragItem.parent as RectTransform : null;

            return new DragOptions
            {
                Item = dragItem,
                ClonePrefab = clonePrefab,
                Area = area,
                Axis = axis,
                LockThreshold = lockThreshold
            };
        }
    }
}
