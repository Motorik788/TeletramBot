using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Telegram.BotApi.Objects;

namespace Telegram.BotApi
{
    public enum RequestType
    {
        Get,
        Post
    }

    public class API
    {
        public User Info { get; private set; }

        readonly string token;
        readonly string uri;
        readonly string providerToken;

        public API(string token, string provider_token)
        {
            this.token = token;
            providerToken = provider_token;
            uri = $"https://api.telegram.org/bot{token}/";
            try
            {
                var me = GetMe();
                Info = me;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string ToQueryString(IEnumerable<KeyValuePair<string, string>> param)
        {          
            if (param == null || param.Count() == 0)
                return "";
            string query = "?";
            for (int i = 0; i < param.Count() - 1; i++)
            {
                query += $"{param.ElementAt(i).Key}={param.ElementAt(i).Value}&";
            }
            query += $"{param.ElementAt(param.Count() - 1).Key}={param.ElementAt(param.Count() - 1).Value}";
            return query;
        }

        public static string ToJson(IEnumerable<KeyValuePair<string, string>> param)
        {
            if (param == null || param.Count() == 0)
                return "";
            JObject job = new JObject();
            foreach (var item in param)
            {
                job.Add(item.Key, (item.Key != "reply_markup" ? item.Value : JToken.Parse(item.Value)));
            }
            return job.ToString();
        }

        public Update[] GetUpdates(int? offset = null, int? limit = null, int? timeout = null)
        {
            var query = new List<KeyValuePair<string, string>>();
            if (offset.HasValue)
                query.Add(new KeyValuePair<string, string>("offset", offset.Value.ToString()));
            if (limit.HasValue)
                query.Add(new KeyValuePair<string, string>("limit", limit.Value.ToString()));
            if (timeout.HasValue)
                query.Add(new KeyValuePair<string, string>("timeout", timeout.Value.ToString()));

            var resp = Send("getUpdates", query).Result;
            Update[] updates = new Update[resp.Count()];
            int i = 0;
            foreach (var item in resp)
            {
                // Console.WriteLine(item);
                updates[i] = new Update();
                updates[i].Parse(item);
                //Console.WriteLine(updates[i]);
                i++;
            }
            return updates;
        }

        public Message SendMessage(string chatId, string text, int? replyToMessageId = null, IMarkup reply_markup = null)
        {
            var query = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("chat_id", chatId),
                new KeyValuePair<string, string>("text", text)
            };
            if (replyToMessageId.HasValue)
            {
                query.Add(new KeyValuePair<string, string>("reply_to_message_id", replyToMessageId.Value.ToString()));
            }
            if (reply_markup != null)
            {
                var ser = JsonConvert.SerializeObject(reply_markup);
                query.Add(new KeyValuePair<string, string>("reply_markup", ser));
            }
            Message m = new Message();
            var v = Send("sendMessage", query, RequestType.Post).Result;
            //Console.WriteLine(v);
            m.Parse(v);
            return m;
        }

        public void SendPhoto(string chatId, string photoPath, string caption = "", bool isLocalFile = false, int? replyToMessageId = null, IMarkup reply_markup = null)
        {
            var query = new List<KeyValuePair<string, HttpContent>>()
            {
                new KeyValuePair<string, HttpContent>("chat_id", new StringContent(chatId)),
                new KeyValuePair<string, HttpContent>("caption",new StringContent(caption))
            };

            var r = SendFile("photo", "sendPhoto", photoPath, query, isLocalFile, replyToMessageId, reply_markup);
        }

        public void SendDocument(string chatId, string docPath, string caption = "", bool isLocal = false, int? replyToMessageId = null, IMarkup reply_markup = null)
        {
            var query = new List<KeyValuePair<string, HttpContent>>()
            {
                new KeyValuePair<string, HttpContent>("chat_id", new StringContent(chatId)),
                new KeyValuePair<string, HttpContent>("caption",new StringContent(caption))
            };

            SendFile("document", "sendDocument", docPath, query, isLocal, replyToMessageId, reply_markup);
        }

        public void SendInvoice(string chatId, string title, string description, string payload, string start_parameter,
            string curr, LabeledPrice[] price, string photo_url = null, bool need_name = false, bool need_email = false, bool need_phone_number = false, string provider_token = null, int? replyToMessageId = null, IMarkup reply_markup = null)
        {
            if (string.IsNullOrEmpty(provider_token) && !string.IsNullOrEmpty(providerToken))
                provider_token = providerToken;
            else
                throw new Exception("Необходим provider_token!!");
            var query = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("chat_id", chatId),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("description", description),
                new KeyValuePair<string, string>("payload", payload),
                new KeyValuePair<string, string>("provider_token", provider_token),
                new KeyValuePair<string, string>("start_parameter", start_parameter),
                new KeyValuePair<string, string>("currency",  curr),
                new KeyValuePair<string, string>("prices",JsonConvert.SerializeObject(price)),
                new KeyValuePair<string, string>("need_name",need_name.ToString()),
                new KeyValuePair<string, string>("need_email",need_email.ToString()),
                new KeyValuePair<string, string>("need_phone_number",need_phone_number.ToString())
            };
            if (!string.IsNullOrEmpty(photo_url))
                query.Add(new KeyValuePair<string, string>("photo_url", photo_url));
            if (replyToMessageId.HasValue)
                query.Add(new KeyValuePair<string, string>("reply_to_message_id", replyToMessageId.Value.ToString()));
            if (reply_markup != null)
            {
                var ser = JsonConvert.SerializeObject(reply_markup);
                query.Add(new KeyValuePair<string, string>("reply_markup", ser));
            }
            var v = Send("sendInvoice", query, RequestType.Post).Result;
        }

