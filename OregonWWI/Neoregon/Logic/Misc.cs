using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI.Neoregon.Logic
{
    internal class Misc
    {
        public static readonly GenericTextTurn Start = new(
            new CString[] {
                "hello world",
            },
            new Option[] {
                new Option(Error, Checker.Get('1'), "[1] Test 1")
            });

        public static readonly GenericTextTurn Error = new(
            new CString[] {
                "",
            },
            new Option[] {
                new Option(Start, Checker.Get('1'), "[1] Back to start")
            });
    }
}
