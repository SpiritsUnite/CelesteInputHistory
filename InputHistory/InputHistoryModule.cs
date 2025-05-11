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

        public enum DeathOverrideState
        {
            // Override is off in normal play or forced off by setting.
            INACTIVE,
            // Between death and spawn we keep the list shown even if inputs are pressed.
            FORCED,
            // After spawn, we turn this off on first input.
            WAITING
        }

        private static DeathOverrideState _deathOverride = DeathOverrideState.INACTIVE;
        public static DeathOverrideState DeathOverride
        {
            get => Settings.ShowOnDeath ? _deathOverride : DeathOverrideState.INACTIVE;
        }

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
            Everest.Events.Player.OnDie += Player_OnDie;
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

                string loadCommand = "console ";
                if (session.Area.Mode == AreaMode.Normal)
                {
                    loadCommand += "load";
                }
                else if (session.Area.Mode == AreaMode.BSide)
                {
                    loadCommand += "hard";
                    mapName += "_B";
                }
                else if (session.Area.Mode == AreaMode.CSide)
                {
                    loadCommand += "rmx2";
                    mapName += "_C";
                }

                _replayWriter = new QueuedStreamWriter(Path.Combine(
                    Everest.PathGame, REPLAY_FOLDER,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + mapName + ".tas"));
                if (fromSaveData && session.RespawnPoint.HasValue)
                    _replayWriter.WriteLineQueued(String.Format("{0} {1} {2} {3} 0 0", loadCommand,
                        session.Area.SID, session.RespawnPoint.Value.X, session.RespawnPoint.Value.Y));
                else
                    _replayWriter.WriteLineQueued(loadCommand + " " + session.Area.SID);
            }
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            WriteOutLastEvent();
            Events.Clear();
            _lastReplayEvent = null;
            _deathOverride = DeathOverrideState.INACTIVE;

            if (mode == LevelExit.Mode.Restart || mode == LevelExit.Mode.GoldenBerryRestart)
                return;

            _replayWriter?.CloseQueued();
            _replayWriter = null;
        }

        private void Player_OnDie(Player obj)
        {
            _deathOverride = DeathOverrideState.FORCED;
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

            if (_deathOverride == DeathOverrideState.FORCED &&
                Engine.Scene.Tracker.GetEntity<Player>() is Player player && !player.Dead)
            {
                _deathOverride = DeathOverrideState.WAITING;
            }

            HistoryEvent e = HistoryEvent.CreateDefaultHistoryEvent();
            if (_deathOverride == DeathOverrideState.WAITING && e.hasInput())
            {
                _deathOverride = DeathOverrideState.INACTIVE;
            }
            if (!e.Extends(Events.LastOrDefault(), tas: false))
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
            Everest.Events.Player.OnDie -= Player_OnDie;
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
