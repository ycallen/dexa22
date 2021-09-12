using Caching;
using Common;
using Config;
using DM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    //Find single triple using elastic search in the server
    public class GetCandidates : Sender
    {

        //[LogExecutionTime]
        //public static async Task<SearchResponse>/*SearchResponse*/ Send(List<Triple> triples, int? JenaLimit, int? ESLimitEntities, int? ESLimitLiterals, int? QueryTimeout, bool IsEditDistance, bool IsUnitTest, bool IsUseSynonym, bool IsEsEditDistance)
        public static async Task<SearchResponse>/*SearchResponse*/ Send(List<Triple> triples, IInteraction interaction)
        {
            var url = "https://localhost:44332/GetCandidates";

            foreach (var triple in triples)
            {
                triple.Subject.Value = Utils.ChangePrefix(triple.Subject.Value);
                triple.Predicate.Value = Utils.ChangePrefix(triple.Predicate.Value);
                triple.Object.Value = Utils.ChangePrefix(triple.Object.Value);
            }

            StringBuilder sb = new StringBuilder();
            foreach (var triple in triples)
            {
                sb.Append(triple.Subject.Value);
                sb.Append(" ");
                sb.Append(triple.Predicate.Value);
                sb.Append(" ");
                sb.Append(triple.Object.Value);
                sb.Append(" ");
                sb.Append(" .\n");
            }

            var query = new TripleQuery()
            {
                Triples = triples,
                Size = 10000,
                Url = url,
                Body = sb.ToString(),
                JenaLimit = interaction.JenaLimit,
                ESLimitEntities = interaction.ESLimitEntities,
                ESLimitLiterals = interaction.ESLimitLiterals,
                IsEditDistance = interaction.IsEditDistance,
                IsEsEditDistance = interaction.IsEsEditDistance,
                IsUnitTest = interaction.IsUnitTest,
                IsUseSynonym = interaction.IsUseSynonym,
                QueryTimeout = interaction.QueryTimeout,
                IsPartial = interaction.IsPartial
            };
            //var results = Task.Run(() => SendAsync(query)).Result;
            try
            {
                var results = await SendAsync(query, interaction.QueryTimeout);

                var lst = JsonConvert.DeserializeObject<SearchResponse>(results);
                foreach (var candidate in lst.Candidates)
                {
                    foreach (var triple in candidate.Triples)
                    {
                        triple.Subject.Value = Utils.shorten(triple.Subject.Value);
                        triple.Predicate.Value = Utils.shorten(triple.Predicate.Value);
                        triple.Object.Value = Utils.shorten(triple.Object.Value);                        
                    }
                    foreach (var inner_candidate in candidate.Candidates)
                    {
                        foreach (var triple in inner_candidate.Triples)
                        {
                            triple.Subject.Value = Utils.shorten(triple.Subject.Value);
                            triple.Predicate.Value = Utils.shorten(triple.Predicate.Value);
                            triple.Object.Value = Utils.shorten(triple.Object.Value);
                        }
                    }
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
