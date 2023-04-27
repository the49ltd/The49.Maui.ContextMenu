using Maui.BindableProperty.Generator.Core;

namespace The49.Maui.ContextMenu;

public partial class Preview : Element
{
	[AutoBindable]
	DataTemplate previewTemplate;

    [AutoBindable]
    IShape visiblePath;

    [AutoBindable]
    Color backgroundColor;

    [AutoBindable]
    Thickness padding = new Thickness();
}

