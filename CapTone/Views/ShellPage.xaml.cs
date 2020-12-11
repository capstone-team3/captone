using System;

using CapTone.ViewModels;

using Windows.UI.Xaml.Controls;

namespace CapTone.Views
{
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {

        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        public Frame ShellFrame => shellFrame;

        public string header { get; set; }

        public ShellPage()
        {
            InitializeComponent();
        }

        public void SetRootFrame(Frame frame)
        {
            shellFrame.Content = frame;

            //navigationViewHeaderBehavior.Initialize(frame);
            ViewModel.Initialize(frame, navigationView);
        }
    }
}
