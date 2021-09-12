using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common
{
    public static class Utils
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool IsLog { get; set; } = false;

        public static Guid Guid { get; set; }
        //public static string Header { get; set; }

        public static Dictionary<string, string> Header { get; set; }

        public static string HeaderDbpedia { get; set; }

        public static string HeaderYago { get; set; }

        public static string DefaultQuery { get; set; }

        static Utils()
        {
            HeaderYago = "base <http://yago-knowledge.org/resource/>" + '\n' +
                           "prefix dbp: <http://dbpedia.org/ontology/>" + '\n' +
                           "prefix owl: <http://www.w3.org/2002/07/owl#>" + '\n' +
                           "prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>" + '\n' +
                           "prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>" + '\n' +
                           "prefix skos: <http://www.w3.org/2004/02/skos/core#>" + '\n' +
                           "prefix xsd: <http://www.w3.org/2001/XMLSchema#>";

            /*Header = "base <http://yago-knowledge.org/resource/>" + '\n' +
                     "prefix dbo: <http://dbpedia.org/ontology/>" + '\n' +
                     "prefix dbp: <http://dbpedia.org/property/>" + '\n' +
                     "prefix owl: <http://www.w3.org/2002/07/owl#>" + '\n' +
                     "prefix dbr: <http://dbpedia.org/resource/>" + '\n' +
                     "prefix xsd: <http://www.w3.org/2001/XMLSchema#>" + '\n' +
                     "prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>" + '\n' +
                     "prefix skos: <http://www.w3.org/2004/02/skos/core#>" + '\n' +
                     "prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>";*/


            Header = new Dictionary<string, string>()
            {
                {"base", "http://yago-knowledge.org/resource/" },
                {"dbo", "http://dbpedia.org/ontology/" },
                {"dbp", "http://dbpedia.org/property/" },
                {"owl", "http://www.w3.org/2002/07/owl#" },
                {"dbr", "http://dbpedia.org/resource/" },
                {"xsd", "http://www.w3.org/2001/XMLSchema#" } ,
                {"rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" } ,
                {"skos", "http://www.w3.org/2004/02/skos/core#" } ,
                {"rdfs", "http://www.w3.org/2000/01/rdf-schema#" } ,
                {"wiki", "http://en.wikipedia.org/wiki/" },
                {"foaf", "http://xmlns.com/foaf/0.1/" }
            };




            DefaultQuery = @"Select *" + '\n' +
                            "{" + '\n' +
                                "  ?x lives_in india ." + '\n' +
                                "  columbia has_graduate ?x" + '\n' +
                            "}";

            //DefaultQuery = @"Select *" + '\n' +
            //                 "{" + '\n' +
            //                     "  ?x 'lives_in' 'united_kingdom'." + '\n' +
            //                     "  ?x 'won' 'turing_award' ." + '\n' +
            //                     "  ?x 'advisor' ?y" + '\n' +
            //                 "}";



            //DefaultQuery = @"SELECT ?x" + '\n' +
            //                "WHERE" + '\n' +
            //                "{" + '\n' +
            //                  "?x \"capital\" \"australia\" ." + '\n' +
            //                  "?x \"population\" ?y" + '\n' +
            //                "}";

            //DefaultQuery = @"SELECT ?x " + '\n' +
            //                "WHERE" + '\n' +
            //                "{" + '\n' +
            //                "?x \"wrote\" \"fault_in_our_stars\" ." + '\n' +
            //                "?x \"wrote\" ?y" + '\n' +
            //                "}";


            //DefaultQuery = @"SELECT ?x" + '\n' +
            //                "WHERE" + '\n' +
            //                "{" + '\n' +
            //                  "?x \"label\" \"friends\" ." + '\n' +
            //                  "?x \"number_of_episodes\" ?y" + '\n' +
            //                "}";

            //DefaultQuery = @"SELECT *" + '\n' +
            //                 "WHERE" + '\n' +
            //                 "{" + '\n' +
            //                 "?x \"is_a\" \"chess_player\" ." + '\n' +
            //                 "?x \"born\" ?y ." + '\n' +
            //                 "?x \"died\" ?y" + '\n' +
            //                 "}";

        }

        /*public static Dictionary<string, string> ReverseMapping(Dictionary<string, List<string>> FieldPositions)
        {
            var kvps = from kvp in FieldPositions
                       from value in kvp.Value
                       select new { Key = kvp.Key, Value = value };

            // Turns the sequence into a dictionary, with the old 'Value' as the new 'Key'
            var reverse = kvps.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            return reverse;
        }*/

        public static string GetHeader()
        {
            var sb = new StringBuilder();
            foreach (var key in Header.Keys)
            {
                var line = key.Equals("base") ? String.Format("base <{0}>", Header[key])
                                              : String.Format("prefix {0}: <{1}>", key, Header[key]);
                sb.AppendLine(line);
            }
            var ret = sb.ToString();
            return ret;
        }

        public static bool IsEntity(string value)
        {
            foreach (var key in Header.Keys)
            {
                string prefix = String.Format("{0}:", key);
                if (value.StartsWith(prefix))
                    return true;
            }

            if (value.StartsWith("<") && value.EndsWith(">"))
            {
                return true;
            }            
            else
            {
                return false;
            }
        }

        public static string ChangePrefix(string value)
        {
            string ret = value;
            foreach (var key in Header.Keys)
            {
                string prefix = String.Format("{0}:", key);
                if (value.StartsWith(prefix))
                {
                    ret = String.Format("<{0}{1}>", Header[key], value.Replace(prefix, ""));
                    break;
                }
                if (value.Contains("^^") && value.Contains(key))
                {
                    var split = value.Split("^^");
                    ret = String.Format("{0}^^<{1}>", split[0], split[1].Replace(prefix, Header[key]));
                }
            }
            return ret;
        }

        public static int GetEditDistance(string str1, string str2)
        {
            var new_item = "";
            if (str1.Contains("@"))
                new_item = str1.Split("@").First();
            if (str1.Contains("^^"))
                new_item = str1.Split("^^").First();
            if (str1.StartsWith("<"))
                new_item = str1.Split('/').Last().Replace(">", "");
            if (str1.Contains(":"))
                new_item = str1.Split(":").Last();
            if (new_item.Equals(""))
                new_item = str1;
            var ed = Algorithms.Levenshtein.ComputeDistance(new_item.Underscore().ToLower(), str2.Underscore().ToLower());            
            return ed;
        }
        public static string shorten(string uri)
        {             
            foreach (var key in Header.Keys)
            {
                string path = Header[key];
                string prefix = String.Format("{0}:", key);                
                if (uri.Contains(path))
                {
                    //literal with type
                    if(uri.Contains("^^"))
                    {
                        var split = uri.Split("^^");
                        if(key.Equals("base"))
                        {
                            uri = String.Format("{0}^^<{1}>", split[0], split[1].Replace(path, ""));
                        }
                        else
                        {
                            uri = String.Format("{0}^^{1}", split[0], split[1].Replace(path, prefix)).Replace("<","").Replace(">","");
                        }                        
                    }
                    else
                    {
                        uri = key.Equals("base") ? String.Format("{0}", uri.Replace(path, ""))
                                             : uri.Replace(path, prefix).Replace("<", "").Replace(">", "");
                    }                    
                }
            }
            return uri;
        }



        public static void Log(string message)
        {
            if (IsLog)
            {
                logger.Info(message);
            }
        }


    }
}
