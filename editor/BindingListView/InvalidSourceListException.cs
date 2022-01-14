using System;
using System.Runtime.Serialization;

namespace sth1edwv.BindingListView
{
    [Serializable]
    public class InvalidSourceListException : Exception
    {
        public InvalidSourceListException()
            : base("InvalidSourceList")
        {
            
        }

        public InvalidSourceListException(string message)
            : base(message)
        {

        }

        public InvalidSourceListException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
