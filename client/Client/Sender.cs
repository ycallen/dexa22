using Common;
using Config;
using DM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public abstract class Sender
    { 
        protected static async Task<String> SendAsync(TripleQuery query, int? QueryTimeout = 300)
        {
            var queryJson = Newtonsoft.Json.JsonConvert.SerializeObject(query);
            var uri = new Uri(query.Url);
            var strContent = new StringContent(queryJson);
            strContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            strContent.Headers.ContentType.CharSet = "";
            using (var client = new HttpClient())
            using (var content = strContent)
            {
                //client.Timeout = TimeSpan.FromMinutes(30);
                try
                {
                    client.Timeout = TimeSpan.FromSeconds((double)QueryTimeout);
                    var result = await client.PostAsync(uri, content);
                    var ret = result.Content.ReadAsStringAsync();
                    return ret.Result;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                
            }

        }
    }
}
