using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class GpioWatcher : UserControl, INotifyPropertyChanged
    {
        private Pin[] _pins;
        public Pin[] Pins
        {
            get
            {
                if (_pins == null)
                {
                    _pins = new[]
                    {
                        Pin01, Pin02, Pin03, Pin04, Pin05, Pin06, Pin07, Pin08, Pin09, Pin10,
                        Pin11, Pin12, Pin13, Pin14, Pin15, Pin16, Pin17, Pin18, Pin19, Pin20,
                        Pin21, Pin22, Pin23, Pin24, Pin25, Pin26, Pin27, Pin28, Pin29, Pin30,
                        Pin31, Pin32, Pin33, Pin34, Pin35, Pin36, Pin37, Pin38, Pin39, Pin40
                    };
                }

                return _pins;
            }
        }

        public Dictionary<int, Pin> GpioPinDictionary { get; set; }

        private object _currentPinLock = new object();
        private Pin _currentPin;

        private Pin _pressedPin;

        private GpioController _gpioController;

        public const string CurrentToolTipPropertyName = "CurrentToolTip";
       
        public string CurrentToolTip
        {
            get { return _currentPin != null ? _currentPin.TooltipDescription : string.Empty; }            
        }

       
        public GpioWatcher()
        {
            this.InitializeComponent();
            InitializePointerOnPinEvents();
            InitializeGpioMap();
            InitializeGpio();

        }

        private void InitializeGpioMap()
        {
            GpioPinDictionary = new Dictionary<int, Pin>();

            foreach (Pin pin in Pins)
            {
                if (pin.ToolTip.StartsWith("GPIO", StringComparison.CurrentCultureIgnoreCase))
                {
                    int i;
                    int.TryParse(pin.ToolTip.Substring(4), out i);
                    GpioPinDictionary.Add(i, pin);
                }
            }

        }

        private void InitializeGpio()
        {
             _gpioController = GpioController.GetDefault();
            List<int> keysToRemove = new List<int>();

            foreach (int key in GpioPinDictionary.Keys)
            {
                GpioPin gpioPin;
                GpioOpenStatus status;
                _gpioController.TryOpenPin(key, GpioSharingMode.SharedReadOnly, out gpioPin, out status);

                if (status == GpioOpenStatus.PinOpened)
                {
                    Pin pin = GpioPinDictionary[key];
                    pin.GpioPin = gpioPin;
                    pin.IsActive = gpioPin.Read() == GpioPinValue.High;                    
                    gpioPin.ValueChanged += GpioPinOnValueChanged;
                } else if (status == GpioOpenStatus.PinUnavailable)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (int i in keysToRemove)
            {
                GpioPinDictionary[i].IsUnavailable = true;                
                GpioPinDictionary.Remove(i);
            }
        }

        private async void GpioPinOnValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Pin pin;
            if (!GpioPinDictionary.TryGetValue(sender.PinNumber, out pin)) return;
            bool isActive = args.Edge == GpioPinEdge.RisingEdge;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { pin.IsActive = isActive; });            
        }

        private void InitializePointerOnPinEvents()
        {
            foreach (Pin pin in Pins)
            {
                pin.PointerPressed += PinOnPointerPressed;
                pin.PointerReleased += PinOnPointerReleased;
                pin.PointerEntered += PinOnPointerEntered;
                pin.PointerExited += PinOnPointerExited;
            }
        }

        private void PinOnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            Pin releasedPin = sender as Pin;
            if (_pressedPin != releasedPin || _pressedPin?.GpioPin == null) return;

            using (GpioPin gpioPin = _gpioController.OpenPin(_pressedPin.GpioPin.PinNumber))
            {
                GpioPinValue currentValue = gpioPin.Read();
                gpioPin.Write(currentValue == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
            }

            _pressedPin = null;
        }

        private void PinOnPointerPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            _pressedPin = sender as Pin;
        }

        private void PinOnPointerExited(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            Pin senderPin = sender as Pin;
            bool change = false;

            lock (_currentPinLock)
            {
                if (_currentPin == senderPin)
                {
                    change = true;
                    _currentPin = null;
                }
            }

            if (change)
                OnPropertyChanged(CurrentToolTipPropertyName);


        }

        private void PinOnPointerEntered(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            lock (_currentPinLock)
            {
                _currentPin = sender as Pin;
            }

            OnPropertyChanged(CurrentToolTipPropertyName);
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler == null) return;
            eventHandler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
