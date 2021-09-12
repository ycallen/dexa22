using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class Entity 
    {        
        public string Value { get; set; }

        public bool? IsFeedback { get; set; }

        
        public string Synonyms { get; set; }

        public List<String> UriSynonyms { get; set; }

        public bool IsSynonymValue { get; set; } = false;

        public bool IsAnchor { get; set; } = false;

        public bool IsDisplayed { get; set; }

        public string Anchor { get; set; } = null;

        public int AnchorIndex { get; set; } = -1;

        public string AnchorField { get; set; } = null;

        public bool IsVariable { get; set; }//= false;

        public string Variable { get; set; }// = null;

        public string VariableField { get; set; }// = null;

        public int VariableIndex { get; set; }// = -1;

        public List<string> Filters { get; set; }

        public List<string> Terms { get; set; }

        public bool IsDiplayedVariable()
        {
            if (IsAnchor == true && Value.StartsWith("?") && Anchor.StartsWith("?"))
                return true;
            if (IsVariable)
                return true;
            return false;
        }

        public void getTerms(Dictionary<string, List<string>> termsOrig, Dictionary<string, List<string>> terms)
        {
            if (termsOrig.ContainsKey(Value))
            {
                terms[Value] = termsOrig[Value];
            }
        }
       

        public void AddVariablesToTerms(Entity queryEntity, Dictionary<string, List<string>> terms)
        {
            if(queryEntity.Value.StartsWith("?"))
            {
                if(!terms.ContainsKey(queryEntity.Value))
                {
                    terms[queryEntity.Value] = new List<string>();
                }
                if(!terms[queryEntity.Value].Contains(this.Value))
                {                                       
                        terms[queryEntity.Value].Add(this.Value);                                     
                }                
            }
        }

        public string ReplaceInvalidChars()
        {
            var ret = Utils.ReplaceInvalidChars(Value);//Value.Replace(".", "").Replace("'", "").Replace("’","");
            return ret;
        }

    }
}
