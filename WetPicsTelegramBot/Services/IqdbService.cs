using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IqdbApi;
using IqdbApi.Enums;
using IqdbApi.Models;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class IqdbService : IIqdbService
    {
        private readonly ILogger<IqdbService> _logger;
        private readonly IIqdbClient _iqdbClient;
        private readonly IMessagesService _messagesService;

        public IqdbService(ILogger<IqdbService> logger,
                            IIqdbClient iqdbClient,
                            IMessagesService messagesService)
        {
            _logger = logger;
            _iqdbClient = iqdbClient;
            _messagesService = messagesService;
        }

        public async Task<string> SearchImage(Stream stream)
        {
            var searchResults = await _iqdbClient.SearchFile(stream);

            var replyMessage = BuildSearchResultMessage(searchResults);

            return replyMessage;
        }

        private string BuildSearchResultMessage(SearchResult searchResults)
        {
            var sb = new StringBuilder();

            if (!searchResults.IsFound)
            {
                sb.AppendLine(_messagesService.IqdbNotFound);
                return sb.ToString();
            }

            searchResults
                .Matches
                .Where(x => x.MatchType == MatchType.Best || x.MatchType == MatchType.Additional)
                .OrderBy(RankMatch)
                .ThenByDescending(x => x.Similarity)
                .ToList()
                .ForEach(x => sb.AppendLine($"({x.Similarity} %): {x.Url.Trim(new[] { '/', '\\' })}"));

            return sb.ToString();
        }

        private int RankMatch(Match match)
        {
            switch (match.Source)
            {
                case Source.Yandere:
                    return 0;
                case Source.Danbooru:
                    return 1;
                case Source.SankakuChannel:
                    return 2;
                case Source.Konachan:
                case Source.Gelbooru:
                    return 3;
                default:
                    return 4;
            }
        }
    }
}
