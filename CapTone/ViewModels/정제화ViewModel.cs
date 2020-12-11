using System;
using System.Collections.Generic;
using System.Windows.Input;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using CapTone.Core.Models;
using Windows.UI.Popups;
using System.IO;
using Windows.Storage;
using CapTone.Repository;
using System.Threading.Tasks;
using System.Net;
using Nancy.Json;
using Newtonsoft.Json.Linq;
using System.Text;
/*-----------------------*/




namespace CapTone.ViewModels
{

    public class 정제화ViewModel : ViewModelBase
    {
        /*------------------------------------------------------------------------*/
        private string _fileName;
        /*------------------------------------------------------------------------*/
        private string _dialog;
        /*------------------------------------------------------------------------*/
        private List<outputData> _outputdata;
        private ObservableCollection<outputData> _inText;
        private string _filePath;
        private IList<outputData> _outdatas;
        private IList<string> _inItem;
        private IList<string> _inUser;
        private string _inTextName;
        private string _inTextPhone;
        private string _inTextAdd;
        private string _inTextItem;
        private string _progresstext;
        private bool _namebool;
        private bool _phonebool;
        private bool _addrbool;
        private bool _itembool;
        private List<string> _alldialog;
        private int _count;
        private int mode;
        private int nlp_error = 0;
        private int _nextcount;
        private List<string> _allfile;

        /*------------------------------------------------------------------------*/
        public 정제화ViewModel()
        {
            Init();
        }
        private async void Init()
        {
            namebool = true;
            phonebool = true;
            addrbool = true;
            itembool = true;
            ImportCommand = new DelegateCommand(OnImportCommand);
            // ExportCommand = new DelegateCommand(OnExportCommand);
            OKCommand = new DelegateCommand(OnOKCommand);
            /*-----------------------------------------------*/
            AddItems = new DelegateCommand(OnAddItems);
            //ItemDeleteCommand = new DelegateCommand(OnItemDeleteCommand);

            CheckItems = new DelegateCommand(OnCheckItems);
            DoNLP = new DelegateCommand(OnDoNLP);
            EditCommand = new DelegateCommand(OnEditCommand);
            CancelCommand = new DelegateCommand(OnCancelCommand);
            nlp_error = 0;
            count = 1;
            next_count = 1;
            /*-----------------------------------------------*/
            AddUsers = new DelegateCommand(OnAddUsers);
            CheckUsers = new DelegateCommand(OnCheckUsers);
            mode = 0;


            // 버튼 초기화
            OutputData = new List<outputData>();
            inTextName = "ex)홍길동";
            inTextPhone = "ex)010 0000 0000";
            inTextAdd = "ex)서울시 동작구 흑석로 84";
            inTextItem = "ex) 고등어 10마리";
            inText = new ObservableCollection<outputData>();
            all_dialog = new List<string>();
            inText.Add(new outputData() { Name = "ex)홍길동", PhoneNumber = "ex)010 0000 0000", HomeAddress = "ex)서울시 동작구 흑석로 84", Items = "ex)고등어 10마리" });
            inItem = new ObservableCollection<string>(itemlist.GetAllData());
            inUser = new ObservableCollection<string>(userlist.GetAllData());

            while(inUser.Count <= 0)
            {
                await AddUserTask();
            }



        }

        private async void OnCancelCommand()
        {
            if(next_count >= count)
            {
                var msgboxDlg = new MessageDialog("실행 할 수 있는 파일이 없습니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
                return;
            }

            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "해당 작업을 스킵하시겠습니까?",
                PrimaryButtonText = "확인",
                SecondaryButtonText = "취소"
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                nlp_error = 0;
                if (mode == 2)
                {
                    mode_2_func();
                    if(next_count == count)
                        progressText = next_count + "/" + count + " file";
                }
                else if (mode == 3)
                {
                    mode_3_func();
                    if(next_count == count)
                        progressText = next_count + "/" + count + " file";
                }
                else if(mode == 1)
                {
                    next_count = 1;
                    progressText = next_count + "/" + count + " file";

                }
            }
        }

