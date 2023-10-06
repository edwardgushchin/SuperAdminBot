using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class SetCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, string[] data, int? messageId, CancellationToken cancellationToken)
    {
        if(chatId == Kernel.Settings!.AdminId)
        {
            if (data.Length == 3)
            {
                try
                {
                    var set = data[1];
                    var value = Convert.ToInt32(data[2]);

                    if (set == "timeout")
                    {
                        if (value > 1)
                        {
                            Kernel.Settings.CheckTimeoutInMinutes = value;
                            await botClient.SendTextMessageAsync(chatId, $"✅ Параметр CheckTimeoutInMinutes установлен в значение {value}.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                        }
                        else await botClient.SendTextMessageAsync(chatId, $"❌ Параметр CheckTimeoutInMinutes не может быть меньше 1.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                    }
                    else if (set == "sleep")
                    {
                        if (value > 1)
                        {
                            Kernel.Settings.SleepForRateLimit = value;
                            await botClient.SendTextMessageAsync(chatId, $"✅ Параметр SleepForRateLimit установлен в значение {value}.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                        }
                        else await botClient.SendTextMessageAsync(chatId, $"❌ Параметр SleepForRateLimit не может быть меньше 1.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                    }
                    else if (set == "cash")
                    {
                        if (value is > 1 and <= 200)
                        {
                            Kernel.Settings.CacheSize = value;
                            await botClient.SendTextMessageAsync(chatId, $"✅ Параметр CacheSize установлен в значение {value}.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                        }
                        else await botClient.SendTextMessageAsync(chatId, $"❌ Параметр CacheSize не может быть меньше 1 и больше 200.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"❌ Параметр \"{set}\" не поддерживается.", replyToMessageId: messageId, cancellationToken: cancellationToken);
                    }
                    
                }
                catch (Exception e)
                {
                    Debug.Log($"[SetCommandHandleAsync] {e.Message}", Debug.Sender.TGSubsystem, Debug.MessageStatus.FAIL);
                    await botClient.SendTextMessageAsync(chatId, $"❌ [{e.InnerException}] {e.Message}", replyToMessageId: messageId, cancellationToken: cancellationToken);
                }
                return;
            }
            await botClient.SendTextMessageAsync(chatId, "❌ Команда введена неправильно.", replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(chatId, "❌ У вас нет доступа для вызова этой команды.", replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}