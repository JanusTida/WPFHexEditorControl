﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using WPFHexaEditor.Control.Core;

namespace WPFHexaEditor.Control
{
    /// <summary>
    /// 2016 - Derek Tremblay (derektremblay666@gmail.com)
    /// WPF Hexadecimal editor control
    /// </summary>
    public partial class HexaEditor : UserControl
    {
        private string _fileName = "";
        private const double _lineInfoHeight = 22;
        private int _bytePerLine = 16;
        private ByteProvider _provider = null;
        private double _scrollLargeChange = 100;
        private bool _readOnlyMode = false;  
                              
        //Event
        public event EventHandler SelectionStartChanged;
        public event EventHandler SelectionStopChanged;
        public event EventHandler SelectionLenghtChanged;
        public event EventHandler DataCopied;

        public HexaEditor()
        {
            InitializeComponent();

            RefreshView(true);
        }

        /// <summary>
        /// Set or Get the file with the control will show hex
        /// </summary>
        public string FileName
        {
            get
            {
                return this._fileName;
            }

            set
            {
                
                this._fileName = value;
                                
                OpenFile(value);                               
            }
        }

        /// <summary>
        /// Open file name
        /// </summary>
        /// <param name="filename"></param>
        private void OpenFile(string filename)
        {
            if (File.Exists(filename))
            {
                CloseFile();
                //bool readOnlyMode = false;
                                
                _provider = new ByteProvider(filename);
                _provider.ReadOnlyChanged += Provider_ReadOnlyChanged;
                _provider.DataCopied += Provider_DataCopied;

                RefreshView(true);

                UnSelectAll();

                UpdateSelectionColorMode(FirstColor.HexByteData);

                //if (readOnlyMode)
                //ReadOnlyMode = _provider.ReadOnlyMode;
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private void Provider_DataCopied(object sender, EventArgs e)
        {
            if (DataCopied != null)
                DataCopied(sender, e);
        }

        private void Provider_ReadOnlyChanged(object sender, EventArgs e)
        {
            if (ByteProvider.CheckIsOpen(_provider))
                ReadOnlyMode = _provider.ReadOnlyMode;
        }

        public int BytePerLine
        {
            get
            {
                return _bytePerLine;
            }
            set
            {
                _bytePerLine = value;

                RefreshView(true);                
            }
        }

        public double ScrollLargeChange {
            get
            {
                return _scrollLargeChange;
            }
            set
            {
                this._scrollLargeChange = value;

                UpdateVerticalScroll();
            }
        }

        /// <summary>
        /// Put the control on readonly mode.
        /// </summary>
        public bool ReadOnlyMode
        {
            get
            {
                return _readOnlyMode;
            }

            set
            {             
                _readOnlyMode = value;

                RefreshView(false);
            }
        }

        /// <summary>
        /// Update de StatusPanel
        /// </summary>
        private void UpdateStatusPanel()
        {
            
        }

        /// <summary>
        /// Clear modification
        /// </summary>
        private void ClearBytesModifiedsList()
        {
            if (ByteProvider.CheckIsOpen(_provider))
                _provider.ClearBytesModifiedsList();
        }

        /// <summary>
        /// Make undo of last the last bytemodified
        /// </summary>
        public void Undo(int repeat = 1)
        {            
            if (ByteProvider.CheckIsOpen(_provider))
            {
                for (int i = 0; i < repeat; i++)
                    _provider.Undo();

                RefreshView(false);
            }
        }

        /// <summary>
        /// Refresh currentview of hexeditor
        /// </summary>
        /// <param name="ControlResize"></param>
        private void RefreshView(bool ControlResize = false)
        {
            UpdateLinesInfo();
            UpdateVerticalScroll();
            UpdateHexHeader();
            UpdateStringDataViewer(ControlResize);
            UpdateDataViewer(ControlResize);            
            UpdateByteModified();
            UpdateSelection();
        }
        
        /// <summary>
        /// Update the selection of byte in hexadecimal panel
        /// </summary>
        private void UpdateSelectionColorMode(FirstColor coloring)
        {
            int stackIndex = 0;

            switch (coloring)
            {
                case FirstColor.HexByteData:
                    stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                            byteControl.HexByteFirstSelected = true;

                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                            byteControl.StringByteFirstSelected = false;

                        stackIndex++;
                    }
                    break;
                case FirstColor.StringByteData:
                    stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                            byteControl.HexByteFirstSelected = false;

                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                            byteControl.StringByteFirstSelected = true;

                        stackIndex++;
                    }
                    break;
            }
        }

