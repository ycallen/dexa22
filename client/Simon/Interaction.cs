using Caching;
using DM;
using DM;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Simon
{
    public class Interaction : IInteraction
    {
        //positive feedback cache for anchors
        public FIFOCache<string, string> PositiveFeedbackCache = new FIFOCache<string, string>(Int32.MaxValue, Int32.MaxValue);
        //negative feedback cache for anchors
        public FIFOCache<string, List<string>> NegativeFeedbackCache = new FIFOCache<string, List<string>>(Int32.MaxValue, Int32.MaxValue);

        public FIFOCache<string, List<string>> candidatesCache = new FIFOCache<string, List<string>>(Int32.MaxValue, Int32.MaxValue);

        public Guid Guid { get; set; } = Guid.NewGuid();        

        public bool HasAnswer { get; set; } = false;

        public int? JenaLimit { get; set; }

        public bool IsTestPassed { get; set; }        

        
        public string  Reason { get; set; }

        public bool IsChangeOperationRoot{ get; set; }        

        public Operation CurrOperation { get; set; }

        public int? ESLimitEntities { get; set; }

        public int? ESLimitLiterals { get; set; }        


        public bool IsEditDistance { get; set; }

        public bool IsUnitTest { get; set; }

        public bool IsUseSynonym { get; set; } = true;

        public bool IsEsEditDistance { get; set; } = true;

        public int? QueryTimeout { get; set; }

        public string InputStr { get; set; }

        public bool IsPartial { get; set; }

        public string KB { get; set; }

        public List<Triple> InputTriples { get; set; }

        public Candidate Candidate { get; set; }

        public int CorrectTriplesCnt { get; set; }

        public double InteractionCnt { get; set; } = 0;

        public int RoundTripsCnt { get; set; } = 0;

        public double EsElapsedTime { get; set; } = 0;

        public int JenaCnt { get; set; } = 0;

        public int ZeroCnt { get; set; } = 0;

        public double JenaElapsedTime { get; set; } = 0;

        public double StructureCnt { get; set; } = 0;

        public double FilteredCnt { get; set; } = 0;

        public double PartialTime { get; set; } = 0;

        public double ElaspedTime { get; set; }

        public string TestName { get; set; }

        public Stopwatch StopWatch { get; set; }

        public void Init(List<Triple> triples)
        {
            Common.Utils.Guid = this.Guid;
            this.InputTriples = triples;
            candidatesCache.AddReplace("candidates", new List<string>());
            foreach (var triple in triples)
            {
                if(Common.Utils.IsEntity(triple.Subject.Value))
                {
                    triple.Subject.IsFeedback = true;
                    CacheAnchorAndVariableFeedback(triple.Subject);                    
                }
                if (Common.Utils.IsEntity(triple.Predicate.Value))
                {
                    triple.Predicate.IsFeedback = true;
                    CacheAnchorAndVariableFeedback(triple.Predicate);                    
                }
                if (Common.Utils.IsEntity(triple.Object.Value))
                {
                    triple.Object.IsFeedback = true;
                    CacheAnchorAndVariableFeedback(triple.Object);
                }
            }        
        }

        //Set anchors for each entity of new operation
        private void SetAnchors(Entity entity)
        {
            //the key is based on the anchor field it represents by name and triple index
            string key = null;
            if(entity.IsAnchor)
            {
                key = entity.AnchorField + "_" + entity.AnchorIndex;
            }
            if(entity.IsVariable)
            {
                key = "var" + entity.VariableField + "_" + entity.VariableIndex;
            }

            if(key == null)
            {
                return;
            }
            
            if (PositiveFeedbackCache.Contains(key))
            {
                entity.IsFeedback = true;
                entity.Value = PositiveFeedbackCache.Get(key);
            }            
        }



        //Set anchors for new operation
        public void SetAnchors(Operation operation)
        {
            foreach (var triple in operation.Triples)
            {
                SetAnchors(triple.Subject);
                SetAnchors(triple.Predicate);
                SetAnchors(triple.Object);               
            }
        }

        private void SetFilters(Entity entity)
        {
            //the key is based on the anchor field it represents by name and triple index
            string key = null;
            if(entity.IsAnchor)
            {
                key = entity.AnchorField + "_" + entity.AnchorIndex;
            }
            if(entity.IsVariable)
            {
                key = "var" + entity.VariableField + "_" + entity.VariableIndex;
            }

            if(key == null)
            {
                return;
            }

            
            if (NegativeFeedbackCache.Contains(key) && entity.IsFeedback != true
                && !entity.Value.Equals("#empty#"))
            {
                entity.Filters = NegativeFeedbackCache.Get(key);
                var lst = new List<String>();
                foreach (var filter in entity.Filters)
                {
                    if(Common.Utils.IsEntity(filter))
                    {
                        lst.Add(Common.Utils.ChangePrefix(filter));
                    }                    
                }
                entity.Filters = lst;
            }
        }

        public void SetFilters(Operation operation)
        {
            foreach (var triple in operation.Triples)
            {
                //SetFilters(triple.Subject);
                SetFilters(triple.Predicate);
                //SetFilters(triple.Object);
            }
        }

        //Save negative feedback to cache so we can 
        //later use it to disqualify candidates 
        private List<string> SetNegativeFeedback(string key, string value)
        {
            List<string> data;
            if (!NegativeFeedbackCache.TryGet(key, out data))
            {
                data = new List<string>();
                data.Add(value);
                NegativeFeedbackCache.AddReplace(key, data);
            }
            else
            {
                data.Add(value);
                NegativeFeedbackCache.AddReplace(key, data);
            }
            return data;
        }

        //Cache both positive and negative user feedback for each entity
        private void CacheAnchorAndVariableFeedback(Entity entity)
        {
            if (entity.IsAnchor == true && entity.IsFeedback == true
                 && !entity.Value.Equals("#empty#"))
            {
                var key = entity.AnchorField + "_" + entity.AnchorIndex;
                PositiveFeedbackCache.AddReplace(key, entity.Value);
            }
            else if (entity.IsAnchor == true && entity.IsFeedback == false
                  && !entity.Value.Equals("#empty#"))
            {                
                var key = entity.AnchorField + "_" + entity.AnchorIndex;
                SetNegativeFeedback(key, entity.Value);                
            }
            else if (entity.IsVariable == true && entity.IsFeedback == true
                 && !entity.Value.Equals("#empty#"))
            {
                var key = "var" + entity.VariableField + "_" + entity.VariableIndex;
                PositiveFeedbackCache.AddReplace(key, entity.Value);
            }
            else if (entity.IsVariable == true && entity.IsFeedback == false
                  && !entity.Value.Equals("#empty#"))
            {
                var key = "var" + entity.VariableField + "_" + entity.VariableIndex;
                SetNegativeFeedback(key, entity.Value);
            }
        }

        //Set both positive and negative user feedback for each entity for all triples
        public void SetAnchorAndVariableFeedback()
        {
            if (Candidate == null)
                return;
            foreach (var triple in Candidate.Triples)
            {
                CacheAnchorAndVariableFeedback(triple.Subject);
                CacheAnchorAndVariableFeedback(triple.Predicate);
                CacheAnchorAndVariableFeedback(triple.Object);                
            }
        }

        
        //check if an entity is in negative feedback cache
        private bool IsInNegativeFeedbackCache(Entity entity)
        {
            string key = null;
            if(entity.IsAnchor)
            {
                key = entity.AnchorField + "_" + entity.AnchorIndex;
            }
            
            if(entity.IsVariable)
            {
                key = "var" +  entity.VariableField + "_" + entity.VariableIndex;
            }

            var ret = true;
            if (key != null && NegativeFeedbackCache.Contains(key))
            {

                if (NegativeFeedbackCache.Get(key).Where(x => x.Equals(entity.Value)).FirstOrDefault() != null)
                {
                    ret = false;
                }
            }

            return ret;
        }
        
        //Check if we should disqualify a candidate because the user
        //historically gave negative feedback to some of its parts.
        private bool IsKeywordInNegativeCache(Candidate candidate)
        {
            foreach (var triple in candidate.Triples)
            {
                var ret1 = IsInNegativeFeedbackCache(triple.Subject);
                var ret2 = IsInNegativeFeedbackCache(triple.Predicate);
                var ret3 = IsInNegativeFeedbackCache(triple.Object);

                var ret = ret1 && ret2 && ret3;
                if (!ret)
                {
                    return false;
                }
            }
            return true;
        }

        //Check if the user has already seen this candidate
        private bool IsCandidateInCache(Candidate candidate)
        {
            var ncandidate = candidate.DeepClone();
            ncandidate.Triples = ncandidate.Triples.OrderBy(x => x.StrWithoutVariables()).ToList();

            var b = candidatesCache.Get("candidates").Contains(ncandidate.GetTriplesStr());
            if (b)
            {
                return true;
            }
            else
            {
                var candidates = candidatesCache.Get("candidates");
                candidates.Add(ncandidate.GetTriplesStr());
                candidatesCache.AddReplace("candidates", candidates);
            }
            return false;
        }

        //check if we should show the user this candidate
        public bool IsCandidateValid(Candidate candidate)
        {
            var res1 = IsKeywordInNegativeCache(candidate);
            var res2 = IsCandidateInCache(candidate);

            return res1 && !res2;

        }

        public int GetNumberOfVariables()
        {
            int cnt = 0;
            foreach (var item in InputTriples)
            {
                cnt += item.GetVariables().Count;
            }
            return cnt;
        }


        public int GetNumberOfDistinctVariables()
        {
            var variables = new List<string>();
            foreach (var item in InputTriples)
            {
               item.GetVariables().ForEach(x => variables.Add(x));               
            }
            var cnt = variables.Distinct().Count(); 
            return cnt;
        }


        public Interaction(string query, string testName = "")
        {
            TestName = testName;
            InputStr = query;
        }
    }
}
