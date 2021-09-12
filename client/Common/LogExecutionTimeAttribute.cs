using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common
{
    [Serializable]
    //Aop class to log execution time of methods
    public class LogExecutionTimeAttribute : OnMethodBoundaryAspect
    {
        [NonSerialized]
        public Stopwatch StopWatch = new Stopwatch();
        public string FuncName { get; set; }
        public override void OnEntry(MethodExecutionArgs args)
        {
            StopWatch = new Stopwatch();
            StopWatch.Start();
        }

        
        public override void OnExit(MethodExecutionArgs args)
        {
            double elapsedTime = this.StopWatch.Elapsed.TotalMilliseconds;
            this.StopWatch.Stop();
            Console.Out.WriteLine(String.Format("Function {0}.{1} : {2} ms", args.Method.DeclaringType.Name, args.Method.Name,elapsedTime.ToString("N2")));
        }
    }
}
