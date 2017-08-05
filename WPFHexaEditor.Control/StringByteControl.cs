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
using WPFHexaEditor.Control.Interface;
using WPFHexaEditor.Core;
using WPFHexaEditor.Core.Bytes;
using WPFHexaEditor.Core.CharacterTable;

namespace WPFHexaEditor.Control {
    /// <summary>
    /// Dictionaries are move here for better performance;
    /// </summary>
    internal partial class StringByteControl {
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

    internal partial class StringByteControl : Border, IByteControl {
        private TBLStream _TBLCharacterTable = null;

        public event EventHandler Click;
        public event EventHandler RightClick;
        public event EventHandler MouseSelection;
        public event EventHandler StringByteModified;
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

        private void LoadDict(string url) {
            var ttLocaltor = new System.Uri(url, System.UriKind.Relative);
            var ttRes = new ResourceDictionary();
            ttRes.Source = ttLocaltor;
            this.Resources.MergedDictionaries.Add(ttRes);
        }
        public StringByteControl() {
            LoadDict("/WPFHexaEditor;component/Resources/Dictionary/ToolTipDictionary.xaml");
            
            Width = 12;
            Height = 22;
            
            Focusable = true;
            DataContext = this;
            //TextAlignment = TextAlignment.Center;
            var txtBinding = new Binding();
            txtBinding.Source = this.FindResource("ByteToolTip");
            txtBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            txtBinding.Mode = BindingMode.OneWay;
            this.SetBinding(TextBlock.ToolTipProperty, txtBinding);


            MouseEnter += UserControl_MouseEnter;
            MouseLeave += UserControl_MouseLeave;
            KeyDown += UserControl_KeyDown;
            MouseDown += StringByteLabel_MouseDown;
        }

        #region DependencyProperty
        /// <summary>
        /// Position in file
        /// </summary>
        public long BytePositionInFile {
            get { return (long)GetValue(BytePositionInFileProperty); }
            set { SetValue(BytePositionInFileProperty, value); }
        }

        public static readonly DependencyProperty BytePositionInFileProperty =
            DependencyProperty.Register("BytePositionInFile", typeof(long), typeof(StringByteControl), new PropertyMetadata(-1L));

        /// <summary>
        /// Used for selection coloring
        /// </summary>
        public bool FirstSelected { get; set; }

        /// <summary>
        /// Byte used for this instance
        /// </summary>
        public byte? Byte {
            get { return (byte?)GetValue(ByteProperty); }
            set { SetValue(ByteProperty, value); }
        }

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("Byte", typeof(byte?), typeof(StringByteControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Byte_PropertyChanged)));

