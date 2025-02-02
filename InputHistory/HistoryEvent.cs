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

        private readonly IEnumerable<InputEvent> _events;

        private HistoryEvent(IEnumerable<InputEvent> events)
        {
            Time = Engine.RawDeltaTime;
            Frames = 1;
            _events = events;
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
            events.Add(new ButtonInputEvent(Input.Talk, Microsoft.Xna.Framework.Input.Keys.N));
            events.Add(new ButtonInputEvent(Input.MenuConfirm, Microsoft.Xna.Framework.Input.Keys.O));
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
            float x = InputHistoryModule.Settings.HorizontalPosition;

            foreach (var e in _events)
            {
                x = e.Render(x, y, fontSize);
            }

            if (InputHistoryModule.Settings.ShowFrameCount)
            {
                ActiveFont.DrawOutline(Frames.ToString(),
                        position: new Vector2(x, y),
                        justify: new Vector2(0f, 0f),
                        scale: Vector2.One * scale,
                        color: Color.White, stroke: 2f,
                        strokeColor: Color.Black);
            }
            return y + fontSize;
        }

        public bool Extends(HistoryEvent other, bool tas)
        {
            if (other == null) return false;

            if (_events.Count() != other._events.Count()) return false;
            // Extends has side effects, so do it via Aggregate
            // to ensure we call it on all elements.
            return _events.Zip(other._events, Tuple.Create)
                .Aggregate(true, (res, e) => e.Item1.Extends(e.Item2, tas) && res);
        }

        public string ToTasString()
        {
            string ret = Frames.ToString();
            foreach (var e in _events)
            {
                string s = e.ToTasString();
                if (s != "") ret += "," + s;
            }
            return ret;
        }
    }
}
