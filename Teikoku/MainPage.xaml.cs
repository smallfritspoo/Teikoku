using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teikoku
{
    /// <summary>
    /// Class for interacting with UWP and XAML Controls to
    /// read files into memory to be restored back to their
    /// original file location on demand.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        #region Class Scope Variables

        private PickerLocationId readFileDefaultLocation = PickerLocationId.ComputerFolder;
        private List<string> fileFilterList = new List<string>();
        private FocusedItem<FileController> listFocus = new FocusedItem<FileController>();
        private Stopwatch stopwatch;
        private FileControllerManager fileControllerManager = new FileControllerManager();

        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            
            FileListView.DataContext = fileControllerManager; // Sets the context for viewlist UI items to the collection of files

            /*This should be removed eventually as we do not care
             *about the file extension as we are reading bytes and
             *storing them in memory to be read back later. For now
             *this simply eases testing burdens.*/
            fileFilterList.Add(".xml");
            fileFilterList.Add(".txt");

            stopwatch = new Stopwatch(); // timer for timing operations
        }

        #region ListView Helpers

        /// <summary>
        /// Track which UI element is selected in list view
        /// so that we can preform actions on that object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListView lv = sender as ListView; // Cast sender object as ListView as this is the originator

            listFocus.Property = lv.SelectedItem as FileController; // Set our object tracking to that of the selected UI element

            if (listFocus.Property.FilePath != null)
                Debug.WriteLine("Focus set to: " + listFocus.Property.FilePath);
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Configure the filepicker by setting the default read location
        /// as well as the file extensions to display by default
        /// </summary>
        /// <param name="fileFilterList">list of strings representing file extensions to display in picker.</param>
        /// <returns>Configured filepicker with <see cref="FileOpenPicker.FileTypeFilter">FileTypeFilter</see>set</returns>
        private FileOpenPicker FilePickerSetup(List<string> fileFilterList)
        {
            var picker = new FileOpenPicker() // create new FileOpenPicker with provided settings
            {
                ViewMode = PickerViewMode.Thumbnail, // Set visual view mode of picker display to use thumbnails
                SuggestedStartLocation = readFileDefaultLocation // Set default location to 'this pc' in file explorer.
            };

            foreach (var filter in fileFilterList) // Loop through filter list and apply each filefiltertype
            {
                picker.FileTypeFilter.Add(filter);
            }

            return picker; // return the configured filepicker
        }

        /// <summary>
        /// Open a readonly stream to the selected file
        /// </summary>
        /// <param name="file"><see cref="StorageFile">StorageFile</see> that you wish to open a stream to</param>
        /// <returns>readonly <see cref="IRandomAccessStream"/> to provided <c>StorageFIle</c></returns>
        private static async Task<IRandomAccessStream> OpenStreamAsync(StorageFile file)
        {
            if (file != null)
            {
                var stream = await file.OpenAsync(FileAccessMode.Read);
                return stream;
            }
            else
                return null;
        }

        /// <summary>
        /// Read the file data and save it in memory.
        /// </summary>
        /// <param name="stream"><see cref="IRandomAccessStream"/> to read data from</param>
        /// <returns></returns>
        private static async Task<IBuffer> ReadDataFromStream(IRandomAccessStream stream)
        {
            if (stream.Size > 0) // If there is anything in the stream.
            {
                ulong size = stream.Size; // When to stop reading
                using (var inputStream = stream.GetInputStreamAt(0)) // streamline the inputstream disposal
                {
                    using (var DataReader = new DataReader(inputStream)) // DataReader with limited lifetime.
                    {
                        await DataReader.LoadAsync((uint)size); // Must be called prior to load data before passing to datareader
                        return DataReader.ReadBuffer((uint)size); // Read the data from the input stream.
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Handles file reading methods so we can do initial loading
        /// and on demand loading later.
        /// </summary>
        /// <param name="fc">A <see cref="FileController"/> object with a valid <see cref="FileController.File"/> property</param>
        /// <returns>The provided <see cref="FileController"/> which has had the data loaded into it</returns>
        private static async Task<FileController> LoadFileFromController(FileController fc)
        {
            if (fc.FilePath != null) // when the file path is set
            {
                var stream = await OpenStreamAsync(fc.File); // Open the stream

                fc.FileBytesLoaded = (uint)stream.Size; // Set file size property

                fc.FileData = await ReadDataFromStream(stream); // load the contents of the file into the filecontroller buffer

                return fc;
            }
            return null;
        }
        #endregion

        #region Button Logic

        /// <summary>
        /// SelectFile button logic. Sets up new FileController and FilePicker
        /// tracks file operation time and preforms neccessary operations for
        /// data stream and datareader.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SelectFileLocationButton(object sender, RoutedEventArgs e)
        {
        
            var fc = new FileController(); // Create method scoped FileController

            stopwatch.Start(); // start stopwatch to track file operation time.

            Debug.WriteLine("Beginning File load and read operations");

            fc.FilePicker = FilePickerSetup(fileFilterList); // setup item picker dialog

            fc.File = await fc.FilePicker.PickSingleFileAsync(); // Run item picker dialog and wait for selection

            if (fc.FilePath != null) // When our path is set we can do something
            {
                fc = await LoadFileFromController(fc); // preform our file operations
            }

            stopwatch.Stop(); // stop file operation timer

            Debug.WriteLine("Read Operations completed in: " + stopwatch.Elapsed);

            if (!fileControllerManager.Collection.Any(c => c.FilePath == fc.FilePath)) // iterate over collection, see if file exists.
                fileControllerManager.Collection.Add(fc); // Add non matching FileControllers

        }

        /// <summary>
        /// Button to display ContentDialog screen with file details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowFileDetailsButton(object sender, RoutedEventArgs e)
        {
            FileDetailsContentDialog dialog = new FileDetailsContentDialog(listFocus.Property); // Create a new content dialog window.

            ContentDialogResult result = await dialog.ShowAsync(); // record the return value of our dialog window

            if (result.Equals(ContentDialogResult.Secondary)) 
            {
                var item = fileControllerManager.Collection.FirstOrDefault(i => i.FilePath == dialog.FileController.FilePath); // query our collection for the selected item

                if (item != null)
                {
                    var fc = await LoadFileFromController(listFocus.Property); // load the data from file into new FileController

                    fileControllerManager.Collection[fileControllerManager.Collection.IndexOf(item)] = fc;  // replace the FileController with new data

                    listFocus.Property = fileControllerManager.Collection[fileControllerManager.Collection.IndexOf(item)]; // Set focus to new item.
                }
            }
        }

        /// <summary>
        /// Writes the in memory data back to the StorageFile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WriteFileButton(object sender, RoutedEventArgs e)
        {
            if (listFocus.Property.FileBytesLoaded != 0) // If there is any stored data
            {
                var stream = await listFocus.Property.File.OpenAsync(FileAccessMode.ReadWrite); // Open readwrite stream to file

                using (var outputStream = stream.GetOutputStreamAt(0)) // create output stream
                {
                    using (var dataWriter = new DataWriter(outputStream)) // datawriter from stream
                    {
                        dataWriter.WriteBuffer(listFocus.Property.FileData); // Write buffer to output stream
                        await dataWriter.StoreAsync(); // write output stream to file location
                        await outputStream.FlushAsync(); // flush the outputstream
                    }
                }
                stream.Dispose(); // close stream
            }
        }
        #endregion
    }
}