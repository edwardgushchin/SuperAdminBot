using Telegram.Bot.Types.ReplyMarkups;

namespace SuperAdminBot.Keyboards;

public class Admin
{
    public static readonly string STATUS_BUTTON = "📝 Статус";
    public static readonly string HELP_BUTTON = "🆘 Помощь";
    public static readonly string ABOUT_BUTTON = "ℹ О разработчиках";
    
    public static ReplyKeyboardMarkup GetKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new []
            {
                new KeyboardButton(STATUS_BUTTON),
                new KeyboardButton(HELP_BUTTON),
            },
            new []
            {
                new KeyboardButton(ABOUT_BUTTON),
            }
        })
        {
            ResizeKeyboard = true
        };
    }
}