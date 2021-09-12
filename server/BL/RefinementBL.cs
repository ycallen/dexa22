using DAL;
using DM;
using MethodTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using System.Text;
using Common;
using Humanizer;
using VDS.RDF.Query.Optimisation;
using System.Diagnostics;
using Force.DeepCloner;

namespace BL
{
    public class RefinementBL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private void GetSynonyms(Triple triple, TripleFields field, Dictionary<string, List<string>> terms, bool IsEsEditDistance)
        {
            triple.Predicate.Synonyms = new RetrieveSynonyms().Retrieve(triple.Predicate);
            var cardinality = new CardinalityNonVariables().Retrieve(triple, TripleFields.Predicate);
            var predicates = new TermsNonVariable().Retrieve(triple, cardinality, TripleFields.Predicate, IsEsEditDistance);
            foreach (var item in predicates)
            {
                if (!terms[triple.Predicate.Value].Contains(item))
                {
                    if(triple.Predicate.Filters != null && triple.Predicate.Filters.Contains(item))
                    {
                        continue;
                    }
                    terms[triple.Predicate.Value].Add(item);
                }
            }

        }
       

        private void GetSynonyms(Triple triple, Dictionary<string, List<string>> terms, bool IsEsEditDistance)
        {
            if (!triple.Subject.Value.StartsWith("?")) //&& !triple.IsKeyWord(TripleFields.Subject) && !Utils.IsEntity(triple.Subject.Value))
            {
                GetSynonyms(triple, TripleFields.Subject, terms, IsEsEditDistance);
            }

            if (!triple.Object.Value.StartsWith("?"))//&& !triple.IsKeyWord(TripleFields.Object) && !Utils.IsEntity(triple.Object.Value))
            {
                GetSynonyms(triple, TripleFields.Object, terms, IsEsEditDistance);
            }
        }

        [Time]
        private (Dictionary<string, List<string>>, double) GetTerms(TripleQuery query, Triple triple)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var terms = new Dictionary<string, List<string>>();
            var obj = new Dictionary<string, bool>();
            
            if (!triple.Predicate.Value.StartsWith("?") && !triple.IsKeyWord(TripleFields.Predicate) && !Utils.IsEntity(triple.Predicate.Value))
            {
                var cardinality = new CardinalityNonVariables().Retrieve(triple, TripleFields.Predicate);
                var predicates = new TermsNonVariable().Retrieve(triple, cardinality, TripleFields.Predicate, query.IsEsEditDistance);
                terms[triple.Predicate.Value] = Utils.FilterTerms(triple.Predicate.Filters,predicates);                    
                if(query.IsUseSynonym == true)
                {
                    GetSynonyms(triple, terms, query.IsEsEditDistance);
                }
                    
            }
            if (!triple.Subject.Value.StartsWith("?") && !triple.IsKeyWord(TripleFields.Subject) && !Utils.IsEntity(triple.Subject.Value))
            {
                var cardinality = new CardinalityNonVariables().Retrieve(triple, TripleFields.Subject);
                    var subjects = new TermsNonVariable().Retrieve(triple, cardinality, TripleFields.Subject, query.IsEsEditDistance);
                terms[triple.Subject.Value] = Utils.FilterTerms(triple.Subject.Filters, subjects);
            }
            if (!triple.Object.Value.StartsWith("?") && !triple.IsKeyWord(TripleFields.Object) && !Utils.IsEntity(triple.Object.Value))
            {
                var cardinality = new CardinalityNonVariables().Retrieve(triple, TripleFields.Object);
                var objects = new TermsNonVariable().Retrieve(triple, cardinality, TripleFields.Object, query.IsEsEditDistance);
                terms[triple.Object.Value] = Utils.FilterTerms(triple.Object.Filters, objects); ;
                obj[triple.Object.Value] = true;
            }
                

            //When predicate starts with a question mark then we wnat to get the underlying anchors synonyms so we can rank
            //the returned predicates accordingly
            if (triple.Predicate.Value.StartsWith("?")  && (triple.Predicate.Anchor != null) && !triple.Predicate.Anchor.StartsWith("?"))
            {  
                if(query.IsUseSynonym)
                {
                    triple.Predicate.Synonyms = new RetrieveSynonyms().Retrieve(triple.Predicate);
                }                    
                var cardinality = new CardinalityNonVariables().Retrieve(triple, TripleFields.Predicate);
                var predicates = new TermsNonVariable().Retrieve(triple, cardinality, TripleFields.Predicate, query.IsEsEditDistance);
                if(query.IsUseSynonym)
                {
                    triple.Predicate.UriSynonyms = Utils.FilterTerms(triple.Predicate.Filters, predicates);
                }                    
            }
                      
