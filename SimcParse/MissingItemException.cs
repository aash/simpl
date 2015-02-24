using System;

namespace SimcParse
{
    class MissingItemException : Exception
    {
        public MissingItemException(string p) : base(p)
        {
 
        }
    }
}