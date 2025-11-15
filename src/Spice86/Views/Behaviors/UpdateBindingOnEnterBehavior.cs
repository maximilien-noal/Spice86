using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Spice86.Views.Behaviors;

/// <summary>
/// Represents update binding on enter behavior.
/// </summary>
public class UpdateBindingOnEnterBehavior : Behavior<TextBox> {
    /// <summary>
    /// Called when attached.
    /// </summary>
    protected override void OnAttached() {
        base.OnAttached();
        if (AssociatedObject != null) {
            AssociatedObject.KeyUp += OnKeyUp;
        }
    }

    /// <summary>
    /// Called when detaching.
    /// </summary>
    protected override void OnDetaching() {
        base.OnDetaching();
        if (AssociatedObject != null) {
            AssociatedObject.KeyUp -= OnKeyUp;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Enter && AssociatedObject != null) {
            BindingExpressionBase? binding = BindingOperations.
                GetBindingExpressionBase(AssociatedObject, TextBox.TextProperty);
            binding?.UpdateSource();
        }
    }
}