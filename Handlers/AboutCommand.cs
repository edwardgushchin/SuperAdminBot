using System.Text;
using Telegram.Bot;

namespace SuperAdminBot.Handlers;

public static class AboutCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, string? username, int? messageId, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Привет. Меня зовут Эдуард. Я разработчик этого бота.");
        sb.AppendLine();
        sb.AppendLine("Когда я окончательно перекочевал из ВК в Телеграм, я заметил, что мне нехватает видое-контента, " +
                      "по типу ВКшных пабликов с рандомными видосами, в которые можно часами аутировать в свободное время. " +
                      "Тогда я придумал идею, что можно взять все мои подписки из ВК, и закидывать оттуда новый контент " +
                      "прям мне в личку. Чуть позже я подумал, что он еще может сохранять понравившиеся мне видосы на " +
                      "определенный канал-свалку. Так родился этот бот и канал @thevideochan.");
        sb.AppendLine();
        sb.AppendLine(
            "Я довольно хороший программист, и люблю все. что связано с социальными сетями. Если вам нужно нечто похожее на этого бота, вы можете написать мне @eduardgushchin");
        
        await botClient.SendTextMessageAsync(chatId, sb.ToString(), disableWebPagePreview: true,
            replyToMessageId: messageId, cancellationToken: cancellationToken);
    }
}