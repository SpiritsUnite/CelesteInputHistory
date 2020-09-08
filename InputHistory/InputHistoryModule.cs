using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.InputHistory
{
    public class InputHistoryModule : EverestModule
    {
        public static InputHistoryModule Instance;

        public override Type SettingsType => typeof(InputHistorySettings);
        public static InputHistorySettings Settings => (InputHistorySettings)Instance._Settings;
        public static Queue<HistoryEvent> Events = new Queue<HistoryEvent>();

        public InputHistoryModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            Everest.Events.Level.OnLoadLevel += AddList;
            On.Monocle.Scene.Begin += ClearEvents;
            On.Monocle.Engine.Update += UpdateList;
        }

        private void EnqueueEvent(HistoryEvent e)
        {
            Events.Enqueue(e);
            while (Events.Count > InputHistorySettings.MAX_POSSIBLE_INPUTS_SHOWN)
            {
                Events.Dequeue();
            }
        }

        private void UpdateList(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);

            HistoryEvent e = new HistoryEvent();
            if (Events.Count == 0 || !Events.Last().Equals(e))
            {
                EnqueueEvent(e);
            }
            else
            {
                Events.Last().Time += e.Time;
                Events.Last().Frames++;
            }
        }

        private void ClearEvents(On.Monocle.Scene.orig_Begin orig, Scene self)
        {
            orig(self);
            Events.Clear();
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= AddList;
            On.Monocle.Scene.Begin -= ClearEvents;
            On.Monocle.Engine.Update -= UpdateList;
        }

        private void AddList(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new InputHistoryListEntity());
        }
    }
}
