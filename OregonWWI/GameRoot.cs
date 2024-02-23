using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OregonWWI.Neoregon;
using OregonWWI.Neoregon.Logic;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OregonWWI
{
    public class GameRoot : Game
    {
        public static GraphicsDevice Device { get; private set; }
        public static SpriteBatch Batch { get; private set; }
        public static SpriteFont Font { get; private set; }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;

        private StateData currentState;

        private TextReader _textReader;

        ITurn Current;

        public GameRoot()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Device = _graphics.GraphicsDevice;
        }

        protected override void Initialize()
        {
            //GameLogic.Initalize(FiniteStateAI);
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Batch = _spriteBatch;
            font = Content.Load<SpriteFont>("font");
            Font = font;
            //currentState = FiniteStateAI.Update(States.ChooseCharacter);
            _textReader = new TextReader(8);
            Current = Misc.Start;

        }
        protected override void Update(GameTime gameTime)
        {
#if DEBUG
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif
            InputHelper.TickUpdate();

            _textReader.Update();
            if (InputHelper.RisingEdge(Keys.Enter))
            {
                string txt = _textReader.ToString();
                var next = Current.GetNext(txt);
                if (next != null)
                {
                    Current = next;
                    _textReader.Clear();
                    Info.TurnsSurvived++;
                }

                /*
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
                }*/
            }

            base.Update(gameTime);
        }
        const int padding = 24;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            Current.Draw(_textReader.ToString());
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}