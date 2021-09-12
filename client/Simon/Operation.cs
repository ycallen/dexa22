using System;
using System.Collections.Generic;
using System.Text;
using TreeLib;
using Common;
using DM;
using Force.DeepCloner;
using System.Linq;

namespace Simon
{
    public class Operation
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public List<Operation> Children { get; set; }
        public Operation Parent { get; set; }
        public bool IsVisited { get; set; } = false;

        public int Cost { get; set; }

        public List<Triple> Triples { get; set; }

        public string TriplesStr { get; set; }

        public Operation(Operation parent, string path, List<Triple> query)
        {
            Parent = parent;
            Path = path;
            Id = Guid.NewGuid().ToString();
            Children = new List<Operation>();
            Triples = query;
            SetTriplesStr();
            Cost = GetCost(Triples);
        }

        //Check that operation doesn't have a subject that is
        //a literal
        public bool isValid()
        {
            var ret = true;
            foreach (var triple in Triples)
            {
                if (triple.Subject.IsFeedback == true &&
                    triple.Subject.IsAnchor == true &&
                    !triple.Subject.Value.StartsWith("?") &&
                    !triple.Subject.Value.StartsWith("<") &&
                    !Utils.IsEntity(triple.Subject.Value))
                {
                    ret = false;
                }
            }
            return ret;
        }

