using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.InputHistory
{
    public class IntegerAxisInputEvent : InputEvent
    {
        public int X { get; }
        public int Y { get; }
        public bool MenuLeft { get; } = false;
        public bool MenuRight { get; } = false;
        public bool MenuUp { get; } = false;
        public bool MenuDown { get; } = false;

        public IntegerAxisInputEvent(VirtualIntegerAxis axisX, VirtualIntegerAxis axisY)
        {
            X = (int)axisX;
            Y = (int)axisY;
            if (Engine.Scene is Level level)
            {
                // The frame you hit a button to close the pause menu, level.Pause becomes false,
                // so check wasPaused instead, as that stays true for one extra frame.
                if (level.Paused || DynamicData.For(level).Get<bool>("wasPaused"))
                {
                    MenuLeft = Input.MenuLeft;
                    MenuRight = Input.MenuRight;
                    MenuUp = Input.MenuUp;
                    MenuDown = Input.MenuDown;
                }
            }
        }

        public float Render(float x, float y, float fontSize)
        {
            var icon = Input.GuiDirection(new Vector2(X, Y));
            icon?.Draw(new Vector2(x, y), Vector2.Zero, InputHistoryModule.GetColor(), fontSize / icon.Height);
            var rightDir = Input.GuiDirection(new Vector2(1, 0));
            return x + rightDir.Width * fontSize / rightDir.Height;
        }

        public bool Extends(InputEvent orig, bool tas)
        {
            if (orig is IntegerAxisInputEvent axisEvent)
            {
                return X == axisEvent.X && Y == axisEvent.Y &&
                    (!tas || (
                        MenuLeft == axisEvent.MenuLeft &&
                        MenuRight == axisEvent.MenuRight &&
                        MenuUp == axisEvent.MenuUp &&
                        MenuDown == axisEvent.MenuDown));
            }
            return false;
        }

        public string ToTasString()
        {
            var ret = "";
            if (X == -1 || MenuLeft) ret += "L,";
            if (X == 1 || MenuRight) ret += "R,";
            if (Y == -1 || MenuUp) ret += "U,";
            if (Y == 1 || MenuDown) ret += "D,";
            return ret.Trim(',');
        }

        public bool hasInput()
        {
            return X != 0 || Y != 0;
        }
    }
}
