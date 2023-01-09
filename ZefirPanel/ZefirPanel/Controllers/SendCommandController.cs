using Microsoft.AspNetCore.Mvc;
using ZefirPanel.Core;

namespace ZefirPanel.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendCommandController : ControllerBase 
    {
        [HttpPost]
        public void Post([FromForm]string command)
        {
            HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            SSHHandler shandler = new SSHHandler();
            Config conf = ConfigLoader.Load();
            shandler.Open(conf.Host, conf.Username, conf.Password);
            shandler.SendCommand(command);
        }
    }
}