        private async void OnCheckUsers()
        {
            ListView userListView = new ListView()
            {
                ItemsSource = inUser,
                IsItemClickEnabled = true,

            };
            userListView.SelectionMode = ListViewSelectionMode.Multiple;
            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "등록하신 카카오톡(밴드)명 입니다",

                PrimaryButtonText = "전체 삭제",
                SecondaryButtonText = "선택 삭제",
                CloseButtonText = "확인",


                Content = userListView
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                ContentDialog DelAll = new ContentDialog()
                {
                    Title = "모든 카카오톡(밴드)명을 삭제하시겠습니까?",

                    PrimaryButtonText = "확인",
                    SecondaryButtonText = "취소",

                };
                ContentDialogResult delRes = await DelAll.ShowAsync();
                if (delRes == ContentDialogResult.Primary)
                {
                    userlist.DeleteAllData();
                    inUser.Clear();

                    var msgboxDlg = new MessageDialog("삭제되었습니다.");
                    msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg.DefaultCommandIndex = 0;
                    await msgboxDlg.ShowAsync();
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                List<string> selUser = new List<string>();
                foreach (string user in userListView.SelectedItems)
                {
                    selUser.Add(user);
                }

                if (selUser.Count <= 0)
                {
                    var msgboxDlg = new MessageDialog("선택된 카카오톡(밴드)명이 없습니다.");
                    msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg.DefaultCommandIndex = 0;
                    await msgboxDlg.ShowAsync();

                }
                else
                {
                    userDeleteCommand(selUser);
                }

            }
        }
        private async Task AddUserTask()
        {
            
            TextBox input = new TextBox()
            {
                Height = (double)App.Current.Resources["TextControlThemeMinHeight"],
                PlaceholderText = "카카오톡(밴드)명을 입력하세요.t"
            };
            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "카카오톡(밴드)명을 등록해 주세요",
                PrimaryButtonText = "등록",
                SecondaryButtonText = "취소",
                Content = input
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                input = (TextBox)Cdialog.Content;
                CapTone.Repository.userlist.AddData(input.Text);

                inUser.Clear();
                inUser = new ObservableCollection<string>(userlist.GetAllData());

                //this.inUser.Add(input.Text);
                await new Windows.UI.Popups.MessageDialog(input.Text + "이(가) 등록되었습니다.").ShowAsync();
            }
        }
        private async void OnAddUsers()
        {
            TextBox input = new TextBox()
            {
                Height = (double)App.Current.Resources["TextControlThemeMinHeight"],
                PlaceholderText = "카카오톡(밴드)명을 입력하세요."
            };
            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "카카오톡(밴드)명을 등록해야합니다.",
                PrimaryButtonText = "등록",
                SecondaryButtonText = "취소",
                Content = input
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                input = (TextBox)Cdialog.Content;
                CapTone.Repository.userlist.AddData(input.Text);

                inUser.Clear();

                inUser = new ObservableCollection<string>(userlist.GetAllData());

                await new Windows.UI.Popups.MessageDialog(input.Text + "이(가) 등록되었습니다.").ShowAsync();
            }
        }

        private void readonlybool()
        {

            namebool = true;
            phonebool = true;
            addrbool = true;
            itembool = true;
        }

        private void OnEditCommand()
        {
            this.namebool = false;
            this.phonebool = false;
            this.addrbool = false;
            this.itembool = false;
        }

        private async void OnDoNLP()
        {

            readonlybool();
            while (inUser.Count <= 0)
            {
                await AddUserTask();
            }
            while (inItem.Count <= 0)
            {
                await AddItemTask();
            }

            if(count == 1)
            {
                waittext wt = new waittext();
                wt.Show();
                await request_dataAsync();
                progressText = next_count + "/" + count + " file";


                wt.Close();
            }
            else if(count > 1)
            {
                ContentDialog Cdialog = new ContentDialog()
                {
                    Title = "한번에 실행하시겠습니까?",
                    Content = "(\"예\"를 선택하시면 자동으로 데이터 베이스에 저장됩니다. 오류가 있으면 해당 페이지를 수정하실 수 있습니다.)\n(\"아니오\"를 선택하시면 하나씩 확인하면서 등록하실 수 있습니다.)",
                    CloseButtonText = "취소",
                    PrimaryButtonText = "예",
                    SecondaryButtonText = "아니오"
                };
                ContentDialogResult result = await Cdialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    mode = 2;
                    if (nlp_error == 2)
                        nlp_error = 0;
                    mode_2_func();

                }//자동 등록
                else if(result == ContentDialogResult.Secondary)
                {
                    mode = 3;
                    mode_3_func();

                }// 1개씩 등록
            }
        }
        /*----------------------------------*/
        private async void mode_2_func()
        {
            waittext wt = new waittext();
            wt.Show();
            for (int i = next_count; i < count; i++)
            {
                dialog = all_dialog[i];

                await request_dataAsync();

                progressText = next_count + "/" + count + " files";


                if (nlp_error == 1 || nlp_error == 2)
                {
                    for (int j = 0; j < OutputData.Count; j++)
                    {
                        outputData odb = OutputData[j];
                        CapTone.Repository.outputs.AddData(odb);
                    }
                    wt.Close();
                    return;
                    
                }

            }
            for (int j = 0; j < OutputData.Count; j++)
            {
                outputData odb = OutputData[j];
                CapTone.Repository.outputs.AddData(odb);
            }
            OutputData.Clear();
            var enddlog = new MessageDialog("총 " + count + "개의 파일을 등록하였습니다.");
            enddlog.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
            enddlog.DefaultCommandIndex = 0;
            await enddlog.ShowAsync();

            
            wt.Close();
        }
        private async void mode_3_func()
        {
            waittext wt = new waittext();
            dialog = all_dialog[next_count];
            wt.Show();
            await request_dataAsync();

            progressText = next_count + "/" + count + " files";


            wt.Close();
        }
        /*----------------------------------*/


        private async Task AddItemTask()
        {
            TextBox input = new TextBox()
            {
                Height = (double)App.Current.Resources["TextControlThemeMinHeight"],
                PlaceholderText = "품목을 입력하세요."
            };
            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "최소 한 개의 품목을 등록해야합니다.",
                PrimaryButtonText = "등록",
                SecondaryButtonText = "취소",
                Content = input
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                input = (TextBox)Cdialog.Content;
                CapTone.Repository.itemlist.AddData(input.Text);
                inItem.Clear();
                inItem = new ObservableCollection<string>(itemlist.GetAllData());
                await new Windows.UI.Popups.MessageDialog(input.Text + "이(가) 등록되었습니다.").ShowAsync();

            }
        }

        private async void OnCheckItems()
        {
            
            ListView itemListView = new ListView()
            {
                ItemsSource = inItem
                
            };
            itemListView.IsItemClickEnabled = true;
            itemListView.SelectionMode = ListViewSelectionMode.Multiple;
            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "등록하신 품목 리스트 입니다",
                CloseButtonText = "확인",
                PrimaryButtonText = "전체 삭제",
                SecondaryButtonText = "선택 삭제",
                
                Content = itemListView
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                ContentDialog DelAll = new ContentDialog()
                {
                    Title = "모든 품목을 삭제하시겠습니까?",

                    PrimaryButtonText = "확인",
                    SecondaryButtonText = "취소",

                };
                ContentDialogResult delRes = await DelAll.ShowAsync();
                if (delRes == ContentDialogResult.Primary)
                {
                    itemlist.DeleteAllData();
                    inItem.Clear();

                    var msgboxDlg1 = new MessageDialog("모든 품목이 삭제되었습니다.");
                    msgboxDlg1.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg1.DefaultCommandIndex = 0;
                    await msgboxDlg1.ShowAsync();
                }



            }
            else if(result == ContentDialogResult.Secondary)
            {



                List<string> selItem = new List<string>();
                foreach (string item in itemListView.SelectedItems)
                {
                    selItem.Add(item);
                }

                if (selItem.Count <= 0)
                {
                    var msgboxDlg1 = new MessageDialog("선택된 품목이 없습니다.");
                    msgboxDlg1.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg1.DefaultCommandIndex = 0;
                    await msgboxDlg1.ShowAsync();

                }
                else
                {
                    ItemDeleteCommand(selItem);
                }
                
            }

        }
        private async void ItemDeleteCommand(List<string> selItem)
        {
            string delItem = string.Empty;
            for(int i = 0; i < selItem.Count; i++)
            {
                delItem += selItem[i];
                if(i != selItem.Count - 1)
                {
                    delItem += ", ";
                }

            }
            ContentDialog DelOne = new ContentDialog()
            {
                Title = delItem + "을(를) 삭제하시겠습니까?",

                PrimaryButtonText = "확인",
                SecondaryButtonText = "취소",

            };
            ContentDialogResult delRes = await DelOne.ShowAsync();
            if(delRes == ContentDialogResult.Primary)
            {
                for(int i = 0;i<selItem.Count; i++)
                {
                    itemlist.DeleteOneItem(selItem[i]);
                    inItem.Remove(selItem[i]);
                }


                var msgboxDlg = new MessageDialog(delItem+"이(가) 삭제되었습니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
            }



        }

        private async void userDeleteCommand(List<string> selUser)
        {
            string delUser = string.Empty;
            for (int i = 0; i < selUser.Count; i++)
            {
                delUser += selUser[i];
                if (i != selUser.Count - 1)
                {
                    delUser += ", ";
                }

            }
            ContentDialog DelOne = new ContentDialog()
            {
                Title = delUser + "을(를) 삭제하시겠습니까?",

                PrimaryButtonText = "확인",
                SecondaryButtonText = "취소",

            };
            ContentDialogResult delRes = await DelOne.ShowAsync();
            if (delRes == ContentDialogResult.Primary)
            {
                for (int i = 0; i < selUser.Count; i++)
                {
                    userlist.DeleteOneItem(selUser[i]);
                    inUser.Remove(selUser[i]);
                }


                var msgboxDlg = new MessageDialog(delUser + "이(가) 삭제되었습니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
            }



        }

        private async void OnAddItems()
        {
            TextBox input = new TextBox()
            {
                Height = (double)App.Current.Resources["TextControlThemeMinHeight"],
                PlaceholderText = "품목을 입력하세요."
            };
            ContentDialog Cdialog = new ContentDialog()
            {
                Title = "품목을 등록해 주세요",
                PrimaryButtonText = "등록",
                SecondaryButtonText = "취소",
                Content = input
            };
            ContentDialogResult result = await Cdialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                input = (TextBox)Cdialog.Content;
                CapTone.Repository.itemlist.AddData(input.Text);

                inItem.Clear();
                inItem = new ObservableCollection<string>(itemlist.GetAllData());

                await new Windows.UI.Popups.MessageDialog(input.Text + "이(가) 등록되었습니다.").ShowAsync();
            }
        }


        private async void OnImportCommand()
        {
            if(next_count < count)
            {
                ContentDialog Cdialog = new ContentDialog()
                {
                    Title = "아직 실행중인 파일이 있습니다.\n취소하고 새로운 파일을 여시겠습니까?",
                    PrimaryButtonText = "확인",
                    SecondaryButtonText = "취소"
                };
                ContentDialogResult result = await Cdialog.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    return;
                }
            }
            fileName = "";

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".txt");


            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 1)
            {
                next_count = 0;

                count = files.Count;
                int chk_num = 0;
                StringBuilder output = new StringBuilder("파일 : ");
                all_dialog.Clear();
                // Application now has read/write access to the picked file(s)
                foreach (Windows.Storage.StorageFile file in files)
                {
                    
                    IBuffer buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
                    DataReader dataReader = DataReader.FromBuffer(buffer);
                    all_dialog.Add(dataReader.ReadString(dataReader.UnconsumedBufferLength));
                    chk_num++;
                }
                fileName = "총 " + files.Count + "개의 파일을 입력받았습니다...";

                this.dialog = "";
                progressText = next_count + "/" + count + " files";

            }
            else if(files.Count == 1)
            {
                mode = 1;
                next_count = 0;
                count = 1;
                var file = files[0];
                StringBuilder output = new StringBuilder("파일 : ");
                output.Append(file.Name);
                fileName = output.ToString();

                IBuffer buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);

                DataReader dataReader = DataReader.FromBuffer(buffer);

                this.dialog = dataReader.ReadString(dataReader.UnconsumedBufferLength);

                count = 1;
                progressText = next_count + "/" + count + " file";


            }
            else
            {
                fileName = "파일을 불러오지 못했습니다.";
                dialog = "";
            }

            readonlybool();
        }

        private async Task request_dataAsync()
        {
            
            StorageFolder storageFolder1 = Windows.ApplicationModel.Package.Current.InstalledLocation;

            //string stop_text = @"CAS\stopwords.txt";
            string city_text = @"CAS\city.txt";
            string firstname_text = @"CAS\firstname.txt";
            string lastname_text = @"CAS\lastname.txt";
            string capital_text = @"CAS\capital.txt";
            string town_text = @"CAS\town.txt";
            string village_text = @"CAS\village.txt";
            string addressdic_text = @"CAS\addressdic.txt";
            string cardinal_text= @"CAS\cardinal.txt";
            string road_text = @"CAS\road.txt";
            string unit_text = @"CAS\unit.txt";
            string kill_text = @"CAS\kill.txt";
            //StorageFile stopit = await storageFolder1.GetFileAsync(stop_text);
            StorageFile cityit = await storageFolder1.GetFileAsync(city_text);
            StorageFile firstnameit = await storageFolder1.GetFileAsync(firstname_text);
            StorageFile lastnameit = await storageFolder1.GetFileAsync(lastname_text);
            StorageFile capit = await storageFolder1.GetFileAsync(capital_text);
            StorageFile townit = await storageFolder1.GetFileAsync(town_text);
            StorageFile villit = await storageFolder1.GetFileAsync(village_text);
            StorageFile addreit = await storageFolder1.GetFileAsync(addressdic_text);
            StorageFile cardianlit = await storageFolder1.GetFileAsync(cardinal_text);
            StorageFile roadit = await storageFolder1.GetFileAsync(road_text);
            StorageFile unitit = await storageFolder1.GetFileAsync(unit_text);

            StorageFile killit = await storageFolder1.GetFileAsync(kill_text);
            //string stopTXT;
            string cityTXT;
            string firstnameTXT;
            string lastnameTXT;
            string capTXT;
            string townTXT;
            string villTXT;
            string addreTXT;
            string cardinalTXT;
            string roadTXT;
            string unitTXT;
            string killTXT;
            //IBuffer buffer1 = await Windows.Storage.FileIO.ReadBufferAsync(stopit);
            IBuffer buffer2 = await Windows.Storage.FileIO.ReadBufferAsync(cityit);
            IBuffer buffer3 = await Windows.Storage.FileIO.ReadBufferAsync(firstnameit);
            IBuffer buffer31 = await Windows.Storage.FileIO.ReadBufferAsync(lastnameit);
            IBuffer buffer4 = await Windows.Storage.FileIO.ReadBufferAsync(capit);
            IBuffer buffer5 = await Windows.Storage.FileIO.ReadBufferAsync(townit);
            IBuffer buffer6 = await Windows.Storage.FileIO.ReadBufferAsync(villit);
            IBuffer buffer7 = await Windows.Storage.FileIO.ReadBufferAsync(addreit);
            IBuffer buffer8 = await Windows.Storage.FileIO.ReadBufferAsync(cardianlit);
            IBuffer buffer9 = await Windows.Storage.FileIO.ReadBufferAsync(roadit);
            IBuffer buffer0 = await Windows.Storage.FileIO.ReadBufferAsync(unitit);
            IBuffer buffer10 = await Windows.Storage.FileIO.ReadBufferAsync(killit);

            //DataReader dataReader1 = DataReader.FromBuffer(buffer1);
            DataReader dataReader2 = DataReader.FromBuffer(buffer2);
            DataReader dataReader3 = DataReader.FromBuffer(buffer3);
            DataReader dataReader31 = DataReader.FromBuffer(buffer31);
            DataReader dataReader4 = DataReader.FromBuffer(buffer4);
            DataReader dataReader5 = DataReader.FromBuffer(buffer5);
            DataReader dataReader6 = DataReader.FromBuffer(buffer6);
            DataReader dataReader7 = DataReader.FromBuffer(buffer7);
            DataReader dataReader8 = DataReader.FromBuffer(buffer8);
            DataReader dataReader9 = DataReader.FromBuffer(buffer9);
            DataReader dataReader0 = DataReader.FromBuffer(buffer0);
            DataReader dataReader10 = DataReader.FromBuffer(buffer10);
            //stopTXT = dataReader1.ReadString(dataReader1.UnconsumedBufferLength);
            cityTXT = dataReader2.ReadString(dataReader2.UnconsumedBufferLength);
            firstnameTXT = dataReader3.ReadString(dataReader3.UnconsumedBufferLength);
            lastnameTXT = dataReader31.ReadString(dataReader31.UnconsumedBufferLength);
            capTXT = dataReader4.ReadString(dataReader4.UnconsumedBufferLength);
            townTXT = dataReader5.ReadString(dataReader5.UnconsumedBufferLength);
            villTXT = dataReader6.ReadString(dataReader6.UnconsumedBufferLength);
            addreTXT = dataReader7.ReadString(dataReader7.UnconsumedBufferLength);
            cardinalTXT = dataReader8.ReadString(dataReader8.UnconsumedBufferLength);
            roadTXT = dataReader9.ReadString(dataReader9.UnconsumedBufferLength);
            unitTXT = dataReader0.ReadString(dataReader0.UnconsumedBufferLength);
            killTXT = dataReader10.ReadString(dataReader10.UnconsumedBufferLength);


            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5000/phone");
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    text = dialog,
                    //stop = stopTXT,
                    dist = cityTXT,
                    firstname = firstnameTXT,
                    lastname = lastnameTXT,
                    capital = capTXT,
                    town = townTXT,
                    vill = villTXT,
                    dic = addreTXT,
                    cardinal = cardinalTXT,
                    road = roadTXT,
                    unit = unitTXT,
                    thisisitem = inItem,
                    kill = killTXT,
                    seller = inUser
                }) ;

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            
            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                JObject jobj = JObject.Parse(result);
                inText.Clear();
                inTextName = "";
                inTextPhone = "";
                inTextAdd = "";
                inTextItem = "";


                if (jobj["seller"].ToString() == "seller_error")
                {
                    nlp_error = 2;
                    var msgboxDlg = new MessageDialog("등록하신 사용자 명이 올바르지 않습니다.\n재 등록 후 다시 실행해 주시기 바랍니다.");
                    msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg.DefaultCommandIndex = 0;
                    await msgboxDlg.ShowAsync();

                    await AddUserTask();
                }
                else if(jobj["seller"].ToString() == "file_error")
                {
                    nlp_error = 2;
                    var msgboxDlg = new MessageDialog("올바른 파일이 아닙니다. 확인 후 실행해 주시기 바랍니다.");
                    msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg.DefaultCommandIndex = 0;
                    await msgboxDlg.ShowAsync();

                }
                else if (jobj["itemlist"].ToString() == "itemlist_error")
                {
                    nlp_error = 2;

                    var msgboxDlg = new MessageDialog("등록하신 품목명이 올바르지 않습니다.\n재 등록 후 다시 실행해 주시기 바랍니다.");
                    msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg.DefaultCommandIndex = 0;
                    await msgboxDlg.ShowAsync();

                    await AddItemTask();
                }

                else if (jobj["number"].ToString() == "phone_error" || jobj["name"].ToString() == "name_error"|| jobj["address"].ToString() == "address_error"|| jobj["items"].ToString() == "item_error")
                {
                    nlp_error = 1;
                    inTextName = jobj["name"].ToString();
                    inTextPhone = jobj["number"].ToString();
                    inTextAdd = jobj["address"].ToString();
                    inTextItem = jobj["items"].ToString();
                    var msgboxDlg = new MessageDialog("추출하지 못한 정보가 있습니다.");
                    msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    msgboxDlg.DefaultCommandIndex = 0;
                    await msgboxDlg.ShowAsync();

                    if (jobj["number"].ToString() == "phone_error")
                    {

                        inTextPhone = "";

                        this.phonebool = false;
                    }

                    if (jobj["name"].ToString() == "name_error")
                    {
                        inTextName = "";

                        this.namebool = false;
                    }
                    if (jobj["address"].ToString() == "address_error")
                    {

                        inTextAdd = "";
                        this.addrbool = false;
                    }

                    if (jobj["items"].ToString() == "item_error")
                    {

                        inTextItem = "";
                        this.itembool = false;
                    }

                }
                else
                {
                    nlp_error = 0;
                    inTextName = jobj["name"].ToString();
                    inTextPhone = jobj["number"].ToString();
                    inTextAdd = jobj["address"].ToString();
                    inTextItem = jobj["items"].ToString();
                    if(mode == 2)
                    {
                        OutputData.Add(new outputData() { Name = inTextName, PhoneNumber = inTextPhone, HomeAddress = inTextAdd, Items = inTextItem });
                    }
                }
                if(nlp_error != 2)
                    next_count++;

            }

        }

        private async void OnOKCommand()
        {



            if(mode == 0)
            {
                var msgboxDlg = new MessageDialog("실행 버튼 클릭 후 눌러주시기 바랍니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
                return;
            }
            if(Views.정제화Page.newname == "" || Views.정제화Page.newphone == "" || Views.정제화Page.newaddr == "" || Views.정제화Page.newitem == "")
            {
                var msgboxDlg = new MessageDialog("비어있는 칸이 있습니다. 입력 후 등록해 주시기 바랍니다.");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
                return;
            }
            OutputData.Clear();
            
            OutputData.Add(new outputData() { Name = Views.정제화Page.newname, PhoneNumber = Views.정제화Page.newphone, HomeAddress = Views.정제화Page.newaddr, Items = Views.정제화Page.newitem});
            for (int i = 0; i < OutputData.Count; i++)
            {
                outputData odb = OutputData[i];
                CapTone.Repository.outputs.AddData(odb);
            }

            readonlybool();
            if(mode == 2)
            {
                var msgboxDlg = new MessageDialog("DB에 저장되었습니다!");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
            }
            else
            {
                var msgboxDlg = new MessageDialog("중복된 결과를 제외하고 DB에 저장되었습니다!");
                msgboxDlg.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                msgboxDlg.DefaultCommandIndex = 0;
                await msgboxDlg.ShowAsync();
            }


            nlp_error = 0;
            if(mode == 3 || mode == 2)
            {
                
                if (next_count < count )
                {
                    if(mode == 3)
                        mode_3_func();
                    else if(mode == 2)
                        mode_2_func();

                }
                else
                {

                    var enddlog = new MessageDialog("총 " + count + "개의 파일을 등록하였습니다.");
                    enddlog.Commands.Add(new Windows.UI.Popups.UICommand("확인") { Id = 0 });
                    enddlog.DefaultCommandIndex = 0;
                    await enddlog.ShowAsync();
                }
            }



        }

        public ICommand ImportCommand { get; set; } //import 버튼 클릭 시
        public ICommand OKCommand { get; set; } // ok 버튼 클릭 시
        public ICommand AddItems { get; set; }
        public ICommand CheckItems { get; private set; }
        public ICommand DoNLP { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AddUsers { get; private set; }
        public ICommand CheckUsers { get; private set; }

        /*------------------------------------------------------------------------*/
        public string fileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);

        }
        // 파일명
        /*------------------------------------------------------------------------*/
        public string filePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);

        }
        // 파일 경로
        /*------------------------------------------------------------------------*/
        public string dialog
        {
            get => _dialog;
            set => SetProperty(ref _dialog, value);
        }

        //카카오톡 전문
        public IList<string> inItem
        {
            get => _inItem;
            set => SetProperty(ref _inItem, value);
        }

        public IList<string> inUser
        {
            get => _inUser;
            set => SetProperty(ref _inUser, value);
        }

        public string inTextName
        {
            get => _inTextName;
            set => SetProperty(ref _inTextName, value);
        }
        public string inTextPhone
        {
            get => _inTextPhone;
            set => SetProperty(ref _inTextPhone, value);
        }
        public string inTextAdd
        {
            get => _inTextAdd;
            set => SetProperty(ref _inTextAdd, value);
        }
        public string inTextItem
        {
            get => _inTextItem;
            set => SetProperty(ref _inTextItem, value);
        }
        public bool namebool
        {
            get => _namebool;
            set => SetProperty(ref _namebool, value);
        }
        public bool phonebool
        {
            get => _phonebool;
            set => SetProperty(ref _phonebool, value);
        }
        public bool addrbool
        {
            get => _addrbool;
            set => SetProperty(ref _addrbool, value);
        }
        public bool itembool
        {
            get => _itembool;
            set => SetProperty(ref _itembool, value);
        }
        public int count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }
        public int next_count
        {
            get => _nextcount;
            set => SetProperty(ref _nextcount, value);
        }
        public string progressText
        {
            get => _progresstext;
            set => SetProperty(ref _progresstext, value);
        }
        public  List<string> all_dialog
        {
            get => _alldialog;
            set => SetProperty(ref _alldialog, value);
        }
        public List<string> all_filename
        {
            get => _allfile;
            set => SetProperty(ref _allfile, value);
        }
        /*------------------------------------------------------------------------*/
        public List<outputData> OutputData
        {
            get => _outputdata;
            set => SetProperty(ref _outputdata, value);
        }
        public ObservableCollection<outputData> inText
        {
            get => _inText;
            set => SetProperty(ref _inText, value);
        }
        public IList<outputData> OutDatas
        {
            get => _outdatas;
            set => SetProperty(ref _outdatas, value);
        }

        // 정제된 결과 값 (이름, 전화번호, 주소, 품목)
    }
}
