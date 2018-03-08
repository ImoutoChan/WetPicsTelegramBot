using System.Collections.Generic;
using System.IO;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    interface ITopImageDrawService
    {
        Stream DrawTopImage(List<Stream> imageStreams);
    }
}