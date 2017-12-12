using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using System.Threading;
using Telegram.BotClient;
using Telegram.BotApi;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Подождите, идет инициализация....");
            string token;
            bool useOnlyBehavior;
            string payToken;
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            token = settings["BotToken"];
            payToken = settings["payToken"];
            bool.TryParse(settings["useOnlyBehavior"], out useOnlyBehavior);

            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException("Необходим token bot api !!");

            BotClient client = new BotClient(token, payToken);
            client.Start();
            client.SetCommandHandler("command1", (arg) => { arg.API.SendMessage(arg.ChatId, DateTime.Now.ToString()); });
            client.SetCommandHandler("kek", (arg) => { arg.API.SendMessage(arg.ChatId, "кек мек))"); });
            client.SetBotBehavior(new CreditBotBehavior());
            Console.Clear();
            Console.WriteLine($"BotToken - {token}\nPayToken - {payToken}");
            Console.WriteLine("Нажмите любую кнопку для того, чтобы сохранить состояние и выйти...");
            Console.ReadKey();
            client.SaveBotBehaviorState();
        }
    }
}
