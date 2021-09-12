using DM;
using Nest;
using Elasticsearch.Net;
using System.Collections.Generic;

namespace DAL
{
    public class CardinalityNonVariables : BaseQuery
    {
        public double? Retrieve(Triple triple,TripleFields cardinalityField)
        {
            var sr = Critirea(triple, cardinalityField);
            //var json = Client.RequestResponseSerializer.SerializeToString(sr);
            var result = Client.Search<Triple>(sr);

            var predicateCount = result.Aggregations.Cardinality("cardinality_count");
            if(predicateCount == null)
                return 0;            
            else
                return predicateCount.Value;
        }


        
        private SearchRequest Critirea(Triple triple, TripleFields cardinalityField)
        {
            var cardinalityFieldStr = (cardinalityField == TripleFields.Subject) ? "subject.keyword" :
                              (cardinalityField == TripleFields.Predicate) ? "predicate.keyword" : "object.keyword";

            var agg = new CardinalityAggregation("cardinality_count", cardinalityFieldStr)
            {
                PrecisionThreshold = 100
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
                Aggregations = agg
            };

            return sr;
        }
    }
}
