using Client;
using Common;
using Config;
using DM;
using Meziantou.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simon
{
    //This class is for the automatic tests.
    //This class simulates a user.
    public class User
    {

        //Load the config file that gives the query input
        //and the answer expected.
        public User(YamlConfig config)
        {
            InputQuery = config.UserQuery;
            CorrectTriples = ExtractTriplesFromQuery.Send(config.CorrectQuery).ToList();
            QueryTriples = ExtractTriplesFromQuery.Send(config.UserQuery).ToList();
            PopulateFields();
        }

        private void PopulateFields()
        {
            for (int i = 0; i < QueryTriples.Count; i++)
            {
                if ((i + 1) > CorrectTriples.Count)
                    break;
                var subject = new Entity("S");
                subject.Value = CorrectTriples[i].Subject.Value;
                subject.IsAnchor = true;
                subject.Anchor = QueryTriples[i].Subject.Value;
                subject.AnchorIndex = i;
                subject.AnchorField = "Subject";
                subject.IsFeedback = null;
                Entities[subject.GetKey()] = subject;


                var predicate = new Entity("P");
                predicate.IsAnchor = true;
                predicate.Value = CorrectTriples[i].Predicate.Value;
                predicate.Anchor = QueryTriples[i].Predicate.Value;
                predicate.AnchorIndex = i;
                predicate.AnchorField = "Predicate";
                predicate.IsFeedback = null;
                Entities[predicate.GetKey()] = predicate;

                var obj = new Entity("O");
                obj.IsAnchor = true;
                obj.Value = CorrectTriples[i].Object.Value;
                obj.Anchor = QueryTriples[i].Object.Value;
                obj.AnchorIndex = i;
                obj.AnchorField = "Object";
                obj.IsFeedback = null;
                Entities[obj.GetKey()] = obj;
            }
        }

        private void GiveFeedback(Entity candidate, Entity correct, ref bool ret)
        {                                        
            if (Entities.ContainsKey(candidate.GetKey()))
            {
                
                if (Entities[candidate.GetKey()].Value.Replace("\"", "").Equals(candidate.Value))
                {
                    candidate.IsFeedback = true;
                }
                /*else if (candidate.Value.Equals("6' 2\"@en"))
                {
                    candidate.IsFeedback = true;
                }*/
                else
                {
                    candidate.IsFeedback = false;
                    ret = false;
                }

                
            }
            else if (correct != null)
            {
                if (correct.Value.Replace("\"", "").Equals(candidate.Value))
                {
                    candidate.IsFeedback = true;
                }
                else
                {
                    candidate.IsFeedback = false;
                    ret = false;
                }
            }
            else
            {
                candidate.IsFeedback = false;
                ret = false;
            }
        }

        //This function simulates the user giving feedback
        public bool GiveFeedBack(Interaction interaction)
        {

            var candidate = interaction.Candidate;

            var ret = true;

            //Added for goldman
            if (candidate.Triples.Count > CorrectTriples.Count)
                return false;

            for (int i = 0; i < candidate.Triples.Count; i++)
            {
                GiveFeedback(candidate.Triples[i].Subject,
                             CorrectTriples.Count >= i ? CorrectTriples[i].Subject : null, ref ret);
                GiveFeedback(candidate.Triples[i].Predicate,
                             CorrectTriples.Count >= i ? CorrectTriples[i].Predicate : null, ref ret);
                GiveFeedback(candidate.Triples[i].Object,
                             CorrectTriples.Count >= i ? CorrectTriples[i].Object : null, ref ret);

            }
            return ret;
        }

        public string InputQuery { get; set; }

        public List<Triple> CorrectTriples { get; set; }

        public List<Triple> QueryTriples { get; set; }

        public Dictionary<string, Entity> Entities { get; set; } = new Dictionary<string, Entity>();

    }
}
