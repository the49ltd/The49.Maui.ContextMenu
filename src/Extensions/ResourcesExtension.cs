namespace The49.Maui.ContextMenu;

public static class ResourcesExtension
{
    public static T GetResourceOrDefault<T>(this Application app, string key, T defaultValue)
    {
        if (app.Resources.TryGetValue(key, out object value))
        {
            return (T)value;
        }
        return defaultValue;
    }
}
