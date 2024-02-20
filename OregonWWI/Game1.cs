using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace OregonWWI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private FiniteStateAI<States> FiniteStateAI = new();
        private SpriteFont font;

        private StateData currentState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GameLogic.Initalize(FiniteStateAI);
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            currentState = FiniteStateAI.Update(States.ChooseCharacter);
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
            if(acceptingInput)
            {
                //TODO: input lol
            }

            if (totalTicks % 2 == 0)
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

            if(totalTicks % 20 > 10)
            {
                Vector2 BottomLeft = new Vector2(0, _graphics.PreferredBackBufferHeight) - new Vector2(-1, 1) * padding;
                _spriteBatch.DrawString(font, "_", BottomLeft, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
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