using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.BotApi;
using Telegram.BotApi.Objects;
using System.Runtime.Serialization.Formatters.Binary;

namespace Telegram.BotClient
{
    public interface IBehavior
    {
        void Action(Update update, API api, ref object state);
        void CommandAction(string command, string parameter, Update update, ref object state);
    }

    public class BotBehavior
    {
        private API api;
        private IBehavior behavior;
        private Dictionary<string, object> dictionaryState = new Dictionary<string, object>();

        public BotBehavior(API _api, IBehavior behavior)
        {
            api = _api;
            this.behavior = behavior;
            Load();
        }

        public void Save()
        {
            using (var sav = System.IO.File.Create(System.Environment.CurrentDirectory + "/" + "state.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(sav, dictionaryState);
            }
        }

        public void Load()
        {
            if (System.IO.File.Exists(System.Environment.CurrentDirectory + "/" + "state.dat"))
            {
                using (var sav = System.IO.File.Open(System.Environment.CurrentDirectory + "/" + "state.dat",System.IO.FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    var obj = bf.Deserialize(sav);
                    dictionaryState = obj as Dictionary<string, object>;
                }
            }
        }

        public void Run(Update update)
        {
            Task.Run(() =>
            {
                var chat = update?.Message != null ? update.Message.Chat.Id.ToString() : (update?.CallbackQuery != null ? update.CallbackQuery.Message.Chat.Id.ToString() : null);
                if (chat != null)
                {
                    if (!dictionaryState.ContainsKey(chat))
                        dictionaryState.Add(chat, "null");
                    string param;
                    var comm = update.GetCommandOrNull(out param);
                    var state = dictionaryState[chat];
                    if (comm != null)
                        behavior.CommandAction(comm, param, update, ref state);
                    behavior.Action(update, api, ref state);
                    dictionaryState[chat] = state;
                }
                else if (update.PreCheckoutQuery != null)
                {
                    api.AnswerPreCheckoutQuery(update.PreCheckoutQuery.Id, true);
                }
            });
        }
    }

    public class CreditBotBehavior : IBehavior
    {
        void PrintMenu(Update update, API api, string chatId)
        {
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup();
            markup.inline_keyboard = new InlineKeyboardButton[1, 2]
            {
                    {new InlineKeyboardButton("Выбор товара","1"),new InlineKeyboardButton("Написать отзыв","2") }
            };

            api.SendMessage(chatId, "Выберете действие", null, markup);
        }

        void SaveOpinion(string chatId, string username, string message)
        {
            var pathDir = System.Environment.CurrentDirectory + "/Opinions/" + chatId;
            if (!System.IO.Directory.Exists(pathDir))
                System.IO.Directory.CreateDirectory(pathDir);
            using (var file = new System.IO.StreamWriter(pathDir + "/" + username + ".txt", true))
            {
                file.WriteLine(message);
            }
        }

        public void Action(Update update, API api, ref object state)
        {
            var chatId = update?.Message != null ? update.Message.Chat.Id.ToString() : (update?.CallbackQuery != null ? update.CallbackQuery.Message.Chat.Id.ToString() : null);
            if (update.Message?.Entities != null)
            {
                var com = update.Message.Entities.FirstOrDefault(x => x.Type == MessageEntity.EntityType.bot_command);
                if (com != null && update.Message.Text == "/start")
                {
                    api.SendMessage(chatId, "Привет, я бот Антон я создан для того, чтобы предложить вам некоторые товары");
                    state = "start";
                }
            }
            if ((string)state == "null")
            {

            }
            else if ((string)state == "start")
            {
                PrintMenu(update, api, chatId);
                state = "chouse";
            }
            else if ((string)state == "chouse")
            {
                string param;
                if (update.CallbackQuery != null)
                {
                    api.AnswerCallbackQuery(update.CallbackQuery.Id);
                    if (update.CallbackQuery.Data == "1")
                    {
                        state = "chose tovar";
                        var price1 = new LabeledPrice[] { new LabeledPrice() { Label = "Масленка", Amount = 6000 }, new LabeledPrice() { Amount = 20000, Label = "Масло элитное" } };
                        var price2 = new LabeledPrice[] { new LabeledPrice() { Label = "Кекнутая копия игры", Amount = 66666 }, new LabeledPrice() { Amount = -7000, Label = "Скидка тип" } };
                        api.SendInvoice(chatId, "Maslo", "Супер мега крутое масло, обеспечит вас биткоинами", "test", "1", "RUB", price1,
                            "http://belive.ru/wp-content/uploads/2016/12/slivochnoe-maslo-hranit.jpg");
                        api.SendInvoice(chatId, "Фоллаут 5", "Если вы хотите замочить пару сотен мутантов, взорвать институт и провести 260 лет игры в морозильнике, то эта игра для вас", "test", "2", "RUB", price2,
                           "https://cdn.wallpapersafari.com/26/71/LhpB9P.jpg");
                    }
                    else
                    {
                        api.SendMessage(chatId, $"Напишите пожалуйста свой отзыв!");
                        state = "opinion";
                    }
                }
                else if (!update.HasCommand("*", out param) && update.Message != null)
                    api.SendMessage(chatId, $"{update.Message.From.FirstName}, Я вас не понимаю, выберете действие из предложеных");

            }
            else if ((string)state == "chose tovar")
            {
                if (update?.Message?.SuccessfulPayment != null)
                {
                    api.SendMessage(chatId, "Благодарим за покупку)))");
                }
                if (update.CallbackQuery != null)
                {
                    if (update.CallbackQuery.Data == "1")
                    {
                        state = "chouse";
                        PrintMenu(update, api, chatId);
                    }
                    else
                    {
                        api.SendMessage(chatId, "Тогда продолжайте покупки)))");
                    }
                    api.AnswerCallbackQuery(update.CallbackQuery.Id);
                }

                if (update.Message != null)
                {
                    api.SendMessage(chatId, "Хотите перейти на шаг назад?", null,
                        new InlineKeyboardMarkup()
                        {
                            inline_keyboard = new InlineKeyboardButton[1, 2]
                            {
                                {new InlineKeyboardButton("да","1"),new InlineKeyboardButton("нет","0") }
                            }
                        });
                }
            }
            else if ((string)state == "opinion")
            {
                state = "start";
                api.SendMessage(chatId, "Спасибо! Выш отзыв очень важен для нас!))");
                api.SendMessage(chatId, "Чтобы продолжить отправьте любое сообщение....");
                SaveOpinion(chatId, update.Message.From.FirstName, update.Message.Text);
            }
        }

        public void CommandAction(string command, string parameter, Update update, ref object state)
        {

        }
    }
}
