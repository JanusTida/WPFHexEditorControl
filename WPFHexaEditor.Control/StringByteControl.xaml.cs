//////////////////////////////////////////////
// MIT License  - 2016-2017
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFHexaEditor.Core;
using WPFHexaEditor.Core.Bytes;
using WPFHexaEditor.Core.CharacterTable;

namespace WPFHexaEditor.Control
{
    ///// <summary>
    ///// Interaction logic for StringByteControl.xaml
    ///// </summary>
    //internal partial class StringByteControl : UserControl
    //{
    //    //private bool _isByteModified = false;
    //    private bool _readOnlyMode;
    //    private HexaEditor _parent;

    //    private TBLStream _TBLCharacterTable = null;

    //    public event EventHandler Click;

    //    public event EventHandler RightClick;

    //    public event EventHandler MouseSelection;

    //    public event EventHandler StringByteModified;

    //    public event EventHandler MoveNext;

    //    public event EventHandler MovePrevious;

    //    public event EventHandler MoveRight;

    //    public event EventHandler MoveLeft;

    //    public event EventHandler MoveUp;

    //    public event EventHandler MoveDown;

    //    public event EventHandler MovePageDown;

    //    public event EventHandler MovePageUp;

    //    public event EventHandler ByteDeleted;

    //    public event EventHandler EscapeKey;

    //    public event EventHandler CTRLZKey;

    //    public event EventHandler CTRLVKey;

    //    public event EventHandler CTRLCKey;

    //    public event EventHandler CTRLAKey;

    //    /// <summary>
    //    /// Default constructor
    //    /// </summary>
    //    public StringByteControl(HexaEditor parent)
    //    {
    //        InitializeComponent();

    //        DataContext = this;
    //        _parent = parent;
    //    }

    //    #region DependencyProperty

    //    /// <summary>
    //    /// Position in file
    //    /// </summary>
    //    public long BytePositionInFile
    //    {
    //        get { return (long)GetValue(BytePositionInFileProperty); }
    //        set { SetValue(BytePositionInFileProperty, value); }
    //    }

    //    public static readonly DependencyProperty BytePositionInFileProperty =
    //        DependencyProperty.Register("BytePositionInFile", typeof(long), typeof(StringByteControl), new PropertyMetadata(-1L));

    //    /// <summary>
    //    /// Used for selection coloring
    //    /// </summary>
    //    public bool StringByteFirstSelected
    //    {
    //        get { return (bool)GetValue(StringByteFirstSelectedProperty); }
    //        set { SetValue(StringByteFirstSelectedProperty, value); }
    //    }

    //    public static readonly DependencyProperty StringByteFirstSelectedProperty =
    //        DependencyProperty.Register("StringByteFirstSelected", typeof(bool), typeof(StringByteControl), new PropertyMetadata(true));

    //    /// <summary>
    //    /// Byte used for this instance
    //    /// </summary>
    //    public byte? Byte
    //    {
    //        get { return (byte?)GetValue(ByteProperty); }
    //        set { SetValue(ByteProperty, value); }
    //    }

    //    public static readonly DependencyProperty ByteProperty =
    //        DependencyProperty.Register("Byte", typeof(byte?), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Byte_PropertyChanged)));

    //    private static void Byte_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        if (e.NewValue != e.OldValue)
    //        {
    //            if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false)
    //                ctrl.StringByteModified?.Invoke(ctrl, new EventArgs());

    //            ctrl.UpdateLabelFromByte();
    //            ctrl.UpdateHexString();

    //            ctrl.UpdateVisual();
    //        }
    //    }

    //    /// <summary>
    //    /// Next Byte of this instance (used for TBL/MTE decoding)
    //    /// </summary>
    //    public byte? ByteNext
    //    {
    //        get { return (byte?)GetValue(ByteNextProperty); }
    //        set { SetValue(ByteNextProperty, value); }
    //    }

    //    public static readonly DependencyProperty ByteNextProperty =
    //        DependencyProperty.Register("ByteNext", typeof(byte?), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ByteNext_PropertyChanged)));

    //    private static void ByteNext_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        if (e.NewValue != e.OldValue)
    //        {
    //            ctrl.UpdateLabelFromByte();
    //            ctrl.UpdateVisual();
    //        }
    //    }

