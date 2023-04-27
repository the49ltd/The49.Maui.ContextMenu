## What is Maui.ContextMenu?

Maui.ContextMenu is a .NET MAUI library for Android and iOS used to open a native context menu on long press.


## Getting Started

Enable this plugin by calling `UseContextMenu()` in your `MauiProgram.cs`

```csharp
using The49.Maui.ContextMenu;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		
		// Initialise the plugin
		builder.UseMauiApp<App>().UseContextMenu();

		// the rest of your logic...
	}
}
```

### XAML usage

In order to make use of the plugin within XAML you can use this namespace:

```xml
xmlns:the49="https://schemas.the49.com/dotnet/2023/maui"
```

Then you can add a context menu to any control:

```xml
<ContentView HeightRequest="200" WidthRequest="200" Background="GreenYellow">
    <the49:ContextMenu.Menu>
        <DataTemplate>
            <the49:Menu>
                <the49:Action Title="Upload documents" />
                <the49:Action Title="Copy" />
                <the49:Action Title="Cut" />
                <the49:Action Title="Paste" />
            </the49:Menu>
        </DataTemplate>
    </the49:ContextMenu.Menu>
</ContentView>
```

### Defining the menu

The available elements for the menu are:

 - `<the49:Menu>`: The top level menu, can be nested for submenus
 - `<the49:Action>`: An action users can select from the menu
 - `<the49:Group>`: A group of `Action`s or `Menu`s. `Group`s cannot be nested

The `<the49:Menu>` element supports the following parameters:

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `Title`   | `string`    |  A string title to be displayed at the top of the context menu | ✅ | ❌      |

The `<the49:Group>` element supports the following parameters:

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `Title`   | `string`    |  A string title to be displayed at the top of the group        | ✅ | ❌      |


The `<the49:Action>` element supports the following parameters:

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `Title`   | `string`|  The text for the action        | ✅ | ✅      |
| `Command`   | `ICommand`    |  A command to execute when the user selects this action        | ✅ | ✅      |
| `CommandParameter`   | `object`    |  A parameter that will be passed to the `Command`        | ✅ | ✅      |
| `Icon`   | `ImageSource`    |  An icon to be displayed next to the action text        | ✅ | ✅      |
| `SystemIcon`   | `string`    |  A string reference to a system icon to use instead of the `Icon`        | ✅* | ❌**      |
| `IsEnabled`   | `bool`    |  Enables/Disables the action in the menu        | ✅ |    ✅   |
| `IsVisible`   | `bool`    |  Hides/Displays the action in the menu        | ✅ |    ✅   |
| `IsDestructive`   | `bool`    |  Displays the action as destructive       | ✅ |    ✅   |

 - \* SystemIcons on iOS use the SFSymbols collection of icons: https://developer.apple.com/sf-symbols/
 - \*\* Android does not recommend using system icons in your apps

Here is what a more complex menu would look like using most properties available:

```xml
<the49:Menu Title="More options">
    <the49:Action Title="Copy" SystemIcon="doc.on.clipboard" />
    <the49:Action Title="Upload documents" Icon="dotnet_bot.png" />
    <the49:Group Title="Lifecycle">
        <the49:Action Title="Start" Command="{Binding StartCommand, Source={x:Reference this}}" CommandParameter="foo" />
        <the49:Action Title="Stop" IsDestructive="True" />
    </the49:Group>
    <the49:Menu Title="Clipboard">
        <the49:Action Title="Copy" SystemIcon="doc.on.clipboard" IsEnabled="False" />
        <the49:Action Title="Paste" IsVisible="False" />
    </the49:Menu>
</the49:Menu>
```

### Customising the preview

When an item is selected for its context menu to open, a highlighted preview can be displayed. This preview by default is a snapshot of the view itself. This can be customised using the `Preview` property.

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `PreviewTemplate`   | `DataTemplate`    |  Provide a different view to render the preview        | ✅ | ✅      |
| `VisiblePath`   | `IShape`|  Customise the path used to clip the preview        | ✅* | ✅      |
| `BackgroundColor`   | `Color`    |  The background color for the hightlight preview        | ✅ | ✅      |
| `Padding`   | `Thickness`    |  The padding of the VisiblePath within the highlight preview        | ✅ | ✅      |

 - \* By default the Visible path on iOS has a corner radius set

