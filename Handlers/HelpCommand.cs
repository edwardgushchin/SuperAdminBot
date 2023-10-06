using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SuperAdminBot.Handlers;

public static class HelpCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, string? username, int? messageId, CancellationToken cancellationToken)
    {
        if (chatId == Kernel.Settings!.AdminId)
        {
            var sr = new StringBuilder();
            sr.AppendLine("/start \\- Запуск бота");
            sr.AppendLine("/admin \\- Задать админа, если он не задан");
            sr.AppendLine("/status \\- Получить информацию о состоянии бота");
            sr.AppendLine("/moderator \\[add\\|del\\] \\[@username\\] \\- добавить/удалить модератора");
            sr.AppendLine("/set \\[timeout\\|sleep\\|cash\\] \\[value\\] \\- задать значение параметра");
            sr.AppendLine("/clearorder \\- Отчистить очередь запросов");
            sr.AppendLine("/clearfolder \\- Отчистить папку кэша");
            sr.AppendLine("/updatekeyboard \\- Обновить главную клавиатуру");
            sr.AppendLine("/about \\- Получить информацию о разработчиках");
            sr.AppendLine("/help \\- Получить данную справку");
            sr.AppendLine();
            sr.AppendLine("*Публикация видео*");
            sr.AppendLine();
            sr.AppendLine("Опубликовать видео можно несколькими способами:");
            sr.AppendLine();
            sr.AppendLine("1\\. Видео пришло из подписок, можно нажать кнопку _*Одобрить*_ и видео опубликуется на канале и в чатах, либо нажать на кнопку _*Отклонить*_, и видео удалится из кэша и очереди запросов\\.");
            sr.AppendLine("2\\. Можно ответить на видео с подписок в личке бота и добавить к нему подпись, которая опубликуется вместе с видео на канале и в чатах\\. В этом случае видео удалиться из очереди запросов и кэша после публикации\\.");
            sr.AppendLine("3\\. Можно залить видео в личку с подписью или без, и оно опубликуется на канале и в чатах\\.");
            sr.AppendLine("4\\. Можно переслать видео из другого чата или канала в личку боту, и оно опубликуется на канале и в чатах\\. В этом случае подпись установить не получится\\.");
            
            await botClient.SendTextMessageAsync(chatId, sr.ToString(), ParseMode.MarkdownV2,
                replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        
        if (Kernel.Settings.Moderators!.Contains($"@{username}"))
        {
            var sr = new StringBuilder();
            sr.AppendLine("/start \\- Запуск бота");
            sr.AppendLine("/status \\- Получить информацию о состоянии бота");
            sr.AppendLine("/about \\- Получить информацию о разработчиках");
            sr.AppendLine("/help \\- Получить данную справку");
            sr.AppendLine();
            sr.AppendLine("*Публикация видео*");
            sr.AppendLine();
            sr.AppendLine("Опубликовать видео можно несколькими способами:");
            sr.AppendLine();
            sr.AppendLine("1. Можно залить видео в личку с подписью или без, и оно опубликуется на канале и в чатах\\.");
            sr.AppendLine("2. Можно переслать видео из другого чата или канала в личку боту, и оно опубликуется на канале и в чатах. В этом случае подпись установить не получится\\.");
            
            await botClient.SendTextMessageAsync(chatId, sr.ToString(), ParseMode.MarkdownV2,
                replyToMessageId: messageId, cancellationToken: cancellationToken);
            return;
        }
        
        var r = new StringBuilder();
        r.AppendLine("/start \\- Запуск бота");
        r.AppendLine("/about \\- Получить информацию о разработчиках");
        r.AppendLine("/help \\- Получить данную справку");
        
        await botClient.SendTextMessageAsync(chatId, r.ToString(), ParseMode.MarkdownV2,
            replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}