    //    /// <summary>
    //    /// Get or set if control as selected
    //    /// </summary>
    //    public bool IsSelected
    //    {
    //        get { return (bool)GetValue(IsSelectedProperty); }
    //        set { SetValue(IsSelectedProperty, value); }
    //    }

    //    public static readonly DependencyProperty IsSelectedProperty =
    //        DependencyProperty.Register("IsSelected", typeof(bool), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsSelected_PropertyChangedCallBack)));

    //    private static void IsSelected_PropertyChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        if (e.NewValue != e.OldValue)
    //            ctrl.UpdateVisual();
    //    }

    //    /// <summary>
    //    /// Get the hex string {00} representation of this byte
    //    /// </summary>
    //    public string HexString
    //    {
    //        get { return (string)GetValue(HexStringProperty); }
    //        internal set { SetValue(HexStringProperty, value); }
    //    }

    //    public static readonly DependencyProperty HexStringProperty =
    //        DependencyProperty.Register("HexString", typeof(string), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(string.Empty));

    //    /// <summary>
    //    /// Get of Set if control as marked as highlighted
    //    /// </summary>
    //    public bool IsHighLight
    //    {
    //        get { return (bool)GetValue(IsHighLightProperty); }
    //        set { SetValue(IsHighLightProperty, value); }
    //    }

    //    public static readonly DependencyProperty IsHighLightProperty =
    //        DependencyProperty.Register("IsHighLight", typeof(bool), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(false,
    //                new PropertyChangedCallback(IsHighLight_PropertyChanged)));

    //    private static void IsHighLight_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        if (e.NewValue != e.OldValue)
    //            ctrl.UpdateVisual();
    //    }

    //    /// <summary>
    //    /// Used to prevent StringByteModified event occurc when we dont want!
    //    /// </summary>
    //    public bool InternalChange
    //    {
    //        get { return (bool)GetValue(InternalChangeProperty); }
    //        set { SetValue(InternalChangeProperty, value); }
    //    }

    //    // Using a DependencyProperty as the backing store for InternalChange.  This enables animation, styling, binding, etc...
    //    public static readonly DependencyProperty InternalChangeProperty =
    //        DependencyProperty.Register("InternalChange", typeof(bool), typeof(StringByteControl), new PropertyMetadata(false));

    //    /// <summary>
    //    /// Action with this byte
    //    /// </summary>
    //    public ByteAction Action
    //    {
    //        get { return (ByteAction)GetValue(ActionProperty); }
    //        set { SetValue(ActionProperty, value); }
    //    }

    //    public static readonly DependencyProperty ActionProperty =
    //        DependencyProperty.Register("Action", typeof(ByteAction), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(ByteAction.Nothing,
    //                new PropertyChangedCallback(Action_ValueChanged),
    //                new CoerceValueCallback(Action_CoerceValue)));

    //    private static object Action_CoerceValue(DependencyObject d, object baseValue)
    //    {
    //        ByteAction value = (ByteAction)baseValue;

    //        if (value != ByteAction.All)
    //            return baseValue;
    //        else
    //            return ByteAction.Nothing;
    //    }

    //    private static void Action_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        if (e.NewValue != e.OldValue)
    //            ctrl.UpdateVisual();
    //    }

    //    #endregion DependencyProperty

    //    #region Characters tables

    //    /// <summary>
    //    /// Show or not Multi Title Enconding (MTE) are loaded in TBL file
    //    /// </summary>
    //    public bool TBL_ShowMTE
    //    {
    //        get { return (bool)GetValue(TBL_ShowMTEProperty); }
    //        set { SetValue(TBL_ShowMTEProperty, value); }
    //    }

    //    // Using a DependencyProperty as the backing store for TBL_ShowMTE.  This enables animation, styling, binding, etc...
    //    public static readonly DependencyProperty TBL_ShowMTEProperty =
    //        DependencyProperty.Register("TBL_ShowMTE", typeof(bool), typeof(StringByteControl), 
    //            new FrameworkPropertyMetadata(true, 
    //                new PropertyChangedCallback(TBL_ShowMTE_PropetyChanged)));

