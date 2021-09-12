using Caching;
using Force.DeepCloner;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using Common;
using System.Linq;

namespace DM
{
    public class TripleQuery
    {
        public String Header { get; set; }
        public String Body { get; set; }
        public List<Triple> Triples { get; set; }
        public int Size { get; set; }

        public int? JenaLimit { get; set; }

        public int? ESLimitEntities { get; set; }

        public int? ESLimitLiterals { get; set; }

        public int? QueryTimeout { get; set; }

        public bool IsEditdistance { get; set; }

        public bool IsUseSynonym { get; set; }

        public bool IsEsEditDistance { get; set; }

        public bool IsPartial { get; set; }


        public bool IsUnitTest { get; set; }

        public Guid Guid { get; set; }

        public String Sparql { get => (Header + '\n' + Body); }



        private string CombineValues(List<string> values, string key, string comma = "")
        {
            StringBuilder valuesStr = new StringBuilder();
            foreach (var value in values)
            {
                if (value.Length > 150)
                    continue;
                if (value.Contains("u0022"))
                    continue;
                if (value.Contains("u0060"))
                    continue;
                if (value.Contains("u00"))
                    continue;
                if (value.Contains("_\\n"))
                    continue;
                if (value.Contains("\n"))
                    continue;
                if (value.Contains("\\N"))
                    continue;
                if (value.Contains("\\\""))
                    continue;
                if (value.EndsWith("\","))
                    continue;
                if (value.Contains("Friends of \\Seagate\\\","))
                    continue;
                if (value.Contains("The SS Heraklion (sometimes spelled out in books as the \\Iraklion\\\")"))
                    continue;
                if (value.Contains("William Shatner Reflects on his hit \\R"))
                    continue;
                valuesStr.Append(/*yudaAddValue*/(String.Format("({0}){1}", Utils.ObjectTreatment(value), comma)));
            }
            return valuesStr.ToString();
        }

        public string getSelect(Dictionary<string, List<string>> terms)
        {
            StringBuilder sb = new StringBuilder();
            List<string> lst = new List<string>();
            foreach (var triple in this.Triples)
            {
                if (triple.Subject.IsFeedback != true)
                {
                    if (triple.Subject.Value.StartsWith("?"))
                    {
                        lst.Add(triple.Subject.ReplaceInvalidChars());
                    }
                    else
                    {
                        lst.Add("?" + triple.Subject.ReplaceInvalidChars());
                    }
                }

                if (triple.Predicate.IsFeedback != true)
                {
                    if (triple.Predicate.Value.StartsWith("?"))
                    {
                        lst.Add(triple.Predicate.ReplaceInvalidChars());
                    }
                    else
                    {
                        lst.Add("?" + triple.Predicate.ReplaceInvalidChars());
                    }
                }

                if (triple.Object.IsFeedback != true)
                {
                    if (triple.Object.Value.StartsWith("?"))
                    {
                        lst.Add(triple.Object.ReplaceInvalidChars());
                    }
                    else
                    {
                        lst.Add("?" + triple.Object.ReplaceInvalidChars());
                    }
                }
            }
            lst.Distinct().ToList().ForEach(x => sb.Append(x + " "));
            return sb.ToString();
        }

        
        public string buildQuery(Dictionary<string, List<string>> termsOrig, int? jenaLimit)
        {
            //var terms = termsOrig.DeepClone();

            var terms = new Dictionary<string, List<string>>();
            //only get relevant terms
            this.Triples.ForEach(x => x.getTerms(termsOrig, terms));
            

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.Header);

            string select = getSelect(terms);
            if (select.Equals(""))
            {
                select = "*";
            }
            sb.AppendLine(String.Format("\nSELECT {0}", select));
            sb.AppendLine("{");

            var combine = new Dictionary<string, List<string>>();
            foreach (var key in terms.Keys)
            {
                combine.Add(key, terms[key]);
            }


