using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNet.SignalR.Hubs;
using Autofac;
using System.Collections.Generic;
using System.Linq;

namespace SignalRChat
{
    /// <summary>
    /// WinForms host for a SignalR server. The host can stop and start the SignalR
    /// server, report errors when trying to start the server on a URI where a
    /// server is already being hosted, and monitor when clients connect and disconnect. 
    /// The hub used in this server is a simple echo service, and has the same 
    /// functionality as the other hubs in the SignalR Getting Started tutorials.
    /// </summary>
    public partial class WinFormsServer : Form
    {
        private IDisposable SignalR { get; set; }

        const string ServerURI = "http://A10876:8080";

        internal WinFormsServer()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Calls the StartServer method with Task.Run to not
        /// block the UI thread. 
        /// </summary>
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            WriteToConsole("Starting server...");
            ButtonStart.Enabled = false;
            Task.Run(() => StartServer());
        }

        /// <summary>
        /// Stops the server and closes the form. Restart functionality omitted
        /// for clarity.
        /// </summary>
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            //SignalR will be disposed in the FormClosing event
            Close();
        }

        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException)
            {
                WriteToConsole("Server failed to start. A server is already running on " + ServerURI);
                //Re-enable button to let user try to start server again
                this.Invoke((Action)(() => ButtonStart.Enabled = true));
                return;
            }
            this.Invoke((Action)(() => ButtonStop.Enabled = true));
            WriteToConsole("Server started at " + ServerURI);
        }
        /// <summary>
        /// This method adds a line to the RichTextBoxConsole control, using Invoke if used
        /// from a SignalR hub thread rather than the UI thread.
        /// </summary>
        /// <param name="message"></param>
        internal void WriteToConsole(String message)
        {
            if (RichTextBoxConsole.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                    WriteToConsole(message)
                ));
                return;
            }
            RichTextBoxConsole.AppendText(message + Environment.NewLine);
        }

        private void WinFormsServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (SignalR != null)
            {
                SignalR.Dispose();
            }
        }
    }
    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        private static IContainer Container { get; set; }
        public void Configuration(IAppBuilder app)
        {

            var builder = new ContainerBuilder();

            builder.RegisterType<ConnectionMapping>().AsImplementedInterfaces();

            builder.RegisterType<AuthorizeHubConnectionHandler>().AsImplementedInterfaces();

            Container = builder.Build();

            using (var scope = Container.BeginLifetimeScope())
            {
                var connectionMapping = scope.Resolve<IConnectionMapping>();

                var authorizeHubConnection = scope.Resolve<IAuthorizeHubConnection>();

                var authorizeHubMethodInvocation = scope.Resolve<IAuthorizeHubMethodInvocation>();

                //app.UseCors(CorsOptions.AllowAll);

                // Make long polling connections wait a maximum of 110 seconds for a
                // response. When that time expires, trigger a timeout command and
                // make the client reconnect.
                GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(100);

                // Wait a maximum of 30 seconds after a transport connection is lost
                // before raising the Disconnected event to terminate the SignalR connection.
                //GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);

                // For transports other than long polling, send a keepalive packet every
                // 10 seconds. 
                // This value must be no more than 1/3 of the DisconnectTimeout value.
                GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);

                var module = new AuthorizeModule(authorizeHubConnection, authorizeHubMethodInvocation);
                GlobalHost.HubPipeline.AddModule(module);
                app.MapSignalR();

            }           
        }
    }
    /// <summary>
    /// Echoes messages sent using the Send message by calling the
    /// addMessage method on the client. Also reports to the console
    /// when clients connect and disconnect.
    /// </summary>
    public class MyHub : Hub
    {
        private static ConnectionMapping _connections = new ConnectionMapping();

        public void Send(string name, string message)
        {
            string who = Context.ConnectionId;
            string connection = string.Empty;

            connection = _connections.SearchAdmin(who);

            if(connection == string.Empty)
                connection = _connections.SearchClient(who);

            if (connection != string.Empty)
            {
                Clients.Client(connection).addMessage(name, message);
            }
            else
            {
                Clients.Client(who).addMessage("Remove Server", "All HelpDesk are busy, please try again");
            }
        }
        public override Task OnConnected()
        { 
            List<string> adminlist = new List<string>();
            adminlist.Add("Ken");
            adminlist.Add("Kaden");

            Program.MainForm.WriteToConsole("Client connected: " + Context.ConnectionId);
            string connectionId = Context.ConnectionId;
            string user = Context.Headers["UserName"];

            if(adminlist.Where(x => x == user).Count() > 0)
            {
                _connections.AddAdmin(connectionId);
            }
            else
            {
                _connections.AddClient(connectionId);
            }
            
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            Program.MainForm.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            Program.MainForm.WriteToConsole("Client Reconnected: " + Context.ConnectionId);
            string connectionId = Context.ConnectionId;
            return base.OnReconnected();
        }

    }
}
