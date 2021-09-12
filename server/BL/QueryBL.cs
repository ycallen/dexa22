using DAL;
using DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

namespace BL
{
    public class QueryBL
    {

        public Dictionary<string,List<string>> GetResultsFromQuery(TripleQuery query)
        {
            var res = new Retriever().RetrieveResults(query.Header,query.Triples);
            return res;
        }       

        public List<DM.Triple> ExtractTriplesFromQuery(TripleQuery query) 
        {

            var response = new List<DM.Triple>();

            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Extended);
            
            parser.QueryOptimiser = new VDS.RDF.Query.Optimisation.NoReorderOptimiser();

            SparqlQuery q = null;

            try
            {
                q = parser.ParseFromString(query.Sparql);

            }
            catch(Exception ex)
            {
                q = parser.ParseFromString(EnloseElementsQuotes(query.Sparql));                                
            }
                       

            var triples = q.RootGraphPattern.TriplePatterns;

            int cnt = 0;

            foreach (var triple in triples)
            {
                var ntriple = new DM.Triple();
                ntriple.Subject = new Entity();
                ntriple.Predicate = new Entity();
                ntriple.Object = new Entity();

                ntriple.Subject.Value = (triple as TriplePattern).Subject.ToString();
                ntriple.Predicate.Value = (triple as TriplePattern).Predicate.ToString();
                ntriple.Object.Value = (triple as TriplePattern).Object.ToString();

                if(!ntriple.Object.Value.StartsWith("?"))
                {
                    var node = ((triple as TriplePattern).Object as NodeMatchPattern).Node;
                    ntriple.Object.Value = node.ToString();
                    if (node.NodeType == NodeType.Uri)
                    {
                        ntriple.Object.Value = "<" + ntriple.Object.Value + ">";
                    }
                }
                

                ntriple.Index = cnt++;
                response.Add(ntriple); 
            }

            return response;
        }

        private string EnloseElementsQuotes(string queryStr)
        {
            var sb = new StringBuilder();
            var splitBraces = queryStr.Split('{', '}');

            sb.Append(splitBraces[0]);
            sb.Append("{");
            var splitSpace = splitBraces[1].Replace('\n', ' ').Split(' ',  StringSplitOptions.RemoveEmptyEntries);
            foreach (var split in splitSpace)
            {
                var b = Char.IsLetter(split.FirstOrDefault()) || Char.IsDigit(split.FirstOrDefault());
                if (b == true)
                {
                    sb.Append(String.Format("'{0}' ", split));
                }
                else
                {
                    sb.Append(String.Format("{0} ", split));
                }

                if(split.Equals("."))
                {
                    sb.AppendLine();
                }
            }
            sb.Length--;
            sb.Append("}");
            return sb.ToString();
        }


    }
}
