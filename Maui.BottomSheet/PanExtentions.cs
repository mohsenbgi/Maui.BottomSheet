namespace Maui.BottomSheet;
public static class PanExtentions
{
    public enum PanDirection
    {
        None,

        Horizontal,

        Vertical
    }

    public static PanDirection GetPanDirection(this PanUpdatedEventArgs eventArgs)
    {
        var result = PanDirection.None;

        if (Math.Abs(eventArgs.TotalY) > Math.Abs(eventArgs.TotalX))
            result = PanDirection.Vertical;
        else
            result = PanDirection.Horizontal;

        return result;
    }

    public static bool IsVertical(this PanUpdatedEventArgs eventArgs)
    {
        return eventArgs.GetPanDirection() == PanDirection.Vertical;
    }

    public static bool IsHorizontal(this PanUpdatedEventArgs eventArgs)
    {
        return eventArgs.GetPanDirection() == PanDirection.Horizontal;
    }
}
