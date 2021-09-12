using Algorithms;
using Common;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class Candidate
    {
        public List<Candidate> Candidates { get; set; } = new List<Candidate>();
        public List<Triple> Triples { get; set; } = new List<Triple>();

        public string Query { get; set; }

        public string FullQuery { get; set; }


        public int EditDistance { get; set; } = 0;

        public void CalcEditDistance(List<Triple> triples,bool isUseSynonym)
        {
            int ed = 0;
            foreach (var triple in Triples)
            {                
                ed += triple.CalcEditDistance(triple,isUseSynonym);
            }
            this.EditDistance = ed;
        }

        public Candidate()
        {
            
        }

        //Publish in the sub-pub pattern
        public void Publish()
        {
            foreach (var triple in Triples)
            {
                triple.Subject.Publish();
                triple.Predicate.Publish();
                triple.Object.Publish();
            }
        }

        //Subscribe in the pub-sub pattern
        private void Subscribe(Dictionary<string, Entity> dict, Entity entity)
        {
            entity.IsDisplayed = true;
            var key = entity.GetElement();
            if (dict.ContainsKey(key))
            {
                var value = dict[key];
                if (key.StartsWith("?"))
                {
                    entity.Subscribe(value);
                    entity.IsDisplayed = false;
                }

            }
            else
            {
                dict[key] = entity;
            }
        }        


        public void SetFeedback()
        {
            foreach (var triple in Triples)
            {
                triple.SetFeedback();
            }
        }

        public void GetFeedbackUI()
        {
            foreach (var triple in Triples)
            {
                triple.GetFeedbackUI();
            }
        }

        public void Sucbscribe()
        {
            var dict = new Dictionary<string, Entity>();
            foreach (var triple in Triples)
            {            
                Subscribe(dict, triple.Subject);
                Subscribe(dict, triple.Predicate);
                Subscribe(dict, triple.Object);
            }
        }


        public string GetTriplesStr()
        {
            var sb = new StringBuilder();
            foreach (var triple in Triples)
            {
                sb.Append("  ");
                sb.Append(triple.Subject.Value);
                sb.Append(" ");
                sb.Append(triple.Predicate.Value);
                sb.Append(" ");
                sb.Append(triple.Object.Value);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        

        public void SetQuery()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("Select *");
            sb.AppendLine("{");
            for (int i = 0; i < Triples.Count; i++)
            {
                var triple = Triples[i];
                sb.Append("  ");
                if(Triples[i].Subject.IsAnchor && Triples[i].Subject.Anchor.StartsWith("?"))
                {
                    sb.Append(Triples[i].Subject.Anchor);                    
                }
                else if (Triples[i].Subject.IsVariable)
                {
                    sb.Append(Triples[i].Subject.Variable);
                }
                else
                {
                    sb.Append(Triples[i].Subject.Value);
                }                
                sb.Append(" ");
                if (Triples[i].Predicate.IsAnchor && Triples[i].Predicate.Anchor.StartsWith("?"))
                {                    
                    sb.Append(Triples[i].Predicate.Anchor);                    
                }
                else
                {
                    sb.Append(Triples[i].Predicate.Value);
                }
                sb.Append(" ");
                if (Triples[i].Object.IsAnchor && Triples[i].Object.Anchor.StartsWith("?"))
                {                    
                    sb.Append(Triples[i].Object.Anchor);                    
                }
                else if (Triples[i].Object.IsVariable)
                {
                    sb.Append(Triples[i].Object.Variable);
                }
                else
                {
                    sb.Append(Triples[i].Object.Value);
                }
                sb.Append(" ");
                sb.Append(".");
                sb.Append("\n");
            }
            sb.Length -= 3;
            sb.AppendLine("\n}");
            sb.AppendLine("");
            Query = sb.ToString();
        }
        

        public List<string> getLetters()
        {
            var letters = new List<string>();
            foreach (var candidate in Candidates)
            {                
                foreach (var triple in candidate.Triples)
                {
                    if (Triples[triple.Index].Subject.IsDisplayed)
                    {
                        if (!String.IsNullOrEmpty(triple.Subject.GetAssignment()))
                        {
                            letters.Add(triple.Subject.GetElement().Replace("?", ""));                            
                        }
                    }

                    if (Triples[triple.Index].Predicate.IsDisplayed)
                    {
                        letters.Add(triple.Predicate.GetElement().Replace("?", ""));
                    }
                    if (Triples[triple.Index].Object.IsDisplayed)
                    {
                        letters.Add(triple.Object.GetElement().Replace("?", ""));
                    }
                }                
            }
            return letters;
        }

       

        public List<List<string>> getRows()
        {
            var lst = new List<List<string>>(); 
            foreach (var candidate in Candidates)
            {
                var row = new List<string>();
                foreach (var triple in candidate.Triples)
                {
                    if (Triples[triple.Index].Subject.IsDisplayed)
                    {
                        if (!String.IsNullOrEmpty(triple.Subject.GetAssignment()))
                        {
                            //byte[] bytes = Encoding.Default.GetBytes(triple.Subject.Value);
                            //var myString = Encoding.Unicode.GetString(bytes);
                            row.Add(triple.Subject.Value);                                      
                        }
                    }

                    if (Triples[triple.Index].Predicate.IsDisplayed)
                    {
                        if (!String.IsNullOrEmpty(triple.Predicate.GetAssignment()))
                        {
                            //byte[] bytes = Encoding.Default.GetBytes(triple.Predicate.Value);
                            //var myString = Encoding.Unicode.GetString(bytes);
                            row.Add(triple.Predicate.Value);
                        }
                    }
                    if (Triples[triple.Index].Object.IsDisplayed)
                    {
                        if (!String.IsNullOrEmpty(triple.Object.GetAssignment()))
                        {
                            //row.Add(triple.Object.Value);
                            //byte[] bytes = Encoding.Default.GetBytes(triple.Object.Value);
                            //var myString = Encoding.Unicode.GetString(bytes);
                            row.Add(triple.Object.Value);
                        }
                    }
                }
                lst.Add(row);
            }
            return lst;
        }


        public bool IsCandidateEqual(Candidate candidate)
        {            
            foreach (var item in this.Triples)
            {
                var ret1 = item.Subject.IsEqual(candidate.Triples[item.Index].Subject);
                var ret2 = item.Predicate.IsEqual(candidate.Triples[item.Index].Predicate);
                var ret3 = item.Object.IsEqual(candidate.Triples[item.Index].Object);
                if (ret1 == false || ret2 == false || ret3 == false)
                    return false;
            }
            return true;
        }

        public bool IsVariableFeedbackChanged(Candidate candidate)
        {
            if (candidate == null)
                return false;
            for (int i = 0; i < Triples.Count; i++)
            {
                if(Triples[i].Subject.IsVariable == true)
                {
                    if(Triples[i].Subject.IsFeedback != candidate.Triples[i].Subject.IsFeedback)
                    {
                        return true;
                    }
                }

                if (Triples[i].Predicate.IsVariable == true)
                {
                    if (Triples[i].Predicate.IsFeedback != candidate.Triples[i].Predicate.IsFeedback)
                    {
                        return true;
                    }
                }

                if (Triples[i].Object.IsVariable == true)
                {
                    if (Triples[i].Object.IsFeedback != candidate.Triples[i].Object.IsFeedback)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsContained(List<Triple> OperationTriples)
        {
            var cnt = 0;
            
            foreach (var triple in Triples)
            {
                foreach (var operationTriple in OperationTriples)
                {
                    if(triple.Equals(operationTriple))
                    {
                        cnt++; 
                    }
                }                                
            }
            var ret = (Triples.Count == cnt ? true : false);
            return ret;
        }

        public void SetFullQuery()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Select *");
            sb.AppendLine("{");
            for (int i = 0; i < Triples.Count; i++)
            {
                var triple = Triples[i];
                sb.Append("  ");
                if (Triples[i].Subject.IsAnchor && Triples[i].Subject.Anchor.StartsWith("?"))
                {
                    sb.Append(String.Format("{0} [{1}]", Triples[i].Subject.Anchor, Triples[i].Subject.Value));
                }
                else if (Triples[i].Subject.IsVariable)
                {
                    sb.Append(Triples[i].Subject.Variable);
                }
                else
                {
                    sb.Append(Triples[i].Subject.Value);
                    if (Triples[i].Subject.IsAnchor == true)
                    {
                        sb.Append(String.Format(" [{0}]", Triples[i].Subject.Anchor));
                    }
                    
                }
                sb.Append(" ");
                if (Triples[i].Predicate.IsAnchor && Triples[i].Predicate.Anchor.StartsWith("?"))
                {
                    sb.Append(String.Format("{0} [{1}]",Triples[i].Predicate.Anchor, Triples[i].Predicate.Value));
                }
                else
                {
                    sb.Append(Triples[i].Predicate.Value);
                    if (Triples[i].Predicate.IsAnchor == true)
                    {
                        sb.Append(String.Format(" [{0}]", Triples[i].Predicate.Anchor));
                    }
                }
                sb.Append(" ");
                if (Triples[i].Object.IsAnchor && Triples[i].Object.Anchor.StartsWith("?"))
                {
                    sb.Append(String.Format("{0} [{1}]", Triples[i].Object.Anchor, Triples[i].Object.Value));
                }
                else if (Triples[i].Object.IsVariable)
                {
                    sb.Append(Triples[i].Object.Variable);
                }
                else
                {                    
                    sb.Append(Triples[i].Object.Value);
                    if (Triples[i].Object.IsAnchor == true)
                    {
                        sb.Append(String.Format(" [{0}]", Triples[i].Object.Anchor));
                    }
                }
                sb.Append(" ");
                sb.Append(".");
                sb.Append("\n");
            }
            sb.Length -= 3;
            sb.AppendLine("\n}");
            sb.AppendLine("");
            FullQuery = sb.ToString();
        }        
    }
}
