using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WetPicsTelegramBot
{
    class DbRepository
    {
        private DbRepository()
        {
        }

        public static DbRepository Instance => Nested.Instance;

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly DbRepository Instance = new DbRepository();
        }

        public async Task AddOrUpdateVote(string userId, string chatId, int messageId, int? score = null, bool? isLiked = null)
        {
            try
            {
                using (var db = new WetPicsDbContext())
                {
                    var photoVote =
                        await db.PhotoVotes.FirstOrDefaultAsync(x => x.ChatId == chatId && x.MessageId == messageId && x.UserId == userId);

                    if (photoVote != null)
                    {
                        if (score.HasValue)
                        {
                            photoVote.Score = score.Value;
                        }
                        if (isLiked.HasValue)
                        {
                            photoVote.IsLiked = isLiked.Value;
                        }
                    }
                    else
                    {
                        photoVote = new PhotoVote
                        {
                            ChatId = chatId,
                            MessageId = messageId,
                            UserId = userId,
                            Score = score,
                            IsLiked = isLiked
                        };

                        await db.PhotoVotes.AddAsync(photoVote);
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task<Vote> GetVotes(long messageId)
        {
            try
            {
                using (var db = new WetPicsDbContext())
                {
                    var photoVotes = await db.PhotoVotes.Where(x => x.MessageId == messageId).ToListAsync();

                    var result = new Vote
                    {
                        Scores =
                        {
                            [1] = photoVotes.Count(x => x.Score == 1),
                            [2] = photoVotes.Count(x => x.Score == 2),
                            [3] = photoVotes.Count(x => x.Score == 3),
                            [4] = photoVotes.Count(x => x.Score == 4),
                            [5] = photoVotes.Count(x => x.Score == 5)
                        },
                        Liked = photoVotes.Count(x => x.IsLiked == true),
                        Disliked = photoVotes.Count(x => x.IsLiked == false)
                    };



                    return result;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return default(Vote);
            }
        }

        public async Task RemoveChatSettings(string chatId)
        {
            try
            {
                using (var db = new WetPicsDbContext())
                {
                    var chatSettings = await db.ChatSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings != null)
                    {
                        db.ChatSettings.Remove(chatSettings);
                    }

                    await db.SaveChangesAsync();
                }

                OnChatSettingsChanged();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public async Task SetChatSettings(string chatId, string targetId)
        {
            try
            {
                using (var db = new WetPicsDbContext())
                {
                    var chatSettings = await db.ChatSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings == null)
                    {
                        var newChatSettings = new ChatSetting
                        {
                            ChatId = chatId,
                            TargetId = targetId
                        };

                        await db.ChatSettings.AddAsync(newChatSettings);
                    }
                    else
                    {
                        chatSettings.TargetId = targetId;
                    }

                    await db.SaveChangesAsync();
                }

                OnChatSettingsChanged();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public async Task<List<ChatSetting>> GetChatSettings()
        {
            try
            {
                using (var db = new WetPicsDbContext())
                {
                    var chatSettings = await db.ChatSettings.ToListAsync();
                    
                    return chatSettings;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public event EventHandler ChatSettingsChanged;

        private void OnChatSettingsChanged()
        {
            var handler = ChatSettingsChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    class Vote
    {
        public int[] Scores = new int[6];

        public int Liked;

        public int Disliked;
    }
}
