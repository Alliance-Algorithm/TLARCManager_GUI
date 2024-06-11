
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TLARCManager.ViewModels
{
    public class MainComponentsViewModel : ViewModelBase
    {
        public MainComponentsViewModel()
        {
            var service = new DataModels.ComponentsServeice();
            Components = new ComponentsViewModel(service.GetItems());
        }

        public ComponentsViewModel Components { get; }
    }

    public class ComponentsViewModel : ViewModelBase
    {
        public ComponentsViewModel(IEnumerable<DataModels.ComponentsItem> items)
        {
            ListItems = new ObservableCollection<DataModels.ComponentsItem>(items);
        }

        public ObservableCollection<DataModels.ComponentsItem> ListItems { get; }
    }

}