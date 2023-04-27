using Maui.BindableProperty.Generator.Core;
using System.Collections.ObjectModel;

namespace The49.Maui.ContextMenu;

[ContentProperty(nameof(Children))]
public partial class Menu : MenuElement
{
    [AutoBindable]
    readonly ObservableCollection<MenuElement> children;

    public Menu() : base()
    {
        Children = new ObservableCollection<MenuElement>();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        foreach (var item in Children)
        {
            SetInheritedBindingContext(item, BindingContext);
        }
    }
}
