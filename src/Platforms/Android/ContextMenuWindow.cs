using Android;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Kotlin.Jvm.Functions;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace The49.Maui.ContextMenu;

internal class ContextMenuBackgroundView : FrameLayout
{
    public event EventHandler DismissEvent;
    public ContextMenuBackgroundView(Context context) : base(context)
    { }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        var color = Application.Current.GetResourceOrDefault(ContextMenu.PreviewWindowBackgroundColorResource, Colors.Gray);
        var opacity = Application.Current.GetResourceOrDefault(ContextMenu.PreviewWindowBackgroundColorResource, .05f);
        Background = new ColorDrawable(color.ToPlatform());
        Background.Alpha = (int)Math.Round(255 - opacity * 255);
    }

    public override bool OnTouchEvent(MotionEvent e)
    {
        if (e.Action == MotionEventActions.Down)
        {
            Dismiss();
            return true;
        }
        else
        {
            return base.OnTouchEvent(e);
        }
    }

    void Dismiss()
    {
        DismissEvent?.Invoke(this, EventArgs.Empty);
    }
}

internal class ContextMenuTemplatedPreview : ContextMenuPreview
{
    public ContextMenuTemplatedPreview(Context context, Preview? preview) : base(context, preview)
    {
    }
}

internal class CustomOutline : ViewOutlineProvider
{
    Context _context;
    int _width;
    int _height;
    Preview? _preview;

    public CustomOutline(Context context, int width, int height, Preview? preview)
    {
        _context = context;
        _width = width;
        _height = height;
        _preview = preview;
    }
    public override void GetOutline(AView view, Outline outline)
    {
        if (_preview == null)
        {
            outline.SetRect(0, 0, _width, _height);
        }
        else
        {
            var density = _context.Resources.DisplayMetrics.Density;
            var bounds = new Microsoft.Maui.Graphics.Rect(
                _preview.Padding.Left,
                _preview.Padding.Top,
                _width / density - _preview.Padding.HorizontalThickness,
                _height / density - _preview.Padding.VerticalThickness
            );
            outline.SetPath(_preview.VisiblePath.PathForBounds(bounds).AsAndroidPath(0, 0, density, density));
        }
    }
}

internal class ContextMenuPreview : FrameLayout
{
    Context _context;
    Preview? _preview;
    protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
    {
        if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
        {
            OutlineProvider = new CustomOutline(_context, w, h, _preview);
        }
    }

    public ContextMenuPreview(Context context, Preview? preview) : base(context)
    {
        _context = context;
        _preview = preview;
        ClipToOutline = true;
        Elevation = ViewUtils.DpToPx(8);
    }
    public override bool OnTouchEvent(MotionEvent e)
    {
        return true;
    }
}

internal class ContextMenuSnapshotPreview : ContextMenuPreview
{
    public ContextMenuSnapshotPreview(Context context, Bitmap bitmap, Preview? preview) : base(context, preview)
    {
        Background = new BitmapDrawable(context.Resources, bitmap);
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        SetMeasuredDimension(Background.IntrinsicWidth, Background.IntrinsicHeight);
    }
}

internal class ContextMenuWindow
{
    public const int DismissAnimationDuration = 120;
    public const int ShowAnimationDuration = 200;
    public const int ContextMenuSpacing = 16;
    public const int ContextMenuEdgeSpacing = 16;

    Context _context;
    IWindowManager _windowManager;
    FrameLayout _decorView;
    AView _parent;
    AView _content;
    ContextMenuBackgroundView _backgroundView;
    ContextMenuPopup _menu;
    private int _offsetX;
    private int _offsetY;

    public ContextMenuWindow(Context context, AView parent, Preview? p)
    {
        var bitmap = GetViewSnapshot(parent);
        var preview = new ContextMenuSnapshotPreview(context, bitmap, p);
        Setup(context, parent, preview);
    }

