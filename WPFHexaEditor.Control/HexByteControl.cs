using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFHexaEditor.Control.Interface;
using WPFHexaEditor.Core;
using WPFHexaEditor.Core.Bytes;

namespace WPFHexaEditor.Control {
    /// <summary>
    /// Resources are moved here;
    /// </summary>
    public partial class HexByteControl {
        #region Fonts
        private static readonly FontWeight NormalFontWeight = FontWeights.Normal;
        private static readonly FontWeight BoldFontWeight = FontWeights.Bold;
        #endregion

        #region Colors
        private static readonly SolidColorBrush BookMarkColor = new SolidColorBrush(Color.FromArgb(0xb2, 0x00, 0x00, 0xff));
        private static readonly SolidColorBrush SearchBookMarkColor = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x8b, 0x00));
        private static readonly SolidColorBrush SelectionStartBookMarkColor = new SolidColorBrush(Colors.Blue);
        private static readonly SolidColorBrush ByteModifiedMarkColor = new SolidColorBrush(Color.FromArgb(0xcc, 0x68, 0x71, 0x7c));
        private static readonly SolidColorBrush ByteDeletedMarkColor = new SolidColorBrush(Color.FromArgb(0xb2, 0xff, 0x00, 0x00));

        private static readonly SolidColorBrush FirstColor = new SolidColorBrush(Color.FromArgb(0xcc, 0x00, 0x14, 0xff));
        private static readonly SolidColorBrush SecondColor = new SolidColorBrush(Color.FromArgb(0xcc, 0x00, 0x78, 0xff));
        private static readonly SolidColorBrush ByteModifiedColor = new SolidColorBrush(Color.FromArgb(0xcc, 0x68, 0x71, 0x7c));
        private static readonly SolidColorBrush MouseOverColor = new SolidColorBrush(Color.FromArgb(0xb2, 0x00, 0x81, 0xff));
        private static readonly SolidColorBrush HighLightColor = new SolidColorBrush(Color.FromArgb(0xb2, 0xff, 0xff, 0x00));
        private static readonly SolidColorBrush ByteDeletedColor = new SolidColorBrush(Color.FromArgb(0xb2, 0xff, 0x00, 0x00));
        #endregion
    }
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WPFHexaEditor.Control"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WPFHexaEditor.Control;assembly=WPFHexaEditor.Control"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:HexByteControl2/>
    ///
    /// </summary>
    [TemplatePart(Name = FirstHexCharName, Type = typeof(TextBlock))]
    [TemplatePart(Name = SecondHexCharName, Type = typeof(TextBlock))]
    public partial class HexByteControl : System.Windows.Controls.Control, IByteControl {
        public const string FirstHexCharName = "FirstHexChar";
        public const string SecondHexCharName = "SecondHexChar";

        internal TextBlock FirstHexChar;
        internal TextBlock SecondHexChar;
        static HexByteControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HexByteControl), new FrameworkPropertyMetadata(typeof(HexByteControl)));
        }
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            FirstHexChar = GetTemplateChild(FirstHexCharName) as TextBlock;
            SecondHexChar = GetTemplateChild(SecondHexCharName) as TextBlock;
        }
        public HexByteControl() {
            DataContext = this;
            KeyDown += UserControl_KeyDown;
            MouseEnter += UserControl_MouseEnter;
            MouseLeave += UserControl_MouseLeave;
            Focusable = true;
        }
        
        private KeyDownLabel _keyDownLabel = KeyDownLabel.FirstChar;

        public event EventHandler ByteModified;
        public event EventHandler MouseSelection;
        public event EventHandler MoveNext;
        public event EventHandler MovePrevious;
        public event EventHandler MoveRight;
        public event EventHandler MoveLeft;
        public event EventHandler MoveUp;
        public event EventHandler MoveDown;
        public event EventHandler MovePageDown;
        public event EventHandler MovePageUp;
        public event EventHandler ByteDeleted;
        public event EventHandler EscapeKey;


        #region DependencyProperty

        /// <summary>
        /// Position in file
        /// </summary>
        public long BytePositionInFile {
            get { return (long)GetValue(BytePositionInFileProperty); }
            set { SetValue(BytePositionInFileProperty, value); }
        }

        public static readonly DependencyProperty BytePositionInFileProperty =
            DependencyProperty.Register("BytePositionInFile", typeof(long), typeof(HexByteControl), new PropertyMetadata(-1L));

        /// <summary>
        /// Action with this byte
        /// </summary>
        public ByteAction Action {
            get { return (ByteAction)GetValue(ActionProperty); }
            set { SetValue(ActionProperty, value); }
        }

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(ByteAction), typeof(HexByteControl),
                new FrameworkPropertyMetadata(ByteAction.Nothing,
                    new PropertyChangedCallback(Action_ValueChanged),
                    new CoerceValueCallback(Action_CoerceValue)));

        private static object Action_CoerceValue(DependencyObject d, object baseValue) {
            ByteAction value = (ByteAction)baseValue;

            if (value != ByteAction.All)
                return baseValue;
            else
                return ByteAction.Nothing;
        }

        private static void Action_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue)
                ctrl.UpdateVisual();
        }

        /// <summary>
        /// Used for selection coloring
        /// </summary>
        public bool FirstSelected {get ; set;}

        /// <summary>
        /// Byte used for this instance
        /// </summary>
        public byte? Byte {
            get { return (byte?)GetValue(ByteProperty); }
            set { SetValue(ByteProperty, value); }
        }

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("Byte", typeof(byte?), typeof(HexByteControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Byte_PropertyChanged)));

        private static void Byte_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue) {
                if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false) {
                    ctrl.ByteModified?.Invoke(ctrl, new EventArgs());
                }

                ctrl.UpdateLabelFromByte();
            }
        }

        /// <summary>
        /// Used to prevent ByteModified event occurc when we dont want! 
        /// </summary>
        public bool InternalChange {
            get { return (bool)GetValue(InternalChangeProperty); }
            set { SetValue(InternalChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InternalChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalChangeProperty =
            DependencyProperty.Register("InternalChange", typeof(bool), typeof(HexByteControl), new PropertyMetadata(false));

        #endregion

        public bool ReadOnlyMode { get; set; }

        /// <summary>
        /// Get the hex string representation of this byte
        /// </summary>
        public string HexString {
            get {
                if (Byte != null) {
                    var chArr = ByteConverters.ByteToHexCharArray(Byte.Value);
                    return new string(chArr);
                }
                else {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Get or Set if control as selected
        /// </summary>
        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(HexByteControl),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(IsSelected_PropertyChange)));
        
        private static void IsSelected_PropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue) {
                ctrl._keyDownLabel = KeyDownLabel.FirstChar;
                ctrl.UpdateVisual();
            }
        }

        /// <summary>
        /// Get of Set if control as marked as highlighted
        /// </summary>                        
        public bool IsHighLight {
            get { return (bool)GetValue(IsHighLightProperty); }
            set { SetValue(IsHighLightProperty, value); }
        }

        public static readonly DependencyProperty IsHighLightProperty =
            DependencyProperty.Register("IsHighLight", typeof(bool), typeof(HexByteControl),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(IsHighLight_PropertyChanged)));

        private static void IsHighLight_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue) {
                ctrl._keyDownLabel = KeyDownLabel.FirstChar;
                ctrl.UpdateVisual();
            }
        }

        public bool IsFocus {
            get { return (bool)GetValue(IsFocusProperty); }
            set { SetValue(IsFocusProperty, value); }
        }

        public static readonly DependencyProperty IsFocusProperty =
            DependencyProperty.Register("IsFocus", typeof(bool), typeof(HexByteControl),
                new FrameworkPropertyMetadata(false, IsFocus_PropertyChanged));

        private static void IsFocus_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            HexByteControl ctrl = d as HexByteControl;
            if (e.NewValue != e.OldValue) {
                ctrl._keyDownLabel = KeyDownLabel.FirstChar;
                ctrl.UpdateVisual();
            }
        }

        /// <summary>
        /// Update Background
        /// </summary>
        internal void UpdateVisual() {
            if (IsFocus) {
                FirstHexChar.Foreground = Brushes.White;
                SecondHexChar.Foreground = Brushes.White;
                Background = Brushes.Black;
            }
            else if (IsSelected) {
                FontWeight = NormalFontWeight;
                FirstHexChar.Foreground = Brushes.White;
                SecondHexChar.Foreground = Brushes.White;

                if (FirstSelected)
                    Background = FirstColor;
                else
                    Background = SecondColor;
            }
            else if (IsHighLight) {
                FontWeight = NormalFontWeight;
                FirstHexChar.Foreground = Brushes.Black;
                SecondHexChar.Foreground = Brushes.Black;

                Background = HighLightColor;
            }
            else if (Action != ByteAction.Nothing) {
                switch (Action) {
                    case ByteAction.Modified:
                        FontWeight = BoldFontWeight;
                        Background = ByteModifiedColor;
                        FirstHexChar.Foreground = Brushes.Black;
                        SecondHexChar.Foreground = Brushes.Black;
                        break;
                    case ByteAction.Deleted:
                        FontWeight = BoldFontWeight;
                        Background = ByteDeletedColor;
                        FirstHexChar.Foreground = Brushes.Black;
                        SecondHexChar.Foreground = Brushes.Black;
                        break;
                }
            }
            else {
                FontWeight = NormalFontWeight;
                Background = Brushes.Transparent;
                FirstHexChar.Foreground = Brushes.Black;
                SecondHexChar.Foreground = Brushes.Black;
            }
        }


        long priLevel = 0;
        private void UpdateLabelFromByte() {
            if (Byte != null) {
                var bt = Byte.Value;
                ThreadPool.QueueUserWorkItem(cb => {
                    var curLevel = ++priLevel;
                    var hexabyte = ByteConverters.ByteToHexCharArray(bt);
                    if (priLevel == curLevel) {
                        this.Dispatcher.Invoke(() => {
                            FirstHexChar.Text = hexabyte[0].ToString();
                            SecondHexChar.Text = hexabyte[1].ToString();
                        });
                    }
                });
            }
            else {
                FirstHexChar.Text = string.Empty;
                SecondHexChar.Text = string.Empty;
            }
        }
        
        private void UserControl_KeyDown(object sender, KeyEventArgs e) {
            if (KeyValidator.IsUpKey(e.Key)) {
                e.Handled = true;
                MoveUp?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsDownKey(e.Key)) {
                e.Handled = true;
                MoveDown?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsLeftKey(e.Key)) {
                e.Handled = true;
                MoveLeft?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsRightKey(e.Key)) {
                e.Handled = true;
                MoveRight?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsPageDownKey(e.Key)) {
                e.Handled = true;
                MovePageDown?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsPageUpKey(e.Key)) {
                e.Handled = true;
                MovePageUp?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsDeleteKey(e.Key)) {
                if (!ReadOnlyMode) {
                    e.Handled = true;
                    ByteDeleted?.Invoke(this, new EventArgs());

                    return;
                }
            }
            else if (KeyValidator.IsBackspaceKey(e.Key)) {
                e.Handled = true;
                ByteDeleted?.Invoke(this, new EventArgs());

                MovePrevious?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsEscapeKey(e.Key)) {
                e.Handled = true;
                EscapeKey?.Invoke(this, new EventArgs());
                return;
            }

            //MODIFY BYTE
            if (!ReadOnlyMode)
                if (KeyValidator.IsHexKey(e.Key)) {
                    string key;
                    if (KeyValidator.IsNumericKey(e.Key))
                        key = KeyValidator.GetDigitFromKey(e.Key).ToString();
                    else
                        key = e.Key.ToString().ToLower();

                    switch (_keyDownLabel) {
                        case KeyDownLabel.FirstChar:
                            FirstHexChar.Text = key;
                            _keyDownLabel = KeyDownLabel.SecondChar;
                            Action = ByteAction.Modified;
                            Byte = ByteConverters.HexToByte(FirstHexChar.Text.ToString() + SecondHexChar.Text.ToString())[0];
                            break;
                        case KeyDownLabel.SecondChar:
                            SecondHexChar.Text = key;
                            _keyDownLabel = KeyDownLabel.NextPosition;

                            Action = ByteAction.Modified;
                            Byte = ByteConverters.HexToByte(FirstHexChar.Text.ToString() + SecondHexChar.Text.ToString())[0];

                            //Move focus event
                            MoveNext?.Invoke(this, new EventArgs());
                            break;
                    }
                }
        }

        private void UpdateHexString() {
            if (Byte != null) {
                var chArr = ByteConverters.ByteToHexCharArray(Byte.Value);
                //HexString = new string(chArr);
            }
            else {
                //HexString = string.Empty;
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e) {
            if (Byte != null)
                if (Action != ByteAction.Modified &&
                    Action != ByteAction.Deleted &&
                    Action != ByteAction.Added &&
                    !IsSelected && !IsHighLight && !IsFocus)
                    Background = MouseOverColor;

            if (e.LeftButton == MouseButtonState.Pressed)
                MouseSelection?.Invoke(this, e);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e) {
            if (Byte != null)
                if (Action != ByteAction.Modified &&
                    Action != ByteAction.Deleted &&
                    Action != ByteAction.Added &&
                    !IsSelected && !IsHighLight && !IsFocus)
                    Background = Brushes.Transparent;
        }

    }
}
