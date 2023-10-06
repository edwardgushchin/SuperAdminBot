using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class ClearFolderCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, int? messageId, CancellationToken cancellationToken)
    {
        if (chatId == Kernel.Settings!.AdminId)
        {
            try
            {
                var files = Directory.GetFiles("cache");

                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                await botClient.SendTextMessageAsync(chatId, $"❌ Произошла ошибка: {e.Message}", replyToMessageId: messageId, cancellationToken: cancellationToken);
            }
            
            await botClient.SendTextMessageAsync(chatId, "✅ Директория кэша успешно очищена.", replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(chatId, "❌ У вас нет доступа для вызова этой команды.", replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}