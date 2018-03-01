using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Services;

namespace WetPicsTelegramBot.WebApp.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly INotificationService _updateService;

        public UpdateController(INotificationService updateService)
        {
            _updateService = updateService;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _updateService.NotifyAsync(update);
            return Ok();
        }
    }
}
