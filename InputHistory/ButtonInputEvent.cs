using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.InputHistory
{
    public class ButtonInputEvent : InputEvent
    {
        public int Check { get; }
        public bool MenuCheck { get; } = false;
        public bool Pressed { get; }
        private readonly VirtualButton _button;
        private readonly List<int> _checkedBindingIds;
        public Microsoft.Xna.Framework.Input.Keys Key { get; }

        public ButtonInputEvent(VirtualButton button, Microsoft.Xna.Framework.Input.Keys key)
        {
            _button = button;
            Key = key;
            _checkedBindingIds = CheckCount(button);
            Check = _checkedBindingIds.Count();
            Pressed = button.Binding.Pressed(button.GamepadIndex, button.Threshold);

            if (Engine.Scene is Level level)
            {
                // The frame you hit a button to close the pause menu, level.Pause becomes false,
                // so check wasPaused instead, as that stays true for one extra frame.
                if (level.Paused || DynamicData.For(level).Get<bool>("wasPaused"))
                {
                    // Various menu button hacks
                    if (button == Input.Jump) MenuCheck = Input.MenuConfirm.Check;
                    if (button == Input.Dash) MenuCheck = Input.MenuCancel.Check;
                }
            }
        }

        public float Render(float x, float y, float fontSize)
        {
            var icon = Input.GuiKey(Key);
            float multiScale = 5f / (5 + Check - 1);
            for (int i = 0; i < Check; i++)
            {
                var shift = new Vector2(icon.Width * fontSize / icon.Height * (i / (5f + Check - 1)),
                    fontSize * ((Check - i - 1) / (5f + Check - 1)));
                icon.Draw(new Vector2(x, y) + shift, Vector2.Zero, Color.White, fontSize / icon.Height * multiScale);
            }
            return x + icon.Width * fontSize / icon.Height;
        }
        public bool Extends(InputEvent orig, bool tas)
        {
            if (orig is ButtonInputEvent origEvent)
            {
                return !Pressed && Check == origEvent.Check && (!tas || MenuCheck == origEvent.MenuCheck);
            }
            return false;
        }

        public override string ToString()
        {
            if (Check == 0) return "";

            var ret = "";
            if (_button == Input.Jump) ret += "Jump";
            else if (_button == Input.Dash) ret += "Dash";
            else if (_button == Input.CrouchDash) ret += "CrouchDash";
            else if (_button == Input.Grab) ret += "Grab";
            else if (_button == Input.Pause) ret += "Pause";
            else if (_button == Input.QuickRestart) ret += "QuickRestart";
            else if (_button == Input.MenuJournal) ret += "Journal";
            else ret += "Unknown";
            ret += " " + Check.ToString();
            if (Pressed) ret += "P";
            return ret;
        }

        public string ToTasString()
        {
            if (Check == 0 && !MenuCheck) return "";

            var ret = "";
            if (_button == Input.Jump)
            {
                bool addJ = false;
                bool addK = false;
                foreach (var bindingIdx in _checkedBindingIds)
                {
                    if (bindingIdx % 2 == 0) addJ = true;
                    else addK = true;
                }
                if (addJ) ret += "J,";
                if (addK) ret += "K,";
                if (MenuCheck) ret += "O,";
                ret = ret.Trim(',');
            }
            else if (_button == Input.Dash) ret += "X";
            else if (_button == Input.CrouchDash) ret += "Z";
            else if (_button == Input.Grab) ret += "G";
            else if (_button == Input.Pause) ret += "S";
            else if (_button == Input.QuickRestart) ret += "Q";
            else if (_button == Input.MenuJournal) ret += "N";
            return ret;
        }

        private static List<int> CheckCount(VirtualButton button)
        {
            var ret = new List<int>();
            int idx = 0;
            foreach (var key in button.Binding.Keyboard)
            {
                if (MInput.Keyboard.Check(key)) ret.Add(idx);
                idx++;
            }
            // Hack to deal with Pause and ESC being different.
            if (button == Input.Pause && Input.ESC.Check) ret.Add(idx);

            idx = 100;
            foreach (var padButton in button.Binding.Controller)
            {
                if (MInput.GamePads[button.GamepadIndex].Check(padButton, button.Threshold))
                    ret.Add(idx);
                idx++;
            }
            return ret;
        }

    }
}
