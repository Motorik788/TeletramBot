using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Telegram.BotApi
{
    public class KeyboardButton
    {
        public string text { get; set; }
        public bool request_contact { get; set; }
        public bool request_location { get; set; }

        public KeyboardButton(string text) { this.text = text; }
    }

    public class ReplyKeyboardMarkup : IMarkup
    {
        public KeyboardButton[,] keyboard { get; set; }
        public bool resize_keyboard { get; set; }
        public bool one_time_keyboard { get; set; }
        public bool selective { get; set; }
    }

    public class InlineKeyboardButton
    {
        public string text { get; set; }
        public string callback_data { get; set; }
        public bool pay { get; set; }

        public InlineKeyboardButton(string text,string callbackData) { this.text = text;this.callback_data = callbackData; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string url { get; set; }
       
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string switch_inline_query { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string switch_inline_query_current_chat { get; set; }
    }

    public class InlineKeyboardMarkup : IMarkup
    {
        public InlineKeyboardButton[,] inline_keyboard { get; set; }
    }

    public interface IMarkup { }
}
