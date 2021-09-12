using CodeBeautify;
using DM;
using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    
    public class RetrieveSynonyms
    {
        private const string uri = "https://api.datamuse.com/words?ml=";

        public string Retrieve(Entity entity)
        {
            string ret = null;
            using (var client = new HttpClient())
            {
                //client.Timeout = TimeSpan.FromMinutes(30);
                try
                {
                    if(entity.Anchor.ToLower().Equals("type"))
                    {
                        return "";
                    }

                    var word = entity.Anchor.Underscore().Replace("_", "+");                    

                    string json = null; 
                    if (File.Exists(@"C:\thesis\sparqlit\synonyms\" + word + ".txt"))
                    {
                        json = File.ReadAllText(@"C:\thesis\sparqlit\synonyms\" + word + ".txt");
                    }
                    else
                    {
                        var result = client.GetAsync(uri + entity.Anchor.Underscore().Replace("_", "+")).Result;
                        json = result.Content.ReadAsStringAsync().Result;
                        File.WriteAllText(@"C:\thesis\sparqlit\synonyms\" + word + ".txt", json);
                    }

                    
                    var welcome6 = Welcome6.FromJson(json);
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in welcome6)
                    {
                        try
                        {
                            if (item.Tags.Length > 1)
                            {
                                if (item.Tags[0].ToString().ToLower().Equals("n")
                                    && item.Tags[1].ToString().ToLower().Equals("prop"))
                                {
                                    continue;
                                }
                            }
                            if (item.Word.ToLower().Contains("property") || item.Word.ToLower().Contains("ontology"))
                            {
                                continue;
                            }
                            sb.Append(item.Word.Replace(" ", "_") + " ");
                        }
                        catch(Exception ex)
                        {
                            continue;
                        }
                        
                    }
                    ret = sb.ToString().Trim();
                }
                catch (Exception ex)
                {

                    ret = entity.Anchor;
                }

            }
            return ret;
        }
    }
}
