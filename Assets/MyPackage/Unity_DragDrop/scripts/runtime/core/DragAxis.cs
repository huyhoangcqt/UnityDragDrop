namespace MyPackage.DragDrop
{
    public enum DragAxis
    {
        Free,
        Horizontal,
        Vertical,
        // Picks horizontal or vertical from the first significant movement, then stays on it.
        Auto
    }
}
