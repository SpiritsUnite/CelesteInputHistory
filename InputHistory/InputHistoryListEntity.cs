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
            Tag = Tags.HUD | Tags.Global;
        }

        public override void Render()
        {
            bool forceShown =
                InputHistoryModule.DeathOverride != InputHistoryModule.DeathOverrideState.INACTIVE;

            var settings = InputHistoryModule.Settings;
            if (settings.ToggleVisibility.Pressed)
            {
                settings.ToggleVisibility.ConsumePress();
                settings.Visible = !settings.Visible;
            }

            if (!(settings.Visible ^ settings.ToggleVisibilityHold.Check) && !forceShown) return;

            float y = settings.VerticalPosition;
            foreach (var e in InputHistoryModule.Events.Reverse().Take(InputHistoryModule.Settings.MaxInputsShown))
            {
                y = e.Render(y, InputHistoryModule.Settings.ShowFrameCount || forceShown);
            }
        }
    }
}
