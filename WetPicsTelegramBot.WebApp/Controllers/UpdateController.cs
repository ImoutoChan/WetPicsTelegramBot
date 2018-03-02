using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly INotificationService _notificationService;

        public UpdateController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _notificationService.NotifyAsync(update);
            return Ok();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ok!");
        }
    }
}
