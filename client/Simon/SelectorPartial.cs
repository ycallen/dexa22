using Common;
using DM;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simon
{
    public class SelectorPartial : ISelector
    {
        //public C5.IPriorityQueue<Candidate> Heap { get; set; } = new C5.IntervalHeap<Candidate>(CandidateComparer);

        public Queue<Candidate> Heap { get; set; } = new Queue<Candidate>();

        public SelectorPartial(Dictionary<string, List<string>> terms, Operation operation, bool IsCalcEditDistance)
        {            
            if (IsCalcEditDistance)
            {
                var new_terms = new Dictionary<string, List<string>>();
                foreach (var key in terms.Keys)
                {
                    new_terms[key] = new List<string>();
                    foreach (var item in terms[key])
                    {
                        new_terms[key].Add(Utils.shorten(item));
                    }
                    new_terms[key] = new_terms[key].OrderBy(x => Algorithms.Levenshtein.ComputeDistance(key, x)).ToList();
                }
                terms = new_terms;       
            }

            int numOfCandidates = 0;

            foreach (var key in terms.Keys)
            {
                var cnt = terms[key].Count;
                if (cnt > numOfCandidates)
                    numOfCandidates = cnt;
            }        
            
            var candidates = new List<Candidate>();
            for (int i = 0; i < numOfCandidates; i++)
            {
                var candidate = new Candidate();
                candidate.Triples = operation.Triples.DeepClone();
                candidate.Triples.ForEach(x => x.Shorten());
                foreach (var triple in candidate.Triples)
                {
                    if(terms.ContainsKey(triple.Subject.Value))
                    {
                        if(terms[triple.Subject.Value].Count > i)
                        {
                            triple.Subject.Value = terms[triple.Subject.Value][i];
                        }
                        else
                        {
                            triple.Subject.Value = "#empty#";
                        }
                    }                    


                    if (terms.ContainsKey(triple.Predicate.Value))
                    {
                        if (terms[triple.Predicate.Value].Count > i)
                        {
                            triple.Predicate.Value = terms[triple.Predicate.Value][i];
                        }
                        else
                        {
                            triple.Predicate.Value = "#empty#";
                        }
                    }                    


                    if (terms.ContainsKey(triple.Object.Value))
                    {
                        if (terms[triple.Object.Value].Count > i)
                        {
                            triple.Object.Value = terms[triple.Object.Value][i];
                        }
                        else
                        {
                            triple.Object.Value = "#empty#";
                        }
                    }                    
                }
                candidate.EditDistance = i;
                candidates.Add(candidate);                
            }
            candidates.ForEach(x => x.SetQuery());
            candidates.ForEach(x => x.SetFullQuery());
            candidates.ForEach(x => Heap.Enqueue(x));
        }
                

        public int Count()
        {
            return Heap.Count;
        }
       

        //Check if priority queue is empty
        public bool IsEmpty()
        {
            return Heap.Count() == 0;
        }

        //Get next candidate
        public Candidate GetNext()
        {
            var candidate = Heap.Dequeue();
            return candidate;
        }

        public void FilterByFeedback(Candidate candidate)
        {            
            var heap = Heap.ToList();
            var newHeap = new Queue<Candidate>();
            foreach (var item in heap)
            {                
                foreach (var triple in item.Triples)
                {
                    if(candidate.Triples[triple.Index].Subject.IsFeedback == true)
                    {                        
                        triple.Subject.Value = candidate.Triples[triple.Index].Subject.Value;
                    }
                    if (candidate.Triples[triple.Index].Predicate.IsFeedback == true)
                    {
                        triple.Predicate.Value = candidate.Triples[triple.Index].Predicate.Value; ;
                    }
                    if (candidate.Triples[triple.Index].Object.IsFeedback == true)
                    {
                        triple.Object.Value = candidate.Triples[triple.Index].Object.Value;
                    }                    
                }                                
            }
            Candidate lastCandidate = null;
            foreach (var item in heap)
            {
                if (lastCandidate != null)
                {
                    if (lastCandidate.IsCandidateEqual(item))
                        break;
                }
                lastCandidate = item.DeepClone();
                newHeap.Enqueue(item);
            }
            Heap = newHeap;
        }
        
    }
}
