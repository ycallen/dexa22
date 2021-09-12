using Common;
using DM;
using Force.DeepCloner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class GetResults : Sender
    {
        //[LogExecutionTime]
        public static async Task<IDictionary<string, List<string>>>/*SearchResponse*/ Send(Candidate candidate)
        {
            var url = "https://localhost:44332/GetResults";

            var triples = candidate.Triples.DeepClone();
            foreach (var triple in triples)
            {
                triple.Subject.Value = Utils.ChangePrefix(triple.Subject.Value);
                triple.Predicate.Value = Utils.ChangePrefix(triple.Predicate.Value);
                triple.Object.Value = Utils.ChangePrefix(triple.Object.Value);
            }

            var query = new TripleQuery()
            {
                Url = url,
                Triples = triples            
            };
            IDictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            //var results = Task.Run(() => SendAsync(query)).Result;
            try
            {
                var results = await SendAsync(query, 60);

                dic = JsonConvert.DeserializeObject<IDictionary<string,List<string>>>(results);

                int i = 0;
                //foreach (var candidate in lst.Candidates)
                //{
                //    foreach (var triple in candidate.Triples)
                //    {
                //        if (triple.Predicate.Value.Contains("hasWebsite") && triple.Subject.Value.Contains("Air_China"))
                //        {
                //            int i = 0;
                //        }
                //        triple.Subject.Value = Utils.shorten(triple.Subject.Value);
                //        triple.Predicate.Value = Utils.shorten(triple.Predicate.Value);
                //        triple.Object.Value = Utils.shorten(triple.Object.Value);
                //    }
                //    foreach (var inner_candidate in candidate.Candidates)
                //    {
                //        foreach (var triple in inner_candidate.Triples)
                //        {
                //            triple.Subject.Value = Utils.shorten(triple.Subject.Value);
                //            triple.Predicate.Value = Utils.shorten(triple.Predicate.Value);
                //            triple.Object.Value = Utils.shorten(triple.Object.Value);
                //        }
                //    }
                //}
                //return lst;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return dic;

        }
    }
}
