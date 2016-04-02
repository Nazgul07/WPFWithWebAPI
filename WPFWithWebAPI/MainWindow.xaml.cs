using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFWithWebAPI
{
    public delegate void MessageReceivedEventHandler(string message);

    public static class EventDispatcher
    {
        public static MessageReceivedEventHandler OnMessage;
    }

    public class MessageController : ApiController
    {
        /// <summary>
        /// Dispatches the OnMessage event when a message is received.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpGet]
        public string RecieveViaGet(string message)
        {
            EventDispatcher.OnMessage?.Invoke(message);
            return message;
        }

        [HttpPost]
        public void RecieveViaPost(string message)
        {
            EventDispatcher.OnMessage?.Invoke(message);
        }
    }

    class Program
    {
        static readonly Uri _baseAddress = new Uri("http://localhost:50231/");

        [STAThread]
        static void Main(string[] args)
        {
            HttpSelfHostServer server = null;
            try
            {
                // Set up server configuration
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(_baseAddress);

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{message}",
                    defaults: new { message = RouteParameter.Optional }
                );

                // Create server
                server = new HttpSelfHostServer(config);

                // Start listening
                server.OpenAsync().Wait();

                new App().Run(new MainWindow());

            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not start server: {0}", e.GetBaseException().Message);
            }
            finally
            {
                if (server != null)
                {
                    // Stop listening
                    server.CloseAsync().Wait();
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EventDispatcher.OnMessage += OnMessageRecieved;
        }

        private void OnMessageRecieved(string message)
        {
            //update the UI
            Dispatcher.Invoke(() =>
            {
                Message.Content = message;
            });
        }
    }
}