        //The cost of an operation is the number of distinct 
        //variables that it has
        private int GetCost(List<Triple> triples)
        {
            List<String> lst = new List<string>();
            foreach (var triple in triples)
            {
                triple.GetVariables().ForEach(x => lst.Add(x));                
            }
            int cnt = lst.Distinct().Count() * triples.Count();
            return cnt;
        }               

        
        //Set triple string that reresents the triples
        //as a string
        public void SetTriplesStr()
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
                sb.Append(" .");
                sb.Append("\n");
            }
            sb.Length -= 3;
            TriplesStr = sb.ToString();
        }

        public override bool Equals(object obj)
        {
            var node = obj as Operation;
            if (node != null)
            {
                return node.Id == this.Id;
            }
            return false;
        }

        

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        //The path of the operation from the root in the gragh
        public string GetPath(Operation current = null)
        {
            if (current == null)
                current = this;

            string result = string.Empty;

            // Traverse the list of parents backwards and
            // add each child to the path
            while (current != null)
            {
                result = "/" + current.Path + result;

                current = current.Parent;
            }

            return result;
        }

        public override string ToString()
        {
            if (Id == null)
                return "(null)";

            if (Id == string.Empty)
                return "(empty)";

            if (Parent == null)
                return Id;

            return string.Format("{0}  at: {1}", Id, GetPath());
        }

       

        //This function generates the children operations 
        //for the current  operation node.
        public void makeTree(HashSet<string> operationCache)
        {
            Exchange(this, operationCache);

            RelaxPredicate(this, operationCache);

            AddTripleObject(this, operationCache);

            AddTripleSubject(this, operationCache);
        }
        

        //String representation of operation to cache
        //so we don't resend queries that have already
        //been sent.
         public string getStringRepresentationForCache()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var triple in this.Triples)
            {
                sb.Append(triple.Subject.Value);
                sb.Append(" ");
                sb.Append(triple.Predicate.Value);
                sb.Append(" ");
                sb.Append(triple.Object.Value);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        //exchange subject with object in an 
        //arbitrary triple
        private Operation Exchange(Operation root, HashSet<string> operationCache)
        {
            for (int i = 0; i < root.Triples.Count; i++)
            {
                var triples = root.Triples.DeepClone();
                var triple = triples[i];                
                
                dynamic temp = new System.Dynamic.ExpandoObject();                
                temp.Value = triple.Subject.Value;
                temp.IsAnchor = triple.Subject.IsAnchor;
                temp.Anchor = triple.Subject.Anchor;
                temp.AnchorField = triple.Subject.AnchorField;
                temp.AnchorIndex = triple.Subject.AnchorIndex;
                temp.IsVariable = triple.Subject.IsVariable;
                temp.Variable = triple.Subject.Variable;
                temp.VariableField = triple.Subject.VariableField;
                temp.VariableIndex = triple.Subject.VariableIndex;
                //yuda
                temp.IsFeedback = triple.Subject.IsFeedback;

                triple.Subject.Value = triple.Object.Value;
                triple.Subject.IsAnchor = triple.Object.IsAnchor;
                triple.Subject.Anchor = triple.Object.Anchor;
                triple.Subject.AnchorField = triple.Object.AnchorField;
                triple.Subject.AnchorIndex = triple.Object.AnchorIndex;
                triple.Subject.IsVariable = triple.Object.IsVariable;
                triple.Subject.Variable = triple.Object.Variable;
                triple.Subject.VariableField = triple.Object.VariableField;
                triple.Subject.VariableIndex = triple.Object.VariableIndex;
                //yuda
                triple.Subject.IsFeedback = triple.Object.IsFeedback;

                triple.Object.Value = temp.Value;
                triple.Object.IsAnchor = temp.IsAnchor;
                triple.Object.Anchor = temp.Anchor;
                triple.Object.AnchorField = temp.AnchorField;
                triple.Object.AnchorIndex = temp.AnchorIndex;
                triple.Object.IsVariable = temp.IsVariable;
                triple.Object.Variable = temp.Variable;
                triple.Object.VariableField = temp.VariableField;
                triple.Object.VariableIndex = temp.VariableIndex;
                //yuda
                triple.Object.IsFeedback = temp.IsFeedback;

                var operation = new Operation(root, string.Format("exchage_{0}", i), triples);
                var representation = operation.getStringRepresentationForCache();

                if(!operationCache.Contains(representation))
                {

                    operationCache.Add(representation);
                    root.Children.Add(operation);
                }                
            }
            return root;
        }       
        
        //Change arbitrary predicate into a variable 
        private Operation RelaxPredicate(Operation root, HashSet<string> operationCache)
        {
            for (int i = 0; i < root.Triples.Count; i++)
            {
                var triples = root.Triples.DeepClone();
                var triple = triples[i];
                 //No need to change a variable for a variable               
                if (triple.Predicate.Value.StartsWith("?"))
                    continue;
                triple.Predicate.Value = "?p" + i;//generateVariable(triples);

                var operation = new Operation(root, string.Format("relax_predicate_{0}", i), triples);
                var representation = operation.getStringRepresentationForCache();

                if (!operationCache.Contains(representation))
                {
                    operationCache.Add(representation);
                    root.Children.Add(operation);
                }
            }
            return root;
        }

        //Add another triple for relaxation via the object
        private Operation AddTripleObject(Operation root, HashSet<string> operationCache)
        {
            for (int i = 0; i < root.Triples.Count; i++)
            {
                var triples = root.Triples.DeepClone();
                var triple = triples[i];

                //as not to make a variablr on a variable
                if (triple.Object.IsVariable)
                {
                    continue;
                }               

                var add = new Triple();
                int triplesCount = triples.Count;                
                add.Index = triplesCount;
                triples.Add(add);

                add.Object.Value = triple.Object.Value;
                add.Object.IsAnchor = triple.Object.IsAnchor;
                add.Object.Anchor = triple.Object.Anchor; 
                add.Object.AnchorField = triple.Object.AnchorField;
                add.Object.AnchorIndex = triple.Object.AnchorIndex;
                add.Object.IsVariable = triple.Object.IsVariable;
                add.Object.Variable = triple.Object.Variable;
                add.Object.VariableField = triple.Object.VariableField;
                add.Object.VariableIndex = triple.Object.VariableIndex;

                //yuda
                add.Object.IsFeedback = triple.Object.IsFeedback;

                add.Subject.Value = "?s" + add.Index;//generateVariable(triples);
                add.Subject.IsAnchor = false;
                add.Subject.IsVariable = true;
                add.Subject.Variable = add.Subject.Value;
                add.Subject.VariableField = "Subject";
                add.Subject.VariableIndex = add.Index;


                add.Predicate.Value = "?p" + add.Index;//generateVariable(triples);
                add.Predicate.IsAnchor = false;
                add.Predicate.IsVariable = true;
                add.Predicate.Variable = add.Predicate.Value;
                add.Predicate.VariableField = "Predicate";
                add.Predicate.VariableIndex = add.Index;


                triple.Object.Value = add.Subject.Value;
                triple.Object.IsVariable = true;
                triple.Object.Variable = add.Subject.Value;
                triple.Object.VariableField = "Object";
                triple.Object.VariableIndex = i;
                triple.Object.IsAnchor = false;
                triple.Object.Anchor = null;
                triple.Object.AnchorIndex = -1;
                triple.Object.AnchorField = null;                
                //yuda
                triple.Object.IsFeedback = null;


                var operation = new Operation(root, string.Format("add_triple_object_{0}", i), triples);
                var representation = operation.getStringRepresentationForCache();

                if (!operationCache.Contains(representation))
                {
                    operationCache.Add(representation);
                    root.Children.Add(operation);
                }
            }
            return root;
        }

        //Add another triple for relaxation via the subject
        private Operation AddTripleSubject(Operation root, HashSet<string> operationCache)
        {
            for (int i = 0; i < root.Triples.Count; i++)
            {
                var triples = root.Triples.DeepClone();
                var triple = triples[i];

                //as not to make a variable on a variable
                if (triple.Subject.IsVariable)
                {
                    continue;
                }
                

                var add = new Triple();
                int triplesCount = triples.Count;
                add.Index = triplesCount;
                triples.Add(add);
                
                add.Subject.Value = triple.Subject.Value;
                add.Subject.IsAnchor = triple.Subject.IsAnchor;
                add.Subject.Anchor = triple.Subject.Anchor;
                add.Subject.AnchorField = triple.Subject.AnchorField;
                add.Subject.AnchorIndex = triple.Subject.AnchorIndex;
                add.Subject.IsVariable = triple.Subject.IsVariable;
                add.Subject.Variable = triple.Subject.Value;
                add.Subject.VariableField = triple.Subject.VariableField;
                add.Subject.VariableIndex = triple.Subject.VariableIndex;
                //yuda
                add.Subject.IsFeedback = triple.Subject.IsFeedback;

                add.Predicate.Value = "?p" + add.Index;//generateVariable(triples);
                add.Predicate.IsAnchor = false;
                add.Predicate.IsVariable = true;
                add.Predicate.Variable = add.Predicate.Value;
                add.Predicate.VariableField = "Predicate";
                add.Predicate.VariableIndex = add.Index;


                add.Object.Value = "?o" + add.Index;//generateVariable(triples);
                add.Object.IsAnchor = false;
                add.Object.IsVariable = true;
                add.Object.Variable = add.Object.Value;
                add.Object.VariableField = "Object";
                add.Object.VariableIndex = add.Index;


                triple.Subject.Value = add.Object.Value;
                triple.Subject.IsVariable = true;
                triple.Subject.Variable = add.Object.Value;
                triple.Subject.VariableField = "Subject";
                triple.Subject.VariableIndex = i;
                triple.Subject.IsAnchor = false;
                triple.Subject.Anchor = null;
                triple.Subject.AnchorField = null;
                triple.Subject.AnchorIndex = -1;
                //yuda
                //yuda
                triple.Subject.IsFeedback = null;


                var operation = new Operation(root, string.Format("add_triple_subject_{0}", i), triples);
                var representation = operation.getStringRepresentationForCache();

                if (!operationCache.Contains(representation))
                {
                    operationCache.Add(representation);
                    root.Children.Add(operation);
                }                
            }
            return root;
        }
    }
}

