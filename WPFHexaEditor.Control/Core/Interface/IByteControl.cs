using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFHexaEditor.Core;

namespace WPFHexaEditor.Control.Interface {
    /// <summary>
    /// The interface is to reduce the code,
    /// which the HexByteControl and stringbytecontrol inherit from.
    /// </summary>
    public interface IByteControl {
        long BytePositionInFile { get; set; }
        ByteAction Action { get; set; }
        byte? Byte { get; set; }
        bool IsFocus { get; set; }
        string HexString { get; }
        bool IsHighLight { get; set; }
        bool IsSelected { get; set; }
        bool FirstSelected { get; set; }
        bool ReadOnlyMode { get; set; }
    }
}
