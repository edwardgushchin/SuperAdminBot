using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SuperAdminBot.Handlers;

public static class StatusCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, string? username, int? messageId, CancellationToken cancellationToken)
    {
        if (chatId == Kernel.Settings!.AdminId || Kernel.Settings.Moderators!.Contains($"@{username}"))
        {
            var msg = Kernel.GetSubsystemsStatus();
            await botClient.SendTextMessageAsync(chatId, msg, ParseMode.MarkdownV2, replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(chatId, "❌ У вас нет доступа для вызова этой команды.", replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}