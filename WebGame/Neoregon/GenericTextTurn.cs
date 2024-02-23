using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OregonWWI.Neoregon.Logic;

namespace OregonWWI.Neoregon
{
    internal class GenericTextTurn : ITurn
    {
        public readonly CString[] Text;
        public readonly Option[] Options;

        private Animation animation;

        public GenericTextTurn(CString[] Top, Option[] Options, Animation animation = null)
        {
            Text = Top;
            this.Options = Options;
            this.animation = animation;
        }

        public void SetAnimation(Animation animation) => this.animation = animation;


        public GenericTextTurn(CString Top, Option Options, Animation animation = null) : this(new CString[] { Top }, new Option[] { Options }, animation) { }
        public GenericTextTurn(CString[] Top, Option Options, Animation animation = null) : this(Top, new Option[] { Options }, animation) { }
        public GenericTextTurn(CString Top, Option[] Options, Animation animation = null) : this(new CString[] { Top }, Options, animation) { }

        const int padding = 24;
        int totalCharactersDrawn = 0;

        public Vector2 OffTransform;

        public void Draw(string userText)
        {
            totalCharactersDrawn++;
            animation?.AnimationUpdate(new GameTime(default, TimeSpan.FromMilliseconds(16.666666f)));

            int charsDrawn = 0;
            Vector2 cumulativeLocation = Vector2.One * padding + OffTransform;

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
                    str = new CString(str.String[..(str.Length + totalCharactersDrawn - potentialCharsDrawn)], str.Color, str.Location, str.offsets);
                    hasSkipped = true;
                }


                Vector2 loc = GameRoot.Font.MeasureString(str.String);
                if(str.offsets == null)
                {
                    GameRoot.Batch.DrawString(GameRoot.Font, str.String, cumulativeLocation, str.Color);
                }
                else
                {
                    Vector2 thisStartLoc = cumulativeLocation;
                    for(int i = 0; i < str.String.Length; i++)
                    {
                        string toBeDrawn = new string(str.String[i], 1);
                        thisStartLoc.X += GameRoot.Font.MeasureString(toBeDrawn).X;
                        GameRoot.Batch.DrawString(GameRoot.Font, toBeDrawn, thisStartLoc + str.offsets[i], str.Color);
                    }
                }

                cumulativeLocation += new Vector2(0, loc.Y);
                charsDrawn += str.Length;
            }

            cumulativeLocation += new Vector2(0, GameRoot.Font.MeasureString("|").X);
            GameRoot.Batch.DrawString(GameRoot.Font, userText + (totalCharactersDrawn % 40 < 20 ? "" : "_"), cumulativeLocation, Color.White);
        }

        public ITurn GetNext(string userInput)
        {
            for (int i = 0; i < Options.Length; i++)
            {
                if (Options[i].Checker.Check(userInput))
                {
                    Options[i].OnUsed?.Invoke();
                    return Options[i].GetNext();
                }
            }

            return null;
        }
    }
}
