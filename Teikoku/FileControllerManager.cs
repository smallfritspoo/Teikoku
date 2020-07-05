using System.Collections.ObjectModel;

namespace Teikoku
{
    public class FileControllerManager
    {
        public ObservableCollection<FileController> Collection { get; set; }

        public FileControllerManager()
        {
            Collection = new ObservableCollection<FileController>();
        }
    }
}
