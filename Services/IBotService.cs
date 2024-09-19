using Telegram.Bot;
using Telegram.Bot.Types;

namespace AccordeonBot.Services;
public interface IBotService
{
    public interface IBotService
    {
        Task Start();
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
    }

}