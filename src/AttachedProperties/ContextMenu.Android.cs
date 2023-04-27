using Android.Views;
using static Android.Views.View;
using AView = Android.Views.View;
using Microsoft.Maui.Platform;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Util;
using Java.Lang;
using Android.Content.Res;

namespace The49.Maui.ContextMenu;

public static partial class ContextMenu
{
    public const float LongPressScaleFactor = .95f;
    public const int LongPressShrinkDelay = 100;

    public const string PreviewWindowBackgroundColorResource = "AndroidContextMenuPreviewWindowBackgroundColor";
    public const string PreviewWindowBackgroundOpacityResource = "AndroidContextMenuPreviewWindowBackgroundOpacity";
    public const string ContextMenuBackgroundColorResource = "AndroidContextMenuContextMenuBackgroundColor";
    public const string ContextMenuCornerRadiusResource = "AndroidContextMenuContextMenuCornerRadius";

    public static readonly BindableProperty ChildElementsProperty = BindableProperty.CreateAttached("ChildElements", typeof(List<VisualElement>), typeof(VisualElement), new List<VisualElement>());
    public static void RegisterChildElement(BindableObject bindable, VisualElement element)
    {
        var views = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        views.Add(element);
        if (bindable is CollectionView collectionView)
        {
            if (collectionView.GetValue(MenuProperty) is not null)
            {
                AttachMenuToView(element, collectionView);
            }
            if (collectionView.GetValue(ClickCommandProperty) is not null)
            {
                AttachClickToView(element, collectionView);
            }
        }
    }
    public static void UnregisterChildElement(BindableObject bindable, VisualElement element)
    {
        var views = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        views.Remove(element);
        DetachMenuFromView(element);
        DetachClickFromView(element);
    }
    static void ForEachElement(BindableObject bindable, Action<VisualElement> action)
    {
        var elements = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        foreach (var element in elements)
        {
            action(element);
        }
    }
    static partial void SetupMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            collectionView.ChildAdded += ChildAddedToCollectionView;
            collectionView.ChildRemoved += ChildRemovedFromCollectionView;
            ForEachElement(collectionView, element => AttachMenuToView(element, collectionView));
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachMenuToView(visualElement, visualElement);
        }
    }

    private static void ChildRemovedFromCollectionView(object sender, ElementEventArgs e)
    {
        UnregisterChildElement((BindableObject)sender, (VisualElement)e.Element);
    }

    private static void ChildAddedToCollectionView(object sender, ElementEventArgs e)
    {
        RegisterChildElement((BindableObject)sender, (VisualElement)e.Element);
    }

    public static void AttachMenuToView(VisualElement visualElement, BindableObject propertySource)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        var showOnClick = GetShowMenuOnClick(propertySource);
        if (showOnClick)
        {
            aview.Clickable = true;
            aview.SetOnClickListener(new MenuActionListener(propertySource, visualElement, aview));
        }
        else
        {
            aview.LongClickable = true;
            var listener = new MenuActionListener(propertySource, visualElement, aview);
            aview.SetOnTouchListener(listener);
            aview.SetOnLongClickListener(listener);
        }
    }
    public static void DetachMenuFromView(VisualElement visualElement)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.LongClickable = false;
        aview.SetOnLongClickListener(null);
    }

    public static void AttachClickToView(VisualElement visualElement, BindableObject propertySource)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.Clickable = true;
        aview.SetOnClickListener(new OnClickListener(propertySource, visualElement));
    }
    public static void DetachClickFromView(VisualElement visualElement)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.Clickable = false;
        aview.SetOnClickListener(null);
    }

    static partial void DisposeMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            collectionView.ChildAdded -= ChildAddedToCollectionView;
            collectionView.ChildRemoved -= ChildRemovedFromCollectionView;
            ForEachElement(collectionView, DetachMenuFromView);
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachMenuFromView(visualElement);
        }
    }

    static partial void SetupClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, element => AttachClickToView(element, collectionView));
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachClickToView(visualElement, visualElement);
        }
    }

    static partial void DisposeClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, DetachClickFromView);
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachClickFromView(visualElement);
        }
    }

    public static (int, int) AddRootMenuItem(MenuElement item, IMenu amenu, int rootGroupId, int rootId)
    {
        if (item is Action action)
        {
            AddAction(action, amenu, rootGroupId, rootId++);
        }
        else if (item is Group group)
        {
            rootGroupId++;
            rootId = AddGroup(group, amenu, rootGroupId, rootId++);
            rootGroupId++;
        }
        else if (item is Menu menu)
        {
            AddSubmenu(menu, amenu, rootGroupId++, rootId++);
        }
        return (rootGroupId, rootId);
    }
    static float DpToPixel(float dp)
    {
        return dp * ((float)Platform.CurrentActivity.Resources.DisplayMetrics.DensityDpi / (float)DisplayMetricsDensity.Default);
    }
    static Bitmap ScaleBitmap(Bitmap targetBmp, int reqHeightInPixels, int reqWidthInPixels)
    {
        Matrix matrix = new Matrix();
        matrix.SetRectToRect(new Android.Graphics.RectF(0, 0, targetBmp.Width, targetBmp.Height), new Android.Graphics.RectF(0, 0, reqWidthInPixels, reqHeightInPixels), Matrix.ScaleToFit.Center);
        Bitmap scaledBitmap = Bitmap.CreateBitmap(targetBmp, 0, 0, targetBmp.Width, targetBmp.Height, matrix, true);
        return scaledBitmap;
    }
    public static void SetActionIcon(IMenuItem item, Action action)
    {
        if (action.Icon != null)
        {
            var id = Platform.CurrentActivity.GetDrawableId(((IFileImageSource)action.Icon).File);
            if (id == 0)
            {
                return;
            }
            var drawable = Platform.CurrentActivity.GetDrawable(id);
            var size = (int)DpToPixel(32f);
            var bitmap = ScaleBitmap(((BitmapDrawable)drawable).Bitmap, size, size);
            var r = new BitmapDrawable(Platform.CurrentActivity.Resources, bitmap);
            item.SetIcon(r);
        }
    }
    public static void SetIsDestructive(IMenuItem item, Action action)
    {
        if (action.IsDestructive && action.IsEnabled)
        {
            item.SetIconTintList(new ColorStateList(new int[][]
            {
                new int[] {  }
            },
            new int[]
            {
                Microsoft.Maui.Graphics.Color.FromArgb("#F44336").ToPlatform()
            }));
        }
    }
    public static void AddAction(Action action, IMenu amenu, int groupId, int itemId)
    {
        var item = amenu.Add(groupId, itemId, itemId, action.Title);
        item.SetEnabled(action.IsEnabled);
        item.SetVisible(action.IsVisible);
        SetActionIcon(item, action);
        SetIsDestructive(item, action);
        item.SetOnMenuItemClickListener(new MenuItemClickListener(action));
    }
    public static int AddGroup(Group group, IMenu amenu, int groupId, int itemId)
    {
        foreach (var item in group.Children)
        {
            AddGroupItem(item, amenu, groupId, itemId++);
        }
        return itemId;
    }
    public static void AddSubmenu(Menu menu, IMenu amenu, int groupId, int itemId)
    {
        var submenu = amenu.AddSubMenu(groupId, itemId, itemId, menu.Title);
        var rootGroupId = 0;
        var rootItemId = 0;
        foreach (var item in menu.Children)
        {
            var ids = AddRootMenuItem(item, submenu, rootGroupId, rootItemId);
            rootGroupId = ids.Item1;
            rootItemId = ids.Item2;
        }
    }

    public static void AddGroupItem(MenuElement item, IMenu amenu, int groupId, int itemId)
    {
        if (item is Action action)
        {
            AddAction(action, amenu, groupId, itemId);
        }
        else if (item is Group group)
        {
            throw new InvalidOperationException("Nested ContextMenu groups is not supported");
        }
        else if (item is Menu menu)
        {
            AddGroupMenu(menu, amenu, groupId, itemId);
        }
    }
    public static void AddGroupMenu(Menu menu, IMenu amenu, int groupId, int itemId)
    {
        var submenu = amenu.AddSubMenu(groupId, itemId, itemId, menu.Title);
        var rootGroupId = 0;
        var rootItemId = 0;
        foreach (var item in menu.Children)
        {
            var ids = AddRootMenuItem(item, submenu, rootGroupId, rootItemId);
            rootGroupId = ids.Item1;
            rootItemId = ids.Item2;
        }
    }
}

