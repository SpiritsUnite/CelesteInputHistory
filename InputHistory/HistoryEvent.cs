using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.InputHistory
{
    public class HistoryEvent
    {
        public float Time { get; set; }
        public long Frames { get; set; }

        private readonly IEnumerable<InputEvent> events_;

        private HistoryEvent(IEnumerable<InputEvent> events)
        {
            Time = Engine.RawDeltaTime;
            Frames = 1;
            events_ = events;
        }

        public static HistoryEvent CreateDefaultHistoryEvent()
        {
            return new HistoryEvent(DefaultEventList());
        }

        public static HistoryEvent CreateTasHistoryEvent()
        {
            var events = DefaultEventList();
            events.Add(new ButtonInputEvent(Input.Pause, Microsoft.Xna.Framework.Input.Keys.S));
            events.Add(new ButtonInputEvent(Input.QuickRestart, Microsoft.Xna.Framework.Input.Keys.R));
            events.Add(new ButtonInputEvent(Input.MenuJournal, Microsoft.Xna.Framework.Input.Keys.N));
            return new HistoryEvent(events);
        }

        private static List<InputEvent> DefaultEventList()
        {
            var events = new List<InputEvent>();
            events.Add(new IntegerAxisInputEvent(Input.MoveX, Input.MoveY));
            events.Add(new ButtonInputEvent(Input.Jump, Microsoft.Xna.Framework.Input.Keys.J));
            events.Add(new MultiButtonInputEvent(new[] {
                new ButtonInputEvent(Input.Dash, Microsoft.Xna.Framework.Input.Keys.X),
                new ButtonInputEvent(Input.CrouchDash, Microsoft.Xna.Framework.Input.Keys.Z),
            }));
            events.Add(new ButtonInputEvent(Input.Grab, Microsoft.Xna.Framework.Input.Keys.G));
            return events;
        }

        public float Render(float y)
        {
            var scale = 0.5f;
            var fontSize = ActiveFont.LineHeight * scale;
            var x = 10f;

            foreach (var e in events_)
            {
                x = e.Render(x, y, fontSize);
            }

            ActiveFont.DrawOutline(Frames.ToString(),
                    position: new Vector2(x, y),
                    justify: new Vector2(0f, 0f),
                    scale: Vector2.One * scale,
                    color: Color.White, stroke: 2f,
                    strokeColor: Color.Black);
            return y + fontSize;
        }

        public bool Extends(HistoryEvent other, bool tas)
        {
            if (other == null) return false;

            if (events_.Count() != other.events_.Count()) return false;
            return events_.Zip(other.events_, Tuple.Create)
                .All((es) => es.Item1.Extends(es.Item2, tas));
        }

        public string ToTasString()
        {
            string ret = Frames.ToString();
            foreach (var e in events_)
            {
                string s = e.ToTasString();
                if (s != "") ret += "," + s;
            }
            return ret;
        }
    }
}
