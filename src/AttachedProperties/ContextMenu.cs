using System.Windows.Input;

namespace The49.Maui.ContextMenu;

public static partial class ContextMenu
{
    public static readonly BindableProperty ClickCommandProperty
        = BindableProperty.CreateAttached(
            "ClickCommand",
            typeof(ICommand),
            typeof(VisualElement),
            null,
            propertyChanged: ClickCommandChanged);

    public static readonly BindableProperty ClickCommandParameterProperty
        = BindableProperty.CreateAttached(
            "ClickCommandParameter",
            typeof(object),
            typeof(VisualElement),
            null);

    public static readonly BindableProperty MenuProperty
        = BindableProperty.CreateAttached(
            "Menu",
            typeof(DataTemplate),
            typeof(VisualElement),
            null,
            propertyChanged: MenuChanged);

    public static readonly BindableProperty PreviewProperty
       = BindableProperty.CreateAttached(
           "Preview",
           typeof(Preview),
           typeof(VisualElement),
           null);

    public static readonly BindableProperty ShowMenuOnClickProperty
       = BindableProperty.CreateAttached(
           "ShowMenuOnClick",
           typeof(bool),
           typeof(VisualElement),
           false,
           propertyChanged: ShowMenuOnClickChanged);

    static void MenuChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisualElement visualElement)
        {
            if (visualElement.Handler == null)
            {
                void updateMenu(object s, EventArgs e)
                {
                    MenuChanged(bindable, oldValue, newValue);
                    visualElement.HandlerChanged -= updateMenu;
                }

                visualElement.HandlerChanged += updateMenu;
            }
            else
            {
                if (oldValue == null && newValue != null)
                {
                    SetupMenu(visualElement);
                }
                if (oldValue != null && newValue == null)
                {
                    DisposeMenu(visualElement);
                }
            }
        }
    }
    static void ClickCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisualElement visualElement)
        {
            if (visualElement.Handler == null)
            {
                void updateClickCommand(object s, EventArgs e)
                {
                    ClickCommandChanged(bindable, oldValue, newValue);
                    visualElement.HandlerChanged -= updateClickCommand;
                }

                visualElement.HandlerChanged += updateClickCommand;
            }
            else
            {
                if (oldValue == null && newValue != null)
                {
                    SetupClickCommand(visualElement);
                }
                if (oldValue != null && newValue == null)
                {
                    DisposeClickCommand(visualElement);
                }
            }
        }
    }

    static void ShowMenuOnClickChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var menu = GetMenu(bindable);
        if (menu != null)
        {
            DisposeMenu(bindable);
            SetupMenu(bindable);
        }
    }

    static partial void SetupMenu(BindableObject bindable);
    static partial void DisposeMenu(BindableObject bindable);
    static partial void SetupClickCommand(BindableObject bindable);
    static partial void DisposeClickCommand(BindableObject bindable);


    public static ICommand GetClickCommand(BindableObject view)
    {
        return (ICommand)view.GetValue(ClickCommandProperty);
    }

    public static void SetClickCommand(BindableObject view, ICommand value)
    {
        view.SetValue(ClickCommandProperty, value);
    }

    public static DataTemplate GetMenu(BindableObject view)
    {
        return (DataTemplate)view.GetValue(MenuProperty);
    }

    public static void SetMenu(BindableObject view, DataTemplate value)
    {
        view.SetValue(MenuProperty, value);
    }
    public static Preview GetPreview(BindableObject view)
    {
        return (Preview)view.GetValue(PreviewProperty);
    }

    public static void SetPreview(BindableObject view, Preview value)
    {
        view.SetValue(PreviewProperty, value);
    }

    public static object GetClickCommandParameter(BindableObject view)
    {
        return view.GetValue(ClickCommandParameterProperty);
    }

    public static void SetClickCommandParameter(BindableObject view, object value)
    {
        view.SetValue(ClickCommandParameterProperty, value);
    }

    public static bool GetShowMenuOnClick(BindableObject view)
    {
        return (bool)view.GetValue(ShowMenuOnClickProperty);
    }

    public static void SetShowMenuOnClick(BindableObject view, bool value)
    {
        view.SetValue(ShowMenuOnClickProperty, value);
    }

    public static void ExecuteClickCommand(BindableObject bindable, object defaultValue)
    {
        var command = GetClickCommand(bindable);
        var commandParameter = GetClickCommandParameter(bindable);

        command?.Execute(commandParameter ?? defaultValue);
    }
}
