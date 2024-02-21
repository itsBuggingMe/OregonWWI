using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI
{
    internal class CharacterInformation
    {
        private static CharacterInformation _info;
        public static CharacterInformation Info 
        { 
            get
            {
                _info ??= new CharacterInformation();
                return _info;
            }
        }

        public Prob SurvivalOnAttack = new(0.5f);
        public Prob SurvivalOnDefence = new(0.5f);   

        public int TurnsSurvived;
        public int KillCount;
        public int Cowardice;
        public string Title;
        public int Money = Random.Shared.Next(1, 10);
        public int ShopItemCount;
        public string DeathCause;

        public int EquipLevel = 0;

        public string OffensiveName;

        public string UserInput;

        public StateData AfterShop;
        public StateData CustomState;

        public static void Reset() => _info = null;
    }


    internal struct Prob
    {
        public float Value => Math.Clamp(_value, 0, 1);
        private float _value;

        public Prob(float value)
        {
            _value = value;
        }

        public static implicit operator bool(Prob @this) => Random.Shared.NextSingle() < @this.Value;
        public static implicit operator Prob(float @this) => new Prob(@this);

        public static Prob operator +(Prob left, Prob right) => new(left.Value + right.Value);
        public static Prob operator -(Prob left, Prob right) => new(left.Value - right.Value);
        public static Prob operator *(Prob left, Prob right) => new(left.Value * right.Value);
        public static Prob operator /(Prob left, Prob right) => new(left.Value / right.Value);
    }

}