        public void AnswerPreCheckoutQuery(string pre_checkout_query_id, bool ok, string error_message = "")
        {
            var query = new List<KeyValuePair<string, string>>()
            {
                 new KeyValuePair<string, string>("pre_checkout_query_id", pre_checkout_query_id),
                 new KeyValuePair<string, string>("ok", ok.ToString())
            };
            if (!string.IsNullOrEmpty(error_message))
                query.Add(new KeyValuePair<string, string>("error_message", error_message));
            var r = Send("answerPreCheckoutQuery", query, RequestType.Post);
        }

        public void AnswerCallbackQuery(string callback_query_id, string text = null, bool show_alert = false, string url = null)
        {
            var query = new List<KeyValuePair<string, string>>()
            {
                 new KeyValuePair<string, string>("callback_query_id", callback_query_id),
                 new KeyValuePair<string, string>("show_alert", show_alert.ToString())
            };
            if (!string.IsNullOrEmpty(text))
                query.Add(new KeyValuePair<string, string>("text", text));
            if (!string.IsNullOrEmpty(url))
                query.Add(new KeyValuePair<string, string>("url", url));
            var r = Send("answerCallbackQuery", query, RequestType.Post);
        }

        public File GetFile(string fileId)
        {
            var query = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("file_id", fileId) };
            File f = new File();
            f.Parse(Send("getFile", query).Result);
            return f;
        }

        public async void DownloadFile(File file, string path)
        {
            using (var client = new HttpClient())
            {
                using (var f = System.IO.File.Create(path))
                {
                    var bytes = await client.GetByteArrayAsync($"{uri}/{file.FilePath}");
                    f.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public User GetMe()
        {
            var user = new User();
            user.Parse(Send("getMe").Result);
            return user;
        }

        private JToken SendFile(string type, string method, string path, List<KeyValuePair<string, HttpContent>> query, bool isLocal = false, int? replyToMessageId = null, IMarkup reply_markup = null)
        {
            if (replyToMessageId.HasValue)
                query.Add(new KeyValuePair<string, HttpContent>("reply_to_message_id", new StringContent(replyToMessageId.Value.ToString())));
            if (reply_markup != null)
            {
                var ser = JsonConvert.SerializeObject(reply_markup);
                query.Add(new KeyValuePair<string, HttpContent>("reply_markup", new StringContent(ser)));
            }
            if (isLocal)
            {
                using (var file = System.IO.File.OpenRead(path))
                {
                    query.Add(new KeyValuePair<string, HttpContent>(type, new StreamContent(file)));
                    return SendByMultipartFormData(method, query).Result;
                }
            }
            else
            {
                query.Add(new KeyValuePair<string, HttpContent>(type, new StringContent(path)));
                return Send(method, query.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ReadAsStringAsync().Result)), RequestType.Post).Result;
            }
        }

        private async Task<JToken> SendByMultipartFormData(string method, IEnumerable<KeyValuePair<string, HttpContent>> query)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var cont = new MultipartFormDataContent();
                    //текстовые параметры добавляеются в тело ответа в цикле, а параметр файла отдельно
                    {
                        foreach (var item in query.Where(x => x.Value is StringContent))
                            cont.Add(item.Value, item.Key);
                        var g = query.First(x => x.Value is StreamContent);
                        cont.Add(g.Value, g.Key, g.Key);
                    }
                    var resp = await client.PostAsync(uri + method, cont);
                    var Jo = JObject.Parse(await resp.Content.ReadAsStringAsync());
                    if (((string)Jo["ok"]).ToLower() == "false")
                        throw new Exception((string)Jo["description"]);
                    return Jo["result"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        private async Task<JToken> Send(string method, IEnumerable<KeyValuePair<string, string>> query = null, RequestType requestType = RequestType.Get)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string resp;
                    if (requestType == RequestType.Get)
                        resp = await client.GetStringAsync(uri + method + ToQueryString(query));
                    else
                    {
                        var r = await client.PostAsync(uri + method, new StringContent(ToJson(query), Encoding.UTF8, "application/json"));
                        resp = await r.Content.ReadAsStringAsync();
                    }
                    var Jo = JObject.Parse(resp);
                    if (((string)Jo["ok"]).ToLower() == "false")
                        throw new Exception((string)Jo["description"]);
                    return Jo["result"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
