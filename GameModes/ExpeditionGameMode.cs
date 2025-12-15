using Menu;
using System.Collections.Generic;

namespace RainMeadow
{
    // Expedition mode: reuse StoryGameMode behaviour (gates, syncing, Jolly integration)
    public class ExpeditionGameMode : StoryGameMode
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
            base.AddClientData();
            // add any per-client data types here
        }

        public override void ConfigureAvatar(OnlineCreature onlineCreature)
        {
            base.ConfigureAvatar(onlineCreature);
            // attach mode-specific avatar data if needed
        }

        public override void Customize(Creature creature, OnlineCreature oc)
        {
            // reuse StoryGameMode customizations
            base.Customize(creature, oc);
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

        public override void ResourceAvailable(OnlineResource onlineResource)
        {
            base.ResourceAvailable(onlineResource);
            if (onlineResource is Lobby lobby)
            {
                lobby.AddData(new ExpeditionLobbyData());
            }
        }
    }
}
