using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Spice86.Views.Behaviors;

/// <summary>
/// Represents use parent list box context menu behavior.
/// </summary>
public class UseParentListBoxContextMenuBehavior {
    /// <summary>
    /// The use parent context menu property.
    /// </summary>
    public static readonly AttachedProperty<bool> UseParentContextMenuProperty = AvaloniaProperty.RegisterAttached<Control, bool>("UseParentContextMenu", typeof(UseParentListBoxContextMenuBehavior));

    static UseParentListBoxContextMenuBehavior() {
        UseParentContextMenuProperty.Changed.AddClassHandler<Control>((control, e) => {
            if (e.NewValue is true) {
                control.AddHandler(Control.ContextRequestedEvent, OnContextRequested, RoutingStrategies.Tunnel);
            } else {
                control.RemoveHandler(Control.ContextRequestedEvent, OnContextRequested);
            }
        });
    }

    private static void OnContextRequested(object? sender, ContextRequestedEventArgs e) {
        if (sender is Control control) {
            e.Handled = true;

            // Find parent ListBox and the associated ListBoxItem
            Control? parent = control;
            ListBox? listBox = null;
            ListBoxItem? listBoxItem = null;

            while (parent != null) {
                if (parent is ListBoxItem lbi) {
                    listBoxItem = lbi;
                }

                if (parent is ListBox lb) {
                    listBox = lb;

                    break;
                }

                parent = parent.Parent as Control;
            }

            if (listBox?.ContextMenu != null && listBoxItem != null) {
                // Important: Select the item we right-clicked on
                listBox.SelectedItem = listBoxItem.DataContext;

                // Open the context menu
                listBox.ContextMenu.Open(listBox);
            }
        }
    }

    /// <summary>
    /// Sets use parent context menu.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="value">The value.</param>
    public static void SetUseParentContextMenu(Control element, bool value) {
        element.SetValue(UseParentContextMenuProperty, value);
    }

    /// <summary>
    /// Gets use parent context menu.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public static bool GetUseParentContextMenu(Control element) {
        return element.GetValue(UseParentContextMenuProperty);
    }
}