internal class ShrinkContextMenuTarget : Java.Lang.Object, IRunnable
{
    private AView _target;

    public ShrinkContextMenuTarget(AView target)
    {
        _target = target;
    }
    public void Run()
    {
        _target.Animate()
            .ScaleX(ContextMenu.LongPressScaleFactor)
            .ScaleY(ContextMenu.LongPressScaleFactor)
            .SetDuration(ViewConfiguration.LongPressTimeout - ContextMenu.LongPressShrinkDelay)
            .Start();
    }
}

public class MenuItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
{
    readonly Action _action;

    public MenuItemClickListener(Action action)
    {
        _action = action;
    }

    public bool OnMenuItemClick(Android.Views.IMenuItem item)
    {
        _action.Command?.Execute(_action.CommandParameter);

        return true;
    }
}

public class OnClickListener : Java.Lang.Object, IOnClickListener
{
    private BindableObject _propertyOwner;
    private BindableObject _contextOwner;


    public OnClickListener(BindableObject propertyOnwer, BindableObject contextOwner) : base()
    {
        _propertyOwner = propertyOnwer;
        _contextOwner = contextOwner;
    }
    public void OnClick(AView v)
    {
        ContextMenu.ExecuteClickCommand(_propertyOwner, _contextOwner.BindingContext);
    }
}

