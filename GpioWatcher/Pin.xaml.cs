using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GpioWatcher
{
    public sealed partial class Pin : UserControl
    {
        private static readonly Brush InactiveBrush = new SolidColorBrush(Colors.DarkSlateGray);
        private static readonly Brush ActiveBrush = new SolidColorBrush(Colors.White);

        public Pin()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double size = Math.Min(ActualWidth, ActualHeight);
            Width = size;
            Height = size;

            Ellipse.StrokeThickness = size * 0.22D;
        }

        public static readonly DependencyProperty IsUnavailableProperty =
            DependencyProperty.Register("IsUnavailable", typeof (bool), typeof (Pin), new PropertyMetadata(false, IsUnavailablePropertyChanged));

        private static void IsUnavailablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Pin pin = d as Pin;
            if (pin == null) return;

            bool b = (e.NewValue as bool?) ?? false;
            pin.Ellipse.Fill = InactiveBrush;
            pin.OnPropertyChanged(TooltipDescriptionPropertyName);
        }

        public bool IsUnavailable
        {
            get { return (bool) GetValue(IsUnavailableProperty); }
            set { SetValue(IsUnavailableProperty, value); }
        }

        public static readonly DependencyProperty PinColorProperty =
            DependencyProperty.Register("PinColor", typeof(Brush), typeof(Pin), null);

        public Brush PinColor
        {
            get { return (Brush)GetValue(PinColorProperty); }
            set { SetValue(PinColorProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(Pin), new PropertyMetadata(false, IsActivePropertyChanged));

        private static void IsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Pin pin = d as Pin;
            if (pin == null) return;

            bool b = (e.NewValue as bool?) ?? false;
            pin.Ellipse.Fill = b ? ActiveBrush : InactiveBrush;
            pin.OnPropertyChanged(TooltipDescriptionPropertyName);
        }

        public static readonly DependencyProperty ToolTipProperty =
            DependencyProperty.Register("ToolTip", typeof(string), typeof(Pin), new PropertyMetadata(string.Empty, ToolTipPropertyChanged));

        private static void ToolTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            Pin pin = d as Pin;
            if (pin == null) return;

            pin.OnPropertyChanged(TooltipDescriptionPropertyName);
        }

        public string ToolTip
        {
            get { return (string)GetValue(ToolTipProperty); }
            set { SetValue(ToolTipProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public const string TooltipDescriptionPropertyName = "TooltipDescription";

        public string TooltipDescription
        {
            get { return string.Format("{0}: {1}", ToolTip, IsUnavailable ? "Unavailable" : (IsActive ? "High" : "Low")); }
        }

        public GpioPin GpioPin { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler == null) return;
            eventHandler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
