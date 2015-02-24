using System;
using Styx;

namespace Simcraft
{
    public class Behavior : Attribute
    {
        private readonly WoWClass _class;
        private readonly WoWContext _context;
        private readonly WoWSpec _spec;

        public Behavior(WoWClass cl, WoWSpec sp, WoWContext cont)
        {
            _class = cl;
            _spec = sp;
            _context = cont;
        }

        public bool Match(WoWClass cl, WoWSpec sp, WoWContext cont)
        {
            return (cl == _class &&  cont == _context);
        }

        public override String ToString()
        {
            return _spec + " " + _class + " in " + _context;
        }

        /*public override bool Equals(object obj)
        {
            //return base.Equals(obj);
        }*/
    }
}