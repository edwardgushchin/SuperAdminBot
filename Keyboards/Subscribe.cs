using Telegram.Bot.Types.ReplyMarkups;

namespace SuperAdminBot.Keyboards;

public static class Subscribe
{
    public static InlineKeyboardMarkup GetKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithUrl("Подписаться", Kernel.Settings!.SubscribeButtonUrl!)
        });
    }
}