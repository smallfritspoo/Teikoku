using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;

namespace Teikoku
{

    /// <summary>
    /// FileController class to handle all aspects of each StorageFile
    /// </summary>
    /// <remarks>This Class will format values for display in UI and
    /// inherits from <see cref="INotifyPropertyChanged"/> so changed 
    /// values will create events to be subscribed to.</remarks>
    public class FileController : INotifyPropertyChanged
    {

        #region Class Scope Variables

        private string _filePath;
        private uint _fileSize;
        private FileOpenPicker _filePicker;
        private StorageFile _file;
        private IBuffer _fileData;
        private uint _fileBytesLoaded;
        private string _fileFormattedSize;

        #endregion

        #region Events and Properties

        public event PropertyChangedEventHandler PropertyChanged; // our event handler

        public bool DebugLogging { get; set; } // Set DebugLogging

        /// <value>full path to file represented by string</value>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged(); // Notify Handler

                if (DebugLogging)
                    Debug.WriteLine("File Path: " + _filePath);
            }
        }

        /// <value>File path string formatted for UI Display</value>
        public string FilePathFormatted
        {
            get { return "File Location: " + _filePath; }
        }

        /// <value>File Size in bytes formatted for UI display</value>
        public string FileFormattedSize
        {
            get { return _fileFormattedSize; }
            set
            {
                _fileFormattedSize = "File Size: " + value;
            }
        }

        /// <value>File size in bytes</value>
        public uint FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                OnPropertyChanged();

                if (DebugLogging)
                    Debug.WriteLine("File Size: " + _fileSize);
            }
        }

        /// <value><see cref="IBuffer"/> of data loaded from file</value>
        public IBuffer FileData
        {
            get { return _fileData; }
            set
            {
                _fileData = value;

                if (DebugLogging)
                    Debug.WriteLine("Buffer set with byte length of: " + _fileData.Length);
            }
        }

        /// <value><see cref="FilePicker"/> file picker object used to select file</value>
        public FileOpenPicker FilePicker
        {
            get { return _filePicker; }
            set
            {
                _filePicker = value;
                UpdateFilePath();

                if (DebugLogging)
                    Debug.WriteLine("File Picker object set");
            }
        }

        /// <value>Windows StorageFile representing the selected file</value>
        public StorageFile File
        {
            get { return _file; }
            set
            {
                _file = value;
                UpdateFilePath();

                if (_file.Path != null && DebugLogging)
                    Debug.WriteLine("Storage File set with path of: " + _file.Path);
            }
        }

        /// <value>the quantity of bytes loaded</value>
        public uint FileBytesLoaded
        {
            get { return _fileBytesLoaded; }
            set
            {
                _fileBytesLoaded = value;
                UpdateFileSize(_fileBytesLoaded);

                if (DebugLogging)
                    Debug.WriteLine("File Bytes set, loaded bytes: " + _fileBytesLoaded);
            }
        }

        #endregion
        #region Delegates

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Helpers

        protected void UpdateFileSize(ulong size)
        {
            if (size != 0)
            {
                FileSize = (uint)size;
                FileFormattedSize = BytesToString((long)size);
            }
        }

        protected void UpdateFilePath()
        {
            if (File != null)
                FilePath = File.Path;
        }

        static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        #endregion

    }
}
