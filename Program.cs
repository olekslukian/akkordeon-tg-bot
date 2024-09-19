using Telegram.Bot;
using AccordeonBot.Services;

var botClient = new TelegramBotClient("7001874355:AAEfKeGpn1PBAbQYWmOpROoymD_sJtNdrCA");

BotService botService = new(botClient);

await botService.Start();



