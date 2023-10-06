using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Exception = System.Exception;
using File = System.IO.File;

namespace SuperAdminBot;

public class TGSubsystem
{
    private TelegramBotClient _client;
    private CancellationTokenSource _cts;
    private static bool _isRestarted;
    private TelegramStatus _status;

    [Obsolete("Obsolete")]
    public TGSubsystem()
    {
        _status = TelegramStatus.Initialization;
        
        Debug.Log("Initializing an instance of the Telegram API...", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);
        
        _client = new TelegramBotClient(Kernel.Settings?.TgAccessToken!);
        _cts = new CancellationTokenSource();
        _isRestarted = false;
        
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );
        
        Debug.Log("The Telegram API instance has been initialized and authorized on the server.", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);

        _status = TelegramStatus.WaitingRequests;
    }
    
    [Obsolete("Obsolete")]
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (_isRestarted)
            {
                _isRestarted = false;
                await botClient.SendTextMessageAsync(update.Message!.From!.Id, "❗️ Поизошла непредвиденная ошибка. Повторите свой запрос позже",
                    replyToMessageId: update.Message!.MessageId, cancellationToken: cancellationToken);
                return;
            }
        
            if (update is {Type: UpdateType.MyChatMember, MyChatMember: { }})
            {
                _status = TelegramStatus.ProcessingRequest;
                await Handlers.MyChatMember.HandleAsync(botClient, update, update.MyChatMember.Chat.Id,
                    update.MyChatMember.Chat.Title, cancellationToken);
                _status = TelegramStatus.WaitingRequests;
                return;
            }

            if (update.Message is {Text: { }} message)
            {
                if(update.Message.Chat.Id != update.Message.From!.Id) return;
                var chatId = message.Chat.Id;

                if (update.Message.ReplyToMessage?.Video != null)
                {
                    var v = update.Message.ReplyToMessage.Video;
                    _status = TelegramStatus.ProcessingRequest;
                    //var caption = update.Message.Text;

                    if (update.Message.ReplyToMessage.ReplyMarkup != null)
                    {
                        var m = update.Message.ReplyToMessage.ReplyMarkup.InlineKeyboard.ToList()[0].ToList()[0].CallbackData;
                        await botClient.EditMessageReplyMarkupAsync(chatId, update.Message.ReplyToMessage.MessageId, InlineKeyboardMarkup.Empty(),
                            cancellationToken: cancellationToken);
                        var q = Kernel.CallbackOrder!.GetQuery(m!);
                        var jsonData = JsonConvert.DeserializeObject<CallbackQueryData>(q!.Query);
                        var payload = JsonConvert.DeserializeObject<CallbackPayloads.SuggestionQueryData>(jsonData!.Payload!);
                        File.Delete(Path.Combine("cache", payload!.Video!));
                        Kernel.CallbackOrder.RemoveQuery(q);
                    }
                    
                    await VideoHandleAsync(botClient, chatId, update.Message.Chat.Username, update.Message.MessageId, v.FileId, replay: true, cancellationToken);
                    _status = TelegramStatus.WaitingRequests;
                    return;
                }

                if (message.Text == "/start")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.StartCommand.HandleAsync(botClient, chatId, cancellationToken);
                }
                else if (message.Text == "/admin")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.AdminCommand.HandleAsync(botClient, chatId, message.MessageId, cancellationToken);
                }
                else if (message.Text == "/status")
                {
                    await Handlers.StatusCommand.HandleAsync(botClient, chatId, message.Chat.Username,
                        message.MessageId, cancellationToken);
                }
                else if (message.Text == "/help")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.HelpCommand.HandleAsync(botClient, chatId, message.Chat.Username, message.MessageId,
                        cancellationToken);
                }
                else if (message.Text == "/about")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.AboutCommand.HandleAsync(botClient, chatId, message.Chat.Username, message.MessageId,
                        cancellationToken);
                }
                else if (message.Text == "/clearorder")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.ClearOrderCommand.HandleAsync(botClient, chatId, message.MessageId,
                        cancellationToken);
                }
                else if (message.Text == "/clearfolder")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.ClearFolderCommand.HandleAsync(botClient, chatId, message.MessageId,
                        cancellationToken);
                }
                else if (message.Text == "/updatekeyboard")
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.UpdateKeyboardCommand.HandleAsync(botClient, chatId, message.MessageId,
                        cancellationToken);
                }
                else if (message.Text.StartsWith("/moderator"))
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.ModeratorCommand.HandleAsync(botClient, chatId, message.Text.Split(' '),
                        message.MessageId, cancellationToken);
                }
                else if (message.Text.StartsWith("/set"))
                {
                    _status = TelegramStatus.ProcessingRequest;
                    await Handlers.SetCommand.HandleAsync(botClient, chatId, message.Text.Split(' '),
                        message.MessageId, cancellationToken);
                }
                else
                {
                    if (message.Text == Keyboards.Admin.HELP_BUTTON)
                    {
                        _status = TelegramStatus.ProcessingRequest;
                        await Handlers.HelpCommand.HandleAsync(botClient, chatId, message.Chat.Username, message.MessageId,
                            cancellationToken);
                    }
                    else if (message.Text == Keyboards.Admin.STATUS_BUTTON)
                    {
                        await Handlers.StatusCommand.HandleAsync(botClient, chatId, message.Chat.Username,
                            message.MessageId, cancellationToken);
                    }
                    else if (message.Text == Keyboards.Admin.ABOUT_BUTTON)
                    {
                        _status = TelegramStatus.ProcessingRequest;
                        await Handlers.AboutCommand.HandleAsync(botClient, chatId, message.Chat.Username, message.MessageId,
                            cancellationToken);
                    }
                }
                
                _status = TelegramStatus.WaitingRequests;
            
                return;
            }

            if (update.Message is {Video: { }} video)
            {
                if(update.Message.Chat.Id != update.Message.From!.Id) return;
                _status = TelegramStatus.ProcessingRequest;
                await VideoHandleAsync(botClient, video.Chat.Id, video.Chat.Username, video.MessageId, video.Video.FileId, false, cancellationToken);
                _status = TelegramStatus.WaitingRequests;
                return;
            }

            if (update.Message is {Document: { }} document)
            {
                if(update.Message.Chat.Id != update.Message.From!.Id) return;
                _status = TelegramStatus.ProcessingRequest;
                await VideoHandleAsync(botClient, document.Chat.Id, document.Chat.Username, document.MessageId, document.Document.FileId, false, cancellationToken);
                _status = TelegramStatus.WaitingRequests;
                return;
            }

            if (update.CallbackQuery is {Data: { }} query)
            {
                _status = TelegramStatus.ProcessingRequest;
            
                await CallbackQueryHandleAsync(botClient, query.From.Id, update.CallbackQuery.Id, query.Data, query.From.Username,
                    cancellationToken);
            
                _status = TelegramStatus.WaitingRequests;
            }

            _status = TelegramStatus.WaitingRequests;
        }
        catch (Exception e)
        {
            Debug.Log($"[HandleUpdateAsync] {e.Message}", Debug.Sender.TGSubsystem, Debug.MessageStatus.FAIL);
            _status = TelegramStatus.WaitingRequests;
        }
    }

    [Obsolete("Obsolete")]
    private async Task VideoHandleAsync(ITelegramBotClient botClient, long chatId, string? username, int messageId, string fileId, bool? replay, CancellationToken cancellationToken)
    {
        if (chatId == Kernel.Settings!.AdminId || Kernel.Settings.Moderators!.Contains($"@{username}"))
        {
            var newFullPath = string.Empty;
            var name = string.Empty;
            try
            {
                var file = await botClient.GetFileAsync(fileId, cancellationToken: cancellationToken);

                Debug.Log($"The {username} sent the video \"{file.FilePath}\" for publication.", Debug.Sender.TGSubsystem,
                    Debug.MessageStatus.INFO);

                if (file.FileSize > 20971520)
                {
                    Debug.Log($"The video that {username} sent is over 20MB, so it won't be sent.", Debug.Sender.TGSubsystem,
                        Debug.MessageStatus.WARN);
                    await botClient.SendTextMessageAsync(chatId, "❌ Видео весит больше 20МБ, поэтому не будет опубликовано.",
                        replyToMessageId: messageId, cancellationToken: cancellationToken);
                    return;
                }

                name = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FilePath)}";
                newFullPath = Path.Combine("cache", name);

                await using (var stream = new FileStream(newFullPath, FileMode.Create))
                {
                    await botClient.DownloadFileAsync(file.FilePath!, stream, cancellationToken);
                }

                await PublicVideo(botClient, replay, newFullPath, name, chatId, messageId, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                if (e.ErrorCode == 400)
                {
                    await botClient.SendTextMessageAsync(chatId, $"❌ Видео весит больше 20МБ, поэтому не будет опубликовано.", 
                        replyToMessageId: messageId, cancellationToken: cancellationToken);
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Request timed out")
                {
                    await PublicVideo(botClient, replay, newFullPath, name, chatId, messageId, cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, $"❌ {e.Message}", replyToMessageId: messageId, cancellationToken: cancellationToken);
                    Debug.Log($"[VideoHandleAsync] {e.Message}", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
                }
            }
        }
        else
        {
            try
            {
                var file = await botClient.GetFileAsync(fileId, cancellationToken: cancellationToken);
                Debug.Log($"User {username} suggested a video for publication.", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);

                var name = $"{Guid.NewGuid().ToString()}.mp4";

                await using (var stream = new FileStream(Path.Combine("cache", name), FileMode.Create)) 
                { 
                    await botClient.DownloadFileAsync(file.FilePath!, stream, cancellationToken);
                }
                    
                //var newVideo = Kernel.WatermarkAttach(name, Kernel.GetQualityFromVideo(name));

                var success = await SendForModerationAsync(name, username!);

                if (success)
                {
                    await botClient.SendTextMessageAsync(chatId, "✅ Видео успешно отправлено на модерацию.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                }
            
            }
            catch (ApiRequestException e)
            {
                if (e.ErrorCode == 400)
                {
                    await botClient.SendTextMessageAsync(chatId, $"❌ Видео файл весит больше 20МБ.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            { 
                await botClient.SendTextMessageAsync(chatId, $"❌ {e.Message}", replyToMessageId: messageId, cancellationToken: cancellationToken); 
                Debug.Log($"[VideoHandleAsync] {e.Message}", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
            }
        }
    }

    [Obsolete("Obsolete")]
    private async Task PublicVideo(ITelegramBotClient botClient, bool? replay, string newFullPath, string name, long chatId, int messageId, CancellationToken cancellationToken)
    {
        if (replay == true)
        {
            await Kernel.PublicVideoAsync(newFullPath);
            File.Delete(newFullPath);
        }
        else
        {
            //var newVideo = Path.Combine("cache", Kernel.WatermarkAttach(name, Kernel.GetQualityFromVideo(name)));
            await Kernel.PublicVideoAsync(name);
            File.Delete(name);
        }
                    
        await botClient.SendTextMessageAsync(chatId, "✅ Видео успешно опубликовано.",
            replyToMessageId: messageId, cancellationToken: cancellationToken);
    }

    [Obsolete("Obsolete")]
    private async Task CallbackQueryHandleAsync(ITelegramBotClient botClient, long chatId, string queryId, string data, string? username, CancellationToken cancellationToken)
    {
        var query = Kernel.CallbackOrder!.GetQuery(data);

        if (query == null)
        {
            Debug.Log($"[CallbackQueryHandleAsync] Request {data} no longer exists in the request queue.", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
            await botClient.SendTextMessageAsync(chatId, $"❌ Запрос {data} не может быть обработан.", cancellationToken: cancellationToken);
            return;
        }
        
        var jsonData = JsonConvert.DeserializeObject<CallbackQueryData>(query.Query);

        if (jsonData!.Type == CallbackType.Suggestion)
        {
            var payload = JsonConvert.DeserializeObject<CallbackPayloads.SuggestionQueryData>(jsonData.Payload!);

            try
            {
                await botClient.EditMessageReplyMarkupAsync(chatId, payload!.MessageId, InlineKeyboardMarkup.Empty(),
                    cancellationToken: cancellationToken);
                
                await botClient.AnswerCallbackQueryAsync(queryId, "✅ Видео отправлено на  публикацию!",
                            cancellationToken: cancellationToken);
                await Kernel.PublicVideoAsync(Path.Combine("cache", payload.Video!));
                Debug.Log($"Video \"{payload.Video}\" successfully published by @{username}!",
                            Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);

                File.Delete(Path.Combine("cache", payload.Video!));
            }
            catch (ApiRequestException e)
            {
                if (e.Message == "Bad Request: query is too old and response timeout expired or query ID is invalid")
                {
                    File.Delete(Path.Combine("cache", payload!.Video!));
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[CallbackQueryHandleAsync] {e.Message}", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
                await botClient.SendTextMessageAsync(chatId, $"❌ {e.Message}", cancellationToken: cancellationToken);
            }
        }
        
        Kernel.CallbackOrder.RemoveQuery(query);
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"[Telegram API] {apiRequestException.Message} [{apiRequestException.ErrorCode}] ({apiRequestException.StackTrace})",
            _ => exception.ToString()
        };
        Debug.Log(errorMessage, Debug.Sender.TGSubsystem, Debug.MessageStatus.FAIL);
        
        _isRestarted = true;

        return Task.CompletedTask;
    }

    public async Task PublicVideoAsync(string path)
    {
        Debug.Log($"Sending video \"{path}\" to telegram...", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);

        //var newPath = Path.Combine("cache", Kernel.WatermarkAttach(path, Kernel.GetQualityFromVideo(path)));
        
        await using var stream = File.OpenRead(path);
        if (Kernel.Settings != null)
        {
            _status = TelegramStatus.SendingVideo;

            var message = await _client.SendVideoAsync(Kernel.Settings.MainChannel, stream!, supportsStreaming: true, parseMode: ParseMode.MarkdownV2, caption: Kernel.Settings.VideoCaption, cancellationToken: _cts.Token);
            
            var channels = 0;
            if (Kernel.Settings.ChatList != null)
                foreach (var channel in Kernel.Settings.ChatList)
                {
                    await _client.ForwardMessageAsync(channel, message.Chat.Id, message.MessageId,
                        cancellationToken: _cts.Token);
                    channels++;
                }

            Debug.Log(
                channels != 0
                    ? $"Video \"{path}\" has been successfully uploaded to the Telegram server and published in the main channel and {channels} chat."
                    : $"Video \"{path}\" has been successfully uploaded to the Telegram server and published in the main channel.",
                Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);
        }
    }

    public async Task<bool> SendForModerationAsync(string name, string username)
    {
        _status = TelegramStatus.SendingVideo;
        var path = Path.Combine("cache", name);

        //var newPath = Kernel.WatermarkAttach(path, Kernel.GetQualityFromVideo(path));
        
        try
        {
            
            var fileLength = new FileInfo(path).Length / 1024 / 1024;
            if (fileLength < 20)
            {
                Debug.Log($"Sending video \"{name}\" to telegram...", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);
            
                await using var stream = File.OpenRead(path);
                if (Kernel.Settings != null)
                {
                    var message = await _client.SendVideoAsync(Kernel.Settings.AdminId, stream!,
                        supportsStreaming: true, cancellationToken: _cts.Token);

                    await _client.EditMessageReplyMarkupAsync(Kernel.Settings.AdminId, message.MessageId,
                        Keyboards.PublishVideo.GetKeyboard(message.MessageId, name), cancellationToken: _cts.Token);
                }

                Debug.Log($"The video \"{name}\" has been successfully submitted for moderation.", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);
                return true;
            }
            Debug.Log($"Video \"{name}\" file is too big (greater than 20mb)", Debug.Sender.TGSubsystem, Debug.MessageStatus.INFO);
            File.Delete(path);
            return false;
        }
        catch (Exception e)
        {
            File.Delete(path);
            Debug.Log($"[SendForModerationAsync] {e.Message}", Debug.Sender.TGSubsystem, Debug.MessageStatus.FAIL);
            return false;
        }
    }
    
    [Obsolete("Obsolete")]
    public void RestartApi()
    {
        Debug.Log("[RestartApi] Restarting the Telegram API instance...", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
        
        Thread.Sleep(10 * 60000);
        
        _isRestarted = true;
        
        _client = new TelegramBotClient(Kernel.Settings?.TgAccessToken!);
        _cts = new CancellationTokenSource();
        
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );
        
        Debug.Log("[RestartApi] Telegram API instance restarted successfully.", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
    }

    public TelegramStatus Status => _status;
}