            var new_terms = new Dictionary<string, List<string>>();
            foreach (var key in terms.Keys)
            {
                var take_ent = (query.ESLimitEntities != null ? query.ESLimitEntities : 100);
                if (obj.ContainsKey(key))
                {
                    var take_lit = (query.ESLimitLiterals != null ? query.ESLimitLiterals : 500);
                    var lst1 = terms[key].Where(x=>x.StartsWith("<")).Take(take_ent.Value).ToList();
                    var lst2 = terms[key].Where(x => !x.StartsWith("<") && !x.StartsWith("?")).Take(take_lit.Value).ToList();
                    new_terms[key] = lst1.Union(lst2).ToList();
                }      
                else
                {
                    new_terms[key] = terms[key].Take(take_ent.Value).ToList();
                }                
            }

            terms = new_terms;                        

            double elasped = stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();

            return (terms, elasped);
        }

        public void SetPriority(TripleQuery query)
        {            
            StringBuilder sb = new StringBuilder();
            sb.Append(query.Header);
            sb.Append("\nselect *\n");
            sb.Append("{\n");
            foreach (var triple in query.Triples)
            {
                sb.Append(/*Char.IsLetter(triple.Subject.Value.FirstOrDefault()) ?*/
                          !triple.Subject.Value.StartsWith("<") ?
                          String.Format("'{0}' ", triple.Subject.Value) :
                          String.Format("{0} ",triple.Subject.Value));

                sb.Append(/*Char.IsLetter(triple.Predicate.Value.FirstOrDefault()) ?*/
                          !triple.Predicate.Value.StartsWith("<") ?
                          String.Format("'{0}' ", triple.Predicate.Value) :
                          String.Format("{0} ", triple.Predicate.Value));

                sb.Append(/*Char.IsLetter(triple.Object.Value.FirstOrDefault()) ?*/
                          !triple.Object.Value.StartsWith("<") ?
                          String.Format("'{0}' .\n", triple.Object.Value) :
                          String.Format("{0} .\n", triple.Object.Value));

            }            
            sb.Append("}");
            var str = sb.ToString();

            SparqlQueryParser parser1 = new SparqlQueryParser(SparqlQuerySyntax.Extended);
            
            parser1.QueryOptimiser = new VDS.RDF.Query.Optimisation.DefaultOptimiser();            
            SparqlQuery q1 = null;            
            q1 = parser1.ParseFromString(str);
            var triplesReorder = q1.RootGraphPattern.TriplePatterns;


            var parser2 = new SparqlQueryParser(SparqlQuerySyntax.Extended);
            parser2.QueryOptimiser = new VDS.RDF.Query.Optimisation.NoReorderOptimiser();            

            var q2 = parser2.ParseFromString(str);
            var triplesNoReorder = q2.RootGraphPattern.TriplePatterns;

            for (int j = 0; j < triplesNoReorder.Count; j++)
            {
                for (int i = 0; i < triplesReorder.Count; i++)
                {
                    if (triplesNoReorder[j].ToString().Equals(triplesReorder[i].ToString()))
                    {
                        query.Triples[j].Priority = i;
                    }
                }
            }                                                        
        }

        public void GetNextTriple(TripleQuery query)
        {
            var variablesTrue = new List<string>();
            foreach (var triple in query.Triples.Where(x => x.IsSentToJena == true))
            {
                variablesTrue = variablesTrue.Union(triple.GetAllElements()).ToList();
            }

            foreach (var triple in query.Triples.Where(x => x.IsSentToJena == false))
            {
                var variableFalse = triple.GetAllElements();
                var intersect = variablesTrue.Intersect(variableFalse).ToList();
                if (intersect.Count > 0)
                {
                    triple.IsSentToJena = true;
                    break;
                }
            }
        }



        public List<Triple> GetNextTriples(TripleQuery query)
        {
            var ret = new List<Triple>();
            if (query.Triples.Count < 3)
            {
                foreach (var triple in query.Triples.OrderBy(x => x.Priority))
                {
                    triple.IsSentToJena = true;
                }
                ret = query.Triples;
            }
            else if(query.Triples.Where(x => x.IsSentToJena == true).Count() == 0)
            {
                var index = 0;
                foreach (var triple in query.Triples.OrderBy(x => x.Priority))
                {
                    triple.IsSentToJena = true;
                    index++;
                    ret.Add(triple);
                    if(index == 2)
                    {
                        break;
                    }
                }                
            }
            else
            {
                foreach (var item in query.Triples.OrderBy(x => x.Priority))
                {
                    if (item.IsSentToJena == false)
                    {
                        item.IsSentToJena = true;
                        ret.Add(item);
                        break;
                    }
                }
            }            
            return ret;            
        }


        

        //public List<Triple> GetNextTriples(TripleQuery query)
        //{
        //    var ret = new List<Triple>();
        //    foreach (var item in query.Triples.OrderBy(x => x.Priority))
        //    {
        //        if (item.IsSentToJena == false)
        //        {
        //            item.IsSentToJena = true;
        //            ret.Add(item);
        //            break;
        //        }
        //    }
        //    return ret;
        //}

        public void GetFirstTwoTriples(TripleQuery query)
        {
            if (query.Triples.Count < 3)
            {
                foreach (var triple in query.Triples.OrderBy(x=>x.Priority))
                {
                    triple.IsSentToJena = true;
                }
                return;
            }
                

            List<string> firstVars = new List<string>();
            int cnt = -1;
            foreach (var item in query.Triples.OrderBy(x=>x.Priority))
            {
                cnt++;
                if(cnt == 0)
                {
                    item.IsSentToJena = true;
                    firstVars = item.GetAllElements();
                    continue;
                }

                var secondVars = item.GetAllElements();

                var intersect = secondVars.Intersect(firstVars).ToList();
                if(intersect.Count > 0)
                {
                    item.IsSentToJena = true;
                    break;
                }

            }
        }

        public void SyncTerms(Dictionary<string, List<string>> terms_new, Dictionary<string, List<string>> terms)
        {
            foreach (var key in terms_new.Keys)
            {               
               terms[key] = terms_new[key];                
            }
        }

        public void ReturnTermKeys(Dictionary<string, List<string>> terms)
        {
            var remove = new Dictionary<String, String>();
            foreach (var key in terms.Keys)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (key.EndsWith("TT_" + i))
                    {
                        remove[key] = key.Replace("TT_" + i, "");
                        break;
                    }
                }
            }
            foreach (var key in remove.Keys)
            {
                terms[remove[key]] = terms[key];
                terms.Remove(key);
            }
        }

        public SearchResponse GetCandidates(TripleQuery query)
        {
            var sr = new SearchResponse();
            var candidates = new List<Candidate>();
            var terms = new Dictionary<string, List<string>>();
            List<Triple> triples = null;

            //SetPriority(query);
            logger.Info(String.Format("\n{0}[{1}]", query.Body, String.Join(",", query.Triples.Select(x=>x.Priority))));
            sr.IsESFilter = true;            
            while (true)
            {
                triples = GetNextTriples(query);
                foreach (var triple in triples)
                {
                    (var terms_new, var elapsed_es) = GetTerms(query, triple);
                    sr.EsElapsedTime = sr.EsElapsedTime + elapsed_es;
                    SyncTerms(terms_new, terms);
                }
                
                var hasTermZero = terms.Values.ToList().Where(x => x.Count() == 0).FirstOrDefault() != null;
                
                if(hasTermZero)
                {
                    sr.BadCandidate = new Candidate() { Triples = query.Triples.Where(x => x.IsSentToJena == true).ToList() };
                    candidates.Clear();
                    break;
                }

                double elapsed_jena;
                (candidates, elapsed_jena) = new Retriever().RetrieveCandidates(query, terms);
                sr.JenaElapsedTime = sr.JenaElapsedTime + elapsed_jena;
                if (candidates.Count == 0)
                {
                    sr.BadCandidate = new Candidate() { Triples = query.Triples.Where(x => x.IsSentToJena == true).ToList() };
                    if(sr.BadCandidate.Triples.Count == 1)
                    {
                        var i = 0;
                        i++;
                    }
                    break;
                }
                    

                if (query.Triples.Where(x => x.IsSentToJena == true).Count() ==
                    query.Triples.Count)
                {
                    break;
                }                                
            }
                        
            if(query.IsPartial)
            {
                candidates.ForEach(x => x.Triples.ForEach(y=>y.AddVariablesToTerms(query, terms)));
                candidates = new List<Candidate>();                
            }

            if(candidates.Count > 0)
            {                
                sr.IsESFilter = false;
                logger.Info(String.Format("Candidates count : {0}", candidates.Count.ToString()));
            }
            else
            {               
                sr.Terms = terms;
            }

            sr.Candidates = candidates;
            /*{
                Candidates = candidates
            };*/
            return sr;
        }                   
    }
}
