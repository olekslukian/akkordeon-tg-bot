using System.ComponentModel;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace AccordeonBot.Services;
public class BotService : IBotService
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ReceiverOptions _receiverOptions = new()
    {
        AllowedUpdates = [],
    };
    private readonly TelegramBotClient _botClient;


    public BotService(TelegramBotClient? botClient)
    {
        if (botClient == null)
        {
            Console.WriteLine("BotService: botClient is null");

            throw new ArgumentNullException(nameof(botClient));
        }
        _botClient = botClient;

    }

    public async Task Start()
    {
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: _receiverOptions,
            cancellationToken: _cts.Token
            );

        var me = await _botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        _cts.Cancel();
    }

    protected static async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
        )
    {
        if (update.Message is not { } message)
            return;

        if (message.Photo is not { } messagePhoto)
            return;


        var fileId = messagePhoto.Last().FileId;

        const string destinationFilePath = "../image.jpg";

        await using Stream fileStream = System.IO.File.Create(destinationFilePath);

        var file = await botClient.GetInfoAndDownloadFileAsync(
            fileId: fileId,
            destination: fileStream,
            cancellationToken: cancellationToken);

        fileStream.Close();

        Image<Rgba32> image = await Image.LoadAsync<Rgba32>(destinationFilePath, cancellationToken);

        string hash = GetHash(image, destinationFilePath);

        Console.WriteLine($"Hash: {hash}");

        System.IO.File.Delete(destinationFilePath);
    }

    private static string GetHash(Image<Rgba32> image, string destinationFilePath)
    {
        image.Mutate(x => x.Resize(64, 64));

        Rgba32 white = SixLabors.ImageSharp.Color.White;

        Rgba32 black = SixLabors.ImageSharp.Color.Black;

        image.Mutate(x => x.BinaryThreshold(0.5f, black, white));

        image.Save(destinationFilePath);

        var hashList =  CreateHashList(image, white);

        return CreateHashString(hashList);
    }


    private static List<bool> CreateHashList(Image<Rgba32> image, Rgba32 white)
    {
        var list = new List<bool>();


        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                bool isWhite = image[x, y] == white;
                list.Add(isWhite);
            }
        }

        return list;
    }

    private static string CreateHashString(List<bool> hashList)
    {
        StringBuilder hashBuilder = new();

        for (int i = 0; i < hashList.Count; i += 8)
        {
            byte currentByte = 0;

            for (int j = 0; j < 8; j++)
            {
                bool pixelValue = hashList[i + j];
                if (pixelValue)
                {
                    currentByte |= (byte)(1 << (7 - j));
                }
            }

            hashBuilder.Append(currentByte.ToString("X2"));
        }

        string hash = hashBuilder.ToString();

        return hash;
    }

    private static Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
        )
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}