Example:

```xml
<the49:ContextMenu.Preview>
    <the49:Preview BackgroundColor="Red" Padding="16">
        <the49:Preview.PreviewTemplate>
            <DataTemplate>
                <ContentView WidthRequest="100" HeightRequest="100" Background="Green" />
            </DataTemplate>
        </the49:Preview.PreviewTemplate>
        <the49:Preview.VisiblePath>
            <RoundRectangle CornerRadius="10, 20, 30, 40" />
        </the49:Preview.VisiblePath>
    </the49:Preview>
</the49:ContextMenu.Preview>
```


### Standard click

Because of how context menus ar attached to view on each different platforms, the `TapGestureRecognizer` might not work in combination with the context menu. This is why this plugin also offers a `ClickCommand` property to ensure your command is called on every platform.

```xml
<ContentView 
        the49:ContextMenu.ClickCommand="{Binding ClickCommand}"
        the49:ContextMenu.ClickCommandParameter="Hello">
    <the49:ContextMenu.Menu>
        <DataTemplate>
            <!-- ... -->
        </DataTemplate>
    </the49:ContextMenu.Menu>
</ContentView>
```

### Show menu on click

Sometimes you want to show the context menu on a click instead of the default long press/right click. Setting `ShowMenuOnClick` to true will simply do that. The highlight preview will however not be shown in this mode.

On iOS it is not possible to open a context menu on click. This plugin uses the UIKit `showsMenuAsPrimaryAction` property to support this feature. This is however only supported on UIKit.UIbutton, meaning `ShowMenuOnClick` will only work on `Button` on iOS

### TableView, ListView, CollectionView

To add a contextmenu to all items of a CollectionView for example:

```xml
<CollectionView>
    <the49:ContextMenu.Menu>
        <DataTemplate>
            <the49:Menu>
                <the49:Action Title="Upload" />
                <the49:Action Title="Delete" />
            </the49:Menu>
        </DataTemplate>
    </the49:ContextMenu.Menu>
    <!-- ... -->
</CollectionView>
```

This will add a contextmenu to each item of a CollectionView. However all the menus will be identical. If you need to have the menu change based on the item of the CollectionView selected, you can do so by using bindings. By default, the BindingContext of the `<the49:Menu>` is the item the user opened the context menu from. 

This can also be useful to pass the item to the command of an action.

For example, if the Items of the CollectionView are:

```cs

public class UserDocuments {
    public string Name { get; set; }
    public bool CanUpload { get; set; }
}

```

You can configure the context menu for these items like so:

```xml
<the49:CollectionView>
    <the49:ContextMenu.Menu>
        <DataTemplate>
            <the49:Menu>
                <the49:Action
                    Title="Upload"
                    IsVisible="{Binding CanUpload}"
                    Command="{Binding UploadDocument, Source={x:Reference this}}"
                    CommandParameter="{Binding .}"
                />
            </the49:Menu>
        </DataTemplate>
    </the49:ContextMenu.Menu>
    <!-- ... -->
</the49:CollectionView>
```

## Implementation details

This plugins aims to use the underlying platform's features as much as possible, however when needed the platform was extended to offer the best user experience.

### iOS

iOS provides a native and comprehensive context menu feature. This was used to support this plugin.

The [UIContextMenuInteraction](https://developer.apple.com/documentation/uikit/uicontextmenuinteraction) was used to support standalone controls. For items views, [contextMenuConfigurationForItemsAt](https://developer.apple.com/documentation/uikit/uicollectionviewdelegate/4002186-collectionview) or an equivalent method was used


### Android

Android offers a native context menu API. However this API basically just opens a [PopupMenu](https://developer.android.com/reference/android/widget/PopupMenu) anchored to the target view.

This plugin implements a more complete context menu experience by adding a highlighted preview and multiple other features to match the featureset provided by iOS.

A [MenuBuilder](https://developer.android.com/reference/android/view/Menu) is still used to conform the custom implementation to all other Android menus.

## TODO:

 - Allows for the specification of the icon color. This would let developer choose whether their icons will match the text color or use the icon's colors.