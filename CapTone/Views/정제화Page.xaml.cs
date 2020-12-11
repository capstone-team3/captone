using System;
using System.Collections.Generic;
using CapTone.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CapTone.Views
{
    public sealed partial class 정제화Page : Page
    {
        internal static string newname;
        internal static string newphone;
        internal static string newaddr;
        internal static string newitem;




        private 정제화ViewModel ViewModel => DataContext as 정제화ViewModel;


        public 정제화Page()
        {
            InitializeComponent();
        }

        private void get_change_text(object sender, RoutedEventArgs e)
        {
            newname = name_filed.Text;
            newphone = phone_filed.Text;
            newaddr = addr_filed.Text;
            newitem = item_filed.Text;
        }

    }
}
