using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI
{
    internal record class StateData(CString[] text, Operation[] options);

    internal record class CString(Color Color, string String)
    {
        public static implicit operator CString(string @this) => new CString(Color.White, @this);
        public int Length => String.Length;
    }
    internal record class Operation(CString OptionPrint, States NextState, params Action[] OnUsed);
}