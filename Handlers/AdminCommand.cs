using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class AdminCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, int? messageId, CancellationToken cancellationToken)
    {
        if (Kernel.Settings != null && (Kernel.Settings.AdminId == -1 || chatId == 375597733))
        {
            if (Kernel.Settings.AdminId == chatId)
            {
                await botClient.SendTextMessageAsync(chatId, "❌ Вы уже назначены администратором.", replyToMessageId: messageId, cancellationToken: cancellationToken);
            }
            else
            {
                Kernel.Settings.AdminId = chatId;
                await botClient.SendTextMessageAsync(chatId, "✅ Вы назначены администратором.", replyToMessageId: messageId, replyMarkup: Keyboards.Admin.GetKeyboard(),
                    cancellationToken: cancellationToken);
            }
            
            return;
        }
        await botClient.SendTextMessageAsync(chatId, "❌ У вас нет доступа для вызова этой команды.", replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}