namespace SpringProject.Core.Editor;

public enum ToolType
{
    Pointer,
    BoxSelect,
    Paint,
    Dropper
}

public static class Tools
{
    public static ToolType CurrentType { get; private set; } = ToolType.Pointer; 

    public static void SetTool(ToolType toolType)
    {
        CurrentType = toolType;
    }
}