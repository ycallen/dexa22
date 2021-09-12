using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class Entity : IPublisher
    {
        public string Value { get; set; }

        public bool? IsFeedback { get; set; } = null;

        public bool? IsFeedbackUI { get; set; } = false;

        //This is a trick that if the user in the last round gave positive 
        //feedback to an entity then it will be positive and disacbled in gui
        public bool IsFeedbackFixed { get; set; } = false;
        
        public bool IsDisplayed { get; set; } = true;


        public bool IsAnchor { get; set; } = false;

        public string Anchor { get; set; } = null;

        public int AnchorIndex { get; set; } = -1;

        public string AnchorField { get; set; } = null;

        public bool IsSynonymValue { get; set; } = false;        

        //Just like anchor has anchor and isAnchor properties, so a generated variable has an variable and isVariable properties
        public bool IsVariable { get; set; } = false;

        public string Variable { get; set; } = null;

        public string VariableField { get; set; } = null;

        public int VariableIndex { get; set; } = -1;

        public string Prefix { get; set; }

        public List<string> Filters { get; set; }

        public Entity(string prefix)
        {
            Prefix = prefix;
        }


        public bool IsEqual(Entity entity)
        {
            if(this.Value.Equals(entity.Value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Shorten()
        {
            if(Value.StartsWith("<") || Value.Contains("^^"))
            {
                Value = Utils.shorten(Value);
            }
        }
        public string GetElement()
        {
            string elem = "";            
            if (IsAnchor && Anchor.StartsWith("?"))
            {
                elem = Anchor;                
            }
            else if (IsVariable)
            {
                if (VariableField.Equals("Predicate"))
                {
                    elem = Value;
                }
                else
                {
                    elem = Variable;
                }                
            }
            else
            {
                elem = Value;                
            }
            return elem;
        }

        public void SetFeedback()
        {
            if (IsFeedbackUI == true)
                IsFeedback = true;
            if (IsFeedbackUI == null)
                IsFeedback = false;
            if (IsFeedbackUI == false)
                IsFeedback = null;
        }

        public void GetFeedbackUI()
        {
            if (IsFeedback == true)
                IsFeedbackUI = true;
            if (IsFeedback == null)
                IsFeedbackUI = false;
            if (IsFeedback == false)
                IsFeedbackUI = null;
        }

        public string GetAssignment()
        {            
            string assignment = "";
            if (IsAnchor && Anchor.StartsWith("?"))
            {
                assignment = Value;
            }
            else if (IsVariable)
            {
                if(VariableField.Equals("Predicate"))
                {
                    assignment = "";
                }
                else
                {
                    assignment = Value;
                }                
            }
            else
            {
                assignment = "";
            }
            return assignment;
        }

        //Publisher
        public event EventHandler<Message> Handler;

        public void OnPublish(Message msg)
        {
            Handler?.Invoke(this, msg);
        }

        public void Publish()
        {
            Message msg = (Message)Activator.CreateInstance(typeof(Message), IsFeedback);
            OnPublish(msg);
        }

        //Subscriber
        public IPublisher Publisher { get; set; }
        public void Subscribe(IPublisher publisher)
        {
            Publisher = publisher;
            Publisher.Handler += Publisher_Handler;
        }

        private void Publisher_Handler(object sender, Message e)
        {
            this.IsFeedback = e.Content;
        }

        public string GetKey()
        {
            string ret = String.Format("{0}_{1}_{2}", Anchor, AnchorField, AnchorIndex.ToString()); ;
            if(IsVariable)
            {
                ret = String.Format("var_{0}_{1}_{2}", Variable, VariableField, VariableIndex.ToString());
            }
            return ret;
        }

        public void FixFeedback()
        {
            if(IsFeedback == true)
            {
                IsFeedbackFixed = true;
            }
            else
            {
                IsFeedbackFixed = false;
            }
        }
    }
}
