using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class Utils
    {

        //yago
        public static bool IsEntity(string value)
        {
            if ((value.StartsWith("<") && value.EndsWith(">")) || value.StartsWith("dbr:") || value.StartsWith("dbo:") || value.StartsWith("dbp:") || value.StartsWith("xsd:") || value.StartsWith("rdf:") || value.StartsWith("rdfs:"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static List<string> FilterTerms(List<string> filters, List<string> terms)
        {
            if (filters == null)
            {
                return terms;
            }
            var lst = new List<string>();
            foreach (var item in terms)
            {
                if (!filters.Contains(item))
                {
                    lst.Add(item);
                }
            }
            return lst;
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
            if (new_item.Equals(""))
                new_item = str1;
            var ed = Algorithms.Levenshtein.ComputeDistance(new_item.Underscore().ToLower(), str2.Underscore().ToLower());
            return ed;
        }

        public static string ReplaceInvalidChars(string value)
        {
            var ret = value.Replace(" ", "_").Replace(".", "").Replace("'", "").Replace("’", "").Replace("[[", "").Replace("]]", "").Replace("(", "").Replace(")", "").Replace(",", "");
            return ret;
        }

        public static string ObjectTreatment(string value)
        {
            string ret = "";

            if (value.EndsWith("@en"))
            {
                var split = value.Split("@en");
                ret = string.Format("\"{0}\"@en", split[0]);
            }
            else if (value.EndsWith("@eng"))
            {
                var split = value.Split("@eng");
                ret = string.Format("\"{0}\"@eng", split[0]);
            }
            else if (value.Contains("^^"))
            {
                var split = value.Split("^^");
                string fix = null;
                if(!split[1].StartsWith("<"))
                {
                    fix = "\"" + split[0] + "\"^^" + "<" +split[1] + ">";
                }
                else
                {
                    fix = "\"" + split[0] + "\"^^" + split[1];
                }
                
                ret = string.Format("{0}", fix);
            }
            else if (Utils.IsEntity(value) || value.StartsWith("?"))
            {
                ret = value;
            }
            else
            {
                ret = string.Format("\"{0}\"", value.Replace("\\", ""));
            }
            ret = ret.Replace("\"\"", "\"");
            return ret;
        }


        public static string shorten(string uri)
        {
            ////if (uri.Contains("http://www.w3.org/2002/07/owl#"))
            ////{
            ////    uri = uri.ToString().Replace("http://www.w3.org/2002/07/owl#", "owl:");
            ////}
            ////if (uri.Contains("http://www.w3.org/1999/02/22-rdf-syntax-ns#"))
            ////{
            ////    uri = uri.ToString().Replace("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf:");
            ////}
            ////if (uri.Contains("http://www.w3.org/2000/01/rdf-schema#"))
            ////{
            ////    uri = uri.ToString().Replace("http://www.w3.org/2000/01/rdf-schema#", "rdfs:");
            ////}
            ////if (uri.Contains("http://www.w3.org/2004/02/skos/core#"))
            ////{
            ////    uri = uri.ToString().Replace("http://www.w3.org/2004/02/skos/core#", "skos:");
            ////}
            ////if (uri.Contains("http://www.w3.org/2001/XMLSchema#"))
            ////{
            ////    uri = uri.ToString().Replace("http://www.w3.org/2001/XMLSchema#", "xsd:");
            ////}

            //////dbpedia
            //if (uri.Contains("http://dbpedia.org/resource"))
            //{                
            //    uri = uri.ToString().Replace("http://dbpedia.org/resource/", "dbr:").Replace("<", "").Replace(">","");                
            //}

            //if (uri.Contains("http://dbpedia.org/ontology/"))
            //{
            //    uri = uri.ToString().Replace("http://dbpedia.org/ontology/", "dbo:").Replace("<", "").Replace(">", "");
            //}

            //if (uri.Contains("http://dbpedia.org/property/"))
            //{
            //    uri = uri.ToString().Replace("http://dbpedia.org/property/", "dbp:").Replace("<", "").Replace(">", "");
            //}

            //if (uri.Contains("http://www.w3.org/2001/XMLSchema#"))
            //{
            //    uri = uri.ToString().Replace("http://www.w3.org/2001/XMLSchema#", "xsd:").Replace("<", "").Replace(">", "");
            //}

            //if (uri.Contains("http://www.w3.org/1999/02/22-rdf-syntax-ns#"))
            //{
            //    uri = uri.ToString().Replace("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf:").Replace("<", "").Replace(">", "");
            //}

            //if (uri.Contains("http://www.w3.org/2000/01/rdf-schema#"))
            //{
            //    uri = uri.ToString().Replace("http://www.w3.org/2000/01/rdf-schema#", "rdfs:").Replace("<", "").Replace(">", "");
            //}

            //if (uri.Contains("http://yago-knowledge.org/resource/"))
            //{
            //    uri = "<" + uri.ToString().Replace("http://yago-knowledge.org/resource/", "") + ">";
            //}


            //if (uri.StartsWith("http:/"))
            //{
            //    uri = "<" + uri + ">";
            //}

            return uri;
        }
        //dbpedia
        public static bool IsEntity_dbpedia(string value)
        {
            if ((value.StartsWith("<") && value.EndsWith(">")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
