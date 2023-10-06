using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;

namespace SuperAdminBot.Keyboards;

public static class PublishVideo
{
    public static InlineKeyboardMarkup GetKeyboard(int messageId, string fileName)
    {
        var payloadJson = GetPayloadSugestion(messageId, fileName);
        
        var query = Kernel.CallbackOrder!.AddQuery(payloadJson);

        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("👍🏻 Опубликовать",query.Guid),
            },
        });
    }
    
    private static string GetPayloadSugestion(int messageId, string path)
    {
        return JsonConvert.SerializeObject(new CallbackQueryData()
        {
            Type = CallbackType.Suggestion,
            Payload = JsonConvert.SerializeObject(new CallbackPayloads.SuggestionQueryData()
            {
                MessageId = messageId,
                Video = path
            })
        });
    }
}