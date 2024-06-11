using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Meadow.Gateways.Bluetooth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace TLARCManager.Views
{
    public partial class WorkSpace : UserControl
    {
        private  IStorageFolder descriptorFolder => (Parent as WorkWindows).DescriptorFolder;
        public WorkSpace()
        {
            InitializeComponent();
        }
    }
}
