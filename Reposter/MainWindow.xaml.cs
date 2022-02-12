using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.IO;
using System.Web.Script.Serialization;

namespace Reposter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public SpeechSynthesizer debugger;
        public string accessToken = "f6896028bd9c6e1723b5da4699e7e897f77d6ca6f9c72da327ce7b497969951f8da04686de562355c783b";
        public string vkUserId = "662354614";
        public string vkGroupId = "-210273208";
        public List<String> publishedGroups;

        public MainWindow()
        {
            InitializeComponent();

            debugger = new SpeechSynthesizer();
            publishedGroups = new List<String>();
            GetGroupsIds();

        }

        private void SendPostHandler(object sender, RoutedEventArgs e)
        {
            string myMessage = request.Text;
            foreach (string publishedGroupId in publishedGroups)
            {
                string uriPath = "https://api.vk.com/method/wall.post?owner_id=" + publishedGroupId + "&message=" + myMessage + "&access_token=" + accessToken + "&v=5.92";
                var webRequest = HttpWebRequest.Create(uriPath);
                webRequest.Method = "GET";
                try
                {
                    using (var webResponse = webRequest.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            var objText = reader.ReadToEnd();
                            ResponseInfo myobj = (ResponseInfo)js.Deserialize(objText, typeof(ResponseInfo));
                            debugger.Speak(myobj.post_id.ToString());
                        }
                    }
                }
                catch (WebException)
                {
                    debugger.Speak("Ошибка запроса");
                }
            }
        }

        public void GetGroupsIds()
        {
            string uriPath = "https://api.vk.com/method/groups.get?user_id=" + vkUserId + "&access_token=" + accessToken + "&v=5.92";
            var webRequest = HttpWebRequest.Create(uriPath);
            webRequest.Method = "GET";
            try
            {
                using (var webResponse = webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ResponseGroupsIdsInfo myobj = (ResponseGroupsIdsInfo)js.Deserialize(objText, typeof(ResponseGroupsIdsInfo));
                        int[] groupsIds = myobj.response.items;
                        string rawGroupsIds = String.Join(",", groupsIds);
                        GetGroups(rawGroupsIds);
                        debugger.Speak(myobj.response.items.Length.ToString());
                    }
                }
            }
            catch (WebException)
            {
                debugger.Speak("Ошибка запроса");
            }
        }

        public void GetGroups(string rawGroupsIds)
        {
            string uriPath = "https://api.vk.com/method/groups.getById?group_ids=" + rawGroupsIds + "&access_token=" + accessToken + "&v=5.92";
            var webRequest = HttpWebRequest.Create(uriPath);
            webRequest.Method = "GET";
            try
            {
                using (var webResponse = webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ResponseGroupsInfo myobj = (ResponseGroupsInfo)js.Deserialize(objText, typeof(ResponseGroupsInfo));
                        GroupInfo[] myGroups = myobj.response;
                        foreach (GroupInfo group in myGroups)
                        {
                            StackPanel newGroup = new StackPanel();
                            newGroup.Orientation = Orientation.Horizontal;
                            newGroup.Height = 75;
                            Image newGroupImage = new Image();
                            newGroupImage.Margin = new Thickness(15);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            string groupPhotoUrl = group.photo_50;
                            bitmapImage.UriSource = new Uri(groupPhotoUrl, UriKind.Absolute);
                            bitmapImage.EndInit();
                            newGroupImage.Source = bitmapImage;
                            newGroupImage.Width = 50;
                            newGroupImage.Height = 50;
                            newGroup.Children.Add(newGroupImage);
                            CheckBox newGroupCheckbox = new CheckBox();
                            newGroupCheckbox.VerticalAlignment = VerticalAlignment.Center;
                            newGroupCheckbox.Margin = new Thickness(15);
                            string parsedGroupId = "-" + group.id;
                            newGroupCheckbox.DataContext = parsedGroupId;
                            newGroupCheckbox.Click += ToggleGroupHandler;
                            newGroup.Children.Add(newGroupCheckbox);
                            TextBlock newGroupName = new TextBlock();
                            newGroupName.Margin = new Thickness(15);
                            newGroupName.Text = group.name;
                            newGroupName.VerticalAlignment = VerticalAlignment.Center;
                            newGroup.Children.Add(newGroupName);
                            groups.Children.Add(newGroup);
                        }
                        debugger.Speak(myobj.response.Length.ToString());
                    }
                }
            }
            catch (WebException)
            {
                debugger.Speak("Ошибка запроса");
            }
        }

        private void ToggleGroupHandler (object sender, RoutedEventArgs e)
        {
            sendPostBtn.IsEnabled = true;
            CheckBox checkBox = ((CheckBox)(sender));
            bool isGroupSelect = ((bool)(checkBox.IsChecked));
            string currentGroupId = ((string)(checkBox.DataContext));
            if (isGroupSelect)
            {
                debugger.Speak("Добавил группу " + currentGroupId);
                publishedGroups.Add(currentGroupId);
            }
            else
            {
                debugger.Speak("Исключил группу " + currentGroupId);
                int currentGroupIndex = publishedGroups.IndexOf(currentGroupId);
                publishedGroups.RemoveAt(currentGroupIndex);
            }
            bool isGroupsUnselected = publishedGroups.Count <= 0;
            if (isGroupsUnselected)
            {
                sendPostBtn.IsEnabled = false;
            }
        }
    }

    public class ResponseInfo
    {
        public int post_id;
    }

    public class ResponseGroupsIdsInfo
    {
        public GroupsIdsInfo response;
    }

    public class GroupsIdsInfo
    {
        public int count;
        public int[] items;
    }

    public class ResponseGroupsInfo
    {
        public GroupInfo[] response;
    }

    public class GroupInfo
    {
        public int id;
        public string name;
        public string screen_name;
        public int is_closed;
        public string type;
        public int is_admin;
        public int is_member;
        public int is_advertiser;
        public string photo_50;
        public string photo_100;
        public string photo_200;
    }

}
