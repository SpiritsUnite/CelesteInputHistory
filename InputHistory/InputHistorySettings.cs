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

        public const int MAX_POSSIBLE_INPUTS_SHOWN = 30;
        [SettingRange(0, MAX_POSSIBLE_INPUTS_SHOWN)]
        public int MaxInputsShown { get; set; } = 25;

        [SettingSubText("Experimental!")]
        public bool EnableReplays { get; set; } = false;
    }
}
