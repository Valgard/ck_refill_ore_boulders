using System.Linq;
using CoreLib;
using CoreLib.Submodule.Command;
using PugMod;
using UnityEngine;

namespace RefillOreBoulders
{
    /// <summary>
    /// Mod bootstrap. The Pugstorm mod loader instantiates this class on game
    /// start and calls the IMod lifecycle methods. The only job here is to
    /// register CoreLib's command module — CoreLib discovers the command
    /// handler itself by scanning this mod's assembly.
    /// </summary>
    public sealed class RefillOreBouldersMod : IMod
    {
        public const string Name = "Refill Ore Boulders";
        public const string Version = "1.0.0";

        private LoadedMod _modInfo;

        /// <summary>
        /// True when this instance owns the simulation — a dedicated-server
        /// process or a local host both hold a ServerWorld; a pure client does
        /// not. The command writes server-authoritative component data, so it
        /// must refuse to run anywhere else.
        /// </summary>
        public static bool IsHostOrSinglePlayer => Manager.ecs != null && Manager.ecs.ServerWorld != null;

        public void EarlyInit()
        {
            _modInfo = API.ModLoader.LoadedMods.FirstOrDefault(mod => mod.Handlers.Contains(this));
            if (_modInfo == null)
            {
                Debug.Log($"[{Name}] Failed to load: mod metadata not found.");
                return;
            }

            CoreLibMod.LoadSubmodule(typeof(CommandModule));
            CommandModule.AddCommands(_modInfo.ModId, Name);
        }

        public void Init()
        {
            Debug.Log($"[{Name}] {Version} loaded.");
        }

        public void ModObjectLoaded(Object obj) { }

        public void Shutdown() { }

        public void Update() { }
    }
}
