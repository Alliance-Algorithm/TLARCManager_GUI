using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace TLARCManager.Views
{
    public partial class WorkWindows : Window
    {
        private IStorageFolder descriptorFolder;
        public IStorageFolder DescriptorFolder => descriptorFolder;
        public WorkWindows()
        {
            InitializeComponent();
        }
        public WorkWindows(IStorageFolder folder)
        {
            InitializeComponent();
            descriptorFolder = folder;
            this.Title = folder.Path.ToString();
        }
        public async void OnOpenDirectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {  // �ӵ�ǰ�ؼ���ȡ TopLevel�����ߣ���Ҳ����ʹ�� Window ���á�
            var topLevel = TopLevel.GetTopLevel(this);

            // �����첽�����Դ򿪶Ի���
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Desciptor Folder",
                AllowMultiple = false
            });

            if (folder.Count >= 1)
            {
                var window = new WorkWindows(folder[0]);
                window.Show();

                this.Close();
            }
        }
        public void OnExitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
    }
}
