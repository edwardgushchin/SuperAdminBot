using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class ClearOrderCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, int? messageId, CancellationToken cancellationToken)
    {
        if (chatId == Kernel.Settings!.AdminId)
        {
            Kernel.CallbackOrder!.Clear();
            await botClient.SendTextMessageAsync(chatId, "✅ Очередь запросов успешно очищена.", replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(chatId, "❌ У вас нет доступа для вызова этой команды.", replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}