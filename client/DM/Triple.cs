using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DM
{
    public partial class Triple
    {
        public Entity Subject { get; set; } = new Entity("S");

        public Entity Predicate { get; set; } = new Entity("P");

        public Entity Object { get; set; } = new Entity("O");

        public int Index { get; set; }

        // override object.Equals
        public bool Equals(Triple triple)
        {
            bool ret = true;
            if(!Subject.Value.Equals(triple.Subject.Value))
            {
                return false;
            }
            if (!Predicate.Value.Equals(triple.Predicate.Value))
            {
                return false;

            }
            if (!Object.Value.Equals(triple.Object.Value))
            {
                return false;
            }
            return ret;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            throw new NotImplementedException();
            return base.GetHashCode();
        }

        
        public Triple()
        {
            
        }

        public void FixFeedback()
        {
            Subject.FixFeedback();
            Predicate.FixFeedback();
            Object.FixFeedback();
        }

        public void SetFeedback()
        {
            Subject.SetFeedback();
            Predicate.SetFeedback();
            Object.SetFeedback();
        }

        public void GetFeedbackUI()
        {
            Subject.GetFeedbackUI();
            Predicate.GetFeedbackUI();
            Object.GetFeedbackUI();
        }

        public bool IsAllPositiveFeedback()
        {
            var ret = (Subject.IsFeedback == true && Predicate.IsFeedback == true && Object.IsFeedback == true);
            return ret;
        }


        public void Shorten()
        {
            Subject.Shorten();
            Predicate.Shorten();
            Object.Shorten();
        }

        public string StrWithoutVariables()
        {
            StringBuilder sb = new StringBuilder();
            if (!Subject.Value.StartsWith("?"))
                sb.Append(Subject.Value);
            if (!Predicate.Value.StartsWith("?"))
                sb.Append(Predicate.Value);
            if (!Object.Value.StartsWith("?"))
                sb.Append(Object.Value);
            var str = sb.ToString();

            if(String.IsNullOrEmpty(str))
            {
                return null;
            }
            return sb.ToString();
        }         

        public int CalcEditDistance(Triple triple, bool isUseSynonym)
        {
            int ed = 0;
            if(triple.Subject.IsAnchor && !triple.Subject.Anchor.StartsWith("?"))
            {
                ed += Utils.GetEditDistance(this.Subject.Value, triple.Subject.Anchor);//Algorithms.Levenshtein.ComputeDistance(this.Subject.Value, triple.Subject.Anchor);
            }
            if (triple.Predicate.IsAnchor && !triple.Predicate.Anchor.StartsWith("?"))
            {
                //If the predicate is one of the synonyms than don't add to edit distance
                //so it will be shown one of the first
                if (triple.Predicate.IsSynonymValue == false || isUseSynonym == false)
                {
                    ed += Utils.GetEditDistance(this.Predicate.Value, triple.Predicate.Anchor);//Algorithms.Levenshtein.ComputeDistance(this.Predicate.Value, triple.Predicate.Anchor);
                }                    
            }
            if (triple.Object.IsAnchor && !triple.Object.Anchor.StartsWith("?"))
            {
                ed += Utils.GetEditDistance(this.Object.Value, triple.Object.Anchor);//Algorithms.Levenshtein.ComputeDistance(this.Object.Value, triple.Object.Anchor); 
            }
            return ed;
        }
        
        public List<string> GetVariables()
        {
            var variables = new List<string>();

            if (Subject.Value != null && Subject.Value.StartsWith("?"))
            {
                variables.Add(Subject.Value);
            }

            if (Predicate.Value != null && Predicate.Value.StartsWith("?"))
            {
                variables.Add(Predicate.Value);
            }

            if (Object.Value != null && Object.Value.StartsWith("?"))
            {
                variables.Add(Object.Value);
            }

            return variables;
        }                        
    }
}
