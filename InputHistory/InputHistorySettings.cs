using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.InputHistory
{
    public class InputHistorySettings : EverestModuleSettings
    {
        public bool Visible { get; set; } = true;

        public bool ShowFrameCount { get; set; } = true;

        public const int MAX_POSSIBLE_INPUTS_SHOWN = 40;
        [SettingRange(0, MAX_POSSIBLE_INPUTS_SHOWN)]
        public int MaxInputsShown { get; set; } = 25;

        [SettingRange(0, 1920, true)]
        [SettingSubText("Default value: 10")]
        public int HorizontalPosition { get; set; } = 10;

        [SettingRange(0, 1080, true)]
        [SettingSubText("Default value: 120")]
        public int VerticalPosition { get; set; } = 120;

        [SettingSubText("Experimental! Won't start recording until next level load.")]
        public bool EnableReplays { get; set; } = false;

        public ButtonBinding ToggleVisibility { get; set; }

        public ButtonBinding ToggleVisibilityHold { get; set; }
    }
}
