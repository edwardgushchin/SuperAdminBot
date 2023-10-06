using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class StartCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, Kernel.Settings?.StartMessage!, replyMarkup: Keyboards.Subscribe.GetKeyboard(), disableWebPagePreview: true,
            cancellationToken: cancellationToken);
    }
}