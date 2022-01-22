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
        public string accessToken = "b2228cf06accc6c34160682dcd46e6232378bb04e996c85a5af76c14585a24b888266908af45d10de1fa8";
        public string vkUserId = "382177113";

        public MainWindow()
        {
            InitializeComponent();

            debugger = new SpeechSynthesizer();

        }

        private void SendPostHandler(object sender, RoutedEventArgs e)
        {
            string myMessage = request.Text;
            string uriPath = "https://api.vk.com/method/wall.post?owner_id=" + vkUserId + "&message=" + myMessage + "&access_token=" + accessToken + "&v=5.92";
            var webRequest = HttpWebRequest.Create(uriPath);
            webRequest.Method = "GET";
            try
            {
                using (var webResponse = webRequest.GetResponse())
                {
                    // debugger.Speak(webResponse.ContentLength.ToString());
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

    public class ResponseInfo
    {
        public int post_id;
    }

}
