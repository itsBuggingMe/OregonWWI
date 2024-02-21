using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI.Neoregon
{
#nullable enable
    internal interface ITurn
    {
        public ITurn? GetNext(string userInput);
        public void Draw(SpriteBatch spriteBatch, int totalCharactersDrawn);
    }
}
