using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class ModeratorCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, string[] data, int? messageId, CancellationToken cancellationToken)
    {
        if (chatId == Kernel.Settings!.AdminId)
        {
            if (data.Length == 3)
            {
                var command = data[1];
                var username = data[2];

                switch (command)
                {
                    case "add" when Kernel.Settings.AddModerator(username):
                        Debug.Log($"The user {username} has been designated as a moderator.", Debug.Sender.TGSubsystem,
                            Debug.MessageStatus.INFO);
                        await botClient.SendTextMessageAsync(chatId, $"✅ Пользователь {username} назначен модератором.",
                            replyToMessageId: messageId, cancellationToken: cancellationToken);
                        break;
                    case "add":
                        await botClient.SendTextMessageAsync(chatId, $"❌ Пользователь {username} уже является модератором.",
                            replyToMessageId: messageId, cancellationToken: cancellationToken);
                        break;
                    case "del" when Kernel.Settings.DeleteModerator(username):
                        Debug.Log($"The user {username} is no longer a moderator.", Debug.Sender.TGSubsystem,
                            Debug.MessageStatus.INFO);
                        await botClient.SendTextMessageAsync(chatId,
                            $"✅ Пользователь {username} больше не является модератором.",
                            replyToMessageId: messageId, cancellationToken: cancellationToken);
                        break;
                    case "del":
                        await botClient.SendTextMessageAsync(chatId, $"❌ Пользователь {username} не является модератором.",
                            replyToMessageId: messageId, cancellationToken: cancellationToken);
                        break;
                }
                return;
            }
            await botClient.SendTextMessageAsync(chatId, "❌ Команда введена неправильно.", replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(chatId, "❌ У вас нет доступа для вызова этой команды.", replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}