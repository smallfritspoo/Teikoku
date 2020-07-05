using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Teikoku
{
    /// <summary>
    /// Potential button types
    /// </summary>
    public enum DialogResult
    {
        Ok,
        No,
        Cancel,
        Reload,
        Nothing
    }

    /// <summary>
    /// ContentDialog class for displaying detailed file information
    /// </summary>
    public sealed partial class FileDetailsContentDialog : ContentDialog
    {

        #region Class Scope Variables

        public FileController FileController { get; set; }
        public DialogResult Result { get; set; }
        public string FileDetails { get; set; }
        public string FileDetailsTitle { get; set; }

        #endregion

        /// <summary>
        /// Setup new ContentDialog and define display information
        /// </summary>
        /// <param name="fc">a populated <see cref="FileController"/></param>
        public FileDetailsContentDialog(FileController fc)
        {
            FileController = fc; // Setup FileController property
            Result = DialogResult.Nothing; // Default dialog response

            if (fc != null)
                FileDetails = "File Path: " + FileController.FilePathFormatted + "\nFile Size: " + FileController.FileFormattedSize
                    + "\nFileCreated :" + FileController.File.DateCreated;

            Debug.WriteLine("File Details Dialog Opened");

            this.InitializeComponent();
        }

        /// <summary> Set dialog response to <see cref="DialogResult.Ok"/> when user selects primary button </summary>
        private void ContentDialog_PrimaryButtonClick(object sender, RoutedEventArgs args)
        {
            this.Result = DialogResult.Ok;

            FileDetailsDialog.Hide();
        }

        /// <summary> Set dialog response to <see cref="DialogResult.Reload"/> when user selects secondary button </summary>
        private void ContentDialog_SecondaryButtonClick(object sender, RoutedEventArgs args)
        {
            this.Result = DialogResult.Reload;

            FileDetailsDialog.Hide();
        }

        /// <summary> Set dialog response to <see cref="DialogResult.Cancel"/> when user selects Close button </summary>
        private void ContentDialog_CloseButtonClick(object sender, RoutedEventArgs args)
        {
            this.Result = DialogResult.Cancel;

            FileDetailsDialog.Hide();
        }
    }
}
