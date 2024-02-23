﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI.Neoregon
{
    internal record class Option(Func<ITurn> GetNext, IChecker Checker, CString Name, Action OnUsed = null);

    internal class AnyChecker : IChecker
    {
        private Func<string, bool> _Check;
        private Action<string> Used;
        public AnyChecker(Func<string, bool> Check, Action<string> onSucsess)
        { 
            this._Check = Check;
            Used = onSucsess;
        }

        public bool Check(string input)
        {
            if(_Check(input))
            {
                Used(input);
                return true;
            }
            return false;
        }
    }

    internal class Checker : IChecker
    {
        public static Checker Get(char input)
        {
            if (checkerCache.TryGetValue(input, out var v))
            {
                return v;
            }
            else
            {
                var newChecker = new Checker(input);
                checkerCache.Add(input, newChecker);
                return newChecker;
            }
        }
        static Checker()
        {

        }


        public bool Check(string input) => input.Contains(str);
        public override int GetHashCode() => str.GetHashCode();

        private static readonly Dictionary<char, Checker> checkerCache = new();
        private char str;
        private Checker(char match) => str = match;
        public static implicit operator Checker(char @char) => Get(@char);
    }

    internal interface IChecker
    {
        bool Check(string input);
    }
}
