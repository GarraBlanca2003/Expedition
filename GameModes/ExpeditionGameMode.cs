using Menu;
using System.Collections.Generic;

namespace RainMeadow
{
    // Minimal scaffold for Expedition mode. Extend as needed.
    public class ExpeditionGameMode : OnlineGameMode
    {
        public ExpeditionGameMode(Lobby lobby) : base(lobby)
        {
            // setup minimal data for expeditions
        }

        public override bool ShouldLoadCreatures(RainWorldGame game, WorldSession worldSession)
        {
            if (OnlineManager.mePlayer.isActuallySpectating)
            {
                return false;
            }

            return worldSession.owner == null || worldSession.isOwner;
        }

        public override bool ShouldSpawnRoomItems(RainWorldGame game, RoomSession roomSession)
        {
            if (OnlineManager.mePlayer.isActuallySpectating)
            {
                return false;
            }

            return roomSession.owner == null || roomSession.isOwner;
        }

        public override ProcessManager.ProcessID MenuProcessId()
        {
            // Use a dedicated Expedition menu process
            return RainMeadow.Ext_ProcessID.ExpeditionMenu;
        }

        public override void AddClientData()
        {
            // add any per-client data types here
        }

        public override void ConfigureAvatar(OnlineCreature onlineCreature)
        {
            // attach mode-specific avatar data if needed
        }

        public override void Customize(Creature creature, OnlineCreature oc)
        {
            // Minimal customization: no changes by default.
            // Extend this to modify `creature` based on `oc` (avatar data) for expedition mode.
        }

        public override void NewPlayerInLobby(OnlinePlayer player)
        {
            base.NewPlayerInLobby(player);
            // initialize expedition-specific settings for the player
        }

        public override void PlayerLeftLobby(OnlinePlayer player)
        {
            base.PlayerLeftLobby(player);
            // cleanup player-specific expedition state
        }
    }
}
