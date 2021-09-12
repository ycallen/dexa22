using DM;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL
{
    public abstract class BaseQuery
    {
        public ElasticClient Client { get; set; }
        public BaseQuery()
        {
            var node = new Uri("http://localhost:9201");
            var settings = new ConnectionSettings(node);
            Client = new ElasticClient(settings);
        }

        protected void EntityMatch(Entity entity, ref QueryContainer queryContainer, string fieldName)
        {
            if(entity.Terms!=null && entity.Terms.Count > 0)
            {
                queryContainer &= new TermsQuery()
                {
                    Field = fieldName + ".keyword",
                    Terms = entity.Terms
                };
            }
            else if (entity.Value.StartsWith("<"))
            {
                queryContainer &= new MatchQuery()
                {
                    Field = fieldName + ".keyword",
                    Query = entity.Value
                };
            }
            else if(!String.IsNullOrEmpty(entity.Synonyms))
            {                
                if (!String.IsNullOrEmpty(entity.Synonyms))
                {
                    var containerOr = new QueryContainer();
                    
                    containerOr |= new MatchQuery()
                    {
                        Field = fieldName,
                        Query = entity.Anchor,
                        Operator = Operator.And
                    };

                    foreach (var item in entity.Synonyms.Split(" "))
                    {
                        containerOr |= new MatchQuery()
                        {
                            Field = fieldName,
                            Query = item,
                            Operator = Operator.And
                        };
                    }
                    queryContainer &= containerOr;
                }
            }
            else if (!entity.Value.StartsWith("?"))
            {                
                queryContainer &= new MatchQuery()
                {
                    Field = fieldName,
                    Query = entity.Value,
                    Operator = Operator.And
                };                    
            }

            if (entity.Filters != null && !entity.Value.StartsWith("<"))
            {
                queryContainer &= !new TermsQuery()
                {
                    Field = fieldName + ".keyword",
                    Terms = entity.Filters
                };
            }

        }
    }
}
