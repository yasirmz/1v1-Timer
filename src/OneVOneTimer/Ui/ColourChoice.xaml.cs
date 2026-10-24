using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OneVOneTimer.Ui;

/// <summary>One of the timer's colours: a swatch with its code that opens the colour picker. <see cref="Colour"/> is "#RRGGBB".</summary>
public partial class ColourChoice : UserControl
{
    public static readonly DependencyProperty ColourProperty = DependencyProperty.Register(nameof(Colour), typeof(string), typeof(ColourChoice),
        new FrameworkPropertyMetadata("#FFFFFF", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((ColourChoice)d).Follow()));

    static readonly DependencyPropertyKey BrushKey = DependencyProperty.RegisterReadOnly(nameof(Brush), typeof(Brush), typeof(ColourChoice),
        new PropertyMetadata(Brushes.White));

    public ColourChoice()
    {
        InitializeComponent();
        Follow();
    }

    public string Colour
    {
        get => (string)GetValue(ColourProperty);
        set => SetValue(ColourProperty, value);
    }

    /// <summary>The swatch's fill: <see cref="Colour"/>.</summary>
    public Brush Brush => (Brush)GetValue(BrushKey.DependencyProperty);

    void Follow()
    {
        var brush = new SolidColorBrush(ColourMath.ToColor(Colour));
        brush.Freeze();
        SetValue(BrushKey, brush);
    }

    void OnOpen(object sender, RoutedEventArgs e) => PickerPopup.IsOpen = true;
}
