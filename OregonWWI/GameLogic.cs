using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace OregonWWI
{
    internal static class GameLogic
    {
        public static void Initalize(FiniteStateAI<States> finiteStateAI)
        {
            finiteStateAI.AddState(States.ChooseCharacter, ChooseCharacter);
            finiteStateAI.AddState(States.US, US);
            finiteStateAI.AddState(States.US_Army, null);
            finiteStateAI.AddState(States.US_Marines, null);
        }

        private static StateData ChooseCharacter(StateData prev)
        {
            return new StateData(
                new CString[] {
                    "Choose your starting country:"
                },
                new Operation[] {
                    new Operation("[1] United States", States.US_Army)
                });
        }

        private static StateData US(StateData prev)
        {
            return new StateData(
                new CString[] {
                    "What branch of the US military?"
                },
                new Operation[] {
                    new Operation("United States Army", States.US_Army),
                    new Operation("United States Marines", States.US_Marines)
                });
        }
    }

    internal enum States
    {
        ChooseCharacter,
        US_Army,
        US_Marines,
        US,
    }
}
