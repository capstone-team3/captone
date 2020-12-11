using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CapTone.Core.Helpers;
using CapTone.Core.Models;

using CapTone.Repository;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Syncfusion.XlsIO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace CapTone.ViewModels
{
    public class 데이터베이스ViewModel : ViewModelBase
    {
        private IList<outputData> _outputdata;


        public 데이터베이스ViewModel()
        {
            Init();
        }
        private void Init()
        {
            DeleteOneCommand = new DelegateCommand(OnDeleteOneCommand);
            ExportCommand = new DelegateCommand(OnExportCommand);
            DeleteCommand = new DelegateCommand(OnDeleteCommand);
        }

        private async void OnDeleteOneCommand()
        {
            List<outputData> delDatas = Views.데이터베이스Page.delData;

            ContentDialog DelOne = new ContentDialog()
            {
                Title = "해당 데이터를 삭제하시겠습니까?",

                PrimaryButtonText = "확인",
                SecondaryButtonText = "취소",

            };
            ContentDialogResult delRes = await DelOne.ShowAsync();
            if (delRes == ContentDialogResult.Primary)
            {
                for (int i = 0; i < delDatas.Count; i++)
                {
                    outputData inputData = delDatas[i];
                    outputs.DeleteOneItem(inputData);
                    //inUser.Remove(selUser[i]);
                }


                var msgboxDlg = new MessageDialog("해당 항목이 삭제되었습니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
            }
            await GetOutputAsync();

        }

        private async void OnDeleteCommand()
        {
            ContentDialog DelAll = new ContentDialog()
            {
                Title = "모든 데이터를 삭제하시겠습니까?",

                PrimaryButtonText = "확인",
                SecondaryButtonText = "취소",

            };
            ContentDialogResult delRes = await DelAll.ShowAsync();
            if (delRes == ContentDialogResult.Primary)
            {
                outputs.DeleteAllData();

                var msgboxDlg = new MessageDialog("삭제되었습니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
            }

            await GetOutputAsync();
        }

        private async void OnExportCommand()
        {

            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                worksheet.Range[1, 1].Text = "이름";
                worksheet.Range[1, 2].Text = "전화번호";
                worksheet.Range[1, 3].Text = "주소";
                worksheet.Range[1, 4].Text = "품목";

                for (int i = 0; i < OutputData.Count; i++)
                {
                    outputData inExcel = OutputData[i];

                    worksheet.Range[2 + i, 1].Text = inExcel.Name;
                    worksheet.Range[2 + i, 2].Text = inExcel.PhoneNumber;
                    worksheet.Range[2 + i, 3].Text = inExcel.HomeAddress;
                    worksheet.Range[2 + i, 4].Text = inExcel.Items;
                }

                StorageFile storageFile;
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
                savePicker.SuggestedFileName = "Output";
                savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                storageFile = await savePicker.PickSaveFileAsync();

                //Saving the workbook
                await workbook.SaveAsAsync(storageFile);

                // Launch the saved file
                await Windows.System.Launcher.LaunchFileAsync(storageFile);
            }
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            await GetOutputAsync();
        }

        private async Task GetOutputAsync()
        {

            OutputData = new List<outputData>(outputs.GetAllData());
            if (OutputData == null)
            {
                return;
            }

        }
        public IList<outputData> OutputData
        {
            get => _outputdata;
            set => SetProperty(ref _outputdata, value);
        }
        public ICommand DeleteOneCommand { get; private set; }
        public ICommand ExportCommand { get; set; } // export 버튼 클릭 시
        public ICommand DeleteCommand { get; set; }
    }
}