        private static void Byte_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue) {
                if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false) {
                    ctrl.StringByteModified?.Invoke(ctrl, new EventArgs());
                }
                ctrl.UpdateLabelFromByte();
                ctrl.UpdateVisual();

            }
        }


        #endregion

        /// <summary>
        /// Next Byte of this instance (used for TBL/MTE decoding)
        /// </summary>
        public byte? ByteNext {
            get { return (byte?)GetValue(ByteNextProperty); }
            set { SetValue(ByteNextProperty, value); }
        }

        public static readonly DependencyProperty ByteNextProperty =
            DependencyProperty.Register("ByteNext", typeof(byte?), typeof(StringByteControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ByteNext_PropertyChanged)));

        private static void ByteNext_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue) {
                //if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false)
                //    ctrl.StringByteModified?.Invoke(ctrl, new EventArgs());

                ctrl.UpdateLabelFromByte();
                //ctrl.UpdateHexString();

                ctrl.UpdateVisual();

            }
        }

        /// <summary>
        /// Get or set if control as selected
        /// </summary>
        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(StringByteControl),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsSelected_PropertyChangedCallBack)));

        private static void IsSelected_PropertyChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
                ctrl.UpdateVisual();
        }

        /// <summary>
        /// Get the hex string {00} representation of this byte
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

        //public static readonly DependencyProperty HexStringProperty =
        //    DependencyProperty.Register("HexString", typeof(string), typeof(StringByteControl),
        //        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Get of Set if control as marked as highlighted
        /// </summary>                        
        public bool IsHighLight {
            get { return (bool)GetValue(IsHighLightProperty); }
            set { SetValue(IsHighLightProperty, value); }
        }

        public static readonly DependencyProperty IsHighLightProperty =
            DependencyProperty.Register("IsHighLight", typeof(bool), typeof(StringByteControl),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(IsHighLight_PropertyChanged)));

        private static void IsHighLight_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
                ctrl.UpdateVisual();
        }

        /// <summary>
        /// The Focused Property;
        /// </summary>
        public bool IsFocus {
            get { return (bool)GetValue(IsFocusProperty); }
            set { SetValue(IsFocusProperty, value); }
        }

        public static readonly DependencyProperty IsFocusProperty =
            DependencyProperty.Register("IsFocus", typeof(bool), typeof(StringByteControl),
                new FrameworkPropertyMetadata(false, IsFocus_PropertyChanged));

        private static void IsFocus_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;
            if (e.NewValue != e.OldValue) {
                ctrl.UpdateVisual();
            }
        }

        /// <summary>
        /// Used to prevent StringByteModified event occurc when we dont want! 
        /// </summary>
        public bool InternalChange {
            get { return (bool)GetValue(InternalChangeProperty); }
            set { SetValue(InternalChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InternalChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalChangeProperty =
            DependencyProperty.Register("InternalChange", typeof(bool), typeof(StringByteControl), new PropertyMetadata(false));

        /// <summary>
        /// Action with this byte
        /// </summary>
        public ByteAction Action {
            get { return (ByteAction)GetValue(ActionProperty); }
            set { SetValue(ActionProperty, value); }
        }

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(ByteAction), typeof(StringByteControl),
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
            var ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
                ctrl.UpdateVisual();
        }

        /// <summary>
        /// Show or not Multi Title Enconding (MTE) are loaded in TBL file
        /// </summary>
        public bool TBL_ShowMTE {
            get { return (bool)GetValue(TBL_ShowMTEProperty); }
            set { SetValue(TBL_ShowMTEProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TBL_ShowMTE.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TBL_ShowMTEProperty =
            DependencyProperty.Register("TBL_ShowMTE", typeof(bool), typeof(StringByteControl),
                new FrameworkPropertyMetadata(true,
                    new PropertyChangedCallback(TBL_ShowMTE_PropetyChanged)));

        private static void TBL_ShowMTE_PropetyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;

            ctrl.UpdateLabelFromByte();
        }

        #region Characters tables
        /// <summary>
        /// Type of caracter table are used un hexacontrol. 
        /// For now, somes character table can be readonly but will change in future
        /// </summary>
        public CharacterTableType TypeOfCharacterTable {
            get { return (CharacterTableType)GetValue(TypeOfCharacterTableProperty); }
            set { SetValue(TypeOfCharacterTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TypeOfCharacterTableType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeOfCharacterTableProperty =
            DependencyProperty.Register("TypeOfCharacterTable", typeof(CharacterTableType), typeof(StringByteControl),
                new FrameworkPropertyMetadata(CharacterTableType.ASCII,
                    new PropertyChangedCallback(TypeOfCharacterTable_PropertyChanged)));


        private static void TypeOfCharacterTable_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as StringByteControl;

            ctrl.UpdateLabelFromByte();
        }

        public TBLStream TBLCharacterTable {
            get {
                return _TBLCharacterTable;
            }
            set {
                _TBLCharacterTable = value;
            }
        }

        #endregion Characters tables

        //storage the action level one by one.
        private long priLevel = 0;
        //I have noticed that this method cost most of time when refreshing.
        /// <summary>
        /// Update control label from byte property
        /// </summary>
        private void UpdateLabelFromByte() {
            if (Byte != null) {
                var curLevel = ++priLevel;
                var bt = Byte.Value;
                var chTable = TypeOfCharacterTable;

                ThreadPool.QueueUserWorkItem(cb => {
                    switch (chTable) {
                        case CharacterTableType.ASCII:
                            //Text = ByteConverters.ByteToChar(bt).ToString();
                            var ch = ByteConverters.ByteToChar(bt).ToString();
                            //Check whether the action has been out of "time".to aviod unnessarsery refreshing.
                            if (curLevel == priLevel) {
                                this.Dispatcher.Invoke(() => {
                                    //Text = ch;
                                });
                            }

                            break;
                        case CharacterTableType.TBLFile:
                            ReadOnlyMode = true;

                            if (_TBLCharacterTable != null) {
                                string content = "#";
                                string MTE = (ByteConverters.ByteToHex2(Byte.Value) + ByteConverters.ByteToHex(ByteNext ?? 0)).ToUpper();
                                content = _TBLCharacterTable.FindTBLMatch(MTE, true);

                                if (content == "#")
                                    content = _TBLCharacterTable.FindTBLMatch(ByteConverters.ByteToHex(Byte.Value).ToUpper().ToUpper(), true);

                                if (curLevel == priLevel) {
                                    this.Dispatcher.Invoke(() => {
                                        //Text = content;
                                        //Adjuste wight
                                        if (content.Length == 1)
                                            Width = 12;
                                        else if (content.Length == 2)
                                            Width = 12 + content.Length * 2D;
                                        else if (content.Length > 2)
                                            Width = 12 + content.Length * 3.8D;
                                    });
                                }
                            }
                            else
                                goto case CharacterTableType.ASCII;
                            break;
                    }
                });
            }
            else { }
                //Text = string.Empty;
        }
        
        /// <summary>
        //    /// Update Background,foreground and font property
        //    /// </summary>
        internal void UpdateVisual() {
            if (IsFocus) {
                //Foreground = Brushes.White;
                Background = Brushes.Black;
            }
            else if (IsSelected) {
                //FontWeight = NormalFontWeight;
                //Foreground = Brushes.White;

                if (FirstSelected)
                    Background = FirstColor;
                else
                    Background = SecondColor;

                return;
            }
            else if (IsHighLight) {
                //FontWeight = NormalFontWeight;
                //Foreground = Brushes.Black;

                Background = HighLightColor;

                return;
            }
            else if (IsFocus) {
                //Foreground = Brushes.White;
                Background = Brushes.Black;
            }
            else if (Action != ByteAction.Nothing) {
                switch (Action) {
                    case ByteAction.Modified:
                        //FontWeight = BoldFontWeight;
                        Background = ByteModifiedColor;
                        //Foreground = Brushes.Black;
                        break;
                    case ByteAction.Deleted:
                        //FontWeight = BoldFontWeight;
                        Background = ByteDeletedColor;
                        //Foreground = Brushes.Black;
                        break;
                }

                return;
            }
            else {
                
                //FontWeight = NormalFontWeight;
                Background = Brushes.Transparent;
                //Foreground = Brushes.Black;

                if (TypeOfCharacterTable == CharacterTableType.TBLFile) { }
                    //switch (DTE.TypeDTE((string)Text)) {
                    //    case DTEType.DualTitleEncoding:
                    //        //Foreground = Brushes.Red;
                    //        break;
                    //    case DTEType.MultipleTitleEncoding:
                    //        Foreground = Brushes.Blue;
                    //        break;
                    //    default:
                    //        Foreground = Brushes.Black;
                    //        break;
                    //}
            }
        }

        public bool ReadOnlyMode { get; set; }

        private void UserControl_KeyDown(object sender, KeyEventArgs e) {
            if (KeyValidator.IsIgnoredKey(e.Key)) {
                e.Handled = true;
                return;
            }
            else if (KeyValidator.IsUpKey(e.Key)) {
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
                if (!ReadOnlyMode) {
                    e.Handled = true;
                    ByteDeleted?.Invoke(this, new EventArgs());

                    MovePrevious?.Invoke(this, new EventArgs());

                    return;
                }
            }
            else if (KeyValidator.IsEscapeKey(e.Key)) {
                e.Handled = true;
                EscapeKey?.Invoke(this, new EventArgs());
                return;
            }

            //MODIFY ASCII... 
            //TODO : MAKE BETTER KEYDETECTION AND EXPORT IN KEYVALIDATOR
            if (!ReadOnlyMode) {
                bool isok = false;

                if (Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled) {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift) {
                        //Text = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift) {
                        isok = true;
                        //Text = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString().ToLower();
                    }
                }
                else {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift) {
                        //Text = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString().ToLower();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift) {
                        isok = true;
                        //Text = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString();
                    }
                }

                //Move focus event
                if (isok)
                    if (MoveNext != null) {
                        Action = ByteAction.Modified;
                        //Byte = ByteConverters.CharToByte(Text.ToString()[0]);

                        MoveNext(this, new EventArgs());
                    }
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

        private void StringByteLabel_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Focus();

                Click?.Invoke(this, e);
            }

            if (e.RightButton == MouseButtonState.Pressed) {
                RightClick?.Invoke(this, e);
            }
        }
    }
}
