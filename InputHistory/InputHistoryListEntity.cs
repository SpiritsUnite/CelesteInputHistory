using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.InputHistory
{
    class InputHistoryListEntity : Entity
    {
        public InputHistoryListEntity()
        {
            Tag = Tags.HUD;
        }

        public override void Render()
        {
            if (!InputHistoryModule.Settings.Enabled) return;

            var ly = 120f;
            foreach (var e in InputHistoryModule.Events.Reverse().Take(InputHistoryModule.Settings.MaxInputsShown))
            {
                ly = e.Render(ly);
            }
        }
    }
}
