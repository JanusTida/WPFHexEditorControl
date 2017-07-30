//////////////////////////////////////////////
// MIT License  - 2016-2017
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using WPFHexaEditor.Core.MethodExtention;

namespace WPFHexaEditor.Core.Bytes
{
    /// <summary>
    /// Used for interaction with file
    /// </summary>
    public class ByteProvider
    {
        //Global variable
        private IDictionary<long, ByteModified> _byteModifiedDictionary = new Dictionary<long, ByteModified>();
        private Stack<ByteModified> _undoStack = new Stack<ByteModified>();
        //private Stack<ByteModified> _redoStack = new Stack<ByteModified>();

        private string _fileName = string.Empty;
        private Stream _stream = null;
        private bool _readOnlyMode = false;
        private bool _isUndoEnabled = true;
        private double _longProcessProgress = 0;
        private bool _isOnLongProcess = false;
        private ByteProviderStreamType _streamType = ByteProviderStreamType.Nothing;

        //Event
        public event EventHandler DataCopiedToClipboard;

        public event EventHandler ReadOnlyChanged;

        public event EventHandler Closed;

        public event EventHandler StreamOpened;

        public event EventHandler PositionChanged;

        public event EventHandler Undone;

        public event EventHandler DataCopiedToStream;

        public event EventHandler ChangesSubmited;

        public event EventHandler LongProcessProgressChanged;

        public event EventHandler LongProcessProgressStarted;

        public event EventHandler LongProcessProgressCompleted;

        public event EventHandler DataPastedNotInserted;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ByteProvider() { }

        /// <summary>
        /// Construct new ByteProvider with filename and try to open file
        /// </summary>
        public ByteProvider(string filename)
        {
            FileName = filename;
        }

        /// <summary>
        /// Constuct new ByteProvider with stream
        /// </summary>
        /// <param name="stream"></param>
        public ByteProvider(MemoryStream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// Set or Get the file with the control will show hex
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                _fileName = value;

                OpenFile();
            }
        }

        /// <summary>
        /// Get the type of stream are opened in byteprovider.
        /// </summary>
        public ByteProviderStreamType StreamType
        {
            get
            {
                return _streamType;
            }
        }

