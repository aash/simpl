using System;

namespace Simcraft
{
    class MissingItemException : Exception
    {
        public MissingItemException(string p)
            : base(p)
        {

        }
    }
}