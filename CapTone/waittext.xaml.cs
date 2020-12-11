using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace CapTone
{
    public sealed partial class waittext : UserControl
    {
Popup popup;
    public waittext()
    {
        this.InitializeComponent();                  
    }

    public  waittext(string message)
    {
        this.InitializeComponent();
        SystemNavigationManager.GetForCurrentView().BackRequested += UWPHUD_BackRequested;
        Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
        msg_Txt.Text = message;
    }

    private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
    {
        UpdateUI();
    }

    private void UWPHUD_BackRequested(object sender, BackRequestedEventArgs e)
    {
        Close();

    }
    private void UpdateUI()
    {

        var bounds = Window.Current.Bounds;
        this.Width = bounds.Width;
        this.Height = bounds.Height;
    }

    public void Show()
    {       
        popup = new Popup();
        popup.Child = this;   
        progress_R.IsActive = true;       
        popup.IsOpen = true;
        UpdateUI();
    }



    public void Close()
    {
        if (popup.IsOpen)
        {
            progress_R.IsActive = false;
            popup.IsOpen = false;
            SystemNavigationManager.GetForCurrentView().BackRequested -= UWPHUD_BackRequested;
            Window.Current.CoreWindow.SizeChanged -= CoreWindow_SizeChanged;
        }
    }
    }
}
