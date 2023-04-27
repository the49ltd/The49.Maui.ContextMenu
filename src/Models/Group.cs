using Maui.BindableProperty.Generator.Core;
using System.Collections;
using System.Collections.ObjectModel;

namespace The49.Maui.ContextMenu;

[ContentProperty(nameof(Children))]
public partial class Group : MenuElement
{
    [AutoBindable]
    ObservableCollection<MenuElement> children;

    public Group() : base()
    {
        Children = new ObservableCollection<MenuElement>();
    }
}
