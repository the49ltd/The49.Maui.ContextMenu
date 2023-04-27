using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace The49.Maui.ContextMenu;

public partial class ContextMenu
{
    static IUIContextMenuInteractionDelegate _delegate = new ContextMenuInteractionDelegate();
    public static readonly BindableProperty InteractionProperty = BindableProperty.CreateAttached("Interaction", typeof(IUIInteraction), typeof(VisualElement), null);
    public static readonly BindableProperty TapGestureRecognizerProperty = BindableProperty.CreateAttached("TapGestureRecognizer", typeof(UITapGestureRecognizer), typeof(VisualElement), null);

    static partial void SetupMenu(BindableObject bindable)
    {
        if (bindable is VisualElement ve)
        {
            if (ve.Handler.PlatformView is UIButton button)
            {
                AttachControlMenu(button, ve);
            }
        }
        if (bindable is CollectionView collectionView)
        {

        }
        else if (bindable is VisualElement visualElement)
        {
            if (!GetShowMenuOnClick(visualElement))
            {
                AttachInteraction(visualElement);
            }
        }
    }

    static partial void DisposeMenu(BindableObject bindable)
    {
        if (bindable is VisualElement visualElement)
        {
            DetachInteraction(visualElement);
        }
    }

    static partial void SetupClickCommand(BindableObject bindable)
    {

        if (bindable is CollectionView collectionView)
        {

        }
        else if (bindable is VisualElement visualElement)
        {
            // Already setup, the correct command is read on click, no need to re-setup
            if (bindable.GetValue(TapGestureRecognizerProperty) != null)
            {
                return;
            }
            var pview = (UIView)visualElement.Handler.PlatformView;
            var gestureRecognizer = new UITapGestureRecognizer(() => ExecuteClickCommand(visualElement, visualElement));
            pview.AddGestureRecognizer(gestureRecognizer);
            visualElement.SetValue(TapGestureRecognizerProperty, gestureRecognizer);
        }
    }

