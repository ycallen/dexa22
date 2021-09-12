using Common;
using DM;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Optimisation;

namespace DAL
{     
    
    public class Retriever
    {
        private SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(@"http://localhost:3030/yago/query"));
        //private SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(@"http://localhost:3030/dbpedia/query"));

        private void PopulateCandidateField(Entity entity, TripleFields fieldType,SparqlResult res, StringBuilder sb)
        {
            if (entity.IsFeedback == true)
                return;            
            var field = entity.ReplaceInvalidChars().Replace("?", "");

            var res_str = System.Web.HttpUtility.UrlDecode(res[field].ToString());

            
            var val = res[field].NodeType == NodeType.Uri ? String.Format("<{0}>", res_str)
                                                                        : String.Format("{0}", res_str);
                        

            if (!entity.IsDiplayedVariable())
            {
                sb.Append(val);
            }

            entity.Value = val;


            if(fieldType == TripleFields.Predicate)
            {                
                if(entity.UriSynonyms != null && entity.UriSynonyms.Contains("<" + res[field].ToString() + ">"))
                {
                    entity.IsSynonymValue = true;
                    
                }
                entity.UriSynonyms = null;
                entity.Synonyms = null;
            }
        }

        public Dictionary<string,List<string>> RetrieveResults(string header, List<DM.Triple> triples)
        {
            endpoint.Timeout = 3600000;

            var candidate = new Candidate() { Triples = triples };
            candidate.SetQuery();
            var query = header + candidate.Query;

            var results = endpoint.QueryWithResultSet(query);
            var dic = new Dictionary<string, List<string>>();

            

            foreach (var res in results.Results)
            {
                foreach (var item in results.Variables)
                {
                    try
                    {
                        if (!dic.ContainsKey(item))
                        {
                            dic[item] = new List<string>();
                        }
                        var res_str = System.Web.HttpUtility.UrlDecode(res[item].ToString());
                        dic[item].Add(res_str);
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }                    

                }
            }

            return dic;
        }

        public (List<Candidate>, double) RetrieveCandidates(TripleQuery query, Dictionary<string, List<string>> terms)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            endpoint.Timeout = query.QueryTimeout.Value * 1000;           
            var candidates = new List<Candidate>();
            
            //Because we send don't send large queries at once
            //but break them up into parts
            var new_query = query.DeepClone();
            new_query.Triples.RemoveAll(x => x.IsSentToJena == false);
            
            
            var q = new_query.buildQuery(terms/*new_terms*/, new_query.JenaLimit);
                    
            SparqlResultSet results = null;
            try
            {
                //endpoint.Timeout = query.QueryTimeout.Value;
                results = endpoint.QueryWithResultSet(q);
            }
            catch(Exception ex)
            {
                return (candidates, 0);
            }            

            var dic = new Dictionary<string, bool>();

            SparqlResult sr = null;

            foreach (var res in results.Results)
            {
                try
                {
                    sr = res;
                    var candidate = new Candidate();                    
                    candidate.Triples = new_query.Triples.DeepClone();
                    StringBuilder sb = new StringBuilder();
                    foreach (var triple in candidate.Triples)
                    {                                                
                        PopulateCandidateField(triple.Subject,TripleFields.Subject , res, sb);
                        PopulateCandidateField(triple.Predicate, TripleFields.Predicate, res, sb);                         
                        PopulateCandidateField(triple.Object, TripleFields.Object, res, sb);
                    }

                    //var key = query.IsUnitTest ? (sb.ToString() + Guid.NewGuid().ToString())
                    //                           : sb.ToString();
                    var key = sb.ToString() + Guid.NewGuid().ToString();

                    if (!dic.ContainsKey(key))
                    {
                        dic[key] = true;
                        //candidate.SetTriplesStr();
                        candidates.Add(candidate);                        
                    }                                    
                }
                catch (Exception ex)
                {
                    continue;
                }                        
            }

            UpdateTerms(terms, candidates);

            var elapsed = stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();
            return (candidates,elapsed);
        }    

        public void UpdateTerms(Dictionary<string, List<string>> terms, List<Candidate> candidates)
        {
            var new_terms = new Dictionary<string, List<string>>();
            foreach (var candidate in candidates)
            {
                foreach (var triple in candidate.Triples)
                {
                    if (!String.IsNullOrEmpty(triple.Subject.Anchor))
                    {
                        if (terms.ContainsKey(triple.Subject.Anchor))
                        {
                            if (!new_terms.ContainsKey(triple.Subject.Anchor))
                            {
                                new_terms[triple.Subject.Anchor] = new List<string>();
                            }
                            if (!new_terms[triple.Subject.Anchor].Contains(triple.Subject.Value))
                            {
                                new_terms[triple.Subject.Anchor].Add(triple.Subject.Value);
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(triple.Predicate.Anchor))
                    {
                        if (terms.ContainsKey(triple.Predicate.Anchor))
                        {
                            if (!new_terms.ContainsKey(triple.Predicate.Anchor))
                            {
                                new_terms[triple.Predicate.Anchor] = new List<string>();
                            }
                            if (!new_terms[triple.Predicate.Anchor].Contains(triple.Predicate.Value))
                            {
                                new_terms[triple.Predicate.Anchor].Add(triple.Predicate.Value);
                            }

                        }
                    }
                    if (!String.IsNullOrEmpty(triple.Object.Anchor))
                    {
                        if (terms.ContainsKey(triple.Object.Anchor))
                        {
                            if (!new_terms.ContainsKey(triple.Object.Anchor))
                            {
                                new_terms[triple.Object.Anchor] = new List<string>();
                            }
                            if (!new_terms[triple.Object.Anchor].Contains(triple.Object.Value))
                            {
                                new_terms[triple.Object.Anchor].Add(triple.Object.Value);
                            }
                        }
                    }
                }
            }


            foreach (var item in terms.Keys.DeepClone())
            {
                if (new_terms.ContainsKey(item))
                {
                    terms[item] = new_terms[item];
                }
                else
                {
                    terms[item] = terms[item];
                }
            }
        }
    }
}
