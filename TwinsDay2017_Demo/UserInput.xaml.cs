using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TwinsDay2017_Demo
{
    /// <summary>
    /// Interaction logic for UserInput.xaml
    /// </summary>
    public partial class UserInput : Window
    {
        public string ServerName { get { return UserTextInput.Text; } }

        public UserInput(string question, string windowTitle = "Input Prompt", string textPrompt = "Input: ", string defaultAnswer = @"\\ServerName\Folder", bool hasImage = true)
        {
            InitializeComponent();
            this.Title = windowTitle;

            InputQuestion.Text = question;
            UserTextInput.Text = defaultAnswer;
            TextPrompt.Content = textPrompt;

            if (hasImage)
                InputIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            else
                InputIcon.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void InputOK_Click(object sender, RoutedEventArgs e)
        {
            // Check validity of server info/request access
            if (((MainWindow)this.Owner).GetAccess(UserTextInput.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // Just in case the path was missing the last backslash,
                // try again with the backslash at the end
                // This is mostly intended if someone was trying to enter in a
                // base server address without the last backslash (ex. "\\ServerName")
                // Since we know our network drives won't connect without the ending
                // backslash, not to mention the needed drive letter (ex. "\\Server\X"),
                // this is just added backup for those rare cases
                if (!UserTextInput.Text[UserTextInput.Text.Length - 1].Equals('\\'))
                {
                    if (((MainWindow)this.Owner).GetAccess(UserTextInput.Text + "\\"))
                    {
                        UserTextInput.Text += "\\";
                        this.DialogResult = true;
                        this.Close();
                    }
                }

                System.Windows.Forms.MessageBox.Show("Could not get access to specified server", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
        }

        private void InputCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void InputWindow_ContentRendered(object sender, EventArgs e)
        {
            UserTextInput.SelectAll();
            UserTextInput.Focus();
        }

        private void InputWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(this.DialogResult == null)
                this.DialogResult = false;
        }


    }
}
