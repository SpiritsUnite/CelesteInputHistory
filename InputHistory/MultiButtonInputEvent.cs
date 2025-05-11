using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.InputHistory
{
    public class MultiButtonInputEvent : InputEvent
    {
        private IEnumerable<ButtonInputEvent> _events;
        public MultiButtonInputEvent(IEnumerable<ButtonInputEvent> events)
        {
            _events = events;
        }

        public float Render(float x, float y, float fontSize)
        {
            int total = _events.Sum(e => e.Check);
            float retx = x;
            float multiScale = 5f / (5 + total - 1);
            int ii = 0;
            foreach (var e in _events)
            {
                var icon = Input.GuiKey(e.Key);
                retx = Math.Max(retx, x + icon.Width * fontSize / icon.Height);
                for (int i = 0; i < e.Check; i++, ii++)
                {
                    var shift = new Vector2(icon.Width * fontSize / icon.Height * (ii / (5f + total - 1)),
                        fontSize * ((total - ii - 1) / (5f + total - 1)));
                    icon.Draw(new Vector2(x, y) + shift, Vector2.Zero, Color.White, fontSize / icon.Height * multiScale);
                }
            }
            return retx;
        }

        public bool Extends(InputEvent orig, bool tas)
        {
            if (orig is MultiButtonInputEvent origEvent)
            {
                if (_events.Count() != origEvent._events.Count()) return false;
                // Extends has side effects, so do it via Aggregate
                // to ensure we call it on all elements.
                return _events.Zip(origEvent._events, Tuple.Create)
                    .Aggregate(true, (res, e) => e.Item1.Extends(e.Item2, tas) && res);
            }
            return false;
        }

        public string ToTasString()
        {
            var ret = "";
            foreach (var e in _events)
            {
                var s = e.ToTasString();
                if (s != "") ret += s + ",";
            }
            return ret.Trim(',');
        }

        public bool hasInput()
        {
            return _events.Any(e => e.hasInput());
        }
    }
}