    //    private static void TBL_ShowMTE_PropetyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        ctrl.UpdateLabelFromByte();
    //    }

    //    /// <summary>
    //    /// Type of caracter table are used un hexacontrol.
    //    /// For now, somes character table can be readonly but will change in future
    //    /// </summary>
    //    public CharacterTableType TypeOfCharacterTable
    //    {
    //        get { return (CharacterTableType)GetValue(TypeOfCharacterTableProperty); }
    //        set { SetValue(TypeOfCharacterTableProperty, value); }
    //    }

    //    // Using a DependencyProperty as the backing store for TypeOfCharacterTableType.  This enables animation, styling, binding, etc...
    //    public static readonly DependencyProperty TypeOfCharacterTableProperty =
    //        DependencyProperty.Register("TypeOfCharacterTable", typeof(CharacterTableType), typeof(StringByteControl),
    //            new FrameworkPropertyMetadata(CharacterTableType.ASCII,
    //                new PropertyChangedCallback(TypeOfCharacterTable_PropertyChanged)));

    //    private static void TypeOfCharacterTable_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var ctrl = d as StringByteControl;

    //        ctrl.UpdateLabelFromByte();
    //        ctrl.UpdateHexString();
    //    }

    //    public TBLStream TBLCharacterTable
    //    {
    //        get
    //        {
    //            return _TBLCharacterTable;
    //        }
    //        set
    //        {
    //            _TBLCharacterTable = value;
    //        }
    //    }

    //    #endregion Characters tables

    //    /// <summary>
    //    /// Update control label from byte property
    //    /// </summary>
    //    private void UpdateLabelFromByte()
    //    {
    //        if (Byte != null)
    //        {
    //            switch (TypeOfCharacterTable)
    //            {
    //                case CharacterTableType.ASCII:
    //                    StringByteLabel.Text = ByteConverters.ByteToChar(Byte.Value).ToString();
    //                    Width = 12;
    //                    break;

    //                case CharacterTableType.TBLFile:
    //                    ReadOnlyMode = !_TBLCharacterTable.AllowEdit;

    //                    if (_TBLCharacterTable != null)
    //                    {
    //                        string content = "#";

    //                        if (TBL_ShowMTE)
    //                            if (ByteNext.HasValue)
    //                            {
    //                                string MTE = (ByteConverters.ByteToHex(Byte.Value) + ByteConverters.ByteToHex(ByteNext.Value)).ToUpper();
    //                                content = _TBLCharacterTable.FindTBLMatch(MTE, true);
    //                            }

    //                        if (content == "#")
    //                            content = _TBLCharacterTable.FindTBLMatch(ByteConverters.ByteToHex(Byte.Value).ToUpper().ToUpper(), true);

    //                        StringByteLabel.Text = content;

    //                        //TODO: CHECK FOR AUTO ADAPT TO CONTENT AND FONTSIZE
    //                        switch (DTE.TypeDTE(content))
    //                        {
    //                            case DTEType.DualTitleEncoding:
    //                                Width = 12 + content.Length * 2.2D;
    //                                break;

    //                            case DTEType.MultipleTitleEncoding:
    //                                Width = 12 + content.Length * 4.2D + (FontSize / 2);
    //                                break;

    //                            case DTEType.EndLine:
    //                                Width = 24;
    //                                break;

    //                            case DTEType.EndBlock:
    //                                Width = 34;
    //                                break;

    //                            default:
    //                                Width = 12;
    //                                break;
    //                        }
    //                    }
    //                    else
    //                        goto case CharacterTableType.ASCII;
    //                    break;
    //            }
    //        }
    //        else
    //            StringByteLabel.Text = "";
    //    }

    //    private void UpdateHexString()
    //    {
    //        if (Byte != null)
    //            HexString = $"0x{ByteConverters.ByteToHex(Byte.Value)}";
    //        else
    //            HexString = string.Empty;
    //    }

    //    /// <summary>
    //    /// Update Background,foreground and font property
    //    /// </summary>
    //    internal void UpdateVisual()
    //    {
    //        if (IsSelected)
    //        {
    //            FontWeight = _parent.FontWeight;
    //            StringByteLabel.Foreground = _parent.ForegroundContrast;

