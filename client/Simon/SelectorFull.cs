using DM;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simon
{
    public class SelectorFull : ISelector
    {
        public C5.IPriorityQueue<Candidate> Heap { get; set; } = new C5.IntervalHeap<Candidate>(new CandidateComparer());

        public SelectorFull(List<Candidate> candidates, Operation operation, bool IsCalcEditDistance, bool isUseSynonym)
        {
            if (IsCalcEditDistance)
            {
                candidates.ForEach(x => x.CalcEditDistance(operation.Triples, isUseSynonym));
            }
            candidates.ForEach(x => x.SetQuery());
            candidates.ForEach(x => x.SetFullQuery());
            candidates.ForEach(x => Heap.Add(x));
        }
        

        
        public int Count()
        {
            return Heap.Count;
        }

        
        //Check if priority queue is empty
        public bool IsEmpty()
        {
            return Heap.IsEmpty;
        }

        //Get next candidate
        public Candidate GetNext()
        {
            var candidate = Heap.DeleteMin();
            return candidate;
        }

        //Check if should be filtered entity by entity
        private static bool FilterByFeedback(Entity candidate, Entity item)
        {
            bool isValid = true;
            if (item.Value.Equals(candidate.Value))
            {
                if (candidate.IsFeedback == true)
                {
                    item.IsFeedback = true;
                }
                //null or false
                else
                {
                    //In all other cases the                     
                    if(candidate.Anchor!=null && !candidate.Anchor.StartsWith("?"))
                    {
                        isValid = false;
                    }                                        
                }
            }
            if (!item.Value.Equals(candidate.Value))
            {
                if (candidate.IsFeedback == true)
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        //Filter candidates by user feedback
        //It should be noted that entities that
        //are not anchors are only filtered for
        //the candidates of the current operation.
        public void FilterByFeedback(Candidate candidate)
        {
            var heap = Heap.ToList();
            var newHeap = new C5.IntervalHeap<Candidate>(new CandidateComparer());
            foreach (var item in heap)
            {
               

                var isValid = true;
                for (int i = 0; i < item.Triples.Count; i++)
                {
                    var isValid1 = FilterByFeedback(candidate.Triples[i].Subject, item.Triples[i].Subject);

                    var isValid2 = FilterByFeedback(candidate.Triples[i].Predicate, item.Triples[i].Predicate);

                    var isValid3 = FilterByFeedback(candidate.Triples[i].Object, item.Triples[i].Object);

                    isValid = isValid1 && isValid2 && isValid3;

                    if (!isValid)
                        break;
                }
                if (isValid == true)
                {
                    newHeap.Add(item);
                }

            }
            Heap = newHeap;
        }
        
    }
}
