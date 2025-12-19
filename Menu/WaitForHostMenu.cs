using Menu;
using UnityEngine;

namespace RainMeadow
{/*
    public class WaitForHostMenu : SmartMenu
    {
        public override MenuScene.SceneID GetScene => MenuScene.SceneID.Landscape_SU;
        public WaitForHostMenu(ProcessManager manager) : base(manager, RainMeadow.Ext_ProcessID.WaitForHostMenu)
        {
            backTarget = RainMeadow.Ext_ProcessID.LobbySelectMenu;

            var page = this.pages[0];
            var label = new MenuLabel(this, page, this.Translate("Please wait for the host to start the expedition"), new Vector2(683f, 320f), new Vector2(1000f, 60f), true, null);
            page.subObjects.Add(label);

            try
            {
                var leaveBtn = new SimplerButton(this, page, this.Translate("LEAVE_LOBBY"), new Vector2(50f, 85f), new Vector2(140f, 30f));
                leaveBtn.OnClick += _ =>
                {
                    try { OnlineManager.LeaveLobby(); } catch { }
                    PlaySound(SoundID.MENU_Switch_Page_Out);
                    manager.RequestMainProcessSwitch(RainMeadow.Ext_ProcessID.LobbySelectMenu);
                };
                page.subObjects.Add(leaveBtn);
            }
            catch { }
        }

        public override void Init()
        {
            base.Init();
        }
    }
*/}
