using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotApi;
using Telegram.BotApi.Objects;

namespace Telegram.BotClient
{
    public static class DelegateExtention
    {
        public static bool EqualsBody(this Delegate del, Delegate del2)
        {
            var p1Body = del.Method.GetMethodBody().GetILAsByteArray();
            var p2Body = del2.Method.GetMethodBody().GetILAsByteArray();
            if (p1Body.Length == p2Body.Length)
            {
                for (int i = 0; i < p1Body.Length; i++)
                {
                    if (p1Body[i] != p2Body[i])
                        return false;
                }
                return true;
            }
            else
                return false;
        }
    }
    public class BotClient
    {
        public class CommandArgs
        {
            public readonly string Parameter;

            internal CommandArgs(string param, API api, string chatId)
            {
                Parameter = param;
                API = api;
                ChatId = chatId;
            }

            public API API { get; private set; }
            public string ChatId { get; private set; }
        }

        public readonly API API;
        public User Me => API.Info;
        public bool IsStarting => isStarting;
        public event Action<Message, API> NewMessage;
        public event Action<Update, API> NewUpdate;

        private readonly string token;
        private volatile bool requestResponseMode;
        private volatile bool useOnlyBehavior;
        private volatile bool isStarting;
        private volatile int lastUpdateId;
        private List<Chat> chats = new List<Chat>();
        private Dictionary<string, Action<CommandArgs>> commands = new Dictionary<string, Action<CommandArgs>>();
        private Dictionary<Predicate<Update>, Action<Update, API>> ConditinalEvent = new Dictionary<Predicate<Update>, Action<Update, API>>();
        private BotBehavior behavior;

        public BotClient(string token, string payProviderToken = null, bool useOnlyBehavior = false, bool requestResponseMode = false)
        {
            this.token = token;
            API = new API(token, payProviderToken);
            this.requestResponseMode = requestResponseMode;
            this.useOnlyBehavior = useOnlyBehavior;
        }

        public void SetBotBehavior(IBehavior behavior)
        {
            this.behavior = new BotBehavior(API, behavior);
        }

        public void SaveBotBehaviorState()
        {
            this.behavior.Save();
        }

        public void SendMessageAll(string text, IMarkup markup = null)
        {
            if (!requestResponseMode)
            {
                foreach (var item in chats)
                    API.SendMessage(item.Id.ToString(), text, reply_markup: markup);
            }
            else throw new InvalidOperationException("В режиме запрос-ответ невозможн отправить сообщение клиентам!");
        }

        public void SetCommandHandler(string command, Action<CommandArgs> action)
        {
            if (commands.ContainsKey(command))
                commands[command] = action;
            else
                commands.Add(command, action);
        }

        public void Start()
        {
            if (!isStarting)
            {
                isStarting = true;
                Task.Run(() =>
                {
                    while (isStarting)
                    {
                        var updates = API.GetUpdates(lastUpdateId, 1, 10);
                        if (updates != null && updates.Length > 0)
                        {
                            lastUpdateId = updates[0].UpdateId + 1;
                            Task.Run(() => { UpdateReaction(updates[0]); });
                        }
                    }
                });
            }
        }

        public void Stop(bool clearUpdates = false)
        {
            if (isStarting)
            {
                isStarting = false;
                if (clearUpdates)
                    API.GetUpdates(lastUpdateId);
            }
        }

        public void AddConditinalEventHandler(Predicate<Update> predicate, Action<Update, API> handler)
        {
            KeyValuePair<Predicate<Update>, Action<Update, API>> contains = ConditinalEvent.FirstOrDefault(x => x.Key.EqualsBody(predicate));
            if (contains.Key != null)
                ConditinalEvent[contains.Key] += handler;
            else
                ConditinalEvent.Add(predicate, handler);
        }

        private void UpdateReaction(Update update)
        {
            if (!requestResponseMode)
            {
                if (update?.Message?.Chat != null && !chats.Exists(x => x.Id == update.Message.Chat.Id))
                    chats.Add(update.Message.Chat);
            }

            //обработка команд
            var commEntity = update?.Message?.Entities?.FirstOrDefault(x => x.Type == MessageEntity.EntityType.bot_command);
            if (!useOnlyBehavior)
            {               
                if (commEntity != null)
                {
                    var commText = update.Message.Text.Split(' ');
                    var comm = commText[0].Trim().Remove(0, 1);
                    if (commands.ContainsKey(comm))
                    {
                        var bag = new CommandArgs(commText.Length > 1 ? commText[1].Trim() : null, API, update.Message.Chat.Id.ToString());
                        commands[comm](bag);
                    }
                }
            }

            if (behavior != null && !requestResponseMode)
                behavior.Run(update);
            //если класс не используется только через поведение производим события
            if (!useOnlyBehavior)
            {
                NewUpdate?.Invoke(update, API);
                foreach (var item in ConditinalEvent)
                    if (item.Key(update))
                        item.Value(update, API);
                if (commEntity == null && update.Message != null)
                {
                    NewMessage?.Invoke(update.Message, API);
                }
            }
        }
    }
}