    static partial void DisposeClickCommand(BindableObject bindable)
    {
        if (bindable is VisualElement visualElement)
        {
            var pview = (UIView)visualElement.Handler.PlatformView;
            var gestureRecognizer = (UITapGestureRecognizer)visualElement.GetValue(TapGestureRecognizerProperty);
            if (gestureRecognizer != null)
            {
                pview.RemoveGestureRecognizer(gestureRecognizer);
                visualElement.SetValue(TapGestureRecognizerProperty, null);
            }
        }
    }
    static void AttachInteraction(VisualElement visualElement)
    {
        DetachInteraction(visualElement);
        visualElement.Dispatcher.Dispatch(() =>
        {
            var pview = (UIView)visualElement.Handler.PlatformView;
            var interaction = new UIContextMenuInteraction(_delegate);
            pview.UserInteractionEnabled = true;
            pview.AddInteraction(interaction);
            visualElement.SetValue(InteractionProperty, interaction);
        });
    }
    static void DetachInteraction(VisualElement visualElement)
    {
        var interaction = (IUIInteraction)visualElement.GetValue(InteractionProperty);
        if (interaction != null)
        {
            var pview = (UIView)visualElement.Handler.PlatformView;
            pview.RemoveInteraction(interaction);
            visualElement.SetValue(InteractionProperty, null);
        }
    }
    static void AttachControlMenu(UIButton button, VisualElement visualElement)
    {
        var menuTemplate = GetMenu(visualElement);

        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            BindableObject.SetInheritedBindingContext(menu, visualElement.BindingContext);
            button.Menu = CreateMenu(menu);
            button.ShowsMenuAsPrimaryAction = GetShowMenuOnClick(visualElement);
        }

    }
    public static UIMenu CreateMenu(Menu menu)
    {
        UIMenuElement[] children = new UIMenuElement[menu.Children.Count];
        var i = 0;
        foreach (var item in menu.Children)
        {
            children[i++] = CreateMenuItem(item);
        }
        if (!string.IsNullOrEmpty(menu.Title))
        {
            return UIMenu.Create(menu.Title, children);
        }
        return UIMenu.Create(children);
    }
    static UIMenuElement CreateMenuItem(MenuElement item)
    {
        if (item is Action action)
        {
            return CreateAction(action);
        }
        else if (item is Group group)
        {
            return CreateGroup(group);
        }
        else if (item is Menu menu)
        {
            return CreateSubMenu(menu);
        }
        return null;
    }

    static UIMenuElement CreateAction(Action action)
    {
        var a = UIAction.Create(action.Title, GetActionImage(action), null, delegate
        {
            action.Command?.Execute(action.CommandParameter);
        });

        if (!string.IsNullOrEmpty(action.SubTitle))
        {
            a.DiscoverabilityTitle = action.SubTitle;
        }

        if (!action.IsVisible)
        {
            a.Attributes = UIMenuElementAttributes.Hidden;
        }
        else if (action.IsDestructive)
        {
            a.Attributes = UIMenuElementAttributes.Destructive;

        }
        else if (!action.IsEnabled)
        {
            a.Attributes = UIMenuElementAttributes.Disabled;
        }

        return a;
    }
    static UIMenuElement CreateGroup(Group group)
    {
        UIMenuElement[] children = new UIMenuElement[group.Children.Count];
        var i = 0;
        foreach (var item in group.Children)
        {
            children[i++] = CreateMenuItem(item);
        }

        if (!string.IsNullOrEmpty(group.Title))
        {
            return UIMenu.Create(group.Title, null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, children);
        }

        return UIMenu.Create("", null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, children);
    }
    static UIMenuElement CreateSubMenu(Menu menu)
    {
        UIMenuElement[] children = new UIMenuElement[menu.Children.Count];
        var i = 0;
        foreach (var item in menu.Children)
        {
            children[i++] = CreateMenuItem(item);
        }
        if (string.IsNullOrEmpty(menu.Title))
        {
            throw new InvalidOperationException("Cannot create a submenu without a title");
        }
        var a = UIMenu.Create(menu.Title, null, UIMenuIdentifier.None, default(UIMenuOptions), children);

        return a;
    }
    static UIImage? GetActionImage(Action action)
    {
        if (action.SystemIcon != null)
        {
            return UIImage.GetSystemImage(action.SystemIcon);
        }
        if (action.Icon == null)
        {
            return null;
        }
        if (action.Icon is IFileImageSource fileImageSource)
        {
            var filename = fileImageSource.File;
            return File.Exists(filename)
                        ? UIImage.FromFile(filename)
                        : UIImage.FromBundle(filename);
        }
        return null;
    }
    public static UITargetedPreview CreateTargetedPreview(UIView target, UIContextMenuConfiguration configuration, object bindingContext, VisualElement visualElement)
    {
        var preview = ContextMenu.GetPreview(visualElement);

        if (preview == null)
        {
            return null;
        }

        var parameters = new UIPreviewParameters();
        if (preview.VisiblePath != null)
        {
            var bounds = target.Bounds.ToRectangle();
            bounds = new Rect(
                bounds.X + preview.Padding.Left,
                bounds.Y + preview.Padding.Top,
                bounds.Width - preview.Padding.HorizontalThickness,
                bounds.Height - preview.Padding.VerticalThickness
            );
            parameters.VisiblePath = preview.VisiblePath.PathForBounds(bounds).AsUIBezierPath();
        }
        parameters.BackgroundColor = (preview.BackgroundColor ?? Colors.Transparent).ToPlatform();

        if (preview.PreviewTemplate == null)
        {
            return new UITargetedPreview(target, parameters);
        }
        else
        {
            var inst = (VisualElement)preview.PreviewTemplate.CreateContent();
            BindableObject.SetInheritedBindingContext(inst, bindingContext);

            inst.Arrange(target.Bounds.ToRectangle());
            inst.HeightRequest = target.Bounds.Height;
            inst.WidthRequest = target.Bounds.Width;

            return new UITargetedPreview(inst.ToPlatform(visualElement.Handler.MauiContext), parameters, new UIPreviewTarget(target, new CGPoint(target.Bounds.GetMidX(), target.Bounds.GetMidY())));
        }

    }
}

public class ContextMenuInteractionDelegate : UIKit.UIContextMenuInteractionDelegate
{
    public override UIContextMenuConfiguration GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
    {
        if (interaction.View is MauiView view)
        {
            var menuTemplate = ContextMenu.GetMenu((VisualElement)view.View);

            var content = menuTemplate.CreateContent();

            if (content is Menu menu)
            {
                BindableObject.SetInheritedBindingContext(menu, ((VisualElement)view.View).BindingContext);
                return UIContextMenuConfiguration.Create(null, null, action =>
                {
                    return ContextMenu.CreateMenu(menu);
                });
            }

        }

        return null;
    }
    public override UITargetedPreview GetHighlightPreview(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration, INSCopying identifier)
    {
        return GetPreviewForHighlightingMenu(interaction, configuration);
    }

    public override UITargetedPreview GetPreviewForHighlightingMenu(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration)
    {
        if (interaction.View is MauiView view)
        {
            return ContextMenu.CreateTargetedPreview(interaction.View, configuration, ((VisualElement)view.View).BindingContext, (VisualElement)view.View);
        }
        return null;
    }
}
