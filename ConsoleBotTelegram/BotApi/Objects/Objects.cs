using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Telegram.BotApi.Objects
{
    public abstract class TelegramObject
    {
        public abstract void Parse(JToken obj);

        public static T GetObj<T>(JToken obj) where T : TelegramObject, new()
        {
            if (obj != null)
            {
                var ob = new T();
                ob.Parse(obj);
                return ob;
            }
            return null;
        }

        public override string ToString()
        {
            return Serialize(this);
        }

        public static string Serialize<T>(T obj) where T : TelegramObject
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }

    public abstract class TelegramMediaObject : TelegramObject
    {
        public string FileId { get; protected set; }
        public string MIME { get; protected set; }
        public int FileSize { get; protected set; }

        public override void Parse(JToken obj)
        {
            FileId = (string)obj["file_id"];
            MIME = (string)obj["mime_type"];
            FileSize = (int)obj["file_size"];
        }
    }

    public sealed class User : TelegramObject
    {
        public int Id { get; private set; }
        public bool IsBot { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Username { get; private set; }

        public override void Parse(JToken obj)
        {
            var res = obj;
            Id = (int)res["id"];
            IsBot = (bool)res["is_bot"];
            FirstName = (string)res["first_name"];
            LastName = (string)res["last_name"];
            Username = (string)res["username"];
        }
    }

    public sealed class Chat : TelegramObject
    {
        public int Id { get; private set; }
        public string Type { get; private set; }
        public string Title { get; private set; }
        public string Username { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Description { get; private set; }
        public ChatPhoto Photo { get; private set; }

        public override void Parse(JToken obj)
        {
            var res = obj;
            Id = (int)res["id"];
            Type = (string)res["type"];
            Title = (string)res["title"];
            Username = (string)res["username"];
            FirstName = (string)res["first_name"];
            LastName = (string)res["last_name"];
            Description = (string)res["description"];
            Photo = GetObj<ChatPhoto>(res["photo"]);
        }
    }

    public sealed class ChatPhoto : TelegramObject
    {
        public string SmallFileId { get; private set; }
        public string BigFileId { get; private set; }

        public override void Parse(JToken obj)
        {
            var res = obj;
            SmallFileId = (string)res["small_file_id"];
            BigFileId = (string)res["big_file_id"];
        }
    }

    public class PhotoSize : TelegramMediaObject
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public override void Parse(JToken obj)
        {
            base.Parse(obj);
            Width = (int)obj["width"];
            Height = (int)obj["height"];
        }
    }

    public sealed class Video : PhotoSize
    {
        public int Duration { get; private set; }
        public PhotoSize Thumb { get; private set; }

        public override void Parse(JToken obj)
        {
            base.Parse(obj);
            Duration = (int)obj["duration"];
            Thumb = GetObj<PhotoSize>(obj["thumb"]);
        }
    }

    public sealed class Audio : TelegramMediaObject
    {
        public int Duration { get; private set; }
        public string Performer { get; private set; }
        public string Title { get; private set; }

        public override void Parse(JToken obj)
        {
            base.Parse(obj);
            Duration = (int)obj["duration"];
            Performer = (string)obj["performer"];
            Title = (string)obj["title"];
        }
    }

    public sealed class Document : TelegramMediaObject
    {
        public PhotoSize Thumb { get; private set; }
        public string FileName { get; private set; }

        public override void Parse(JToken obj)
        {
            base.Parse(obj);
            FileName = (string)obj["file_name"];
            Thumb = GetObj<PhotoSize>(obj["thumb"]);
        }
    }

    public sealed class Contract : TelegramObject
    {
        public string PhoneNumber { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public int UserId { get; private set; }

        public override void Parse(JToken obj)
        {
            PhoneNumber = (string)obj["phone_number"];
            FirstName = (string)obj["first_name"];
            LastName = (string)obj["last_name"];
            UserId = (int)obj["user_id"];
        }
    }

    public sealed class Location : TelegramObject
    {
        public float Longitude { get; private set; }
        public float Latitude { get; private set; }

        public override void Parse(JToken obj)
        {
            Latitude = (float)obj["latitude"];
            Longitude = (float)obj["longitude"];
        }
    }

    public sealed class File : TelegramObject
    {
        public string FileId { get; private set; }
        public int FileSize { get; private set; }
        public string FilePath { get; private set; }

        public override void Parse(JToken obj)
        {
            FileId = (string)obj["file_id"];
            FilePath = (string)obj["file_path"];
            FileSize = (int)obj["file_size"];
        }
    }

    public sealed class MessageEntity : TelegramObject
    {
        public enum EntityType
        {
            mention,
            hashtag,
            bot_command,
            url,
            email,
            bold,
            italic,
            code,
            pre,
            text_link,
            text_mention
        }

        public EntityType Type { get; private set; }
        public int Offset { get; private set; }
        public int Lenght { get; private set; }
        public string URL { get; private set; }
        public User User { get; private set; }

        public override void Parse(JToken obj)
        {
            Offset = (int)obj["offset"];
            Lenght = (int)obj["length"];
            URL = (string)obj["url"];
            User = GetObj<User>(obj["user"]);
            Type = (EntityType)Enum.Parse(typeof(EntityType), (string)obj["type"]);
        }
    }

    public sealed class Message : TelegramObject
    {
        public int MessageId { get; private set; }
        public User From { get; private set; }
        public int Date { get; private set; }
        public Chat Chat { get; private set; }
        public string Text { get; private set; }
        public Audio Audio { get; private set; }
        public Document Document { get; private set; }
        public Video Video { get; private set; }
        public PhotoSize[] Photo { get; private set; }
        public string Caption { get; private set; }
        public Contract Contract { get; private set; }
        public Location Location { get; private set; }
        public MessageEntity[] Entities { get; private set; }
        public Message ReplyToMessage { get; private set; }
        public SuccessfulPayment SuccessfulPayment { get; private set; }
        public Invoice Invoice { get; private set; }

        public override void Parse(JToken obj)
        {
            MessageId = (int)obj["message_id"];
            From = GetObj<User>(obj["from"]);
            Date = (int)obj["date"];
            Chat = GetObj<Chat>(obj["chat"]);
            Text = (string)obj["text"];
            Audio = GetObj<Audio>(obj["audio"]);
            Document = GetObj<Document>(obj["document"]);
            Video = GetObj<Video>(obj["video"]);
            Caption = (string)obj["caption"];
            Contract = GetObj<Contract>(obj["contract"]);
            Location = GetObj<Location>(obj["location"]);
            ReplyToMessage = GetObj<Message>(obj["reply_to_message"]);
            SuccessfulPayment = GetObj<SuccessfulPayment>(obj["successful_payment"]);
            Invoice = GetObj<Invoice>(obj["invoice"]);
            var photosJ = obj["photo"];
            int i = 0;
            if (photosJ != null)
            {
                Photo = new PhotoSize[photosJ.Count()];
                foreach (var item in photosJ)
                {
                    Photo[i] = new PhotoSize();
                    Photo[i] = GetObj<PhotoSize>(item);
                    i++;
                }
            }
            var entitys = obj["entities"];
            if (entitys != null)
            {
                i = 0;
                Entities = new MessageEntity[entitys.Count()];
                foreach (var item in entitys)
                {
                    Entities[i] = new MessageEntity();
                    Entities[i] = GetObj<MessageEntity>(item);
                    i++;
                }
            }
        }
    }

    public sealed class CallbackQuery : TelegramObject
    {
        public string Id { get; private set; }
        public User From { get; private set; }
        public Message Message { get; private set; }
        public string InlineMessageId { get; private set; }
        public string Data { get; private set; }

        public override void Parse(JToken obj)
        {
            Id = (string)obj["id"];
            From = GetObj<User>(obj["from"]);
            Message = GetObj<Message>(obj["message"]);
            InlineMessageId = (string)obj["inline_message_id"];
            Data = (string)obj["data"];
        }
    }

    public sealed class InlineQuery : TelegramObject
    {
        public string Id { get; private set; }
        public User From { get; private set; }
        public Location Location { get; private set; }
        public string Query { get; private set; }
        public string Offset { get; private set; }

        public override void Parse(JToken obj)
        {
            Id = (string)obj["id"];
            From = GetObj<User>(obj["from"]);
            Location = GetObj<Location>(obj["location"]);
            Query = (string)obj["query"];
            Offset = (string)obj["offset"];
        }
    }

    public sealed class LabeledPrice : TelegramObject
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        public override void Parse(JToken obj)
        {
            Label = (string)obj["label"];
            Amount = (int)obj["amount"];
        }
    }

    public sealed class Invoice : TelegramObject
    {
        public string Title { get; set; }
        public string Description { get; set; }
        [JsonProperty(PropertyName = "start_parameter")]
        public string StartParameter { get; set; }
        public string Currency { get; set; }
        [JsonProperty(PropertyName = "total_amount")]
        public int TotalAmount { get; set; }

        public override void Parse(JToken obj)
        {
            Title = (string)obj["title"];
            Description = (string)obj["description"];
            StartParameter = (string)obj["start_parameter"];
            Currency = (string)obj["currency"];
            TotalAmount = (int)obj["total_amount"];
        }
    }

    public sealed class OrderInfo : TelegramObject
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "phone_number")]
        public string PhoneNumber { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        public override void Parse(JToken obj)
        {
            Name = (string)obj["name"];
            PhoneNumber = (string)obj["phone_number"];
            Email = (string)obj["email"];
        }
    }

    public sealed class ShippingQuery : TelegramObject
    {
        public string Id { get; private set; }
        public User From { get; private set; }
        public string InvoicePayload { get; private set; }

        public override void Parse(JToken obj)
        {
            Id = (string)obj["id"];
            From = GetObj<User>(obj["from"]);
            InvoicePayload = (string)obj["invoice_payload"];
        }
    }

    public sealed class PreCheckoutQuery : TelegramObject
    {
        public string Id { get; private set; }
        public User From { get; private set; }
        public string Currency { get; private set; }
        public float TotalAmount { get; private set; }
        public string InvoicePayload { get; private set; }
        public OrderInfo OrderInfo { get; private set; }

        public override void Parse(JToken obj)
        {
            Id = (string)obj["id"];
            From = GetObj<User>(obj["from"]);
            Currency = (string)obj["currency"];
            TotalAmount = float.Parse((string)obj["total_amount"]) / 100f;
            InvoicePayload = (string)obj["invoice_payload"];
            OrderInfo = GetObj<OrderInfo>(obj["order_info"]);
        }
    }

    public sealed class SuccessfulPayment : TelegramObject
    {
        public string Currency { get; private set; }
        public float TotalAmount { get; private set; }
        public string InvoicePayload { get; private set; }
        public OrderInfo OrderInfo { get; private set; }
        public string TelegramPaymentChargeId { get; private set; }
        public string ProviderPaymentChargeId { get; private set; }

        public override void Parse(JToken obj)
        {
            Currency = (string)obj["currency"];
            TotalAmount = float.Parse((string)obj["total_amount"]) / 100f;
            InvoicePayload = (string)obj["invoice_payload"];
            OrderInfo = GetObj<OrderInfo>(obj["order_info"]);
            TelegramPaymentChargeId = (string)obj["telegram_payment_charge_id"];
            ProviderPaymentChargeId = (string)obj["provider_payment_charge_id"];
        }
    }

    public sealed class Update : TelegramObject
    {
        public int UpdateId { get; private set; }
        public Message Message { get; private set; }
        public Message EditedMessage { get; private set; }
        public Message ChannelPost { get; private set; }
        public Message EditedChannelPost { get; private set; }
        public InlineQuery InlineQuery { get; private set; }
        public CallbackQuery CallbackQuery { get; private set; }
        public ShippingQuery ShippingQuery { get; private set; }
        public PreCheckoutQuery PreCheckoutQuery { get; private set; }
        public string ChosenInlineResult { get; private set; }

        /// <summary>
        /// Возвращает true, если это указанная команда и присваивает параметр, если имеется. Есть возможность указать маску * - означает любую команду
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool HasCommand(string command, out string parameter)
        {
            var commEntity = Message?.Entities?.FirstOrDefault(x => x.Type == MessageEntity.EntityType.bot_command);

            if (commEntity != null)
            {
                var commText = Message.Text.Split(' ');
                var comm = commText[0].Trim().Remove(0, 1);
                if (comm == command || command == "*")
                {
                    parameter = commText.Length > 1 ? commText[1].Trim() : null;
                    return true;
                }
            }

            parameter = null;
            return false;
        }

        public string GetCommandOrNull(out string commandParameter)
        {
            var commEntity = Message?.Entities?.FirstOrDefault(x => x.Type == MessageEntity.EntityType.bot_command);

            if (commEntity != null)
            {
                var commText = Message.Text.Split(' ');
                var comm = commText[0].Trim().Remove(0, 1);
                if (!string.IsNullOrEmpty(comm))
                {
                    commandParameter = commText.Length > 1 ? commText[1].Trim() : null;
                    return comm;
                }
            }

            commandParameter = null;
            return null;
        }

        public override void Parse(JToken obj)
        {
            UpdateId = (int)obj["update_id"];
            Message = GetObj<Message>(obj["message"]);
            EditedMessage = GetObj<Message>(obj["edited_message"]);
            ChannelPost = GetObj<Message>(obj["channel_post"]);
            EditedChannelPost = GetObj<Message>(obj["edited_channel_post"]);
            InlineQuery = GetObj<InlineQuery>(obj["inline_query"]);
            CallbackQuery = GetObj<CallbackQuery>(obj["callback_query"]);
            ShippingQuery = GetObj<ShippingQuery>(obj["shipping_query"]);
            PreCheckoutQuery = GetObj<PreCheckoutQuery>(obj["pre_checkout_query"]);
        }
    }
}
