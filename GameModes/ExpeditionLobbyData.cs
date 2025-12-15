using System;
using System.Collections.Generic;
using RainMeadow.Generics;
using static RainMeadow.OnlineResource;

namespace RainMeadow
{
    public class ExpeditionLobbyData : OnlineResource.ResourceData
    {
        public override ResourceDataState MakeState(OnlineResource resource)
        {
            return new State(this, resource);
        }

        public class State : ResourceDataState
        {
            [OnlineField]
            public List<int> missionIds = new();
            [OnlineField]
            public List<string> missionTitles = new();
            [OnlineField]
            public List<int> missionProgress = new();
            [OnlineField]
            public List<int> missionTargets = new();
            [OnlineField]
            public List<OnlinePlayer> missionOwners = new();
            [OnlineField]
            public List<bool> missionComplete = new();

            public State() { }

            public State(ExpeditionLobbyData d, OnlineResource resource)
            {
                missionIds = new(d.missionIds);
                missionTitles = new(d.missionTitles);
                missionProgress = new(d.missionProgress);
                missionTargets = new(d.missionTargets);
                missionOwners = new(d.missionOwners);
                missionComplete = new(d.missionComplete);
            }

            public override Type GetDataType() => typeof(ExpeditionLobbyData);

            public override void ReadTo(ResourceData data, OnlineResource resource)
            {
                var d = (ExpeditionLobbyData)data;
                d.missionIds = new(missionIds);
                d.missionTitles = new(missionTitles);
                d.missionProgress = new(missionProgress);
                d.missionTargets = new(missionTargets);
                d.missionOwners = new(missionOwners);
                d.missionComplete = new(missionComplete);
            }
        }

        // backing lists used by owner to modify missions
        public List<int> missionIds = new();
        public List<string> missionTitles = new();
        public List<int> missionProgress = new();
        public List<int> missionTargets = new();
        public List<OnlinePlayer> missionOwners = new();
        public List<bool> missionComplete = new();

        public ExpeditionLobbyData() { }
    }
}
