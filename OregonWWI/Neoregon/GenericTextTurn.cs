using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI.Neoregon
{
    internal class GenericTextTurn : ITurn
    {
        private CString[] Text;
        private Option[] Options;
        public GenericTextTurn(CString[] Top, Option[] Options)
        {
            Text = Top;
            this.Options = Options;
        }

        public GenericTextTurn(CString Top, Option Options) : this(new CString[] { Top }, new Option[] { Options })
        {
        }
        public GenericTextTurn(CString[] Top, Option Options) : this(Top, new Option[] { Options })
        {
        }
        public GenericTextTurn(CString Top, Option[] Options) : this(new CString[] { Top }, Options)
        {
        }
        const int padding = 24;
        public void Draw(int totalCharactersDrawn)
        {
            int charsDrawn = 0;
            Vector2 cumulativeLocation = Vector2.One * padding;

            bool hasSkipped = false;

            foreach (var str in Text)
            {
                if (hasSkipped)
                    break;
                DrStr(str);
            }

            foreach (var str in Options)
            {
                if (hasSkipped)
                    break;
                DrStr(str.Name);
            }

            void DrStr(CString str)
            {
                int potentialCharsDrawn = str.Length + charsDrawn;
                if (potentialCharsDrawn > totalCharactersDrawn)
                {
                    str = new CString(str.String[..(str.Length + totalCharactersDrawn - potentialCharsDrawn)], str.Color);
                    hasSkipped = true;
                }


                Vector2 loc = GameRoot.Font.MeasureString(str.String);
                GameRoot.Batch.DrawString(GameRoot.Font, str.String, cumulativeLocation, str.Color);

                cumulativeLocation += new Vector2(0, loc.Y);
                charsDrawn += str.Length;
            }
        }

        public ITurn GetNext(string userInput)
        {
            for(int i = 0; i < Options.Length; i++)
            {
                if (Options[i].Checker.Check(userInput))
                {
                    Options[i].OnUsed();
                    return Options[i].Next;
                }
            }

            return null;
        }
    }
}
