using Common;
using DM;
using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DAL
{
    public class TermsNonVariable : BaseQuery
    {
        public List<string> Retrieve(Triple triple, double? fieldCnt, TripleFields field, bool IsEsEditDistance)
        {        
            //var json = Client.RequestResponseSerializer.SerializeToString(sr);
            var lst = new List<string>();
            var dict = new Dictionary<String, double?>();
            var dict_ed  = new Dictionary<String, double?>();
            var numberOfPartitions = Convert.ToInt32(fieldCnt > 10000 ? Math.Ceiling((decimal)(fieldCnt / 10000)) : 1);
            for (int i = 0; i < numberOfPartitions; i++)
            {
                var sr = Criteria(triple, fieldCnt, numberOfPartitions, field, i);
                //var json = Client.RequestResponseSerializer.SerializeToString(sr);
                var result = Client.Search<Triple>(sr);
                var fieldTerms = result.Aggregations.Terms<string>("terms");
                if(fieldTerms == null)
                {
                    return lst;
                }
                foreach (var item in fieldTerms.Buckets)
                {                    
                    dict[item.Key] = (item["maximum_score"] as ValueAggregate).Value;                    
                }                
            }
            dict.Keys.ToList().ForEach(x => { dict_ed[x] = Utils.GetEditDistance(x, triple.GetFieldValue(field)); }) ;            
            
            //return answers by edit distance score
            if(IsEsEditDistance ==false)
            {
                lst = dict_ed.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            }
            //return answers by es score
            else
            {
                lst = dict.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
            }
            
            return lst; 
        }

        

        private SearchRequest Criteria(Triple triple , double? fieldCnt, int numberOfPartitions, TripleFields field, int partitionNum)
        {
            var fieldStr = (field == TripleFields.Subject) ? "subject.keyword" :
                           (field == TripleFields.Predicate) ? "predicate.keyword" : "object.keyword";

            var aggs = new TermsAggregation("terms")
            {
                Field = fieldStr,
                Include = new TermsInclude(partitionNum, numberOfPartitions),
                ShardSize = 10000,  
                Size = 10000,
                ExecutionHint = TermsAggregationExecutionHint.Map,
                Order = new List<TermsOrder>
                {
                    new TermsOrder()
                    {
                        Key = "maximum_score",
                        Order = SortOrder.Descending
                    }
                },
                Aggregations = new MaxAggregation("maximum_score", string.Empty)
                {
                    Script = new InlineScript("_score")
                }
            };

            QueryContainer queryContainer = null;


            EntityMatch(triple.Subject, ref queryContainer, "subject");

            EntityMatch(triple.Predicate, ref queryContainer, "predicate");

            EntityMatch(triple.Object, ref queryContainer, "object");

           

            if (triple.IsLiteral != null)
            {
                queryContainer &= new MatchQuery()
                {
                    Field = "is_literal",
                    Query = triple.IsLiteral.ToString().ToLower(),
                    Operator = Operator.And
                };
            }


            var sr = new SearchRequest
            {
                Size = 0,                
                Query = queryContainer,
                Aggregations = aggs
            };

            return sr;
        }
    }
}
