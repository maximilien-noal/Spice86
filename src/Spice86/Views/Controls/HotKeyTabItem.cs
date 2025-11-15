namespace Spice86.Views.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using System.Windows.Input;

/// <summary>
/// A TabItem with a hot key.
/// <remarks>Source is: https://github.com/AvaloniaUI/Avalonia/discussions/14836</remarks>
/// </summary>
public class HotKeyTabItem : TabItem, ICommandSource {
    /// <summary>
    /// The style key override.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(TabItem);

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public HotKeyTabItem() {
        Command = new TabItemSelectCommand(this);
        CommandParameter = null;
    }

    /// <summary>
    /// Determines whether it can execute changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    public void CanExecuteChanged(object sender, EventArgs e) {
    }

    /// <summary>
    /// Gets command.
    /// </summary>
    public ICommand? Command { get; }
    /// <summary>
    /// Gets command parameter.
    /// </summary>
    public object? CommandParameter { get; }

    /// <summary>
    /// Represents tab item select command.
    /// </summary>
    public class TabItemSelectCommand : ICommand {
        private readonly TabItem _tabItem;

        /// <summary>
        /// Performs the tab item select command operation.
        /// </summary>
        /// <param name="tabItem">The tab item.</param>
        public TabItemSelectCommand(TabItem tabItem) {
            _tabItem = tabItem;
            _tabItem.PropertyChanged += OnTabItemPropertyChanged;
        }

        /// <summary>
        /// Determines whether it can execute.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object? parameter) {
            return _tabItem.IsEffectivelyEnabled;
        }

        /// <summary>
        /// Executes .
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void Execute(object? parameter) {
            _tabItem.IsSelected = true;
        }

        public event EventHandler? CanExecuteChanged;

        private void OnTabItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs changeArgs) {
            if (changeArgs.Property == IsEffectivelyEnabledProperty) {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}