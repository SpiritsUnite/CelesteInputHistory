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

        private QueuedStreamWriter _replayWriter;
        private const string REPLAY_FOLDER = "InputHistoryReplays";
        private HistoryEvent _lastReplayEvent;
        private bool _onEnter = false;

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
            _onEnter = true;

            if (Settings.EnableReplays)
            {
                Directory.CreateDirectory(Path.Combine(Everest.PathGame, REPLAY_FOLDER));
                string mapName = session.Area.SID.Replace(Path.DirectorySeparatorChar, '_');
                mapName = mapName.Replace(Path.AltDirectorySeparatorChar, '_');
                _replayWriter = new QueuedStreamWriter(Path.Combine(
                    Everest.PathGame, REPLAY_FOLDER,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + mapName + ".tas"));
                if (fromSaveData && session.RespawnPoint.HasValue)
                    _replayWriter.WriteLineQueued(String.Format("console load {0} {1} {2} 0 0",
                        session.Area.SID, session.RespawnPoint.Value.X, session.RespawnPoint.Value.Y));
                else
                    _replayWriter.WriteLineQueued("console load " + session.Area.SID);
            }
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            WriteOutLastEvent();
            Events.Clear();
            _lastReplayEvent = null;

            if (mode == LevelExit.Mode.Restart || mode == LevelExit.Mode.GoldenBerryRestart)
                return;

            _replayWriter?.CloseQueued();
            _replayWriter = null;
        }

        private void WriteOutLastEvent()
        {
            if (_lastReplayEvent == null || _replayWriter == null)
                return;

            if (!Settings.EnableReplays)
            {
                _replayWriter.CloseQueued();
                _replayWriter = null;
            }

            _replayWriter?.WriteLineQueued(_lastReplayEvent.ToTasString());
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

            if (_onEnter) return;

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

            if (Settings.EnableReplays)
            {
                HistoryEvent tasEvent = HistoryEvent.CreateTasHistoryEvent();
                if (tasEvent.Extends(_lastReplayEvent, tas: true))
                {
                    _lastReplayEvent.Time += e.Time;
                    _lastReplayEvent.Frames++;
                }
                else
                {
                    WriteOutLastEvent();
                    _lastReplayEvent = tasEvent;
                }
            }
            else
            {
                // To flush the replay file if the player disables replays during a level.
                WriteOutLastEvent();
            }
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= AddList;
            Everest.Events.Level.OnEnter -= Level_OnEnter;
            Everest.Events.Level.OnExit -= Level_OnExit;
            On.Monocle.Engine.Update -= UpdateList;
            _replayWriter?.CloseQueued();
            _replayWriter = null;
        }

        private void AddList(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (_onEnter)
            {
                _onEnter = false;
                Events.Clear();
                _lastReplayEvent = null;
            }
            level.Add(new InputHistoryListEntity());

            _replayWriter?.WriteLineQueued("# " + level.Session.LevelData.Name);
            _replayWriter?.FlushQueued();
        }
    }
}