    //            if (StringByteFirstSelected)
    //                Background = _parent.SelectionFirstColor;
    //            else
    //                Background = _parent.SelectionSecondColor;

    //            return;
    //        }
    //        else if (IsHighLight)
    //        {
    //            FontWeight = _parent.FontWeight;
    //            StringByteLabel.Foreground = _parent.Foreground;

    //            Background = _parent.HighLightColor;

    //            return;
    //        }
    //        else if (Action != ByteAction.Nothing)
    //        {
    //            switch (Action)
    //            {
    //                case ByteAction.Modified:
    //                    FontWeight = (FontWeight)TryFindResource("BoldFontWeight");
    //                    Background = _parent.ByteModifiedColor; 
    //                    StringByteLabel.Foreground = _parent.Foreground;
    //                    break;

    //                case ByteAction.Deleted:
    //                    FontWeight = (FontWeight)TryFindResource("BoldFontWeight");
    //                    Background = _parent.ByteDeletedColor;
    //                    StringByteLabel.Foreground = _parent.Foreground;
    //                    break;
    //            }

    //            return;
    //        }
    //        else //TBL COLORING
    //        {
    //            FontWeight = _parent.FontWeight;
    //            Background = Brushes.Transparent;
    //            StringByteLabel.Foreground = _parent.Foreground;

    //            if (TypeOfCharacterTable == CharacterTableType.TBLFile)
    //                switch (DTE.TypeDTE(StringByteLabel.Text))
    //                {
    //                    case DTEType.DualTitleEncoding:
    //                        StringByteLabel.Foreground = _parent.TBL_DTEColor;
    //                        break;

    //                    case DTEType.MultipleTitleEncoding:
    //                        StringByteLabel.Foreground = _parent.TBL_MTEColor;
    //                        break;

    //                    case DTEType.EndLine:
    //                        StringByteLabel.Foreground = _parent.TBL_EndLineColor;
    //                        break;

    //                    case DTEType.EndBlock:
    //                        StringByteLabel.Foreground = _parent.TBL_EndBlockColor;
    //                        break;

    //                    default:
    //                        StringByteLabel.Foreground = _parent.TBL_DefaultColor;
    //                        break;
    //                }
    //        }
    //    }

    //    public bool ReadOnlyMode
    //    {
    //        get
    //        {
    //            return _readOnlyMode;
    //        }
    //        set
    //        {
    //            _readOnlyMode = value;
    //        }
    //    }

    //    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    //    {
    //        if (KeyValidator.IsIgnoredKey(e.Key))
    //        {
    //            e.Handled = true;
    //            return;
    //        }
    //        else if (KeyValidator.IsUpKey(e.Key))
    //        {
    //            e.Handled = true;
    //            MoveUp?.Invoke(this, new EventArgs());

    //            return;
    //        }
    //        else if (KeyValidator.IsDownKey(e.Key))
    //        {
    //            e.Handled = true;
    //            MoveDown?.Invoke(this, new EventArgs());

    //            return;
    //        }
    //        else if (KeyValidator.IsLeftKey(e.Key))
    //        {
    //            e.Handled = true;
    //            MoveLeft?.Invoke(this, new EventArgs());

    //            return;
    //        }
    //        else if (KeyValidator.IsRightKey(e.Key))
    //        {
    //            e.Handled = true;
    //            MoveRight?.Invoke(this, new EventArgs());

    //            return;
    //        }
    //        else if (KeyValidator.IsPageDownKey(e.Key))
    //        {
    //            e.Handled = true;
    //            MovePageDown?.Invoke(this, new EventArgs());

    //            return;
    //        }
    //        else if (KeyValidator.IsPageUpKey(e.Key))
    //        {
    //            e.Handled = true;
    //            MovePageUp?.Invoke(this, new EventArgs());

    //            return;
    //        }
    //        else if (KeyValidator.IsDeleteKey(e.Key))
    //        {
    //            if (!ReadOnlyMode)
    //            {
    //                e.Handled = true;
    //                ByteDeleted?.Invoke(this, new EventArgs());

    //                return;
    //            }
    //        }
    //        else if (KeyValidator.IsBackspaceKey(e.Key))
    //        {
    //            if (!ReadOnlyMode)
    //            {
    //                e.Handled = true;
    //                ByteDeleted?.Invoke(this, new EventArgs());

