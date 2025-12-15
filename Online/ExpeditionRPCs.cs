using System;
using System.Collections.Generic;

namespace RainMeadow
{
    public static class ExpeditionRPCs
    {
        private static readonly bool DownpourAvailable;
        private static readonly Type? DownpourMissionType;
        private static readonly Type? DownpourMissionManagerType;

        static ExpeditionRPCs()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var an = asm.GetName().Name ?? "";
                    if (an.ToLowerInvariant().Contains("downpour") || (asm.GetTypes().Length > 0 && Array.Exists(asm.GetTypes(), t => t.Name.ToLowerInvariant().Contains("downpour"))))
                    {
                        // try to find mission types
                        foreach (var t in asm.GetTypes())
                        {
                            var tn = t.Name.ToLowerInvariant();
                            if (tn.Contains("mission") && DownpourMissionType is null)
                            {
                                DownpourMissionType = t;
                            }
                            if ((tn.Contains("missionmanager") || tn.Contains("mission_manager") || tn.Contains("missioncontroller")) && DownpourMissionManagerType is null)
                            {
                                DownpourMissionManagerType = t;
                            }
                        }
                    }
                }
            }
            catch { }
            DownpourAvailable = DownpourMissionType is not null || DownpourMissionManagerType is not null;
        }
        [RPCMethod]
        public static void Expedition_AddMission(RPCEvent rpc, string title, int target)
        {
            if (rpc != null && OnlineManager.lobby.owner != rpc.from) return;
            var lobby = OnlineManager.lobby;
            if (lobby == null) return;
            var data = lobby.GetData<ExpeditionLobbyData>();
            if (data == null)
            {
                data = new ExpeditionLobbyData();
                lobby.AddData(data);
            }

            int newId = data.missionIds.Count > 0 ? data.missionIds[data.missionIds.Count - 1] + 1 : 1;
            data.missionIds.Add(newId);
            data.missionTitles.Add(title);
            data.missionTargets.Add(target);
            data.missionProgress.Add(0);
            data.missionOwners.Add(rpc?.from ?? OnlineManager.lobby.owner);
            data.missionComplete.Add(false);

            // Best-effort bridge into Downpour DLC mission system when present.
            if (DownpourAvailable)
            {
                try
                {
                    TryBridgeToDownpour(title, target, rpc?.from);
                }
                catch (Exception ex)
                {
                    RainMeadow.Error("Expedition: Downpour bridge failed: " + ex.Message);
                }
            }
        }

        [RPCMethod]
        public static void Expedition_UpdateMissionProgress(RPCEvent rpc, int missionIndex, int progress)
        {
            if (rpc != null && OnlineManager.lobby.owner != rpc.from) return;
            var lobby = OnlineManager.lobby;
            if (lobby == null) return;
            var data = lobby.GetData<ExpeditionLobbyData>();
            if (data == null) return;
            if (missionIndex < 0 || missionIndex >= data.missionProgress.Count) return;
            data.missionProgress[missionIndex] = progress;
        }

        [RPCMethod]
        public static void Expedition_CompleteMission(RPCEvent rpc, int missionIndex)
        {
            if (rpc != null && OnlineManager.lobby.owner != rpc.from) return;
            var lobby = OnlineManager.lobby;
            if (lobby == null) return;
            var data = lobby.GetData<ExpeditionLobbyData>();
            if (data == null) return;
            if (missionIndex < 0 || missionIndex >= data.missionComplete.Count) return;
            data.missionComplete[missionIndex] = true;
        }

        private static void TryBridgeToDownpour(string title, int target, OnlinePlayer? owner)
        {
            if (!DownpourAvailable) return;

            // Try manager first
            if (DownpourMissionManagerType is not null)
            {
                // look for a method named AddMission/CreateMission with (string,int) signature
                var methods = DownpourMissionManagerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if ((m.Name.Contains("AddMission") || m.Name.Contains("CreateMission") || m.Name.Contains("RegisterMission")) )
                    {
                        var pars = m.GetParameters();
                        if (pars.Length >= 2 && pars[0].ParameterType == typeof(string) && pars[1].ParameterType == typeof(int))
                        {
                            object? instance = null;
                            if (!m.IsStatic)
                            {
                                try { instance = Activator.CreateInstance(DownpourMissionManagerType); } catch { instance = null; }
                            }
                            m.Invoke(instance, new object?[] { title, target });
                            return;
                        }
                    }
                }
            }

            // Fallback: try to instantiate a mission object if constructor matches
            if (DownpourMissionType is not null)
            {
                var ctors = DownpourMissionType.GetConstructors();
                foreach (var c in ctors)
                {
                    var pars = c.GetParameters();
                    if (pars.Length >= 2 && pars[0].ParameterType == typeof(string) && pars[1].ParameterType == typeof(int))
                    {
                        try
                        {
                            var obj = c.Invoke(new object?[] { title, target });
                            // if manager exists, try to register
                            if (DownpourMissionManagerType is not null)
                            {
                                var reg = DownpourMissionManagerType.GetMethod("Register", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
                                if (reg != null)
                                {
                                    object? inst = null;
                                    if (!reg.IsStatic)
                                    {
                                        try { inst = Activator.CreateInstance(DownpourMissionManagerType); } catch { inst = null; }
                                    }
                                    reg.Invoke(inst, new object?[] { obj });
                                }
                            }
                            return;
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