    public ContextMenuWindow(Context context, AView parent, AView content, Preview? p)
    {
        var preview = new ContextMenuTemplatedPreview(context, p);
        preview.AddView(content, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent));
        Setup(context, parent, preview);
    }

    void Setup(Context context, AView parent, ContextMenuPreview content)
    {
        _context = context;
        _parent = parent;
        _content = content;
        _content.ScaleX = ContextMenu.LongPressScaleFactor;
        _content.ScaleY = ContextMenu.LongPressScaleFactor;
        _windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

        _menu = new ContextMenuPopup(content);

        _menu.DismissEvent += OnMenuClosed;
    }

    private void OnMenuClosed(object sender, EventArgs e)
    {
        Dismiss();
    }

    public IMenu Menu => _menu.Menu;

    ContextMenuBackgroundView CreateBackgroundView()
    {
        var backgroundView = new ContextMenuBackgroundView(_context);
        backgroundView.DismissEvent += (s, e) =>
        {
            Dismiss();
        };
        return backgroundView;
    }
    FrameLayout CreateDecorView(AView background, AView content)
    {
        var decorView = new FrameLayout(_context);
        decorView.AddView(background, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent));
        decorView.AddView(content, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WrapContent, FrameLayout.LayoutParams.WrapContent));
        return decorView;
    }

    void Dismiss()
    {
        var pos = new int[2];
        _parent.GetLocationInWindow(pos);
        _menu?.Dismiss();
        _content.Animate()
            .ScaleX(1)
            .ScaleY(1)
            .Y(pos[1])
            .SetDuration(DismissAnimationDuration)
            .WithEndAction(() =>
            {
                _parent.Visibility = ViewStates.Visible;
                if (_decorView.Parent != null)
                {
                    _windowManager.RemoveView(_decorView);
                }
            })
            .Start();

        AnimateBackgroundOut();

    }

    public static Bitmap GetViewSnapshot(AView v)
    {
        Bitmap b = Bitmap.CreateBitmap(v.Width, v.Height, Bitmap.Config.Argb8888);
        Canvas c = new Canvas(b);
        v.Draw(c);
        return b;
    }

    public void Show(int offsetX = 0, int offsetY = 0)
    {
        _offsetX = offsetX;
        _offsetY = offsetY;
        var token = _parent.RootView.WindowToken;
        _backgroundView = CreateBackgroundView();
        _decorView = CreateDecorView(_backgroundView, _content);

        _windowManager.AddView(_decorView, new WindowManagerLayoutParams(
            WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent,
            WindowManagerTypes.ApplicationPanel,
            WindowManagerFlags.LayoutNoLimits,
            Format.Translucent));

        _parent.ScaleX = ContextMenu.LongPressScaleFactor;
        _parent.ScaleY = ContextMenu.LongPressScaleFactor;

        _backgroundView.Alpha = 0;

        ViewKt.DoOnLayout(_backgroundView, new Function1Impl<AView>(v =>
        {
            ShowAfterLayout();
        }));
    }

    void ShowAfterLayout()
    {
        _parent.ScaleX = 1f;
        _parent.ScaleY = 1f;
        Update();

        var overlayHeight = _backgroundView.Height;

        var menuHeight = _menu.GetHeight();

        var previewHeight = _content.MeasuredHeight;

        var endOfStack = _content.GetY()
            + previewHeight
            + ViewUtils.DpToPx(ContextMenuSpacing)
            + menuHeight
            + ViewUtils.DpToPx(ContextMenuEdgeSpacing);

        var willOverflow = endOfStack > overlayHeight;

        var endY = _content.GetY();

        if (willOverflow)
        {
            endY = overlayHeight
                - ViewUtils.DpToPx(ContextMenuEdgeSpacing)
                - menuHeight
                - ViewUtils.DpToPx(ContextMenuSpacing)
                - previewHeight;
        }

        _parent.Visibility = ViewStates.Invisible;

        _content.Animate()
            .ScaleX(1)
            .ScaleY(1)
            .Y(endY)
            .SetDuration(ShowAnimationDuration)
            .WithEndAction(() =>
            {
                _menu.Show(_offsetX, _offsetY + ViewUtils.DpToPx(ContextMenuWindow.ContextMenuSpacing));
            })
            .Start();

        AnimateBackgroundIn();
    }

    void AnimateBackgroundIn()
    {
        _backgroundView.Animate()
            .Alpha(1)
            .SetDuration(ShowAnimationDuration)
            .Start();
    }

    void AnimateBackgroundOut()
    {
        _backgroundView.Animate()
            .Alpha(0)
            .SetDuration(DismissAnimationDuration)
            .Start();
    }

    void Update()
    {
        var pos = new int[2];
        _parent.GetLocationInWindow(pos);
        float x = pos[0];
        float y = pos[1];

        var cX = (int)Math.Round(x + _parent.Width / 2);
        var cY = (int)Math.Round(y + _parent.Height / 2);

        _content.SetX(cX - _content.MeasuredWidth / 2);
        _content.SetY(cY - _content.MeasuredHeight / 2);
    }
}

class Function1Impl<T> : Java.Lang.Object, IFunction1 where T : Java.Lang.Object
{
    private readonly Action<T> OnInvoked;

    public Function1Impl(Action<T> onInvoked)
    {
        OnInvoked = onInvoked;
    }

    public Java.Lang.Object Invoke(Java.Lang.Object objParameter)
    {
        try
        {
            T parameter = (T)objParameter;
            OnInvoked?.Invoke(parameter);
            return null;
        }
        catch (Exception ex)
        {
            // Exception handling, if needed
        }
        return null;
    }
}