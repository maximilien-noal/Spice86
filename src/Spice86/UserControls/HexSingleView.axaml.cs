namespace Spice86.UserControls;

using Avalonia;
using Avalonia.Controls;

using Spice86._3rdParty.Controls.HexView.Models;
using Spice86._3rdParty.Controls.HexView.Services;

public partial class HexSingleView : UserControl {
    private ILineReader? _lineReader;
    private IHexFormatter? _hexFormatter1;

    public HexSingleView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e) {
        _lineReader = new MemoryMappedLineReader(DataSource);
        _hexFormatter1 = new HexFormatter(DataSource.Length);
        HexViewControl1.HexFormatter = _hexFormatter1;
        HexViewControl1.InvalidateScrollable();
        base.OnDataContextChanged(e);
    }

    /// <summary>
    /// Defines a <see cref="StyledProperty{TValue}"/> for the <see cref="DataSource"/> property.
    /// </summary>
    public static readonly StyledProperty<Memory<byte>> DataSourceProperty =
        AvaloniaProperty.Register<PaletteUserControl, Memory<byte>>(nameof(DataSource));

    /// <summary>
    /// Gets or sets the source for the hexadecimal values.
    /// </summary>
    public Memory<byte> DataSource {
        get { return GetValue(DataSourceProperty); }
        set { SetValue(DataSourceProperty, value); }
    }
}