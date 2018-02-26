using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using IqdbApi;
using IqdbApi.Enums;
using IqdbApi.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using WetPicsTelegramBot.Services.Abstract;
using SearchResult = IqdbApi.Models.SearchResult;

namespace WetPicsTelegramBot.Services
{
    class IqdbService : IIqdbService
    {
        private readonly ILogger<IqdbService> _logger;
        private readonly IIqdbClient _iqdbClient;
        private readonly IMessagesService _messagesService;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly DanbooruLoader _danbooruLoader;
        private readonly SankakuLoader _sankakuLoader;
        private readonly YandereLoader _yandereLoader;

        public IqdbService(ILogger<IqdbService> logger,
                           IIqdbClient iqdbClient,
                           IMessagesService messagesService,
                           ITelegramBotClient telegramBotClient,
                           DanbooruLoader danbooruLoader,
                           SankakuLoader sankakuLoader,
                           YandereLoader yandereLoader)
        {
            _logger = logger;
            _iqdbClient = iqdbClient;
            _messagesService = messagesService;
            _telegramBotClient = telegramBotClient;
            _danbooruLoader = danbooruLoader;
            _sankakuLoader = sankakuLoader;
            _yandereLoader = yandereLoader;
        }

        public async Task<string> SearchImage(string fileId)
        {
            _logger.LogTrace($"Searching file {fileId}");

            var searchResults = await SearchIqdb(fileId);

            var replyMessage = BuildSearchResultMessage(searchResults);

            return replyMessage;
        }

        public async Task<string> SearchTags(string fileId)
        {
            _logger.LogTrace($"Searching file {fileId} for tags");

            var searchResults = await SearchIqdb(fileId);

            return await BuildSearchTagsMessage(searchResults);
        }

        private async Task<SearchResult> SearchIqdb(string fileId)
        {
            using (var ms = new MemoryStream())
            {
                await _telegramBotClient.GetInfoAndDownloadFileAsync(fileId, ms);

                var results = await _iqdbClient.SearchFile(ms);

                return results;
            }
        }

        private async Task<string> BuildSearchTagsMessage(SearchResult searchResults)
        {

            if (!searchResults.IsFound)
            {
                return _messagesService.IqdbNotFound;
            }

            var result = searchResults
                .Matches
                .Where(x => x.MatchType == MatchType.Best || x.MatchType == MatchType.Additional)
                .OrderBy(RankMatch)
                .ThenByDescending(x => x.Similarity)
                .ToList();

            var tagString = await LoadAndReturn(_danbooruLoader, Source.Danbooru, result)
                            ?? await LoadAndReturn(_sankakuLoader, Source.SankakuChannel, result)
                            ?? await LoadAndReturn(_yandereLoader, Source.Yandere, result)
                            ?? _messagesService.IqdbNotFound;

            return tagString;
        }

        private async Task<string> LoadAndReturn(IBooruAsyncLoader loader, Source source, List<Match> matches)
        {
            var sourceMatches = matches.FirstOrDefault(x => x.Source == source);
            if (sourceMatches != null)
            {
                var id = sourceMatches.Url.Split(new[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries).Last();

                var post = await loader.LoadPostAsync(Int32.Parse(id));
                return BuildTagMessage(post);
            }
            return null;
        }

        private static string BuildTagMessage(Post post)
        {
            var sb = new StringBuilder();

            AppendTags(post, TagType.Artist, sb);
            AppendTags(post, TagType.Copyright, sb);
            AppendTags(post, TagType.Character, sb);
            
            sb.AppendLine($"<b>Rating</b>: {post.ImageRating}");

            var otherTags = post
                .Tags
                .Where(x => x.Type != TagType.Artist
                        && x.Type != TagType.Copyright
                        && x.Type != TagType.Character)
                .ToList();

            if (otherTags.Any())
            {
                sb.AppendLine();

                foreach (var tag in otherTags)
                {
                    sb.Append($"{tag.Name.Replace(" ", "_")}");
                    sb.Append(" ");
                }
            }

            return sb.ToString().Trim();
        }

        private static void AppendTags(Post post, TagType type, StringBuilder sb)
        {
            var tags = post.Tags.Where(x => x.Type == type).ToList();
            if (tags.Any())
            {
                foreach (var tag in tags)
                {
                    sb.Append($"<b>{type.ToString()}:</b> {tag.Name.Replace(" ", "_")}");
                    if (!String.IsNullOrWhiteSpace(tag.JapName))
                    {
                        sb.Append($" | {tag.JapName.Replace(" ", "_")}");
                    }
                    sb.AppendLine();
                }
            }
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