public class MenuActionListener : Java.Lang.Object, IOnLongClickListener, IOnClickListener, IOnTouchListener
{
    BindableObject _propertyOwner;
    BindableObject _contextOwner;
    ShrinkContextMenuTarget _shrink;


    public MenuActionListener(BindableObject propertyOnwer, BindableObject contextOwner, AView target) : base()
    {
        _propertyOwner = propertyOnwer;
        _contextOwner = contextOwner;
        _shrink = new ShrinkContextMenuTarget(target);
    }

    public void OnClick(AView v)
    {
        ShowMenu(v);
    }

    public bool OnLongClick(AView v)
    {
        v.Animate().Cancel();
        v.ScaleX = 1f;
        v.ScaleY = 1f;
        ShowMenuWithPreview(v);
        return true;
    }

    public bool OnTouch(AView v, MotionEvent e)
    {
        if (!v.LongClickable)
        {
            return false;
        }
        switch (e.Action)
        {
            case MotionEventActions.Down:
                v.Handler.PostDelayed(_shrink, 100);
                break;
            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                v.Handler.RemoveCallbacks(_shrink);
                break;
        }
        return false;
    }

    void PopulateMenu(IMenu amenu)
    {
        var menuTemplate = ContextMenu.GetMenu(_propertyOwner);

        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            BindableObject.SetInheritedBindingContext(menu, _contextOwner.BindingContext);

            var rootGroupId = 0;
            var rootId = 0;

            foreach (var item in menu.Children)
            {
                var ids = ContextMenu.AddRootMenuItem(item, amenu, rootGroupId, rootId);
                rootGroupId = ids.Item1;
                rootId = ids.Item2;
            }
#if ANDROID28_0_OR_GREATER
            amenu.SetGroupDividerEnabled(true);
#endif
        }
    }

    public void ShowMenu(AView aview)
    {
        var menu = new ContextMenuPopup(aview, true);

        PopulateMenu(menu.Menu);

        menu.Show(0, 0);
    }

    public void ShowMenuWithPreview(AView aview)
    {
        ContextMenuWindow w = null;
        int offsetX = 0;
        int offsetY = 0;
        if (_propertyOwner is VisualElement visualElement)
        {
            var preview = ContextMenu.GetPreview(visualElement);

            w = new ContextMenuWindow(Platform.CurrentActivity, aview, preview);

            if (preview != null)
            {
                // The padding of the preview will change the visual position of the content
                // adjust the menu's offset to align with what's shown
                offsetX = (int)preview.Padding.Left;
                offsetY = -(int)preview.Padding.Bottom;
            }
        }

        PopulateMenu(w.Menu);

        w.Show(ViewUtils.DpToPx(offsetX), ViewUtils.DpToPx(offsetY));
    }
}
