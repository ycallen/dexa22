using DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Caching;
using TreeLib.Models;
using Force.DeepCloner;
using System.Windows;

namespace Simon
{
    public class OperationsManager
    {
        public Operation Root { get; set; }

        public HashSet<string> operationCache { get; set; } = new HashSet<string>();        


        public void Init(List<Triple> triples)
        {
            for (int i = 0; i < triples.Count; i++)
            {
                triples[i].Subject.IsAnchor = true;
                triples[i].Subject.Anchor = triples[i].Subject.Value;
                triples[i].Subject.AnchorIndex = i;
                triples[i].Subject.AnchorField = "Subject";
                triples[i].Subject.IsFeedback = null;
                triples[i].Predicate.IsAnchor = true;
                triples[i].Predicate.Anchor = triples[i].Predicate.Value;
                triples[i].Predicate.AnchorIndex = i;
                triples[i].Predicate.AnchorField = "Predicate";
                triples[i].Predicate.IsFeedback = null;
                triples[i].Object.IsAnchor = true;
                triples[i].Object.Anchor = triples[i].Object.Value;
                triples[i].Object.AnchorIndex = i;
                triples[i].Object.AnchorField = "Object";
                triples[i].Object.IsFeedback = null;
            }

            var root = new Operation(null, string.Format("root"), triples);
            var representation = root.getStringRepresentationForCache();
            operationCache.Add(representation);
            Root = root;            
        }

        public void ChangeRoot(Operation operation)
        {
            var root = new Operation(null, string.Format("root"), operation.Triples);

            Root = root;
            
            Root.IsVisited = true;
            
            Root.makeTree(operationCache);
        }
                       
        
        //Get next operation by cost
        public Operation GetNext()
        {
            int cost = int.MaxValue;
            var items = TreeLib.BreadthFirst.Traverse.LevelOrder(Root, i => i.Children);

            Operation ret = null;
            foreach (var item in items)
            {
                
                if (item.Node.IsVisited == false &&
                    item.Node.Cost < cost)
                {
                    ret = item.Node;
                    cost = item.Node.Cost;                    
                }     
                                
            }            
            ret.IsVisited = true;
            ret.makeTree(operationCache);
            return ret; 
        }

    }
}
