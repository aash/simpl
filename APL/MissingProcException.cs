using System;

namespace Simcraft
{
    class MissingProcException : Exception
    {
        public MissingProcException(string p)
            : base(p)
        {

        }
    }
}