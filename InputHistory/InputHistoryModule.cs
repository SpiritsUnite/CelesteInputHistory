using System;
using System.IO;
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

        private StreamWriter replayWriter;
        private const string ReplayFolder = "InputHistoryReplays";
        private HistoryEvent lastReplayEvent;

        public InputHistoryModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            Everest.Events.Level.OnLoadLevel += AddList;
            Everest.Events.Level.OnEnter += Level_OnEnter;
            Everest.Events.Level.OnExit += Level_OnExit;
            On.Monocle.Engine.Update += UpdateList;
        }

        private void Level_OnEnter(Session session, bool fromSaveData)
        {
            Events.Clear();
            lastReplayEvent = null;

            if (Settings.EnableReplays)
            {
                Directory.CreateDirectory(Path.Combine(Everest.PathGame, ReplayFolder));
                replayWriter = new StreamWriter(Path.Combine(
                    Everest.PathGame, ReplayFolder,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + session.MapData.Filename + ".tas"));
                if (fromSaveData && session.RespawnPoint.HasValue)
                    replayWriter.WriteLine(String.Format("console load {0} {1} {2} 0 0",
                        session.Area.SID, session.RespawnPoint.Value.X, session.RespawnPoint.Value.Y));
                else
                    replayWriter.WriteLine("console load " + session.Area.SID);
            }
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            WriteOutLastEvent();
            Events.Clear();
            lastReplayEvent = null;

            if (mode == LevelExit.Mode.Restart || mode == LevelExit.Mode.GoldenBerryRestart)
                return;

            replayWriter?.Close();
            replayWriter = null;
        }

        private void WriteOutLastEvent()
        {
            if (lastReplayEvent == null || replayWriter?.BaseStream == null || !Settings.EnableReplays)
                return;

            replayWriter?.WriteLine(lastReplayEvent.ToTasString());
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

            HistoryEvent e = HistoryEvent.CreateDefaultHistoryEvent();
            if (Events.Count == 0 || !e.Extends(Events.Last(), tas: false))
            {
                EnqueueEvent(e);
            }
            else
            {
                Events.Last().Time += e.Time;
                Events.Last().Frames++;
            }

            HistoryEvent tasEvent = HistoryEvent.CreateTasHistoryEvent();
            if (tasEvent.Extends(lastReplayEvent, tas: true))
            {
                lastReplayEvent.Time += e.Time;
                lastReplayEvent.Frames++;
            }
            else
            {
                WriteOutLastEvent();
                lastReplayEvent = tasEvent;
            }
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= AddList;
            Everest.Events.Level.OnEnter -= Level_OnEnter;
            Everest.Events.Level.OnExit -= Level_OnExit;
            On.Monocle.Engine.Update -= UpdateList;
            replayWriter?.Close();
            replayWriter = null;
        }

        private void AddList(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new InputHistoryListEntity());

            replayWriter?.WriteLine("# " + level.Session.LevelData.Name);
            replayWriter?.Flush();
        }
    }
}
