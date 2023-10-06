using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class UpdateKeyboardCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, int? messageId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, $"✅ Клавиатура обновлена",
            replyToMessageId: messageId, replyMarkup: Keyboards.Admin.GetKeyboard(), cancellationToken: cancellationToken);
    }
}