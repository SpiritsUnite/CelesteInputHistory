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
        public bool MenuPress { get; } = false;
        public bool Pressed { get; }
        private readonly VirtualButton _button;
        public Microsoft.Xna.Framework.Input.Keys Key { get; }
        // -1 = not pressed, 0/1 = alternating presses
        private int _tasButtonIdx;

        public ButtonInputEvent(VirtualButton button, Microsoft.Xna.Framework.Input.Keys key)
        {
            _button = button;
            Key = key;
            Check = CheckCount(button);
            Pressed = button.Binding.Pressed(button.GamepadIndex, button.Threshold);
            _tasButtonIdx = (Check > 0) ? 0 : -1;

            if (Engine.Scene is Level level)
            {
                // The frame you hit a button to close the pause menu, level.Pause becomes false,
                // so check wasPaused instead, as that stays true for one extra frame.
                if (level.Paused || DynamicData.For(level).Get<bool>("wasPaused"))
                {
                    // Various menu button hacks
                    if (button == Input.Dash) MenuPress = Input.MenuCancel.Pressed;
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
                icon.Draw(new Vector2(x, y) + shift, Vector2.Zero, InputHistoryModule.GetColor(), fontSize / icon.Height * multiScale);
            }
            return x + icon.Width * fontSize / icon.Height;
        }
        public bool Extends(InputEvent orig, bool tas)
        {
            if (orig is ButtonInputEvent origEvent)
            {
                if (tas)
                {
                    _tasButtonIdx = origEvent._tasButtonIdx;
                    if (Pressed || MenuPress)
                    {
                        _tasButtonIdx = (_tasButtonIdx + 1) % 2;
                    }
                    else if (Check == 0)
                    {
                        _tasButtonIdx = -1;
                    }
                    return ToTasString() == origEvent.ToTasString();
                }
                else return !Pressed && Check == origEvent.Check;
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
            else if (_button == Input.Talk) ret += "Talk";
            else ret += "Unknown";
            ret += " " + Check.ToString();
            if (Pressed) ret += "P";
            return ret;
        }

        public string ToTasString()
        {
            if (Check == 0 && !MenuPress) return "";

            var ret = "";
            if (_button == Input.Jump)
            {
                if (_tasButtonIdx == 0) ret += "J";
                else if (_tasButtonIdx == 1) ret += "K";
            }
            else if (_button == Input.Dash)
            {
                if (_tasButtonIdx == 0) ret += "C";
                // This can be wrong because this is also talk bind,
                // but there's no other way to double dash bind.
                else if (_tasButtonIdx == 1) ret += "X";
            }
            else if (_button == Input.CrouchDash)
            {
                if (_tasButtonIdx == 0) ret += "Z";
                else if (_tasButtonIdx == 1) ret += "V";
            }
            else if (_button == Input.Grab)
            {
                if (_tasButtonIdx == 0) ret += "G";
                else if (_tasButtonIdx == 1) ret += "H";
            }
            else if (_button == Input.Pause) ret += "S";
            else if (_button == Input.QuickRestart) ret += "Q";
            else if (_button == Input.MenuJournal) ret += "N";
            else if (_button == Input.Talk) ret += "N";
            else if (_button == Input.MenuConfirm) ret += "O";
            return ret;
        }

        private static int CheckCount(VirtualButton button)
        {
            int ret = 0;
            foreach (var key in button.Binding.Keyboard)
            {
                if (MInput.Keyboard.Check(key)) ret++;
            }
            // Hack to deal with Pause and ESC being different.
            if (button == Input.Pause && Input.ESC.Check) ret++;

            foreach (var padButton in button.Binding.Controller)
            {
                if (MInput.GamePads[button.GamepadIndex].Check(padButton, button.Threshold))
                    ret++;
            }

            foreach (var mouseButton in button.Binding.Mouse)
            {
                if (MInput.Mouse.Check(mouseButton)) ret++;
            }

            return ret;
        }

        public bool hasInput()
        {
            return Check != 0;
        }
    }
}
