using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Tasks
{

    public class TaskerAggregateException : Exception
    {
        public List<Exception> InnerExceptions { get; private set; } = new List<Exception>();
        
        public TaskerAggregateException(string message, List<Exception> inner) : base(message)
        {
            InnerExceptions = inner;
        }
        protected TaskerAggregateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());

            foreach (var ex in InnerExceptions)
                sb.Append(ex.Message);

            return sb.ToString();
        }

    }
    
}
