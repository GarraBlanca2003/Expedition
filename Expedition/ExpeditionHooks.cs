using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using On.Watcher;
using RWCustom;
using System;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public partial class RainMeadow
    {
        public static bool isExpeditionMode(out ExpeditionGameMode gameMode)
        {
            gameMode = null;
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is ExpeditionGameMode sgm)
            {
                gameMode = sgm;
                return true;
            }
            return false;
        }

        private void ExpeditionHooks()
        {
            // register only the RegionGate / Warp hooks for Expedition mode
            IL.RegionGate.Update += Expedition_RegionGate_Update;
            On.RegionGate.PlayersInZone += Expedition_RegionGate_PlayersInZone;
            On.RegionGate.PlayersStandingStill += Expedition_RegionGate_PlayersStandingStill;
            On.RegionGate.AllPlayersThroughToOtherSide += Expedition_RegionGate_AllPlayersThroughToOtherSide;
            new Hook(typeof(RegionGate).GetProperty("MeetRequirement").GetGetMethod(), this.RegionGate_MeetRequirement_ExpeditionSync);
        }

        private void Expedition_RegionGate_Update(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                var skip = il.DefineLabel();
                c.GotoNext(moveType: MoveType.After,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<RegionGate>("mode"),
                    i => i.MatchLdsfld<RegionGate.Mode>("MiddleClosed"),
                    i => i.MatchCall("ExtEnum`1<RegionGate/Mode>", "op_Equality"),
                    i => i.MatchBrfalse(out _)
                );
                c.EmitDelegate(() =>
                {
                    if (isExpeditionMode(out var story))
                    {
                        if (story.readyForTransition >= StoryGameMode.ReadyForTransition.Opening) return true;
                        story.storyClientData.readyForTransition = false;
                    }
                    return false;
                });
                c.Emit(OpCodes.Brtrue, skip);
                c.GotoNext(moveType: MoveType.AfterLabel,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdsfld<RegionGate.Mode>("ClosingAirLock"),
                    i => i.MatchStfld<RegionGate>("mode")
                );
                c.EmitDelegate(() =>
                {
                    if (isExpeditionMode(out var story))
                    {
                        story.storyClientData.readyForTransition = true;
                        return true;
                    }
                    return false;
                });
                c.Emit(OpCodes.Brtrue, skip);
                c.Emit(OpCodes.Ret);
                c.MarkLabel(skip);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        private int Expedition_RegionGate_PlayersInZone(On.RegionGate.orig_PlayersInZone orig, RegionGate self)
        {
            var ret = orig(self);
            if (isExpeditionMode(out var storyGameMode))
            {
                foreach (var ac in OnlineManager.lobby.playerAvatars.Where(kvp => !kvp.Key.isMe).Select(kvp => kvp.Value.FindEntity())
                    .Select(oe => (oe as OnlinePhysicalObject)?.apo).OfType<AbstractCreature>())
                {
                    if (ac.Room.index != self.room.abstractRoom.index || ret != self.DetectZone(ac))
                        return -1;
                }
            }
            return ret;
        }

        private bool Expedition_RegionGate_PlayersStandingStill(On.RegionGate.orig_PlayersStandingStill orig, RegionGate self)
        {
            if (isExpeditionMode(out var storyGameMode))
            {
                foreach (var ac in OnlineManager.lobby.playerAvatars.Where(kvp => !kvp.Key.isMe).Select(kvp => kvp.Value.FindEntity())
                    .Select(oe => (oe as OnlinePhysicalObject)?.apo).OfType<AbstractCreature>())
                {
                    if (ac.realizedCreature is Player p)
                    {
                        if (p.touchedNoInputCounter < 20) return false;
                    }
                }
            }

            if (OnlineManager.lobby != null)
            {
                if (!self.room.game.cameras.All(x => (x?.room is null) || (x?.room == self.room))) return false;
            }

            return orig(self);
        }

        private bool Expedition_RegionGate_AllPlayersThroughToOtherSide(On.RegionGate.orig_AllPlayersThroughToOtherSide orig, RegionGate self)
        {
            var ret = orig(self);
            if (isExpeditionMode(out var storyGameMode))
            {
                storyGameMode.storyClientData.readyForTransition = !ret;
                ret = storyGameMode.readyForTransition == StoryGameMode.ReadyForTransition.Closed;
            }
            return ret;
        }

        // Hook for the MeetRequirement property getter
        public bool RegionGate_MeetRequirement_ExpeditionSync(orig_RegionGateBool orig, RegionGate self)
        {
            if (isExpeditionMode(out var storyGameMode))
            {
                var ret = (self.room.game.Players[0].realizedCreature is Player player && player.maxRippleLevel >= 1f) || orig(self);
                try { RainMeadow.Debug($"RegionGate original returned {ret}"); } catch { }
                if (ret) StoryRPCs.RegionGateOrWarpMeetRequirement();
                try { RainMeadow.Debug($"after invoke, readyForTransition={storyGameMode.readyForTransition}"); } catch { }
                return storyGameMode.readyForTransition >= StoryGameMode.ReadyForTransition.MeetRequirement;
            }
            return orig(self);
        }
    }
}
