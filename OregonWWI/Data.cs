using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI
{
    internal static class CharacterInformation
    {
        public static Prob SurvivalOnAttack = new(0.5f);
        public static Prob SurvivalOnDefence = new(0.5f);   

        public static int TurnsSurvived;
        public static int KillCount;
        public static int Cowardice;
        public static string Title;
    }


    internal struct Prob
    {
        public float Value => Math.Clamp(0, 1, _value);
        private float _value;

        public Prob(float value)
        {
            _value = value;
        }

        public static implicit operator bool(Prob @this) => Random.Shared.NextSingle() < @this.Value;

        public static Prob operator +(Prob left, Prob right) => new(left.Value + right.Value);
        public static Prob operator -(Prob left, Prob right) => new(left.Value - right.Value);
        public static Prob operator *(Prob left, Prob right) => new(left.Value * right.Value);
        public static Prob operator /(Prob left, Prob right) => new(left.Value / right.Value);
    }

}
