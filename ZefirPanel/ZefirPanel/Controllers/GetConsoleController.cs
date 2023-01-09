using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using ZefirPanel.Core;

namespace ZefirPanel.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetConsoleController : ControllerBase
    {
        private int _maxLines = 20;

        [HttpGet]
        public async Task<string> PostAsync()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                SSHHandler shandler = new SSHHandler();
                Config conf = ConfigLoader.Load();
                shandler.Open(conf.Host, conf.Username, conf.Password);
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    while (!shandler.IsChannelOpen)
                    { 
                        Thread.Sleep(1);
                    }

                    while (!HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        string data = shandler.GetOutput();

                        byte[] bytes = Encoding.UTF8.GetBytes(data);
                        await webSocket.SendAsync(new System.ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text, true, CancellationToken.None);

                        Thread.Sleep(200);
                    }
                }
                //shandler.Close();
            }
            return "";
        }
    }
}
