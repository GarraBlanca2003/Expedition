using HUD;
using IL.Watcher;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using On.Watcher;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        }
        
    }
}