        /// <summary>
        /// Get or set a MemoryStream for use with byteProvider
        /// </summary>
        public MemoryStream Stream
        {
            get
            {
                return (MemoryStream)_stream;
            }
            set
            {
                var readonlymode = _readOnlyMode;
                Close();
                _readOnlyMode = readonlymode;

                _streamType = ByteProviderStreamType.MemoryStream;

                _stream = value;

                StreamOpened?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Open file are set in FileName property
        /// </summary>
        public void OpenFile()
        {
            if (File.Exists(FileName))
            {
                Close();

                bool readOnlyMode = false;

                try
                {
                    _stream = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); ;
                }
                catch
                {
                    if (MessageBox.Show("The file is locked. Do you want to open it in read-only mode?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        _stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                        readOnlyMode = true;
                    }
                }

                if (readOnlyMode)
                    ReadOnlyMode = true;

                _streamType = ByteProviderStreamType.File;

                StreamOpened?.Invoke(this, new EventArgs());
            }
            else
            {
                throw new FileNotFoundException();
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

                //Launch event
                ReadOnlyChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Close stream
        /// ReadOnlyMode is reset to false
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                _stream.Close();
                _stream = null;
                ReadOnlyMode = false;
                IsOnLongProcess = false;
                LongProcessProgress = 0;

                _streamType = ByteProviderStreamType.Nothing;

                Closed?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Get the lenght of file. Return -1 if file is close.
        /// </summary>
        public long Length
        {
            get
            {
                if (IsOpen)
                    return _stream.Length;

                return -1;
            }
        }

        /// <summary>
        /// Check if position as at EOF.
        /// </summary>
        public bool EOF
        {
            get
            {
                return (_stream.Position == _stream.Length) || (_stream.Position > _stream.Length);
            }
        }

        /// <summary>
        /// Get or Set position in file. Return -1 when file is closed
        /// </summary>
        public long Position
        {
            get
            {
                if (IsOpen)
                    return _stream.Position <= _stream.Length ? _stream.Position : _stream.Length;

                return -1;
            }
            set
            {
                if (IsOpen)
                {
                    _stream.Position = value;

                    PositionChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Get if file is open
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (_stream != null)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Get if file is open
        /// </summary>
        public static bool CheckIsOpen(ByteProvider provider)
        {
            if (provider != null)
                if (provider.IsOpen)
                    return true;

            return false;
        }

        /// <summary>
        /// Readbyte at position if file CanRead. Return -1 is file is closed of EOF.
        /// </summary>
        /// <returns></returns>
        public int ReadByte()
        {
            if (IsOpen)
                if (_stream.CanRead)
                    return _stream.ReadByte();

            return -1;
        }

        /// <summary>
        /// Read bytes, lenght of reading are definid with parameter count. Start at position if file CanRead. Return null is file is closed or can be read.
        /// </summary>
        /// <returns></returns>
        public byte[] Read(int count)
        {
            if (IsOpen)
                if (_stream.CanRead)
                {
                    int countAdjusted = count;

                    if ((Length - Position) <= count)
                        countAdjusted = (int)(Length - Position);

                    byte[] bytesReaded = new byte[countAdjusted];
                    _stream.Read(bytesReaded, 0, countAdjusted);
                    return bytesReaded;
                }

            return null;
        }

        /// <summary>
        /// This is your primitive method of reading,I take it back to save time.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Read(byte[] destination, int offset, int count) {
            if (IsOpen)
                if (_stream.CanRead)
                    return _stream.Read(destination, offset, count);
            return -1;
        }

        #region SubmitChanges to file/stream

        /// <summary>
        /// Submit change to files/stream
        /// TODO : NEED OPTIMISATION FOR LARGE FILE... IT'S AS BEGINING :) USE TEMPS FILE ?
        /// </summary>
        public void SubmitChanges()
        {
            if (CanWrite)
            {
                //Set percent of progress to zero and create and iterator for help mesure progress
                LongProcessProgress = 0;
                int i = 0;

                //Create appropriate temp stream for new file.
                Stream NewStream = null;

                if (Length < ConstantReadOnly.LARGE_FILE_LENGTH)
                    NewStream = new MemoryStream();
                else
                    NewStream = File.Open(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite);

                //Fast change only nothing byte deleted or added
                if (GetModifiedBytes(ByteAction.Deleted).Count() == 0 &&
                    GetModifiedBytes(ByteAction.Added).Count() == 0)
                {
                    //Launch event at process strated
                    IsOnLongProcess = true;
                    LongProcessProgressStarted?.Invoke(this, new EventArgs());

                    var bytemodifiedList = GetModifiedBytes(ByteAction.Modified);
                    double countChange = bytemodifiedList.Count();
                    i = 0;

                    //Fast save. only save byteaction=modified
                    foreach (var bm in bytemodifiedList)
                        if (bm.Value.IsValid)
                        {
                            //Set percent of progress
                            LongProcessProgress = i++ / countChange;

                            //Break process?
                            if (!IsOnLongProcess)
                                break;

                            _stream.Position = bm.Key;
                            _stream.WriteByte(bm.Value.Byte.Value);
                        }

                    //Launch event at process completed
                    IsOnLongProcess = false;
                    LongProcessProgressCompleted?.Invoke(this, new EventArgs());
                }
                else
                {
                    //Launch event at process strated
                    IsOnLongProcess = true;
                    LongProcessProgressStarted?.Invoke(this, new EventArgs());

                    byte[] buffer = new byte[ConstantReadOnly.COPY_BLOCK_SIZE];
                    long bufferlength = 0;
                    var SortedBM = GetModifiedBytes(ByteAction.All).OrderBy(b => b.Key);
                    double countChange = SortedBM.Count();
                    i = 0;

                    //Set position
                    Position = 0;

                    ////Start update and rewrite file.
                    foreach (var nextByteModified in SortedBM)
                    {
                        //Set percent of progress
                        LongProcessProgress = (i++ / countChange);

                        //Break process?
                        if (!IsOnLongProcess)
                            break;

                        //Reset buffer
                        buffer = new byte[ConstantReadOnly.COPY_BLOCK_SIZE];

                        //start read/write / use little block for optimize memory
                        while (Position != nextByteModified.Key)
                        {
                            bufferlength = nextByteModified.Key - Position;

                            //TEMPS
                            if (bufferlength < 0)
                                bufferlength = 1;

                            //EOF
                            if (bufferlength < ConstantReadOnly.COPY_BLOCK_SIZE)
                                buffer = new byte[bufferlength];

                            _stream.Read(buffer, 0, buffer.Length);
                            NewStream.Write(buffer, 0, buffer.Length);
                        }

                        //Apply ByteAction!
                        switch (nextByteModified.Value.Action)
                        {
                            case ByteAction.Added:
                                //TODO : IMPLEMENTING ADD BYTE
                                break;

                            case ByteAction.Deleted:
                                //NOTHING TODO we dont want to add deleted byte
                                Position++;
                                break;

                            case ByteAction.Modified:
                                Position++;
                                NewStream.WriteByte(nextByteModified.Value.Byte.Value);
                                break;
                        }

                        //Read/Write the last section of file
                        if (nextByteModified.Key == SortedBM.Last().Key)
                        {
                            while (!EOF)
                            {
                                bufferlength = _stream.Length - Position;

                                //EOF
                                if (bufferlength < ConstantReadOnly.COPY_BLOCK_SIZE)
                                    buffer = new byte[bufferlength];

                                _stream.Read(buffer, 0, buffer.Length);
                                NewStream.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }

                    //Write new data to current stream
                    Position = 0;
                    NewStream.Position = 0;
                    buffer = new byte[ConstantReadOnly.COPY_BLOCK_SIZE];

                    while (!EOF)
                    {
                        //Set percent of progress
                        LongProcessProgress = ((double)Position / (double)Length);

                        //Break process?
                        if (!IsOnLongProcess)
                            break;

                        bufferlength = _stream.Length - Position;

                        //EOF
                        if (bufferlength < ConstantReadOnly.COPY_BLOCK_SIZE)
                            buffer = new byte[bufferlength];

                        NewStream.Read(buffer, 0, buffer.Length);
                        _stream.Write(buffer, 0, buffer.Length);
                    }
                    _stream.SetLength(NewStream.Length);

                    //dispose resource
                    NewStream.Close();
                    buffer = null;

                    //Launch event at process completed
                    IsOnLongProcess = false;
                    LongProcessProgressCompleted?.Invoke(this, new EventArgs());
                }

                //Launch event
                ChangesSubmited?.Invoke(this, new EventArgs());
            }
            else
                throw new Exception("Cannot write to file.");
        }

        #endregion SubmitChanges to file/stream

        #region Bytes modifications methods

        /// <summary>
        /// Clear changes and undo
        /// </summary>
        public void ClearUndoChange()
        {
            if (_byteModifiedDictionary != null)
                _byteModifiedDictionary.Clear();
        }

        /// <summary>
        /// Check if the byte in parameter are modified and return original Bytemodified from list
        /// </summary>
        public ByteModified CheckIfIsByteModified(long bytePositionInFile, ByteAction action = ByteAction.Modified)
        {
            ByteModified byteModified;
            if (_byteModifiedDictionary.TryGetValue(bytePositionInFile, out byteModified)
                && byteModified.IsValid
                && (byteModified.Action == action || action == ByteAction.All))
            {
                        return byteModified;
                }
            return null;
        }

        /// <summary>
        /// Add/Modifiy a ByteModifed in the list of byte have changed
        /// </summary>
        public void AddByteModified(byte? @byte, long bytePositionInFile, long undoLenght = 1)
        {
            var bytemodifiedOriginal = CheckIfIsByteModified(bytePositionInFile, ByteAction.Modified);

            if (bytemodifiedOriginal != null)
                _byteModifiedDictionary.Remove(bytePositionInFile);

            var byteModified = new ByteModified()
            {
                Byte = @byte,
                UndoLenght = undoLenght,
                BytePositionInFile = bytePositionInFile,
                Action = ByteAction.Modified
            };
            _byteModifiedDictionary.Add(bytePositionInFile, byteModified);
            UndoStack.Push(byteModified);
        }

        /// <summary>
        /// Add/Modifiy a ByteModifed in the list of byte have deleted
        /// </summary>
        public void AddByteDeleted(long bytePositionInFile, long length)
        {
            long position = bytePositionInFile;

            for (int i = 0; i < length; i++)
            {
                if (i % 100 == 0) Application.Current.DoEvents();

                var bytemodifiedOriginal = CheckIfIsByteModified(position, ByteAction.All);

                if (bytemodifiedOriginal != null)
                    _byteModifiedDictionary.Remove(position);

                var byteModified = new ByteModified()
                {
                    Byte = new byte(),
                    UndoLenght = length,
                    BytePositionInFile = position,
                    Action = ByteAction.Deleted
                };
                _byteModifiedDictionary.Add(position, byteModified);
                UndoStack.Push(byteModified);

                position++;
            }
        }

        /// <summary>
        /// Return an IEnumerable ByteModified have action set to Modified
        /// </summary>
        /// <returns></returns>
        public IDictionary<long, ByteModified> GetModifiedBytes(ByteAction action)
        {
            if (action == ByteAction.All)
                {
                return _byteModifiedDictionary;
                }
                else
            {
                return _byteModifiedDictionary.Where(b => b.Value.Action == action).ToDictionary(k => k.Key, v => v.Value);
            }
        }

        #endregion Bytes modifications methods

        #region Copy/Paste/Cut Methods

        /// <summary>
        /// Get the lenght of byte are selected (base 1)
        /// </summary>
        public long GetSelectionLenght(long selectionStart, long selectionStop)
        {
            if (selectionStop == -1 || selectionStop == -1)
                return 0;
            else if (selectionStart == selectionStop)
                return 1;
            else if (selectionStart > selectionStop)
                return selectionStart - selectionStop + 1;
            else
                return selectionStop - selectionStart + 1;
        }

        /// <summary>
        /// Copies the current selection in the hex box to the Clipboard.
        /// </summary>
        /// <param name="copyChange">Set tu true if you want onclude change in your copy. Set to false to copy directly from source</param>
        private byte[] GetCopyData(long selectionStart, long selectionStop, bool copyChange)
        {
            //Validation
            if (!CanCopy(selectionStart, selectionStop)) return new byte[0];
            if (selectionStop == -1 || selectionStop == -1) return new byte[0];

            //Variable
            long byteStartPosition = -1;
            List<byte> bufferList = new List<byte>();

            //Set start position
            if (selectionStart == selectionStop)
                byteStartPosition = selectionStart;
            else if (selectionStart > selectionStop)
                byteStartPosition = selectionStop;
            else
                byteStartPosition = selectionStart;

            //set position
            _stream.Position = byteStartPosition;

            //Exclude byte deleted from copy
            if (!copyChange)
            {
                byte[] buffer = new byte[GetSelectionLenght(selectionStart, selectionStop)];
                _stream.Read(buffer, 0, Convert.ToInt32(GetSelectionLenght(selectionStart, selectionStop)));
                return buffer;
            }
            else
            {
                for (int i = 0; i < GetSelectionLenght(selectionStart, selectionStop); i++)
                {
                    ByteModified byteModified = CheckIfIsByteModified(_stream.Position, ByteAction.All);

                    if (byteModified == null)
                    {
                        bufferList.Add((byte)_stream.ReadByte());
                        continue;
                    }
                    else
                    {
                        switch (byteModified.Action)
                        {
                            case ByteAction.Added:
                                //TODO : IMPLEMENTING ADD BYTE
                                break;

                            case ByteAction.Deleted:
                                //NOTHING TODO we dont want to add deleted byte
                                break;

                            case ByteAction.Modified:
                                if (byteModified.IsValid)
                                    bufferList.Add(byteModified.Byte.Value);
                                break;
                        }
                    }

                    _stream.Position++;
                }
            }

            return bufferList.ToArray();
        }

        /// <summary>
        /// Copy selection of byte to clipboard
        /// </summary>
        /// <param name="copyChange">Set tu true if you want onclude change in your copy. Set to false to copy directly from source</param>
        public void CopyToClipboard(CopyPasteMode copypastemode, long selectionStart, long selectionStop, bool copyChange = true)
        {
            if (!CanCopy(selectionStart, selectionStop)) return;

            //Variables
            byte[] buffer = GetCopyData(selectionStart, selectionStop, copyChange);
            string sBuffer = "";

            DataObject da = new DataObject();
            
            switch (copypastemode)
            {
                case CopyPasteMode.TBLString:
                    throw new NotImplementedException();
                case CopyPasteMode.ASCIIString:
                    sBuffer = ByteConverters.BytesToString(buffer);
                    da.SetText(sBuffer, TextDataFormat.Text);
                    break;

                case CopyPasteMode.HexaString:
                    sBuffer = ByteConverters.ByteToHex(buffer);
                    da.SetText(sBuffer, TextDataFormat.Text);
                    break;

                case CopyPasteMode.Byte:
                    throw new NotImplementedException();

                case CopyPasteMode.CSharpCode:
                    CopyToClipboard_Language(selectionStart, selectionStop, copyChange, da, CodeLanguage.CSharp);
                    break;
                case CopyPasteMode.CCode:
                    CopyToClipboard_Language(selectionStart, selectionStop, copyChange, da, CodeLanguage.C);
                    break;
                case CopyPasteMode.JavaCode:
                    CopyToClipboard_Language(selectionStart, selectionStop, copyChange, da, CodeLanguage.Java);
                    break;
                case CopyPasteMode.FSharp:
                    CopyToClipboard_Language(selectionStart, selectionStop, copyChange, da, CodeLanguage.FSharp);
                    break;
                case CopyPasteMode.VBNetCode:
                    CopyToClipboard_Language(selectionStart, selectionStop, copyChange, da, CodeLanguage.VBNET);
                    break;
            }

            //set memorystream (BinaryData) clipboard data
            MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length, false, true);
            da.SetData("BinaryData", ms);

            Clipboard.SetDataObject(da, true);

            DataCopiedToClipboard?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Copy selection to clipboard in code block
        /// </summary>
        private void CopyToClipboard_Language(long selectionStart, long selectionStop, bool copyChange, DataObject da, CodeLanguage language)
        {
            if (!CanCopy(selectionStart, selectionStop)) return;

            //Variables
            byte[] buffer = GetCopyData(selectionStart, selectionStop, copyChange);
            int i = 0;
            long lenght = 0;
            string delimiter = language == CodeLanguage.FSharp ? ";" : ",";

            StringBuilder sb = new StringBuilder();

            if (selectionStop > selectionStart)
                lenght = selectionStop - selectionStart + 1;
            else
                lenght = selectionStart - selectionStop + 1;

            switch (language )
            {
                case CodeLanguage.C:
                case CodeLanguage.CSharp:
                case CodeLanguage.Java:
                    sb.Append($"/* {FileName} ({DateTime.Now.ToString()}), \r\n StartPosition: 0x{ByteConverters.LongToHex(selectionStart)}, StopPosition: 0x{ByteConverters.LongToHex(selectionStop)}, Lenght: 0x{ByteConverters.LongToHex(lenght)} */");
                    break;
                case CodeLanguage.VBNET:
                    sb.Append($"' {FileName} ({DateTime.Now.ToString()}), \r\n' StartPosition: &H{ByteConverters.LongToHex(selectionStart)}, StopPosition: &H{ByteConverters.LongToHex(selectionStop)}, Lenght: &H{ByteConverters.LongToHex(lenght)}");
                    break;
                case CodeLanguage.FSharp:
                    sb.Append($"// {FileName} ({DateTime.Now.ToString()}), \r\n// StartPosition: 0x{ByteConverters.LongToHex(selectionStart)}, StopPosition: 0x{ByteConverters.LongToHex(selectionStop)}, Lenght: 0x{ByteConverters.LongToHex(lenght)}");
                    break;
            }                

            sb.AppendLine();
            sb.AppendLine();
            
            switch (language)
            {
                case CodeLanguage.CSharp:
                    sb.Append($"string sData =\"{ByteConverters.BytesToString(buffer)}\";");
                    sb.AppendLine();
                    sb.Append($"string sDataHex =\"{ByteConverters.StringToHex(ByteConverters.BytesToString(buffer))}\";");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append("byte[] rawData = {");
                    sb.AppendLine();
                    sb.Append("\t");
                    break;
                case CodeLanguage.Java:
                    sb.Append($"String sData =\"{ByteConverters.BytesToString(buffer)}\";");
                    sb.AppendLine();
                    sb.Append($"String sDataHex =\"{ByteConverters.StringToHex(ByteConverters.BytesToString(buffer))}\";");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append("byte rawData[] = {");
                    sb.AppendLine();
                    sb.Append("\t");
                    break;
                case CodeLanguage.C:
                    sb.Append($"char sData[] =\"{ByteConverters.BytesToString(buffer)}\";");
                    sb.AppendLine();
                    sb.Append($"char sDataHex[] =\"{ByteConverters.StringToHex(ByteConverters.BytesToString(buffer))}\";");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append($"unsigned char rawData[{lenght}] ");
                    sb.AppendLine();
                    sb.Append("\t");
                    break;
                case CodeLanguage.FSharp:
                    sb.Append($"let sData = @\"{ByteConverters.BytesToString(buffer)}\";");
                    sb.AppendLine();
                    sb.Append($"let sDataHex = @\"{ByteConverters.StringToHex(ByteConverters.BytesToString(buffer))}\";");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append("let bytes = [|");
                    sb.AppendLine();
                    sb.Append("    ");
                    break;
                case CodeLanguage.VBNET:
                    sb.Append($"Dim sData as String =\"{ByteConverters.BytesToString(buffer)}\";");
                    sb.AppendLine();
                    sb.Append($"Dim sDataHex as String =\"{ByteConverters.StringToHex(ByteConverters.BytesToString(buffer))}\";");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append("Dim rawData As Byte() = { _");
                    sb.AppendLine();
                    sb.Append("\t");
                    break;
            }
            
            foreach (byte b in buffer)
            {
                i++;
                if (language == CodeLanguage.Java) sb.Append("(byte)");
                
                if (language == CodeLanguage.VBNET)
                    sb.Append($"&H{ByteConverters.ByteToHex(b)}, ");
                else
                    sb.Append($"0x{ByteConverters.ByteToHex(b)}{delimiter} ");

                if (i == (language == CodeLanguage.Java ? 6:12))
                {
                    i = 0;
                    if (language == CodeLanguage.VBNET) sb.Append("_");
                    sb.AppendLine();
                    if (language != CodeLanguage.FSharp)
                        sb.Append("\t");
                    else
                        sb.Append("    ");
                }
            }
            if (language == CodeLanguage.VBNET) sb.Append("_");
            sb.AppendLine();
            if (language != CodeLanguage.FSharp)
                sb.Append("};");
            else
                sb.Append("|]");

            da.SetText(sb.ToString(), TextDataFormat.Text);
        }
        
        /// <summary>
        /// Copy selection of byte to a stream
        /// </summary>
        /// <param name="output">Output stream. Data will be copied at end of stream</param>
        /// <param name="copyChange">Set tu true if you want onclude change in your copy. Set to false to copy directly from source</param>
        public void CopyToStream(Stream output, long selectionStart, long selectionStop, bool copyChange = true)
        {
            if (!CanCopy(selectionStart, selectionStop)) return;

            //Variables
            byte[] buffer = GetCopyData(selectionStart, selectionStop, copyChange);

            if (output.CanWrite)
                output.Write(buffer, (int)output.Length, buffer.Length);
            else
                throw new Exception("An error is occurs when writing");

            DataCopiedToStream?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Paste the string at position
        /// </summary>
        /// <param name="pasteString">The string to paste</param>
        /// <param name="startPosition">The position to start pasting</param>
        public void PasteNotInsert(long startPosition, string pasteString)
        {
            long lenght = pasteString.Length;
            Position = startPosition;

            if (Position > -1)
            {
                foreach (char chr in pasteString)
                {
                    if (!EOF)
                    {
                        AddByteModified(ByteConverters.CharToByte(chr), Position, lenght);

                        Position++;
                    }
                    else
                        break;
                }

                DataPastedNotInserted?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Paste the string at position
        /// </summary>
        /// <param name="pasteString">The string to paste</param>
        /// <param name="startPosition">The position to start pasting</param>
        public void PasteNotInsert(string pasteString)
        {
            PasteNotInsert(Position, pasteString);
        }

        #endregion Copy/Paste/Cut Methods

        #region Undo / Redo

        /// <summary>
        /// Undo last byteaction
        /// </summary>
        /// <returns>Return a list of long contening the position are undone. Return null on error</returns>
        public void Undo()
        {
            if (CanUndo)
            {
                ByteModified last = UndoStack.Pop();
                List<long> bytePositionList = new List<long>();
                var undoLenght = last.UndoLenght;

                bytePositionList.Add(last.BytePositionInFile);
                _byteModifiedDictionary.Remove(last.BytePositionInFile);

                if (undoLenght > 1)
                    for (int i = 0; i < undoLenght; i++)
                    {
                        if (UndoStack.Count > 0)
                        {
                            last = UndoStack.Pop();
                            bytePositionList.Add(last.BytePositionInFile);
                            _byteModifiedDictionary.Remove(last.BytePositionInFile);
                        }
                    }

                Undone?.Invoke(bytePositionList, new EventArgs());
            }
        }

        /// <summary>
        /// Gets the undo count.
        /// </summary>
        public int UndoCount
        {
            get
            {
                return UndoStack.Count;
            }
        }

        /// <summary>
        /// Gets or sets the undo stack.
        /// </summary>
        public Stack<ByteModified> UndoStack
        {
            get { return _undoStack; }
            set { _undoStack = value; }
        }

        /// <summary>
        /// Get or set for indicate if control CanUndo
        /// </summary>
        public bool IsUndoEnabled
        {
            get { return _isUndoEnabled; }
            set { _isUndoEnabled = value; }
        }

        /// <summary>
        /// Check if the control can undone to a previous value
        /// </summary>
        /// <returns></returns>
        public bool CanUndo
        {
            get
            {
                if (IsUndoEnabled)
                    return _byteModifiedDictionary.Count > 0;
                else
                    return false;
            }
        }

        #endregion Undo / Redo

        #region Can do property...

        /// <summary>
        /// Return true if Copy method could be invoked.
        /// </summary>
        public bool CanCopy(long selectionStart, long selectionStop)
        {
            if (GetSelectionLenght(selectionStart, selectionStop) < 1 || !IsOpen)
                return false;

            return true;
        }

        /// <summary>
        /// Update a value indicating whether the current stream is supporting writing.
        /// </summary>
        public bool CanWrite
        {
            get
            {
                if (_stream != null)
                    if (!ReadOnlyMode)
                        return _stream.CanWrite;

                return false;
            }
        }

        /// <summary>
        /// Update a value indicating  whether the current stream is supporting reading.
        /// </summary>
        public bool CanRead
        {
            get
            {
                if (_stream != null)
                    return _stream.CanRead;

                return false;
            }
        }

        /// <summary>
        /// Update a value indicating  whether the current stream is supporting seeking.
        /// </summary>
        public bool CanSeek
        {
            get
            {
                if (_stream != null)
                    return _stream.CanSeek;

                return false;
            }
        }

        #endregion Can do property...

        #region Find methods

        /// <summary>
        /// Find all occurance of string in stream and return an IEnumerable contening index when is find.
        /// </summary>
        public IEnumerable<long> FindIndexOf(string stringToFind, long startPosition = 0)
        {
            return FindIndexOf(ByteConverters.StringToByte(stringToFind), startPosition);
        }

        /// <summary>
        /// Find all occurance of byte[] in stream and return an IEnumerable contening index when is find.
        /// </summary>
        /// <param name="findtest"></param>
        public IEnumerable<long> FindIndexOf(byte[] bytesTofind, long startPosition = 0)
        {
            //start position checkup
            if (startPosition > Length) startPosition = Length;
            else if (startPosition < 0) startPosition = 0;

            //var
            Position = startPosition;
            byte[] buffer = new byte[ConstantReadOnly.FIND_BLOCK_SIZE];
            IEnumerable<long> findindex;
            List<long> indexList = new List<long>();

            //Launch event at process strated
            IsOnLongProcess = true;
            LongProcessProgressStarted?.Invoke(this, new EventArgs());

            //start find
            for (long i = startPosition; i < Length; i++)
            {
                //Do not freeze UI...
                if (i % 2000 == 0)
                    LongProcessProgress = (double)Position / Length;

                //Break long process if needed
                if (!IsOnLongProcess)
                    break;

                if ((byte)ReadByte() == bytesTofind[0])
                {
                    //position correction after read one byte
                    Position--;
                    i--;

                    if (buffer.Length > Length - Position)
                        buffer = new byte[Length - Position];

                    //read buffer and find
                    _stream.Read(buffer, 0, buffer.Length);
                    findindex = buffer.FindIndexOf(bytesTofind);

                    //if byte if find add to list
                    if (findindex.Count() > 0)
                        foreach (long index in findindex)
                            indexList.Add(index + i + 1);

                    //position correction
                    i += buffer.Length;
                }
            }

            //Yield return all finded occurence
            foreach (long index in indexList)
                yield return index;

            //Launch event at process completed
            IsOnLongProcess = false;
            LongProcessProgressCompleted?.Invoke(this, new EventArgs());
        }

        #endregion Find methods

        #region Long process progress

        /// <summary>
        /// Get if byteprovider is on a long process. Set to false to cancel all process.
        /// </summary>
        public bool IsOnLongProcess
        {
            get
            {
                return _isOnLongProcess;
            }

            set
            {
                _isOnLongProcess = value;
            }
        }

        /// <summary>
        /// Get the long progress percent of job.
        /// When set (internal) launch event LongProcessProgressChanged
        /// </summary>
        public double LongProcessProgress
        {
            get
            {
                return _longProcessProgress;
            }

            internal set
            {
                _longProcessProgress = value;

                LongProcessProgressChanged?.Invoke(value, new EventArgs());
            }
        }

        #endregion Long process progress


    }
}