using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace WPFHexaEditorExample {
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window {
        public MainWindow2() {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void MenuItem_OpenFile_Click(object sender, RoutedEventArgs e) {
            hexTbl.Stream?.Close();

            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;

            if(dialog.ShowDialog() == true) {
                var fs = dialog.OpenFile();
                hexTbl.Stream = fs;
            }
        }
    }

    public class MainWindowViewModel:BindableBase {

        private long _position = -1;
        public long Position {
            get {
                return _position;
            }
            set {
                SetProperty(ref _position, value);
            }
        }

    }
}
