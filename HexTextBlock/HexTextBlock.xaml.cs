using System;
using System.Collections.Generic;
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
using WPFHexaEditor.Core.Bytes;

namespace HexTextBlock{
    /// <summary>
    /// Interaction logic for HexEditor.xaml
    /// </summary>
    public partial class HexTextBlockEditor : UserControl {
        public HexTextBlockEditor() {
            InitializeComponent();
            this.MouseWheel += (sender, e) => {
                VerticalScrollBar.Value += -e.Delta;
            };
            this.Loaded += (sender, e) => {
                UpdateVerticalScrollBar();
            };
            this.SizeChanged += (sender, e) => UpdateVerticalScrollBar();
        }
        
        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var val = (long)e.NewValue;
            //This Position_PropertyChanged will refresh automatically.
            Position = val * BytePerLine;
        }
        
        /// <summary>
        /// This occurs when Stream changed or resized;
        /// </summary>
        private void UpdateVerticalScrollBar() {
            if (Stream != null) {
                VerticalScrollBar.Maximum = (Stream?.Length ?? 0) / BytePerLine;
            }
        }
        
        /// <summary>
        /// Get or set the number of byte are show in control
        /// </summary>
        public int BytePerLine {
            get { return (int)GetValue(BytePerLineProperty); }
            set { SetValue(BytePerLineProperty, value); }
        }

        public static readonly DependencyProperty BytePerLineProperty =
            DependencyProperty.Register("BytePerLine", typeof(int), typeof(HexTextBlockEditor),
                new FrameworkPropertyMetadata(16, new PropertyChangedCallback(BytePerLine_PropertyChanged),
                    new CoerceValueCallback(BytePerLine_CoerceValue)));

        private static object BytePerLine_CoerceValue(DependencyObject d, object baseValue) {
            var value = (int)baseValue;

            if (value < 8)
                return 8;
            else if (value > 32)
                return 32;
            else
                return baseValue;
        }

        private static void BytePerLine_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stream is the only datasource to the control,it should be given from out of box,
        /// and Stream can't be closed by the control.
        /// </summary>
        public Stream Stream {
            get {
                return GetValue(StreamProperty) as Stream;
            }
            set {
                SetValue(StreamProperty, value);
            }
        }
        public static readonly DependencyProperty StreamProperty = DependencyProperty.Register(nameof(Stream), typeof(Stream), typeof(HexTextBlockEditor),
            new PropertyMetadata(Stream_PropertyChanged));

        private static void Stream_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var stream = e.NewValue as Stream;
            var ctrl = d as HexTextBlockEditor;
            if(stream != null) {
                ctrl.UpdateVerticalScrollBar();
            }
            ctrl.RefreshView();
        }

        /// <summary>
        /// This property indicate that the first "visible" byte position for the stream;
        /// (To replace the GetFirstVisibleBytePosition),
        /// what'more for mvvm pattern,viewmodel can "scroll" the view by this Property.
        /// </summary>
        public long Position {
            get {
                return (long)GetValue(PositionProperty);
            }
            set {
                SetValue(PositionProperty, value);
            }
        }
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(nameof(Position), typeof(long), typeof(HexTextBlockEditor),
            new FrameworkPropertyMetadata(0L,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Position_PropertyChanged));

        private static void Position_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as HexTextBlockEditor;
            var position = (long)e.NewValue;
            //Check whether position is valid;
            if(position % ctrl.BytePerLine == 0) {
                ctrl.RefreshView();
            }
        }
        
        private void Close() {
            UpdateVerticalScrollBar();
            txbHex.Text = string.Empty;
        }

        private void RefreshView(bool controlResize = false,bool refreshData = true) {
            UpdateStringAndDataViewer();
        }

        long scrollPriLevel = 0;
        private object readLocker = new object();
        private void UpdateStringAndDataViewer() {
            var curLevel = ++scrollPriLevel;
            var acHeight = txbHex.ActualHeight;
            var stream = Stream;
            var bytePerLine = BytePerLine;
            var pos = Position;

            //Use threadpool to let the control scroll more "fluent".
            ThreadPool.QueueUserWorkItem(cb => {
                lock (readLocker) {
                    if (curLevel != scrollPriLevel) {
                        return;
                    }
                    var buffSize = (int)acHeight / 12 * bytePerLine;
                    var buffer = new byte[buffSize];
                    stream.Position = pos;
                    var readLen = stream.Read(buffer, 0, buffSize);
                    var sb = new StringBuilder();
                    for (int i = 0; i < readLen; i++) {
                        sb.Append(new string(ByteConverters.ByteToHexCharArray(buffer[i])) + " ");
                    }
                    if (curLevel == scrollPriLevel) {
                        this.Dispatcher.Invoke(() => {
                            txbHex.Text = sb.ToString();
                        });
                    }
                }
            });
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e) {
            VerticalScrollBar.Value -= e.Delta;
        }
    }
}
