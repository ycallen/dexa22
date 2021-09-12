using Common;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DM
{
    [Serializable]
    public class Triple
    {
        [PropertyName("subject")]
        public Entity Subject { get; set; }

        [PropertyName("predicate")]
        public Entity Predicate { get; set; }

        [PropertyName("object")]
        public Entity Object { get; set; }

        public int Index { get; set; }

        public int Priority { get; set; }

        public bool? IsLiteral { get; set; }

        public Dictionary<string, List<string>> Terms { get; set; } = new Dictionary<string, List<string>>();

        public bool IsSentToJena { get; set; } = false;

        
        public Triple()
        {
            Terms[TripleFields.Subject.ToString()] = null;
            Terms[TripleFields.Predicate.ToString()] = null;
            Terms[TripleFields.Object.ToString()] = null;            
        }


        public string GetFieldValue(TripleFields field)
        {
            var ret = "";
            if (field == TripleFields.Subject)
                ret = Subject.Value;
            if (field == TripleFields.Predicate)
                ret = Predicate.Value;
            if (field == TripleFields.Object)
                ret = Object.Value;
            return ret;
        }        

        public void getTerms(Dictionary<string, List<string>> termsOrig, Dictionary<string, List<string>> terms)
        {
            Subject.getTerms(termsOrig, terms);
            Predicate.getTerms(termsOrig, terms);
            Object.getTerms(termsOrig, terms);
        }


        public TripleFields GetVariableField(string variable)
        {
            if (Subject.Equals(variable))
            {
                return TripleFields.Subject;
            }
            if (Predicate.Equals(variable))
            {
                return TripleFields.Predicate;
            }
            if (Object.Equals(variable))
            {
                return TripleFields.Object;
            }
            return TripleFields.None;
        }

        public List<string> GetAllVariables()
        {
            var variables = new List<string>();
            if (Subject.Value.StartsWith("?"))
            {
                variables.Add(Subject.Value);
            }
            if (Predicate.Value.StartsWith("?"))
            {
                variables.Add(Predicate.Value);
            }
            if (Object.Value.StartsWith("?"))
            {
                variables.Add(Object.Value);
            }
            return variables;
        }

        public void AddVariablesToTerms(TripleQuery query, Dictionary<string, List<string>> terms)
        {
            Subject.AddVariablesToTerms(query.Triples[Index].Subject, terms);
            Predicate.AddVariablesToTerms(query.Triples[Index].Predicate, terms);
            Object.AddVariablesToTerms(query.Triples[Index].Object, terms);
        }

        public List<string> GetAllElements()
        {
            var elements = new List<string>();
            
            elements.Add(Subject.Value);

            elements.Add(Predicate.Value);

            elements.Add(Object.Value);
            
            return elements;
        }

        public List<string> GetAllNonVariables()
        {
            var variables = new List<string>();
            if (!Subject.Value.StartsWith("?") && !Utils.IsEntity(Subject.Value))
            {
                variables.Add(Subject.Value);
            }
            if (!Predicate.Value.StartsWith("?") && !Utils.IsEntity(Predicate.Value))
            {
                variables.Add(Predicate.Value);
            }
            if (!Object.Value.StartsWith("?") && !Utils.IsEntity(Object.Value))
            {
                variables.Add(Object.Value);
            }
            return variables;
        }

        public bool isAllVariables()
        {
            if(Subject.Value.StartsWith("?") && Predicate.Value.StartsWith("?") && Object.Value.StartsWith("?"))
            {
                return true;
            }
            return false;
        }

        public Dictionary<string, TripleFields> getVariables()
        {
            var dic = new Dictionary<string, TripleFields>();
            if (Subject.Value.StartsWith("?"))
            {
                dic[Subject.Value] = TripleFields.Subject;
            }
            if (Predicate.Value.StartsWith("?"))
            {
                dic[Predicate.Value] = TripleFields.Predicate;             
            }
            if (Object.Value.StartsWith("?"))
            {
                dic[Object.Value] = TripleFields.Object;            
            }
            return dic;
        }


        public int GetVariableCount()
        {
            var variables = getVariables().Keys;
            return variables.Count(); 
        }

        public int GetVariableIntersectCount(HashSet<string> externalVariables)
        {
            var internalVariables = getVariables().Keys;
            var intersectCount = internalVariables.Intersect(externalVariables).Count();
            return intersectCount;
        }

        public List<string> getVariablesByOrder(HashSet<string> externalVariables)
        {
            var lst = new List<string>();
            var internalVariables = getVariables().Keys;
            var intersect = internalVariables.Intersect(externalVariables);
            var except = internalVariables.Except(externalVariables);
            lst.AddRange(intersect);
            lst.AddRange(except);
            return lst;
        }

        public bool IsKeyWord(TripleFields field)
        {
            var ret = false;
            if(field == TripleFields.Subject)
            {
                ret =  (Subject.IsFeedback == true && Subject.IsAnchor);
            }

            if (field == TripleFields.Predicate)
            {
                ret = (Predicate.IsFeedback == true && Predicate.IsAnchor);
            }

            if (field == TripleFields.Object)
            {
                ret = (Object.IsFeedback == true && Object.IsAnchor);
            }

            return ret;
        }
    }
}
