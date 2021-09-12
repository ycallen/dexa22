using Common;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DM
{
    public class Candidate
    {
        //public List<Candidate> Candidates { get; set; } = new List<Candidate>();
        public List<Triple> Triples { get; set; } = new List<Triple>();
        public Dictionary<string, List<string>> Variables { get; set; } = new Dictionary<string, List<string>>();

        public string TriplesStr { get; set; }

        public string Query { get; set; }

        public void SetTriplesStr()
        {
            var sb = new StringBuilder();
            foreach (var triple in Triples)
            {
                sb.Append(triple.Subject.Value);
                sb.Append(" ");
                sb.Append(triple.Predicate.Value);
                sb.Append(" ");
                sb.Append(triple.Object.Value);
                sb.Append("\n");
            }

            TriplesStr = sb.ToString();
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
                if (Triples[i].Subject.IsAnchor && Triples[i].Subject.Anchor.StartsWith("?"))
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
                    sb.Append(Utils.ObjectTreatment(Triples[i].Object.Value));
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

        private int getBatchLen()
        {
            int len = 1;
            foreach (var triple in Triples)
            {
                if (triple.Subject.Value.Contains("##"))
                {
                    len = triple.Subject.Value.Split("##").Length;
                    return len;
                }
                if (triple.Predicate.Value.Contains("##"))
                {
                    len = triple.Predicate.Value.Split("##").Length;
                    return len;
                }
                if (triple.Object.Value.Contains("##"))
                {
                    len = triple.Object.Value.Split("##").Length;
                    return len;
                }
            }
            return len;
        }
       
    }
}
