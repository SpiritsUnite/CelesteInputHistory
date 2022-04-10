using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.InputHistory
{
    public interface InputEvent
    {
        /// <summary>
        /// Renders the event at the given coordinate. Returns the x coordinate after the rendered event.
        /// </summary>
        /// <param name="x">Coordinate to render at.</param>
        /// <param name="y">Coordinate to render at.</param>
        /// <param name="fontSize">Height of the rendered event.</param>
        /// <returns>The x coordinate after the rendered event.</returns>
        float Render(float x, float y, float fontSize);

        /// <summary>
        /// Returns rether or not the current event is an extension of the previous event.
        /// </summary>
        /// <param name="orig">The previous event.</param>
        /// <param name="tas"></param>
        bool Extends(InputEvent orig, bool tas);

        /// <summary>
        /// Returns the current event in TAS format.
        /// </summary>
        string ToTasString();
    }
}
