//////////////////////////////////////////////
// MIT License  - 2016-2017
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFHexaEditor.Core;
using WPFHexaEditor.Core.Bytes;
using WPFHexaEditorExample.Properties;

namespace WPFHexaEditorExample
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum SettingEnum
        {
            HeaderVisibility,
            ReadOnly,
            ScrollVisibility
        }

        public MainWindow()
        {
            InitializeComponent();

            UpdateAllSettings();
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            
            if (fileDialog.ShowDialog() != null)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    Application.Current.MainWindow.Cursor = Cursors.Wait;

                    HexEdit.FileName = fileDialog.FileName;
                    //MemoryStream stream = new MemoryStream(File.ReadAllBytes(fileDialog.FileName));
                    //HexEdit.Stream = stream;

                    Application.Current.MainWindow.Cursor = null;
                }
                //else
                //    MessageBox.Show("File not found!", Settings.Default.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;
            ///HexEdit.SaveTBLFile();
            HexEdit.SubmitChanges();
            Application.Current.MainWindow.Cursor = null;
        }

        private void CloseFileMenu_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.Close();
        }

        private void SetReadOnlyMenu_Click(object sender, RoutedEventArgs e)
        {
            UpdateSetting(SettingEnum.ReadOnly);
        }

        private void ShowHeaderMenu_Click(object sender, RoutedEventArgs e)
        {
            UpdateSetting(SettingEnum.HeaderVisibility);
        }

        private void UpdateSetting(SettingEnum setting)
        {
            switch (setting)
            {
                case SettingEnum.HeaderVisibility:
                    if (!Settings.Default.HeaderVisibility)
                        HexEdit.HeaderVisibility = Visibility.Collapsed;
                    else
                        HexEdit.HeaderVisibility = Visibility.Visible;

                    Settings.Default.HeaderVisibility = HexEdit.HeaderVisibility == Visibility.Visible;
                    break;

                case SettingEnum.ScrollVisibility:
                    if (!Settings.Default.ScrollVisibility)
                        HexEdit.VerticalScrollBarVisibility = Visibility.Collapsed;
                    else
                        HexEdit.VerticalScrollBarVisibility = Visibility.Visible;

                    Settings.Default.ScrollVisibility = HexEdit.VerticalScrollBarVisibility == Visibility.Visible;
                    break;

                case SettingEnum.ReadOnly:
                    HexEdit.ReadOnlyMode = Settings.Default.ReadOnly;

                    HexEdit.ClearAllChange();
                    HexEdit.RefreshView();
                    break;
            }
        }

        private void UpdateAllSettings()
        {
            UpdateSetting(SettingEnum.HeaderVisibility);
            UpdateSetting(SettingEnum.ReadOnly);
            UpdateSetting(SettingEnum.ScrollVisibility);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyHexaMenu_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.CopyToClipboard(CopyPasteMode.HexaString);
        }

        private void CopyStringMenu_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.CopyToClipboard();
        }

        private void DeleteSelectionMenu_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.DeleteSelection();
        }

        private void ShowScrollMenu_Click(object sender, RoutedEventArgs e)
        {
            UpdateSetting(SettingEnum.ScrollVisibility);
        }

        private void SetPositionMenu_Click(object sender, RoutedEventArgs e)
        {
        }

        private void GOPosition_Click(object sender, RoutedEventArgs e)
        {
            long position = 0;
            if (long.TryParse(PositionText.Text, out position))
                HexEdit.SetPosition(position, 1);
            else
                MessageBox.Show("Enter long value.");

            ViewMenu.IsSubmenuOpen = false;
        }

        private void GOHexPosition_Click(object sender, RoutedEventArgs e)
        {
            if (ByteConverters.IsHexaValue(PositionHexText.Text))
            {
                long position = ByteConverters.HexLiteralToLong(PositionHexText.Text);
                if (position > 0)
                    HexEdit.SetPosition(position, 1);
                else
                    MessageBox.Show("Enter hexa value.");
            }

            ViewMenu.IsSubmenuOpen = false;
        }

        private void PositionHexText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ByteConverters.IsHexaValue(PositionHexText.Text))
                GoPositionHexaButton.IsEnabled = true;
            else
                GoPositionHexaButton.IsEnabled = false;
        }

        private void PositionText_TextChanged(object sender, TextChangedEventArgs e)
        {
            long position = 0;
            if (long.TryParse(PositionText.Text, out position))
                GoPositionButton.IsEnabled = true;
            else
                GoPositionButton.IsEnabled = false;
        }

        private void UndoMenu_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.Undo();
        }

        private void SetBookMarkButton_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.SetBookMark();
        }

        private void DeleteBookmark_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.ClearScrollMarker(ScrollMarker.Bookmark);
        }

        private void FindText_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void FindFirstButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void FindPreviousButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void FindAllSelection_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.FindAllSelection(true);
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.SelectAll();
        }

        private void FindNextButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CTableASCIIButton_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.TypeOfCharacterTable = CharacterTableType.ASCII;
            CTableASCIIButton.IsChecked = true;
            CTableTBLButton.IsChecked = false;
            CTableTBLDefaultASCIIButton.IsChecked = false;
        }

        private void CTableTBLButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() != null)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    Application.Current.MainWindow.Cursor = Cursors.Wait;

                    HexEdit.LoadTBLFile(fileDialog.FileName);
                    HexEdit.TypeOfCharacterTable = CharacterTableType.TBLFile;
                    CTableASCIIButton.IsChecked = false;
                    CTableTBLButton.IsChecked = true;
                    CTableTBLDefaultASCIIButton.IsChecked = false;

                    Application.Current.MainWindow.Cursor = null;
                }
                //else
                //    MessageBox.Show("File not found!", Settings.Default.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CTableTBLDefaultASCIIButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.TypeOfCharacterTable = CharacterTableType.TBLFile;
            HexEdit.LoadDefaultTBL(WPFHexaEditor.Core.CharacterTable.DefaultCharacterTableType.ASCII);
            CTableASCIIButton.IsChecked = false;
            CTableTBLButton.IsChecked = false;
            CTableTBLDefaultASCIIButton.IsChecked = true;

            Application.Current.MainWindow.Cursor = null;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.Foreground = Brushes.Red;
        }
    }
}