        /// <summary>
        /// Update the selection of byte
        /// </summary>
        private void UpdateSelection()
        {
            int stackIndex = 0;
            foreach (Label infolabel in LinesInfoStackPanel.Children)
            {
                if (SelectionStart <= SelectionStop)
                {
                    //Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                        if (byteControl.BytePositionInFile >= SelectionStart && 
                            byteControl.BytePositionInFile <= SelectionStop && 
                            byteControl.BytePositionInFile > -1) 
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        else
                            byteControl.IsSelected = false;

                    //HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        if (byteControl.BytePositionInFile >= SelectionStart && 
                            byteControl.BytePositionInFile <= SelectionStop &&
                            byteControl.BytePositionInFile > -1)
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true; 
                        else
                            byteControl.IsSelected = false;
                }
                else 
                {
                    //Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                        if (byteControl.BytePositionInFile >= SelectionStop && 
                            byteControl.BytePositionInFile <= SelectionStart && 
                            byteControl.BytePositionInFile > -1)
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        else
                            byteControl.IsSelected = false;

                    //HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        if (byteControl.BytePositionInFile >= SelectionStop &&
                            byteControl.BytePositionInFile <= SelectionStart && 
                            byteControl.BytePositionInFile > -1)
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        else
                            byteControl.IsSelected = false;
                }
                stackIndex++;
            }
        }
        
        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateDataViewer(bool ControlResize)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {                
                if (ControlResize)
                {
                    HexDataStackPanel.Children.Clear();                    

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;
                        
                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());
                        //long correction = 0;

                        for (int i = 0; i < BytePerLine; i++)
                        {
                            _provider.Position = position + i; //+ correction;

                            if (_provider.Position >= _provider.Length)
                                break;

                            //FOR TESTING PURPOSE ONLY... 
                            //if (_provider.CheckIfIsByteModified(_provider.Position, ByteAction.Deleted) != null)
                            //{
                            //    correction++;
                            //    i--;
                            //    continue;
                            //}

                            HexByteControl byteControl = new HexByteControl();

                            byteControl.BytePositionInFile = _provider.Position;
                            byteControl.ReadOnlyMode = _readOnlyMode;
                            byteControl.MouseSelection += Control_MouseSelection;
                            byteControl.Click += Control_Click;
                            byteControl.MoveNext += Control_MoveNext;
                            byteControl.ByteModified += Control_ByteModified;
                            byteControl.MoveUp += Control_MoveUp;
                            byteControl.MoveDown += Control_MoveDown;
                            byteControl.MoveLeft += Control_MoveLeft;
                            byteControl.MoveRight += Control_MoveRight;
                            byteControl.MovePageUp += Control_MovePageUp;
                            byteControl.MovePageDown += Control_MovePageDown;
                            byteControl.ByteDeleted += Control_ByteDeleted;

                            byteControl.Byte = (byte)_provider.ReadByte();
                            
                            dataLineStack.Children.Add(byteControl);                                                        
                        }

                        HexDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            _provider.Position = position++;
                                                        
                            if (_provider.Position >= _provider.Length)
                            {                                
                                byteControl.Action = ByteAction.Nothing;
                                byteControl.BytePositionInFile = -1;
                                byteControl.ReadOnlyMode = _readOnlyMode;
                                byteControl.IsSelected = false;
                                byteControl.Byte = null; 
                            }
                            else
                            {
                                byteControl.Action = ByteAction.Nothing;
                                byteControl.ReadOnlyMode = _readOnlyMode;
                                byteControl.BytePositionInFile = _provider.Position;
                                byteControl.Byte = (byte)_provider.ReadByte();                                
                            }
                        }

                        stackIndex++;                        
                    }
                }
            }
            else
            {
                HexDataStackPanel.Children.Clear();
            }
        }

        private void Control_MovePageDown(object sender, EventArgs e)
        {
            
        }

        private void Control_MovePageUp(object sender, EventArgs e)
        {
            
        }

        private void Control_MoveDown(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart + BytePerLine;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test < _provider.Length)
                    SelectionStart += BytePerLine;
                else
                    SelectionStart = _provider.Length;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    SelectionStart = SelectionStop;
                else
                    SelectionStop = SelectionStart;

                if (test < _provider.Length)
                {
                    SelectionStart += BytePerLine;
                    SelectionStop += BytePerLine;
                }
            }

            if (SelectionStart > GetLastVisibleBytePosition())
                VerticalScrollBar.Value++;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }

        private void Control_MoveUp(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart - BytePerLine;
            
            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                    SelectionStart -= BytePerLine;
                else
                    SelectionStart = 0;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    SelectionStart = SelectionStop;
                else
                    SelectionStop = SelectionStart;

                if (test > -1)
                {
                    SelectionStart -= BytePerLine;
                    SelectionStop -= BytePerLine;
                }
            }

            if (SelectionStart < GetFirstVisibleBytePosition())
                VerticalScrollBar.Value--;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }

        /// <summary>
        /// Update byte are modified
        /// </summary>
        private void UpdateByteModified()
        {
            int stackIndex = 0;
            ByteModified byteModifiedCopy = null;

            if (ByteProvider.CheckIsOpen(_provider))
                foreach (ByteModified byteModified in _provider.ByteModifieds(ByteAction.All))
                {
                    stackIndex = 0;
                    byteModifiedCopy = byteModified.GetCopy();

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            if (byteModifiedCopy.BytePositionInFile == byteControl.BytePositionInFile)
                            {
                                byteControl.InternalChange = true;
                                byteControl.Byte = byteModifiedCopy.Byte;

                                switch (byteModifiedCopy.Action)
                                {
                                    case ByteAction.Modified:
                                        byteControl.Action = ByteAction.Modified;
                                        break;
                                    case ByteAction.Deleted:
                                        byteControl.Action = ByteAction.Deleted;
                                        break;
                                }
                                byteControl.InternalChange = false;
                            }
                        }

                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            if (byteModifiedCopy.BytePositionInFile == byteControl.BytePositionInFile)
                            {
                                byteControl.InternalChange = true;
                                byteControl.Byte = byteModifiedCopy.Byte;

                                switch (byteModifiedCopy.Action)
                                {
                                    case ByteAction.Modified:
                                        byteControl.Action = ByteAction.Modified;
                                        break;
                                    case ByteAction.Deleted:
                                        byteControl.Action = ByteAction.Deleted;
                                        break;
                                }
                                byteControl.InternalChange = false;

                                //switch (byteModifiedCopy.Action)
                                //{
                                //    case ByteAction.Modified:
                                //        byteControl.InternalChange = true;
                                //        byteControl.Byte = byteModifiedCopy.Byte;
                                //        byteControl.IsByteModified = true;
                                //        byteControl.InternalChange = false;
                                //        break;
                                //}
                            }
                        }

                        stackIndex++;
                    }
                }
        }

        private void Control_ByteModified(object sender, EventArgs e)
        {
            HexByteControl ctrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
                _provider.AddByteModified(sbCtrl.Byte, sbCtrl.BytePositionInFile);
            else if (ctrl != null)
                _provider.AddByteModified(ctrl.Byte, ctrl.BytePositionInFile);
        }
        
        /// <summary>
        /// Set focus on byte
        /// </summary>
        /// <param name="bytePositionInFile"></param>
        private void SetFocusHexDataPanel(long bytePositionInFile)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                if (bytePositionInFile >= _provider.Length)
                    return;

                int stackIndex = 0;
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile == bytePositionInFile)
                        {
                            byteControl.Focus();
                            return;
                        }
                    }

                    stackIndex++;
                }

                if (VerticalScrollBar.Value < VerticalScrollBar.Maximum)
                    VerticalScrollBar.Value++;
                
                SetFocusHexDataPanel(bytePositionInFile);
            }
        }

        private void SetFocusStringDataPanel(long bytePositionInFile)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                if (bytePositionInFile >= _provider.Length)
                    return;

                int stackIndex = 0;
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile == bytePositionInFile)
                        {
                            byteControl.Focus();
                            return;
                        }
                    }

                    stackIndex++;
                }

                if (VerticalScrollBar.Value < VerticalScrollBar.Maximum)
                    VerticalScrollBar.Value++;

                SetFocusStringDataPanel(bytePositionInFile);
            }
        }

        /// <summary>
        /// Get first visible byte position in control
        /// </summary>
        /// <returns>Return -1 of no file open</returns>
        private long GetFirstVisibleBytePosition()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                int stackIndex = 0;
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        return byteControl.BytePositionInFile;

                    stackIndex++;
                }

                return -1;
            }
            else
                return -1;
        }

        /// <summary>
        /// Get last visible byte position in control
        /// </summary>
        /// <returns>Return -1 of no file open.</returns>
        private long GetLastVisibleBytePosition()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                int stackIndex = 0;
                long byteposition = GetFirstVisibleBytePosition();
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        byteposition++;

                    stackIndex++;
                }

                return byteposition;
            }
            else
                return -1;
        }

        private void Control_Click(object sender, EventArgs e)
        {
            StringByteControl sbCtrl = sender as StringByteControl;
            HexByteControl ctrl = sender as HexByteControl;

            if (ctrl != null)
            {
                SelectionStart = ctrl.BytePositionInFile;
                SelectionStop = ctrl.BytePositionInFile;
                UpdateSelectionColorMode(FirstColor.HexByteData);
            }

            if (sbCtrl != null)
            {
                SelectionStart = sbCtrl.BytePositionInFile;
                SelectionStop = sbCtrl.BytePositionInFile;
                UpdateSelectionColorMode(FirstColor.StringByteData);
            }
        }

        private void Control_MouseSelection(object sender, EventArgs e)
        {
            //Prevent false mouse selection on file open
            if (SelectionStart == -1)
                return;

            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;
            
            if (hbCtrl != null)
            {
                UpdateSelectionColorMode(FirstColor.HexByteData);

                if (hbCtrl.BytePositionInFile != -1)
                    SelectionStop = hbCtrl.BytePositionInFile;
                else
                    SelectionStop = GetLastVisibleBytePosition();
            }

            if (sbCtrl != null)
            {
                UpdateSelectionColorMode(FirstColor.StringByteData);

                if (sbCtrl.BytePositionInFile != -1)
                    SelectionStop = sbCtrl.BytePositionInFile;
                else
                    SelectionStop = GetLastVisibleBytePosition();
            }

            UpdateSelection();
        }

        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateStringDataViewer(bool ControlResize)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {                
                if (ControlResize)
                {
                    StringDataStackPanel.Children.Clear();

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;
                        
                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < BytePerLine; i++)
                        {
                            _provider.Position = position + i;
                            
                            if (_provider.Position >= _provider.Length)
                                break;
                                                        
                            StringByteControl sbCtrl = new StringByteControl();

                            sbCtrl.BytePositionInFile = _provider.Position;
                            sbCtrl.StringByteModified += Control_ByteModified;
                            sbCtrl.ReadOnlyMode = _readOnlyMode;
                            sbCtrl.MoveNext += Control_MoveNext;
                            sbCtrl.MouseSelection += Control_MouseSelection;
                            sbCtrl.Click += Control_Click;
                            sbCtrl.BytePositionInFile = _provider.Position;
                            sbCtrl.MoveUp += Control_MoveUp;
                            sbCtrl.MoveDown += Control_MoveDown;
                            sbCtrl.MoveLeft += Control_MoveLeft;
                            sbCtrl.MoveRight += Control_MoveRight;
                            sbCtrl.ByteDeleted += Control_ByteDeleted;
                            
                            sbCtrl.Byte = (byte)_provider.ReadByte();
                                                        
                            dataLineStack.Children.Add(sbCtrl);
                        }

                        StringDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {                        
                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        foreach (StringByteControl sbCtrl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            _provider.Position = position++;

                            if (_provider.Position >= _provider.Length)
                            {
                                sbCtrl.Byte = null;
                                sbCtrl.BytePositionInFile = -1;
                                sbCtrl.Action = ByteAction.Nothing;
                                sbCtrl.ReadOnlyMode = _readOnlyMode;
                                sbCtrl.IsSelected = false;
                            }
                            else
                            {
                                sbCtrl.InternalChange = true;
                                sbCtrl.Byte = (byte)_provider.ReadByte();
                                sbCtrl.BytePositionInFile = _provider.Position - 1;
                                sbCtrl.Action = ByteAction.Nothing;
                                sbCtrl.ReadOnlyMode = _readOnlyMode;
                                sbCtrl.InternalChange = false;
                            }
                        }
                        
                        stackIndex++;                        
                    }
                }
            }
            else
            {
                StringDataStackPanel.Children.Clear();
            }
        }

        private void Control_ByteDeleted(object sender, EventArgs e)
        {
            HexByteControl ctrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long position = -1;

            if (SelectionStart > SelectionStop)
                position = SelectionStop;
            else
                position = SelectionStart;

                _provider.AddByteDeleted(position, SelectionLenght);
            
            //TEST
            //RefreshView(true);
            UpdateByteModified();
            UpdateSelection();
        }

        private void Control_MoveRight(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart + 1;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test <= _provider.Length)
                    SelectionStart++;
                else
                    SelectionStart = _provider.Length;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    SelectionStart = SelectionStop;
                else
                    SelectionStop = SelectionStart;

                if (test < _provider.Length)
                {
                    SelectionStart++;
                    SelectionStop++;
                }
            }
            
            //Validation and refresh
            if (SelectionStart >= _provider.Length)
                SelectionStart = _provider.Length;
            
            if (SelectionStart > GetLastVisibleBytePosition())
                VerticalScrollBar.Value++;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }

        private void Control_MoveLeft(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart - 1;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                    SelectionStart--;
                else
                    SelectionStart = 0;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    SelectionStart = SelectionStop;
                else
                    SelectionStop = SelectionStart;

                if (test > -1)
                {
                    SelectionStart--;
                    SelectionStop--;
                }
            }

            //Validation and refresh
            if (SelectionStart < 0)
                SelectionStart = 0;

            if (SelectionStart < GetFirstVisibleBytePosition())
                VerticalScrollBar.Value--;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }

        private void Control_MoveNext(object sender, EventArgs e)
        {
            HexByteControl hexByteCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                sbCtrl.IsSelected = false;
                SetFocusStringDataPanel(sbCtrl.BytePositionInFile + 1);                
            }

            if (hexByteCtrl != null)
            {
                hexByteCtrl.IsSelected = false;
                SetFocusHexDataPanel(hexByteCtrl.BytePositionInFile + 1);            
            }

            if (hexByteCtrl != null || sbCtrl != null)
            {
                SelectionStart++;
                SelectionStop++;
                UpdateByteModified();
            }
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateHexHeader()
        {
            HexHeaderStackPanel.Children.Clear();

            if (ByteProvider.CheckIsOpen(_provider))
            {
                for (int i = 0; i < BytePerLine; i++)
                {
                    //Create control
                    Label LineInfoLabel = new Label();
                    LineInfoLabel.Height = _lineInfoHeight;
                    LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                    LineInfoLabel.Foreground = Brushes.Gray;
                    LineInfoLabel.Width = 25;
                    LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                    LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    LineInfoLabel.Content = Converters.ByteToHex((byte)i);
                    LineInfoLabel.ToolTip = $"Column : {i.ToString()}";

                    HexHeaderStackPanel.Children.Add(LineInfoLabel);
                }
            }
            else
            {

            }
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateLinesInfo()
        {
            LinesInfoStackPanel.Children.Clear();

            if (ByteProvider.CheckIsOpen(_provider))
            {
                for(int i = 0; i < GetMaxVisibleLine(); i++)
                {
                    long fds = GetMaxVisibleLine();
                    //LineInfo
                    
                    long firstLineByte = ((long)VerticalScrollBar.Value + i) * BytePerLine; 
                    string info = "0x" +  firstLineByte.ToString(Constant.HexLineInfoStringFormat, CultureInfo.InvariantCulture);

                    if (firstLineByte < _provider.Length)
                    {
                        //Create control
                        Label LineInfoLabel = new Label();
                        LineInfoLabel.Height = _lineInfoHeight;
                        LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                        LineInfoLabel.Foreground = Brushes.Gray;
                        LineInfoLabel.MouseDown += LineInfoLabel_MouseDown;
                        LineInfoLabel.MouseMove += LineInfoLabel_MouseMove;
                        LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                        LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                        LineInfoLabel.Content = info;
                        LineInfoLabel.ToolTip = $"Byte : {firstLineByte.ToString()}";  

                        LinesInfoStackPanel.Children.Add(LineInfoLabel);
                    }
                } 
            }
            else
            {

            }
        }

        private void LineInfoLabel_MouseMove(object sender, MouseEventArgs e)
        {
            Label line = sender as Label;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectionStop = Converters.HexLiteralToLong(line.Content.ToString()) + BytePerLine - 1;
            }
        }

        private void LineInfoLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //TODO: Multiline selection

            Label line = sender as Label;

            SelectionStart = Converters.HexLiteralToLong(line.Content.ToString());
            SelectionStop = SelectionStart + BytePerLine - 1;
        }

        /// <summary>
        /// Close file and clear control
        /// ReadOnlyMode is reset to false
        /// </summary>
        public void CloseFile()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                _provider.CloseFile();

                ReadOnlyMode = false;
                VerticalScrollBar.Value = 0;
            }

            ClearBytesModifiedsList();
            UnSelectAll();
            RefreshView();
        }

        /// <summary>
        /// Update vertical scrollbar with file info
        /// </summary>
        public void UpdateVerticalScroll()
        {
            VerticalScrollBar.Visibility = Visibility.Collapsed;

            if (ByteProvider.CheckIsOpen(_provider))
            {
                //TODO : check if need to show
                VerticalScrollBar.Visibility = Visibility.Visible;

                VerticalScrollBar.SmallChange = 1;
                VerticalScrollBar.LargeChange = ScrollLargeChange;
                VerticalScrollBar.Maximum = GetMaxLine() - GetMaxVisibleLine() + 1;
            }     
                  
        }

        /// <summary>
        /// Obtain the max line for verticalscrollbar
        /// </summary>
        public long GetMaxLine()
        {
            if (ByteProvider.CheckIsOpen(_provider))
                return _provider.Length / BytePerLine;
            else
                return 0;
        }

        /// <summary>
        /// Get the number of row visible in control 
        /// </summary>
        public long GetMaxVisibleLine()
        {
            return (long)(LinesInfoStackPanel.ActualHeight / _lineInfoHeight); // + 1; //TEMPS
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshView(true);
        }

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshView(false);
        }

        private void BottomRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                VerticalScrollBar.Value++;            
        }

        private void TopRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                VerticalScrollBar.Value--;            
        }

        #region Selection Property/Methods
        /// <summary>
        /// Set the start byte position of selection
        /// </summary>
        public long SelectionStart
        {
            get { return (long)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }
                
        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register("SelectionStart", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStart_ChangedCallBack),
                    new CoerceValueCallback(SelectionStart_CoerceValueCallBack)));

        private static object SelectionStart_CoerceValueCallBack(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            long value = (long)baseValue;

            if (value < -1)
                return -1L;

            if (!ByteProvider.CheckIsOpen(ctrl._provider))
                return -1L;
            else
                return baseValue;
        }

        private static void SelectionStart_ChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            
            ctrl.UpdateSelection();
            ctrl.UpdateStatusPanel();

            if (ctrl.SelectionStartChanged != null)
                ctrl.SelectionStartChanged(ctrl, new EventArgs());

            if (ctrl.SelectionLenghtChanged != null)
                ctrl.SelectionLenghtChanged(ctrl, new EventArgs());
        }
        
        /// <summary>
        /// Set the start byte position of selection
        /// </summary>
        public long SelectionStop
        {
            get { return (long)GetValue(SelectionStopProperty); }
            set { SetValue(SelectionStopProperty, value); }
        }

        public static readonly DependencyProperty SelectionStopProperty =
            DependencyProperty.Register("SelectionStop", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStop_ChangedCallBack),
                    new CoerceValueCallback(SelectionStop_CoerceValueCallBack)));

        private static object SelectionStop_CoerceValueCallBack(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            long value = (long)baseValue;

            Debug.Print($"SelectionStop : {value.ToString()}");

            if (value < -1)
                return -1L;

            if (!ByteProvider.CheckIsOpen(ctrl._provider))
                return -1L;

            if (value >= ctrl._provider.Length)
                return ctrl._provider.Length;
            
            return baseValue;            
        }

        private static void SelectionStop_ChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            ctrl.UpdateSelection();
            ctrl.UpdateStatusPanel();

            if (ctrl.SelectionStopChanged != null)
                ctrl.SelectionStopChanged(ctrl, new EventArgs());

            if (ctrl.SelectionLenghtChanged != null)
                ctrl.SelectionLenghtChanged(ctrl, new EventArgs());            
        }

        /// <summary>
        /// Reset selection to -1
        /// </summary>
        public void UnSelectAll()
        {
            SelectionStart = -1;
            SelectionStop = -1;
        }

        /// <summary>
        /// Select the entire file
        /// If file are closed the selection will be set to -1
        /// </summary>
        public void SelectAll()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                SelectionStart = 0;
                SelectionStop = _provider.Length;
            }
            else
            {
                SelectionStart = -1;
                SelectionStop = -1;
            }

            UpdateSelection();
        }
                

        /// <summary>
        /// Get the lenght of byte are selected (base 1)
        /// </summary>
        public long SelectionLenght
        {
            get
            {
                if (SelectionStop == -1 || SelectionStop == -1)
                    return 0;
                else if (SelectionStart == SelectionStop)
                    return 1;
                else if (SelectionStart > SelectionStop)
                    return SelectionStart - SelectionStop + 1;
                else
                    return SelectionStop - SelectionStart + 1;
            }
        }

        #endregion Selection Property/Methods

        #region Copy/Paste/Cut Methods
        /// <summary>
        /// Return true if Copy method could be invoked.
        /// </summary>
        public bool CanCopy()
        {
            if (SelectionLenght < 1 || !ByteProvider.CheckIsOpen(_provider))
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Copy to clipboard with default CopyPasteMode.ASCIIString
        /// </summary>
        public void CopyToClipboard()
        {            
            CopyToClipboard(CopyPasteMode.ASCIIString);
        }

        /// <summary>
        /// Copy to clipboard
        /// </summary>        
        public void CopyToClipboard(CopyPasteMode copypastemode)
        {
            if (!CanCopy()) return;

            if (ByteProvider.CheckIsOpen(_provider))
                _provider.CopyToClipboard(copypastemode, SelectionStart, SelectionStop, false);

        }

        #endregion Copy/Paste/Cut Methods

        #region Set position methods

        /// <summary>
        /// Set position of cursor
        /// </summary>
        public void SetPosition(long position, long byteLenght)
        {
            //TODO : selected hexbytecontrol
            SelectionStart = position;
            SelectionStop = position + byteLenght - 1;

            if (ByteProvider.CheckIsOpen(_provider))
            {
                VerticalScrollBar.Value = position / BytePerLine;
            }
            else
                VerticalScrollBar.Value = 0;

            RefreshView(true);
        }

        public void SetPosition(long position)
        {
            SetPosition(position, 0);
        }

        /// <summary>
        /// Set position of cursor
        /// </summary>
        public void SetPosition(string HexLiteralPosition)
        {
            try
            {
                SetPosition(Converters.HexLiteralToLong(HexLiteralPosition));
            }
            catch
            {
                throw new InvalidCastException("Invalid hexa string");
            }
        }

        /// <summary>
        /// Set position of cursor
        /// </summary>
        public void SetPosition(string HexLiteralPosition, long byteLenght)
        {
            try
            {
                SetPosition(Converters.HexLiteralToLong(HexLiteralPosition), byteLenght);
            }
            catch
            {
                throw new InvalidCastException("Invalid hexa string");
            }
        }
        #endregion Set position methods

        #region Visibility standard property
        /// <summary>
        /// Set or Get value for change visibility of hexadecimal panel
        /// </summary>
        public Visibility HexDataVisibility
        {
            get { return (Visibility)GetValue(HexDataVisibilityProperty); }
            set { SetValue(HexDataVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HexDataVisibilityProperty =
            DependencyProperty.Register("HexDataVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible, 
                    new PropertyChangedCallback(HexDataVisibility_PropertyChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static object Visibility_CoerceValue(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)baseValue;

            if (value == Visibility.Hidden)
                return Visibility.Collapsed;

            return value;
        }

        private static void HexDataVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.HexDataStackPanel.Visibility = Visibility.Visible;

                    if (ctrl.HeaderVisibility == Visibility.Visible)
                        ctrl.HexHeaderStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.HexDataStackPanel.Visibility = Visibility.Collapsed;
                    ctrl.HexHeaderStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of hexadecimal header
        /// </summary>
        public Visibility HeaderVisibility
        {
            get { return (Visibility)GetValue(HeaderVisibilityProperty); }
            set { SetValue(HeaderVisibilityProperty, value); }
        }
                
        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(HeaderVisibility_PropertyChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));
                
        private static void HeaderVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    if (ctrl.HexDataVisibility == Visibility.Visible)
                        ctrl.HexHeaderStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:                    
                    ctrl.HexHeaderStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of string panel
        /// </summary>
        public Visibility StringDataVisibility
        {
            get { return (Visibility)GetValue(StringDataVisibilityProperty); }
            set { SetValue(StringDataVisibilityProperty, value); }
        }

        public static readonly DependencyProperty StringDataVisibilityProperty =
            DependencyProperty.Register("StringDataVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(StringDataVisibility_ValidateValue),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void StringDataVisibility_ValidateValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.StringDataStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.StringDataStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of vertical scroll bar
        /// </summary>
        public Visibility VerticalScrollBarVisibility
        {
            get { return (Visibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(VerticalScrollBarVisibility_ValueChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void VerticalScrollBarVisibility_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.VerticalScrollBar.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.VerticalScrollBar.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of status bar
        /// </summary>
        public Visibility StatusBarVisibility
        {
            get { return (Visibility)GetValue(StatusBarVisibilityProperty); }
            set { SetValue(StatusBarVisibilityProperty, value); }
        }
                
        public static readonly DependencyProperty StatusBarVisibilityProperty =
            DependencyProperty.Register("StatusBarVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(StatusBarVisibility_ValueChange),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void StatusBarVisibility_ValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.StatusBarGrid.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.StatusBarGrid.Visibility = Visibility.Collapsed;
                    break;
            }

            ctrl.RefreshView(false);
        }        
        #endregion Visibility standard property
    }
}
