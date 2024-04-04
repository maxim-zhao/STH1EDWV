using System;

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
    }
}
