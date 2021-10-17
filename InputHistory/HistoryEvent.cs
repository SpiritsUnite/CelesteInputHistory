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
    public class HistoryEvent : IEquatable<HistoryEvent>
    {
        public static readonly HistoryEvent LEVEL_LOAD = new HistoryEvent(true);
        public float Time { get; set; }
        public long Frames { get; set; }
        private int MoveX;
        private int MoveY;
        private int Jump;
        private bool Dash;
        private bool CrouchDash;
        private bool Grab;
        private bool LevelLoad;

        private int CheckCount(VirtualButton button)
        {
            int ret = 0;
            foreach (var node in button.Nodes)
            {
                if (node.Check) ret += 1;
            }
            foreach (var key in button.Binding.Keyboard)
            {
                if (MInput.Keyboard.Check(key)) ret += 1;
            }
            foreach (var padButton in button.Binding.Controller)
            {
                if (MInput.GamePads[button.GamepadIndex].Check(padButton, button.Threshold))
                    ret += 1;
            }
            return ret;
        }

        public HistoryEvent()
        {
            Time = Engine.RawDeltaTime;
            Frames = 1;
            MoveX = (int)Input.MoveX;
            MoveY = (int)Input.MoveY;
            Jump = CheckCount(Input.Jump);
            Dash = Input.Dash;
            CrouchDash = Input.CrouchDash;
            Grab = Input.Grab;
            LevelLoad = false;
        }

        private HistoryEvent(bool levelLoad) : this()
        {
            if (levelLoad)
            {
                LevelLoad = levelLoad;
                Frames = 0;
                Time = 0;
            }
        }

        public float Render(float y)
        {
            var scale = 0.5f;
            var fontSize = ActiveFont.LineHeight * scale;
            var x = 10f;

            var dir = Input.GuiDirection(new Vector2(MoveX, MoveY));
            dir?.Draw(new Vector2(x, y), Vector2.Zero, Color.White, fontSize / dir.Height);
            var rightDir = Input.GuiDirection(new Vector2(1, 0));
            x += rightDir.Width * fontSize / rightDir.Height;

            var jump = Input.GuiKey(Microsoft.Xna.Framework.Input.Keys.J);
            for (int i = 0; i < Jump; i++)
            {
                float multiScale = 5f / (5 + Jump - 1);
                var shift = new Vector2(jump.Width * fontSize / jump.Height * (i / (5f + Jump - 1)),
                    fontSize * ((Jump - i - 1) / (5f + Jump - 1)));
                jump.Draw(new Vector2(x, y) + shift, Vector2.Zero, Color.White, fontSize / jump.Height * multiScale);;
            }
            x += jump.Width * fontSize / jump.Height;

            var dash = Input.GuiKey(Microsoft.Xna.Framework.Input.Keys.X);
            var crouchDash = Input.GuiKey(Microsoft.Xna.Framework.Input.Keys.Z);
            float dashScale = 1f;
            float dashShift = 0f;
            float crouchDashShift = 0f;
            if (Dash && CrouchDash)
            {
                dashScale = 5 / 6f;
                dashShift = fontSize / 6f;
                crouchDashShift = crouchDash.Width * fontSize / crouchDash.Height / 6;
            }
            if (Dash) dash.Draw(new Vector2(x, y + dashShift), Vector2.Zero, Color.White, fontSize / dash.Height * dashScale);
            if (CrouchDash) crouchDash.Draw(new Vector2(x + crouchDashShift, y), Vector2.Zero, Color.White, fontSize / dash.Height * dashScale);
            x += dash.Width * fontSize / dash.Height;

            var grab = Input.GuiKey(Microsoft.Xna.Framework.Input.Keys.G);
            if (Grab) grab.Draw(new Vector2(x, y), Vector2.Zero, Color.White, fontSize / grab.Height);
            x += grab.Width * fontSize / grab.Height;

            ActiveFont.DrawOutline(Frames.ToString(),
                    position: new Vector2(x, y),
                    justify: new Vector2(0f, 0f),
                    scale: Vector2.One * scale,
                    color: Color.White, stroke: 2f,
                    strokeColor: Color.Black);
            return y + fontSize;
        }

        public bool Equals(HistoryEvent other)
        {
            return (
                MoveX == other.MoveX &&
                MoveY == other.MoveY &&
                Jump == other.Jump &&
                Dash == other.Dash &&
                CrouchDash == other.CrouchDash &&
                Grab == other.Grab &&
                LevelLoad == other.LevelLoad);
        }

        public override string ToString()
        {
            //string ret = TimeSpan.FromSeconds(Time).ToString("s\\.fff") + " ";
            string ret = Frames.ToString() + " ";
            if (MoveX == 1) ret += "Right ";
            if (MoveX == -1) ret += "Left ";
            if (MoveY == 1) ret += "Up ";
            if (MoveY == -1) ret += "Down ";
            if (Jump > 0) ret += "Jump " + Jump.ToString() + " ";
            if (Dash) ret += "Dash ";
            if (CrouchDash) ret += "Crouch Dash ";
            if (LevelLoad) ret += "Level Load";
            return ret;
        }
    }
}
