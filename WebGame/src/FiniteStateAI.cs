using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI
{
    internal class FiniteStateAI<T> where T : Enum
    {
        public T CurrentState { get; private set; }

        private Dictionary<T, Func<StateData, StateData>> States = new();

        private Func<StateData, StateData> ActiveState;

        public void AddState(T Enum, Func<StateData, StateData> Update)
            => States.Add(Enum, Update);

        private StateData NowData;

        public StateData Update(T newState)
        {
            Switch(newState);
            NowData = ActiveState(NowData);
            return NowData;
        }

        public void Switch(T NextState)
        {
            ActiveState = States.TryGetValue(NextState, out var Value) ? Value : throw new Exception("That aint a state bro");
            CurrentState = NextState;
        }
    }
}