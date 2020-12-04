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
        private bool Jump;
        private bool Dash;
        private bool Grab;
        private bool LevelLoad;

        public HistoryEvent()
        {
            Time = Engine.RawDeltaTime;
            Frames = 1;
            MoveX = Input.MoveX;
            MoveY = Input.MoveY;
            Jump = Input.Jump.Check;
            Dash = Input.Dash.Check;
            Grab = Input.Grab.Check;
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

            var jump1 = Input.GuiKey(Microsoft.Xna.Framework.Input.Keys.J);
            if (Jump) jump1.Draw(new Vector2(x, y), Vector2.Zero, Color.White, fontSize / jump1.Height);
            x += jump1.Width * fontSize / jump1.Height;

            var dash = Input.GuiKey(Microsoft.Xna.Framework.Input.Keys.X);
            if (Dash) dash.Draw(new Vector2(x, y), Vector2.Zero, Color.White, fontSize / dash.Height);
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
            if (Jump) ret += "Jump ";
            if (Dash) ret += "Dash ";
            if (LevelLoad) ret += "Level Load";
            return ret;
        }
    }
}
