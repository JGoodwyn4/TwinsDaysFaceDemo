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

namespace TwinsDay2017_Demo
{
    /// <summary>
    /// Interaction logic for ServerSet.xaml
    /// </summary>
    public partial class ServerSet : Window
    {
        public string Server { get { return ServerPath.Text; } }
        public string Data { get { return DataPath.Text; } }

        public ServerSet()
        {
            InitializeComponent();

            ServerPath.Text = @"//ServerName/Path";
            DataPath.Text = @"X://FolderPath";
        }

        public ServerSet(string serverName, string dataName)
        {
            InitializeComponent();

            ServerPath.Text = serverName;
            DataPath.Text = dataName;
        }

        private void ServerButtonSet_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirm = System.Windows.MessageBox.Show("Set new server path?", "Confirm Server Path", MessageBoxButton.YesNo);

            if (confirm == MessageBoxResult.Yes)
            {
                // Send Server to main window to set server path
                ((MainWindow)this.Owner).SetServer(Server);

                // Set the server status to show if we were able to connect to the given path or not
                ServerStatusCheck();
            }
            
            // Do nothing else because we wont send over the data
        }

        private void DataButtonSet_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirm = System.Windows.MessageBox.Show("Set new data folder path?", "Confirm Data Folder Path", MessageBoxButton.YesNo);

            if (confirm == MessageBoxResult.Yes)
            {
                // Set the data status to show if we were able to connect to the given path or not
                if(((MainWindow)this.Owner).GetAccess(DataPath.Text))
                {
                    string tempDataPath = DataPath.Text;
                    bool allExist = true;
                    bool audioExist = false;
                    bool faceExist = false;
                    bool faceImgExist = false;

                    // Check if path ends with '/'
                    if (!DataPath.Text[DataPath.Text.Length - 1].Equals('/'))
                        tempDataPath += "//";

                    // Check audio folder
                    if (!Directory.Exists(tempDataPath + "Audio"))
                        allExist = false;
                    else
                        audioExist = true;

                    // Check face folder
                    if (!Directory.Exists(tempDataPath + "Face"))
                        allExist = false;
                    else
                        faceExist = true;

                    // Check face image folder
                    if (!Directory.Exists(tempDataPath + "Face Img"))
                        allExist = false;
                    else
                        faceImgExist = true;

                    if(!allExist)
                    {
                        MessageBoxResult confirmFolder = System.Windows.MessageBox.Show("One or more of the necessary data folders were missing.\nWould you like to continue and create the missing subfolders?", "Create Data Subfolders", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if(confirmFolder == MessageBoxResult.Yes)
                        {
                            // Create any folder that doesn't exist
                            if (!audioExist)
                                Directory.CreateDirectory(tempDataPath + "Audio");
                            if(!faceExist)
                                Directory.CreateDirectory(tempDataPath + "Face");
                            if(!faceImgExist)
                                Directory.CreateDirectory(tempDataPath + "Face Img");

                            ((MainWindow)this.Owner).SetData(Data);
                            DataStatusCheck();
                        }
                    }
                    else
                    {
                        ((MainWindow)this.Owner).SetData(Data);
                        DataStatusCheck();
                    }
                }
                else
                    System.Windows.MessageBox.Show("Directory could not be accessed or does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                // Check to see if the data folder path contains the appropriate subfolders
                // If not, notify user that they weren't found and must create the necessary subfolders
                // Ask if ok, if not, don't send Data to main window
            }

            // Do nothing else because we wont send over the data
        }

        private void DataFolderSearch_Click(object sender, RoutedEventArgs e)
        {
            // Open a directory search window to select a folder
            System.Windows.Forms.FolderBrowserDialog folderPicker = new System.Windows.Forms.FolderBrowserDialog();
            folderPicker.ShowNewFolderButton = true;

            if(folderPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                DataPath.Text = folderPicker.SelectedPath;
        }

        private void ServerReset_Click(object sender, RoutedEventArgs e)
        {
            ServerPath.Text = Server;
            DataPath.Text = Data;

            // Re-update the status for both
            ServerStatusCheck();
            DataStatusCheck();
        }

        private void ServerClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ServerStatusCheck()
        {
            if (((MainWindow)this.Owner).GetAccess(ServerPath.Text))
                ServerStatus.Fill = new SolidColorBrush(Colors.LightGreen);
            else
                ServerStatus.Fill = new SolidColorBrush(Colors.LightCoral);
        }

        private void DataStatusCheck()
        {
            if (((MainWindow)this.Owner).GetAccess(DataPath.Text))
                DataStatus.Fill = new SolidColorBrush(Colors.LightGreen);
            else
                DataStatus.Fill = new SolidColorBrush(Colors.LightCoral);
        }

        private void ServerPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerStatus.Fill = new SolidColorBrush(Colors.Gray);
        }

        private void DataPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataStatus.Fill = new SolidColorBrush(Colors.Gray);
        }

        private void ServerStatus_Loaded(object sender, RoutedEventArgs e)
        {
            ServerStatusCheck();
        }

        private void DataStatus_Loaded(object sender, RoutedEventArgs e)
        {
            DataStatusCheck();
        }
    }
}