            foreach (var key in combine.Keys)
            {
                string combineValues = CombineValues(combine[key], key);
                var new_key = Utils.ReplaceInvalidChars(key);// key.Replace(" ", "_").Replace(".", "").Replace("?", "");
                sb.AppendLine(string.Format("VALUES(?{0}){{{1}}}", new_key, combineValues));
            }
            foreach (var triple in this.Triples)//this.Triples.OrderBy(x=>x.GetVariableCount()))
            {
                if (terms.ContainsKey(triple.Subject.Value))
                {
                    sb.Append(string.Format("?{0}", triple.Subject.ReplaceInvalidChars()));
                }
                else
                {
                    sb.Append(triple.Subject.Value);
                }
                sb.Append(" ");

                if (terms.ContainsKey(triple.Predicate.Value))
                {
                    sb.Append(string.Format("?{0}", triple.Predicate.ReplaceInvalidChars()));
                }
                else
                {
                    sb.Append(triple.Predicate.Value);
                }
                sb.Append(" ");

                if (terms.ContainsKey(triple.Object.Value))
                {
                    sb.Append(string.Format("?{0}", triple.Object.ReplaceInvalidChars()));
                }
                else
                {
                    var Object = Utils.ObjectTreatment(triple.Object.Value);/*yudaObjectTreatment(triple.Object.Value);*/
                    sb.Append(Object);
                }

                sb.Append(" ");
                sb.Append(".");
                sb.AppendLine();
            }

            combine.Clear();
            foreach (var triple in this.Triples)
            {
                if (triple.Subject.Value.StartsWith("?") && triple.Subject.Filters != null)
                {
                    combine[triple.Subject.Value] = triple.Subject.Filters;
                }
                if (triple.Predicate.Value.StartsWith("?") && triple.Predicate.Filters != null)
                {
                    combine[triple.Predicate.Value] = triple.Predicate.Filters;
                }
                if (triple.Object.Value.StartsWith("?") && triple.Object.Filters != null)
                {
                    combine[triple.Object.Value] = triple.Object.Filters;
                }
            }

            foreach (var key in combine.Keys)
            {
                var combinedValues = CombineValues(combine[key], key, ",");
                if (!combinedValues.Equals(""))
                {
                    combinedValues = combinedValues.Remove(combinedValues.Length - 1, 1);
                    sb.AppendLine(string.Format("Filter({0} not in ({1})) .", key, combinedValues));
                }
            }

            //foreach (var triple in this.Triples)
            //{
            //    if(triple.Subject.Value.StartsWith("?"))
            //    {
            //        sb.AppendLine(string.Format("FILTER (!isBlank({0})) .", triple.Subject.Value));
            //    }
            //    if (triple.Predicate.Value.StartsWith("?"))
            //    {
            //        sb.AppendLine(string.Format("FILTER (!isBlank({0})) .", triple.Predicate.Value));
            //    }
            //    if (triple.Object.Value.StartsWith("?"))
            //    {
            //        sb.AppendLine(string.Format("FILTER (!isBlank({0})) .", triple.Object.Value));
            //    }
            //}

            sb.AppendLine("}");
            //sb.AppendLine(String.Format("group by {0}", groupBy));
            var limit = (jenaLimit != null ? jenaLimit : 100000);
            sb.AppendLine(string.Format("LIMIT {0}", limit.Value.ToString()));
            var query = sb.ToString();
            return query;
        }



        //yudaprivate string ObjectTreatment(string obj)
        //{
        //    if (obj.StartsWith("<") || obj.StartsWith("?"))
        //    {
        //        return obj;
        //    }
        //    else if (obj.EndsWith("@en"))
        //    {
        //        var split = obj.Split("@en");
        //        var ret = string.Format("\"{0}\"@en", split[0]);
        //        return ret;

        //    }
        //    else if (obj.EndsWith("@eng"))
        //    {
        //        var split = obj.Split("@eng");
        //        var ret = string.Format("\"{0}\"@eng", split[0]);
        //        return ret;
        //    }
        //    else
        //    {
        //        var ret = string.Format("\"{0}\"", obj);
        //        return ret;
        //    }
        //}
    }


}
