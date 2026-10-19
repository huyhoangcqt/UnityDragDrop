[Unity_Drag&Drop]
[v0.0.2]

Reusable drag & drop for UGUI. Built for sliding-puzzle style games, usable anywhere.

[Structure]
scripts/runtime/core   Generic code, no game rules.
  DragConstraint       Pure math: axis lock (Free / Horizontal / Vertical / Auto) and area bounds. No input.
  DragDropService      Plain C# class. Creates the clone, hides the item, moves the clone. Call it from any script.
  DragDropInput        MonoBehaviour that only forwards EventSystem drag events to the service.
  DragOptions          What to drag, clone prefab, area, axis, optional bounds.
  DragResult           Handed to the game on drop: Commit(destination) or Cancel().
example                Game-specific code. SceneDemo shows a sliding-puzzle cell (one axis, one cell).

[Flow]
1. Drag starts: the original item is hidden (it keeps its place in the layout) and a clone is
   created from the prefab under the root Canvas, so layouts do not affect it.
2. The clone follows the pointer, limited by the axis and bounds in DragOptions.
3. On drop, DragDropInput.Dropped gives a DragResult. The game decides:
   - result.Commit(worldDestination): the clone moves there; the original stays hidden
     (the game then updates its data, calls result.RestoreItem() and destroys result.Clone).
   - do nothing / result.Cancel(): the clone is destroyed and the original is shown again.

[Usage]
Add DragDropInput to a raycastable UI item (the scene needs an EventSystem). Either set
item / clone prefab / area in the Inspector, or customise per game:

    input.OptionsProvider = () => new DragOptions
    {
        Item = itemRect, ClonePrefab = prefabRect, Area = boardRect,
        Axis = DragAxis.Auto,   // one direction, picked from the first movement
        Bounds = allowedRect    // area-local space; DragDropService.GetLocalRect(area, cell) helps
    };
    input.Dropped += result => result.Commit(targetWorldPosition);

Without DragDropInput (other input sources), drive the service directly:
Begin(options, screenPoint, camera), Move(screenPoint, camera), End(), Abort().
