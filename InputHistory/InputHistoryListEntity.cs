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
            var settings = InputHistoryModule.Settings;
            if (settings.ToggleVisibility.Pressed)
            {
                settings.ToggleVisibility.ConsumePress();
                settings.Visible = !settings.Visible;
            }

            if (!(settings.Visible ^ settings.ToggleVisibilityHold.Check)) return;

            var y = 120f;
            foreach (var e in InputHistoryModule.Events.Reverse().Take(InputHistoryModule.Settings.MaxInputsShown))
            {
                y = e.Render(y);
            }
        }
    }
}