    //                if (BytePositionInFile > 0)
    //                    MovePrevious?.Invoke(this, new EventArgs());

    //                return;
    //            }
    //        }
    //        else if (KeyValidator.IsEscapeKey(e.Key))
    //        {
    //            e.Handled = true;
    //            EscapeKey?.Invoke(this, new EventArgs());
    //            return;
    //        }
    //        else if (KeyValidator.IsCtrlZKey(e.Key))
    //        {
    //            e.Handled = true;
    //            CTRLZKey?.Invoke(this, new EventArgs());
    //            return;
    //        }
    //        else if (KeyValidator.IsCtrlVKey(e.Key))
    //        {
    //            e.Handled = true;
    //            CTRLVKey?.Invoke(this, new EventArgs());
    //            return;
    //        }
    //        else if (KeyValidator.IsCtrlCKey(e.Key))
    //        {
    //            e.Handled = true;
    //            CTRLCKey?.Invoke(this, new EventArgs());
    //            return;
    //        }
    //        else if (KeyValidator.IsCtrlAKey(e.Key))
    //        {
    //            e.Handled = true;
    //            CTRLAKey?.Invoke(this, new EventArgs());
    //            return;
    //        }

    //        //MODIFY ASCII...
    //        if (!ReadOnlyMode)
    //        {
    //            bool isok = false;

    //            if (Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled)
    //            {
    //                if (Keyboard.Modifiers != ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
    //                {
    //                    StringByteLabel.Text = KeyValidator.GetCharFromKey(e.Key).ToString(); //ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key));
    //                    isok = true;
    //                }
    //                else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
    //                {
    //                    isok = true;
    //                    StringByteLabel.Text = KeyValidator.GetCharFromKey(e.Key).ToString().ToLower(); //ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString().ToLower();
    //                }
    //            }
    //            else
    //            {
    //                if (Keyboard.Modifiers != ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
    //                {
    //                    StringByteLabel.Text = KeyValidator.GetCharFromKey(e.Key).ToString().ToLower(); //ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString().ToLower();
    //                    isok = true;
    //                }
    //                else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
    //                {
    //                    isok = true;
    //                    StringByteLabel.Text = KeyValidator.GetCharFromKey(e.Key).ToString(); //ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key));
    //                }
    //            }

    //            //Move focus event
    //            if (isok)
    //                if (MoveNext != null)
    //                {
    //                    Action = ByteAction.Modified;
    //                    Byte = ByteConverters.CharToByte(StringByteLabel.Text.ToString()[0]);

    //                    MoveNext(this, new EventArgs());
    //                }
    //        }
    //    }

    //    private void UserControl_MouseEnter(object sender, MouseEventArgs e)
    //    {
    //        if (Byte != null)
    //            if (Action != ByteAction.Modified &&
    //                Action != ByteAction.Deleted &&
    //                Action != ByteAction.Added &&
    //                !IsSelected && !IsHighLight)
    //                Background = _parent.MouseOverColor; //(SolidColorBrush)TryFindResource("MouseOverColor");

    //        if (e.LeftButton == MouseButtonState.Pressed)
    //            MouseSelection?.Invoke(this, e);
    //    }

    //    private void UserControl_MouseLeave(object sender, MouseEventArgs e)
    //    {
    //        if (Byte != null)
    //            if (Action != ByteAction.Modified &&
    //                Action != ByteAction.Deleted &&
    //                Action != ByteAction.Added &&
    //                !IsSelected && !IsHighLight)
    //                Background = Brushes.Transparent;
    //    }

    //    private void StringByteLabel_MouseDown(object sender, MouseButtonEventArgs e)
    //    {
    //        if (e.LeftButton == MouseButtonState.Pressed)
    //        {
    //            Focus();

    //            Click?.Invoke(this, e);
    //        }

    //        if (e.RightButton == MouseButtonState.Pressed)
    //        {
    //            RightClick?.Invoke(this, e);
    //        }
    //    }
    //}

    /// <summary>
    /// For Betterperformance StringByteControl Are Inherited from textblock directly;
    /// </summary>
    

}