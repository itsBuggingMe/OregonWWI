using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI.Neoregon
{
    public static class ApiGetter
    {
        public static HttpClient client = new HttpClient();

        public static async void GetString(string url, Action<string> GetResponseCallback)
        {
            HttpResponseMessage httpResponse = await client.GetAsync(url);
            if (httpResponse.IsSuccessStatusCode)
            {
                string response = await httpResponse.Content.ReadAsStringAsync();
                GetResponseCallback(response);
            }
        }



        public static async void Post(string url, string data, Action OnError)
        {
            try
            {
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(data));
            }
            catch
            {
                OnError();
            }
        }
        const string GenericDataUploadUrl = "https://highscores.neonrogue.net/uploadmap/";
        const string GenericDataDownloadUrl = "https://highscores.neonrogue.net/map/";
        const string GenericDataDeleteUrl = @"https://highscores.neonrogue.net/delmap/";

        public static void PostGenericString(string data, string tag, int attempts = 3)
        {
            if (attempts <= 0)
                return;

            Post($"{GenericDataUploadUrl}{tag}", data, () => PostGenericString(data, tag, attempts - 1));
        }

        public static async Task<int> GetObject<T>(string url, Action<T> GetResponseCallback, Action onError)
        {
            try
            {
                HttpResponseMessage httpResponse = await client.GetAsync(url);
                if (httpResponse.IsSuccessStatusCode)
                {
                    string response = await httpResponse.Content.ReadAsStringAsync();
                    var t = JsonConvert.DeserializeObject<T>(response);

                    if (t == null)
                    {
                        onError();
                        return -1;
                    }

                    GetResponseCallback(t);
                }
                else
                {
                    onError();
                    return -1;
                }
            }
            catch
            {
                onError();
                return -1;
            }

            return 0;
        }

    }
}
