using System;

using CapTone.ViewModels;
using CapTone.Core.Models;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

namespace CapTone.Views
{
    public sealed partial class 데이터베이스Page : Page
    {
        private 데이터베이스ViewModel ViewModel => DataContext as 데이터베이스ViewModel;

        internal static List<outputData> delData;

        // TODO WTS: Change the grid as appropriate to your app, adjust the column definitions on 데이터베이스Page.xaml.
        // For more details see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid
        public 데이터베이스Page()
        {
            InitializeComponent();
        }

        private void get_delete_data(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            delData = new List<outputData>();

            foreach(outputData item in DataGrid01.SelectedItems)
            {
                delData.Add(item);
            }
        }
    }
}
