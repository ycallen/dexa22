using System;


namespace BlazorUI
{
    using DM;
    using Force.DeepCloner;
    using Simon;    
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Originator
    {
        // For the sake of simplicity, the originator's state is stored inside a
        // single variable.
        public Simon Simon { get; set; }

        public Interaction Interaction { get; set; }

        public Candidate Candidate { get; set; } = null;


        public Originator(string query)
        {                  
            Interaction = new Interaction(query);
            Interaction.Candidate = new Candidate();
            try
            {
                Simon = new Simon(Interaction);
            }
            catch (Exception ex)
            {

                throw ex;
            }            
        }

        // The Originator's business logic may affect its internal state.
        // Therefore, the client should backup the state before launching
        // methods of the business logic via the save() method.
        public async Task /*void*/ GetNext(bool isSkip, CancellationToken tokenSource, Guid guid)
        {
            SparqlitException exception = new SparqlitException();
            Interaction.Candidate.SetFeedback();
            //publish to all subscribers so they can get value input by user
            Interaction.Candidate.Publish();
                        

            try
            {                
                if (Interaction.Candidate.IsVariableFeedbackChanged(this.Candidate))
                {
                    Interaction.IsChangeOperationRoot = true;
                }
                else
                {
                    Interaction.IsChangeOperationRoot = false;
                }                
                await Simon.GetNext(Interaction, isSkip, tokenSource);
                Interaction.Candidate.GetFeedbackUI();
                Interaction.Candidate.Triples.ForEach(x => x.FixFeedback());
                this.Candidate = Interaction.Candidate.DeepClone();
                
                //Subscribe to publisher. 
                //This is needed for cases where a variable appears more than once.
                //We only show one and subscribe the others to it.
                Interaction.Candidate.Sucbscribe();
            }
            catch (TaskCanceledException ex)
            {
                
                exception.Error = "Timeout for query was reached!";
                exception.ErrorDetail = ex.ToString();
                exception.Guid = guid;            
                throw exception;
            }
            catch (OperationCanceledException ex)
            {
                //SparqlitException exception = new SparqlitException();
                exception.Error = "Operation was canceled per your request!";
                exception.ErrorDetail = ex.ToString();
                exception.Guid = guid;
                throw exception;
            }
            catch (Exception ex)
            {
                //SparqlitException exception = new SparqlitException();
                exception.Error = "Unkown";
                exception.ErrorDetail = ex.ToString();
                exception.Guid = guid;
                throw exception;
            }



                   
        }

        

        // Saves the current state inside a memento.
        public IMemento Save()
        {
            return new ConcreteMemento(this.Simon.DeepClone(), this.Interaction.DeepClone()); ;
        }

        // Restores the Originator's state from a memento object.
        public void Restore(IMemento memento)
        {
            if (!(memento is ConcreteMemento))
            {
                throw new Exception("Unknown memento class " + memento.ToString());
            }

            (this.Simon, this.Interaction) = memento.GetState();            
        }
    }
}
