using MyPackage.DragDrop;
using UnityEngine;

// Game-specific rules for a sliding puzzle: a cell moves along one axis, at most one cell,
// and only commits when dragged past half a cell. Everything generic lives in the core.
public class SceneDemo : MonoBehaviour
{
    [SerializeField] private RectTransform dragArea;
    [SerializeField] private Transform item;
    [SerializeField] private Transform itemClonePrefab;
    [SerializeField] private Vector2 cellSpacing;

    private DragDropInput _input;
    private Vector2 _cellStep;

    private void Awake()
    {
        _input = item.GetComponent<DragDropInput>();
        if (_input == null)
        {
            _input = item.gameObject.AddComponent<DragDropInput>();
        }

        _input.OptionsProvider = BuildOptions;
        _input.Dropped += OnDropped;
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Dropped -= OnDropped;
        }
    }

    private DragOptions BuildOptions()
    {
        var itemRect = (RectTransform)item;
        Rect cell = DragDropService.GetLocalRect(dragArea, itemRect);
        _cellStep = cell.size + cellSpacing;

        // One cell of travel around the current cell, never outside the board.
        Rect area = dragArea.rect;
        Rect travel = Rect.MinMaxRect(
            Mathf.Max(area.xMin, cell.xMin - _cellStep.x),
            Mathf.Max(area.yMin, cell.yMin - _cellStep.y),
            Mathf.Min(area.xMax, cell.xMax + _cellStep.x),
            Mathf.Min(area.yMax, cell.yMax + _cellStep.y));

        return new DragOptions
        {
            Item = itemRect,
            ClonePrefab = (RectTransform)itemClonePrefab,
            Area = dragArea,
            Axis = DragAxis.Auto,
            Bounds = travel
        };
    }

    private void OnDropped(DragResult result)
    {
        Vector2 delta = result.Delta;
        bool horizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
        float distance = horizontal ? delta.x : delta.y;
        float step = horizontal ? _cellStep.x : _cellStep.y;

        // Not dragged far enough: leaving it unresolved cancels and restores the cell.
        if (Mathf.Abs(distance) < step * 0.5f)
        {
            return;
        }

        Vector2 snapped = horizontal
            ? new Vector2(Mathf.Sign(distance) * step, 0f)
            : new Vector2(0f, Mathf.Sign(distance) * step);

        Vector3 local = dragArea.InverseTransformPoint(item.position);
        result.Commit(dragArea.TransformPoint(local + (Vector3)snapped));

        // A real game swaps its cell data here, then releases the clone and shows the cell again.
        result.RestoreItem();
        Destroy(result.Clone.gameObject);
    }
}
