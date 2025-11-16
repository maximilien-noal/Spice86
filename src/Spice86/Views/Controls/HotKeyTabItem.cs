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
    protected override Type StyleKeyOverride => typeof(TabItem);

    public HotKeyTabItem() {
        Command = new TabItemSelectCommand(this);
        CommandParameter = null;
    }

    /// <summary>
    /// CanExecuteChanged method.
    /// </summary>
    public void CanExecuteChanged(object sender, EventArgs e) {
    }

    public ICommand? Command { get; }
    public object? CommandParameter { get; }

    /// <summary>
    /// Represents the TabItemSelectCommand class.
    /// </summary>
    public class TabItemSelectCommand : ICommand {
        private readonly TabItem _tabItem;

        public TabItemSelectCommand(TabItem tabItem) {
            _tabItem = tabItem;
            _tabItem.PropertyChanged += OnTabItemPropertyChanged;
        }

        /// <summary>
        /// CanExecute method.
        /// </summary>
        public bool CanExecute(object? parameter) {
            return _tabItem.IsEffectivelyEnabled;
        }

        /// <summary>
        /// Execute method.
        /// </summary>
        public void Execute(object? parameter) {
            _tabItem.IsSelected = true;
        }

        /// <summary>
        /// The EventHandler.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        private void OnTabItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs changeArgs) {
            if (changeArgs.Property == IsEffectivelyEnabledProperty) {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}