using DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client;
using C5;
using System.IO;
using Meziantou.Framework;
using System.Diagnostics;
using System.Runtime.Caching;
using NLog;
using System.Threading;
using Force.DeepCloner;

namespace Simon
{
    public class Simon 
    {
        
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public bool IsCalcEditDistance { get; set; } = true;

        public bool IsPartial { get; set; } = false;

        public ExtractTriplesFromQuery ExtractTriplesFromQuery { get; set; } = new ExtractTriplesFromQuery();

        public List<Triple> Triples { get; set; }

        public List<Candidate> BadCandidates { get; set; } = new List<Candidate>();

        public ISelector Selector { get; set; }

        public OperationsManager OperationsManager { get; set; } = new OperationsManager();        

        public Simon(Interaction interaction)
        {
            try
            {
                Triples = ExtractTriplesFromQuery.Send(interaction.InputStr);

                OperationsManager.Init(Triples);

                interaction.Init(Triples);
            }
            catch (Exception ex)
            {

                throw ex;
            }
                        
        }

        private void GetStatistics(SearchResponse sr, Interaction interaction)
        {
            interaction.RoundTripsCnt = interaction.RoundTripsCnt + 1;
            if (sr.Candidates.Count == 0)
            {
                interaction.ZeroCnt = interaction.ZeroCnt + 1;
            }
            
            if (sr.IsESFilter == false)
            {
                interaction.JenaCnt = interaction.JenaCnt + 1;
            }

            interaction.EsElapsedTime = interaction.EsElapsedTime + sr.EsElapsedTime;

            interaction.JenaElapsedTime = interaction.JenaElapsedTime + sr.JenaElapsedTime;            
        }

        private void FeedBackTreatment(Interaction interaction)
        {
            interaction.HasAnswer = false;
            interaction.SetAnchorAndVariableFeedback();
            if (interaction.IsChangeOperationRoot)
            {
                OperationsManager.ChangeRoot(interaction.CurrOperation);
                Selector = null;
            }
            else
            {
                Selector.FilterByFeedback(interaction.Candidate);
            }
            //Common.Utils.Log(String.Format("selector result : candidate = {0}, full_candidate = {1}, candidate_count = {2}", interaction.Candidate.Query.Replace("\r\n", " ").Replace("  ", "").Replace("\n", " "), interaction.Candidate.FullQuery.Replace("\r\n", " ").Replace("  ", "").Replace("\n", " "), Selector.Count()));
        }

        private Operation GetNextOperation(Interaction interaction)
        {
            var operation = OperationsManager.GetNext();
            Common.Utils.Log(String.Format("start operation : operation = {0}", operation.TriplesStr.Replace("  ", " ").Replace("\n", "")));
            interaction.CurrOperation = operation.DeepClone();
            interaction.SetAnchors(operation);
            interaction.SetFilters(operation);
            if (!operation.isValid())
            {
                Common.Utils.Log(String.Format("sieve operation : operation = {0} ", operation.TriplesStr.Replace("  ", " ").Replace("\n", "")));
                operation = null;
            }
            return operation;
        }

        private void GetSelector(Interaction interaction, Operation operation, List<Candidate> candidates, Dictionary<string, List<string>> terms)
        {
            if (candidates.Count == 0)
            {

                if (IsPartial == true)
                {
                    Selector = new SelectorPartial(terms, operation, IsCalcEditDistance);
                    if (Selector.Count() == 0)
                    {
                        Selector = null;
                    }                    
                }
                else
                {
                    Selector = null;
                }

            }
            else
            {
                Selector = new SelectorFull(candidates, operation, IsCalcEditDistance, interaction.IsUseSynonym);
                interaction.StructureCnt++;
            }
        }

        public bool IsBadOperation(Operation operation)
        {

            bool ret = false;
            
            var bad = BadCandidates.Where(x => x.IsContained(operation.Triples)).ToList();
            //badCandidates.ForEach(x => x.SetFullQuery());
            if(bad.Count > 0)
            {
                ret = true;
            }

            return ret;
        }

        public async Task /*void*/ GetNext(Interaction interaction, bool isSkip, CancellationToken tokenSource)
        {
            //The first time there is no interaction and no feedback
            if(interaction.HasAnswer == true)
            {
                FeedBackTreatment(interaction);
            }
            while (true)
            {
                //If there are no more candidates to show the user 
                //then send a new query to the server to get more.
                if (isSkip || Selector == null || Selector.IsEmpty())
                {
                    var operation = GetNextOperation(interaction);
                    
                    if (operation == null)
                    {
                        continue;
                    }
                    if (IsBadOperation(operation))
                    {
                        continue;
                    }

                    SearchResponse response = null;
                    try
                    {
                       response = await GetCandidates.Send(operation.Triples, interaction);//interaction.JenaLimit, interaction.ESLimitEntities, interaction.ESLimitLiterals, interaction.QueryTimeout, interaction.IsEditDistance, interaction.IsUnitTest, interaction.IsUseSynonym, interaction.IsEsEditDistance);
                       if(response.BadCandidate != null && response.BadCandidate.Triples.Count > 0)
                       {
                            BadCandidates.Add(response.BadCandidate);
                       }
                       GetStatistics(response, interaction);                                                
                    }
                    catch (Exception ex)
                    {
                        if(interaction.IsUnitTest == true)
                        {
                            interaction.Reason = ex.ToString();
                            return;
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                    
                    if(tokenSource.IsCancellationRequested == true)
                    {
                        tokenSource.ThrowIfCancellationRequested();
                    }
                                                      
                    //If user decides to change query then get rid of previous ones
                    if(!Common.Utils.Guid.Equals(interaction.Guid))
                    {
                        break;
                    }

                    var candidates = response.Candidates;
                    var terms = response.Terms;

                    if (interaction.StopWatch != null &&
                        interaction.StopWatch.Elapsed.TotalMilliseconds > (interaction.QueryTimeout * 1000))
                    {
                        interaction.Reason = "timeout";
                        return;
                    }

                    GetSelector(interaction, operation, candidates, terms);
                    if(Selector == null)
                    {
                        continue;
                    }                                                                                                       
                }

                //Get the next candidate from a list of candidates
                //and send it to the user for feedback.
                var candidate = Selector.GetNext();
                if (!interaction.IsCandidateValid(candidate) && !IsPartial)
                {                    
                    continue;
                }

                 if (candidate!=null)
                {
                    //for automatic tests
                    if (interaction.Candidate == null || interaction.Candidate.Triples.Count == 0 || !interaction.Candidate.Query.Equals(candidate.Query))
                        interaction.InteractionCnt++;
                    interaction.HasAnswer = true;
                    interaction.Candidate = candidate;                    
                    break;
                }                
            }
        }        
    }
}
