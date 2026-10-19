using UnityEngine;

namespace MyPackage.DragDrop
{
    // Handed to the game when a drag ends. The game must Commit or Cancel;
    // an unresolved result is cancelled automatically by DragDropInput.
    public sealed class DragResult
    {
        private readonly CanvasGroup _itemGroup;
        private readonly float _originalAlpha;
        private readonly bool _originalInteractable;
        private readonly bool _originalBlocksRaycasts;
        private readonly bool _addedGroup;

        internal DragResult(
            RectTransform item,
            RectTransform clone,
            RectTransform area,
            Vector2 origin,
            Vector2 localPosition,
            CanvasGroup itemGroup,
            bool addedGroup,
            float originalAlpha,
            bool originalInteractable,
            bool originalBlocksRaycasts)
        {
            Item = item;
            Clone = clone;
            Area = area;
            Origin = origin;
            LocalPosition = localPosition;
            _itemGroup = itemGroup;
            _addedGroup = addedGroup;
            _originalAlpha = originalAlpha;
            _originalInteractable = originalInteractable;
            _originalBlocksRaycasts = originalBlocksRaycasts;
        }

        public RectTransform Item { get; }
        public RectTransform Clone { get; }
        public RectTransform Area { get; }
        public Vector2 Origin { get; }
        public Vector2 LocalPosition { get; }
        public Vector2 Delta => LocalPosition - Origin;
        public bool IsResolved { get; private set; }

        // Moves the clone to destination (world). The original item stays hidden; the game owns what happens next.
        public void Commit(Vector3 destination)
        {
            if (IsResolved)
            {
                return;
            }

            IsResolved = true;
            if (Clone != null)
            {
                Clone.position = destination;
            }
        }

        // Destroys the clone and shows the original item again.
        public void Cancel()
        {
            if (IsResolved)
            {
                return;
            }

            IsResolved = true;
            if (Clone != null)
            {
                Object.Destroy(Clone.gameObject);
            }

            RestoreItem();
        }

        public void RestoreItem()
        {
            if (_itemGroup == null)
            {
                return;
            }

            if (_addedGroup)
            {
                Object.Destroy(_itemGroup);
                return;
            }

            _itemGroup.alpha = _originalAlpha;
            _itemGroup.interactable = _originalInteractable;
            _itemGroup.blocksRaycasts = _originalBlocksRaycasts;
        }
    }
}
