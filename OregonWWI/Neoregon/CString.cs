using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI.Neoregon
{
    internal record class CString(string String, Color Color, Vector2 Location, Vector2[] offsets = null)
    {
        public int Length => String.Length;
        public char this[int index] => String[index];
    }
}