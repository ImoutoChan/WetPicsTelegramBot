﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    class TgClient : ITgClient
    {
        private const string _meMemoryCacheKey = "_meMemoryCacheKey";
        private readonly IMemoryCache _memoryCache;

        public TgClient(
            ITelegramBotClient telegramBotClient, 
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            Client = telegramBotClient;
        }

        public Task<User> GetMe() 
            => _memoryCache.GetOrCreateAsync(
                _meMemoryCacheKey,
                entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    return Client.GetMeAsync();
                });

        public ITelegramBotClient Client { get; }

        public async Task<string> GetCommand(Message message)
        {
            var me = await GetMe();
            var botUsername = me.Username;

            var text = message?.Text;

            if (String.IsNullOrWhiteSpace(text) || !text.StartsWith("/"))
            {
                return null;
            }

            var firstWord = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
            var isCommandWithId = firstWord.Contains("@") && firstWord.EndsWith(botUsername);
            var command = isCommandWithId ? firstWord.Split('@').First() : firstWord;

            return command;
        }

        public async Task<bool> CheckOnAdmin(string targetChatId, int userId)
        {
            try
            {
                var admins = await Client.GetChatAdministratorsAsync(targetChatId);

                var isAdmin = admins.FirstOrDefault(x => x.User.Id == userId);

                if (isAdmin == null || isAdmin.CanPostMessages == false)
                {
                    return false;
                }
                
                return true;
            }
            catch (ApiRequestException ex) 
                when (ex.Message == "Bad Request: there is no administrators in the private chat")
            {
                // target chat is private chat
                return true;
            }
        }

        public InlineKeyboardMarkup GetPhotoKeyboard(int likesCount)
            => new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData($"❤️ ({likesCount})", "vote_l"));

        public ReplyKeyboardMarkup GetReplyKeyboardFromEnum<T>(int splitBy = 6)
            where T : struct, IConvertible
        {
            var buttons = Enum
               .GetNames(typeof(T))
               .Select(x => new KeyboardButton(x))
               .ToList();

            return new ReplyKeyboardMarkup(new[] 
                {
                    buttons.Take(splitBy).ToArray(),
                    buttons.Skip(splitBy).ToArray(),
                    buttons.Skip(splitBy * 2).ToArray(),
                    buttons.Skip(splitBy * 3).ToArray(),
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true)
            {
                Selective = true
            };
        }
    }
}