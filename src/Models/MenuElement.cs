using Maui.BindableProperty.Generator.Core;

namespace The49.Maui.ContextMenu;

public partial class MenuElement : Element
{
    [AutoBindable]
    readonly string title;
}
