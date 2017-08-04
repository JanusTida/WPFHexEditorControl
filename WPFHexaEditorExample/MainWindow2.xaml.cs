using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFHexaEditorExample.Abstracts;
using WPFHexaEditorExample.Commands;

namespace WPFHexaEditorExample {
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window {
        private MainWindow2ViewModel vm;
        public MainWindow2() {
            InitializeComponent();
            this.DataContext = vm = new MainWindow2ViewModel();
        }
    }

    public class MainWindow2ViewModel : BindableBase {
        private long _position;
        public long Position {
            get {
                return _position;
            }
            set {
                SetProperty(ref _position, value);
            }
        }

        private Stream _stream;
        public Stream Stream {
            get {
                return _stream;
            }
            set {
                SetProperty(ref _stream, value);
            }
        }

        private RelayCommand _openFileCommand;
        public RelayCommand OpenFileCommand =>
            _openFileCommand ?? (_openFileCommand = new RelayCommand(
                () => {
                    Stream?.Close();

                    var dialog = new OpenFileDialog();
                    dialog.Multiselect = false;

                    if (dialog.ShowDialog() == true) {
                        var fs = dialog.OpenFile();
                        Stream = fs;
                    }
                })
            );
    }
}
