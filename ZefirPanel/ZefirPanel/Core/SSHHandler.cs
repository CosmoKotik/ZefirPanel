using Renci.SshNet;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ZefirPanel.Core
{
    public class SSHHandler
    {
        public ShellStream Shell { get; set; } = null;
        public SshClient Client { get; set; } = null;

        private static List<ShellStream> _openShells = new List<ShellStream>();
        private static List<SshClient> _openClients = new List<SshClient>();
        
        private static List<string> _openedIps = new List<string>();
        private static List<string> _clientOutputs = new List<string>();

        public bool IsChannelOpen
        {
            get { return _isChannelOpen; }
        }

        private bool _isChannelOpen = false;

        private string _oldOutput = "";
        private string _output = "";
        private int _openIndex = 0;
        private int _initIndex = 0;

        public void Open(string ip, string username, string password)
        {
            if (!_openedIps.Contains(ip))
            {
                Client = new SshClient(ip, username, password);
                Client.Connect();

                _openIndex = _openClients.Count;
                _openClients.Add(Client);
                _openedIps.Add(ip);
                _clientOutputs.Add("");

                Shell = Client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                Shell.Write("screen -rd reyme\n");
                Shell.Flush();
                _openShells.Add(Shell);

                Shell.DataReceived += Shell_DataReceived;
            }
            else
            {
                _openIndex = _openedIps.FindIndex(x => x.Contains(ip));
                Client = _openClients[_openIndex];
                Shell = _openShells[_openIndex];
                _isChannelOpen = true;
                _initIndex = 5;
                //Shell.DataReceived += Shell_DataReceived;
            }
        }
        private void Shell_DataReceived(object? sender, Renci.SshNet.Common.ShellDataEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);

            if (_initIndex > 5 && data.Length > 7)
            {
                string replaced = data.Replace(">\b\u001b[K \b", "").Replace("\b\u001b[K \b", "");
                if (replaced[0] == '>')
                    replaced.Remove(0, 2);

                //_output += data.Replace("\b\u001b[K \b", "");
                _output += replaced;
            }
            else if (data.Length > 7)
                _output += data.Substring(0, data.Length - 1);

            _clientOutputs[_openIndex] = _output;
            _isChannelOpen = true;
            _initIndex++;
        }

        public void Close()
        {
            _isChannelOpen = false;
            Shell.Close();
            Client.Disconnect();
        }

        public string GetOutput()
        {
            if (_initIndex >= 4)
            {
                string data = _clientOutputs[_openIndex];
                var result = Regex.Split(data, "\r\n|\r|\n").ToList();

                if (result.Contains(">"))
                    result.Remove(">");
                data = "";

                if (result.Count > 16)
                {
                    for (int i = 16; i < result.Count; i++)
                    {
                        string endres = result[i];
                        if (endres.StartsWith('>'))
                            endres = endres.Substring(1, endres.Length - 1);
                        data += endres + "\n";
                    }
                    return _oldOutput = data;
                }
            }
            return _oldOutput;
        }

        public void SendCommand(string command)
        {
            Shell.Write(command + "\n");
            Shell.Flush();
        }
    }
}
