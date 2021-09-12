using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace Config
{
    public class YamlConfig
    {
        public string UserQuery { get; set; }

        public string UserQueryDBpedia { get; set; }

        public string CorrectQuery { get; set; }

        public string CorrectAnswer { get; set; }
       
        public string Description { get; set; }
       

        public static YamlConfig load(string filePath, string KB)
        {
            var model = new YamlConfig();

            using (var reader = new System.IO.StreamReader(filePath))
            {
                var yaml = new YamlDotNet.RepresentationModel.YamlStream();
                yaml.Load(reader);

                YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                foreach (var entry in mapping.Children)
                {
                    if (entry.Key.ToString().ToLower().Equals("userquery"))
                    {
                        model.UserQuery = entry.Value.ToString();
                    }
                    if (entry.Key.ToString().ToLower().Equals("userquery_dbpedia"))
                    {
                        model.UserQueryDBpedia = entry.Value.ToString();
                    }
                    
                    else if (entry.Key.ToString().ToLower().Equals("correctquery_" + KB))
                    {
                        model.CorrectQuery = entry.Value.ToString();
                    }
                    else if (entry.Key.ToString().ToLower().Equals("correctanswer"))
                    {
                        model.CorrectAnswer = entry.Value.ToString();
                    }
                    else if (entry.Key.ToString().ToLower().Equals("description"))
                    {
                        model.Description = entry.Value.ToString();
                    }

                }
            }
            return model;
        }
    }
}
