using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace OregonWWI
{
    public class WebGameGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private FiniteStateAI<States> FiniteStateAI = new();
        private SpriteFont font;

        private StateData currentState;

        private TextReader _textReader;

        public WebGameGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(25);
        }

        protected override void Initialize()
        {
            GameLogic.Initalize(FiniteStateAI);
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            currentState = FiniteStateAI.Update(States.ChooseCharacter);
            _textReader = new TextReader(8);
        }

        private int TotalCharsDrawn = 0;
        private bool acceptingInput = false;
        private int totalTicks = 0;

        protected override void Update(GameTime gameTime)
        {
            totalTicks++;
#if DEBUG
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif
            InputHelper.TickUpdate();

            if (acceptingInput)
            {
                //TODO: input lol
                _textReader.Update();
                if (InputHelper.RisingEdge(Keys.Enter))
                {
                    string txt = _textReader.ToString();
                    CharacterInformation.Info.UserInput = txt;

                    var validChar = txt.Where(c => char.IsDigit(c) && (c - '0') > 0 && (c - '0') < currentState.options.Length + 1);
                    if (validChar.Any())
                    {
                        Operation choice = currentState.options[validChar.First() - '0' - 1];
                        CharacterInformation.Info.TurnsSurvived++;
                        choice.OnUsed.ToList().ForEach(t => t.Invoke());

                        currentState = FiniteStateAI.Update(choice.NextState);
                        TotalCharsDrawn = 0;
                        _textReader.Clear();
                    }
                }


            }

            TotalCharsDrawn++;

            base.Update(gameTime);
        }
        const int padding = 24;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            int charsDrawn = 0;
            _spriteBatch.Begin();
            Vector2 cumulativeLocation = Vector2.One * padding;

            currentState.Deconstruct(out var text, out var operations);
            bool hasSkipped = false;

            foreach (var str in text)
            {
                if (hasSkipped)
                    break;
                DrStr(str);
            }

            foreach (var str in operations.Select(o => o.OptionPrint))
            {
                if (hasSkipped)
                    break;
                DrStr(str);
            }

            void DrStr(CString str)
            {
                int potentialCharsDrawn = str.Length + charsDrawn;
                if (potentialCharsDrawn > TotalCharsDrawn)
                {
                    str = new CString(str.Color, str.String[..(str.Length + TotalCharsDrawn - potentialCharsDrawn)]);
                    hasSkipped = true;
                }


                Vector2 loc = font.MeasureString(str.String);
                _spriteBatch.DrawString(font, str.String, cumulativeLocation, str.Color);

                cumulativeLocation += new Vector2(0, loc.Y);
                charsDrawn += str.Length;
            }

            if (acceptingInput)
            {
                _spriteBatch.DrawString(font, _textReader.ToString() + (totalTicks % 40 > 20 ? "_" : ""), cumulativeLocation + Vector2.UnitY * padding, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }

            _spriteBatch.End();

            if (!hasSkipped)
            {
                acceptingInput = true;
            }
            base.Draw(gameTime);
        }
    }
}