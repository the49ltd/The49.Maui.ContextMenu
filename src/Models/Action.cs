using Maui.BindableProperty.Generator.Core;
using System.Windows.Input;

namespace The49.Maui.ContextMenu;

public partial class Action : MenuElement
{
    [AutoBindable]
    readonly ICommand command;

    [AutoBindable]
    readonly object commandParameter;

    [AutoBindable]
    readonly ImageSource icon;

    [AutoBindable]
    readonly string systemIcon;

    [AutoBindable(DefaultValue = "true")]
    readonly bool isEnabled;

    [AutoBindable(DefaultValue = "true")]
    readonly bool isVisible;

    [AutoBindable]
    readonly bool isDestructive;

    [AutoBindable]
    readonly string subTitle;
}
