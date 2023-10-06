using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SuperAdminBot.Handlers;

public static class MyChatMember
{
    public static async Task HandleAsync(ITelegramBotClient botClient, Update update, long newChatId, string? chatTitle, CancellationToken cancellationToken)
    {
        switch (update.MyChatMember!.NewChatMember.Status)
        {
            case ChatMemberStatus.Member:
            {
                if (Kernel.Settings != null && !Kernel.Settings.IsChatContains(newChatId))
                {
                    Debug.Log($"[MyChatMemberHandleAsync] Bot added to \"{chatTitle}\" ({newChatId}).", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
                    await botClient.SendTextMessageAsync(newChatId, Kernel.Settings.JoinMessage!,
                        cancellationToken: cancellationToken);
                    Kernel.Settings.AddChat(newChatId);
                }
                break;
            }
            case ChatMemberStatus.Left:
            {
                if (Kernel.Settings != null && Kernel.Settings.IsChatContains(newChatId))
                {
                    Kernel.Settings.RemoveChat(newChatId);
                    Debug.Log($"[MyChatMemberHandleAsync] Bot removed from \"{chatTitle}\" ({newChatId}).", Debug.Sender.TGSubsystem, Debug.MessageStatus.WARN);
                }

                break;
            }
            case ChatMemberStatus.Creator:
                break;
            case ChatMemberStatus.Administrator:
                break;
            case ChatMemberStatus.Kicked:
                break;
            case ChatMemberStatus.Restricted:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}