using Common;
using Config;
using DM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    //Extract a all triples using rdfnet given an rdf query with multiple triples
    public class ExtractTriplesFromQuery : Sender
    {
        //[LogExecutionTime]
        public static List<Triple> Send(string queryStr)
        {
            var url = "https://localhost:44332/ExtractTriplesFromQuery";
            var query = new TripleQuery()
            {
                Body = queryStr,
                Url = url
            };

            try
            {
                var results = Task.Run(() => SendAsync(query)).Result;



                var lst = JsonConvert.DeserializeObject<List<Triple>>(results);

                foreach (var item in lst)
                {
                    item.Subject.Value = Utils.shorten(item.Subject.Value.Replace("\"", ""));
                    item.Predicate.Value = Utils.shorten(item.Predicate.Value.Replace("\"", ""));
                    item.Object.Value = Utils.shorten(item.Object.Value.Replace("\"", ""));
                }

                return lst;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
    }
}
