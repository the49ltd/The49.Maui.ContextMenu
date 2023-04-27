using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace The49.Maui.ContextMenu;

public class CollectionViewDelegator : ReorderableItemsViewDelegator<ReorderableItemsView, CollectionViewController>
{
    NSIndexPath _highlightedIndexPath;
    public CollectionViewDelegator(ItemsViewLayout itemsViewLayout, CollectionViewController itemsViewController) : base(itemsViewLayout, itemsViewController) { }

    public override UITargetedPreview GetPreviewForHighlightingContextMenu(UICollectionView collectionView, UIContextMenuConfiguration configuration)
    {
        _highlightedIndexPath = (NSIndexPath)configuration.Identifier;
        return MakeTargetedPreview(collectionView, configuration);
    }

    public override UITargetedPreview GetPreviewForDismissingContextMenu(UICollectionView collectionView, UIContextMenuConfiguration configuration)
    {
        if (_highlightedIndexPath == null)
        {
            return null;
        }
        var preview = MakeTargetedPreview(collectionView, configuration);
        _highlightedIndexPath = null;
        return preview;
    }

    UITargetedPreview MakeTargetedPreview(UICollectionView collectionView, UIContextMenuConfiguration configuration)
    {
        var item = ViewController.ItemsSource[_highlightedIndexPath];
        var cell = collectionView.CellForItem(_highlightedIndexPath);

        return ContextMenu.CreateTargetedPreview(cell, configuration, item, ViewController.ItemsView);
    }

    public override UIContextMenuConfiguration GetContextMenuConfiguration(UICollectionView collectionView, NSIndexPath indexPath, CGPoint point)
    {
        var menuTemplate = ContextMenu.GetMenu(ViewController.ItemsView);

        if (menuTemplate == null)
        {
            return null;
        }

        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            var item = ViewController.ItemsSource[indexPath];
            BindableObject.SetInheritedBindingContext(menu, item);
            return UIContextMenuConfiguration.Create(indexPath, null, action =>
            {
                return ContextMenu.CreateMenu(menu);
            });
        }
        return null;
    }

    public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
    {
        
        var item = ViewController.ItemsSource[indexPath];
        ContextMenu.ExecuteClickCommand(ViewController.ItemsView, item);
    }
    public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
    {

    }
}

