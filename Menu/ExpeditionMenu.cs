using Menu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RainMeadow.UI.Components;
using System;

namespace RainMeadow
{
    // Minimal Expedition menu scaffold. Expand UI as needed.
    public class ExpeditionMenu : SmartMenu
    {
        EventfulHoldButton startButton;
        public ChatMenuBox? chatMenuBox;
        public PlayerDisplayer? playerDisplayer;

        public override MenuScene.SceneID GetScene => MenuScene.SceneID.Landscape_SU;

        public ExpeditionMenu(ProcessManager manager) : base(manager, RainMeadow.Ext_ProcessID.ExpeditionMenu)
        {
            backTarget = RainMeadow.Ext_ProcessID.LobbySelectMenu;

            this.startButton = new EventfulHoldButton(this, this.pages[0], this.Translate("ENTER"), new UnityEngine.Vector2(683f, 85f), 40f);
            this.startButton.OnClick += (_) => { StartGame(); };
            this.pages[0].subObjects.Add(this.startButton);

            // explicit Back button to ensure correct lobby leave behavior
            try
            {
                var backBtn = new SimplerButton(this, this.pages[0], this.Translate("BACK"), new Vector2(50f, 85f), new Vector2(110f, 30f));
                backBtn.OnClick += _ =>
                {
                    try { OnlineManager.LeaveLobby(); } catch { }
                    PlaySound(SoundID.MENU_Switch_Page_Out);
                    manager.RequestMainProcessSwitch(RainMeadow.Ext_ProcessID.LobbySelectMenu);
                };
                this.pages[0].subObjects.Add(backBtn);
            }
            catch { }

            // Chat and player list (mirrors Story/Arena lobby patterns)
            try
            {
                chatMenuBox = new ChatMenuBox(this, this.pages[0], new Vector2(100f, 125f), new Vector2(300, 425));
                chatMenuBox.roundedRect.size.y = 475f;
                pages[0].subObjects.Add(chatMenuBox);
                ChatLogManager.Subscribe(chatMenuBox);

                playerDisplayer = new PlayerDisplayer(this, pages[0], new Vector2(960f, 130f), OnlineManager.players.OrderByDescending(x => x.isMe).ToList(), GetPlayerButton, 4, ArenaPlayerBox.DefaultSize.x, new(ArenaPlayerBox.DefaultSize.y, 0), new(ArenaPlayerSmallBox.DefaultSize.y, 10));
                pages[0].subObjects.Add(playerDisplayer);
                playerDisplayer.CallForRefresh();

                MatchmakingManager.OnPlayerListReceived += OnlineManager_OnPlayerListReceived;

                // If JollyCoop mod is present, add a small shortcut to its setup dialog so players can access Jolly settings in expedition lobby.
                if (ModManager.JollyCoop)
                {
                    try
                    {
                        SimplerButton jollyBtn = new(this, this.pages[0], this.Translate("JollyCoop"), new Vector2(50f, 50f), new Vector2(140f, 30f));
                        jollyBtn.OnClick += _ =>
                        {
                            try
                            {
                                var dlg = new JollyCoop.JollyMenu.JollySetupDialog(SlugcatStats.Name.White, manager, jollyBtn.pos + new Vector2(0f, 50f));
                                manager.ShowDialog(dlg);
                            }
                            catch (System.Exception ex)
                            {
                                RainMeadow.Debug("ExpeditionMenu: failed to open Jolly dialog: " + ex.Message);
                            }
                        };
                        pages[0].subObjects.Add(jollyBtn);
                    }
                    catch (System.Exception ex)
                    {
                        RainMeadow.Debug("ExpeditionMenu: failed to create Jolly button: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                RainMeadow.Debug("ExpeditionMenu: failed to init chat/player list: " + ex.Message);
            }
        }

        public override void Init()
        {
            base.Init();
            selectedObject = startButton;
        }

        private void StartGame()
        {
            if (OnlineManager.lobby == null || !OnlineManager.lobby.isActive) return;
            // only the lobby owner may trigger the expedition start; non-owners wait in the menu
            if (!OnlineManager.lobby.isOwner)
            {
                PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
                try
                {
                    manager.ShowDialog(new DialogNotify(this.Translate("Waiting for host to start"), manager, null));
                }
                catch { }
                return;
            }

            // Owner starts locally (network start flow handled by game mode/lobby code)
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
        }

        public override void ShutDownProcess()
        {
            if (manager.upcomingProcess != ProcessManager.ProcessID.Game && manager.upcomingProcess != RainMeadow.Ext_ProcessID.ExpeditionMenu)
            {
                OnlineManager.LeaveLobby();
            }

            try
            {
                if (chatMenuBox != null) ChatLogManager.Unsubscribe(chatMenuBox);
                MatchmakingManager.OnPlayerListReceived -= OnlineManager_OnPlayerListReceived;
            }
            catch { }

            base.ShutDownProcess();
        }

        public void OnlineManager_OnPlayerListReceived(PlayerInfo[] players)
        {
            try
            {
                playerDisplayer?.UpdatePlayerList(OnlineManager.players.OrderByDescending(x => x.isMe).ToList());
            }
            catch { }
        }

        public ButtonScroller.IPartOfButtonScroller GetPlayerButton(PlayerDisplayer playerDisplay, bool isLargeDisplay, OnlinePlayer player, Vector2 pos)
        {
            if (isLargeDisplay)
            {
                ArenaPlayerBox playerBox = new(this, playerDisplay, player, OnlineManager.lobby?.isOwner == true, pos);
                return playerBox;
            }
            ArenaPlayerSmallBox playerSmallBox = new(this, playerDisplay, player, OnlineManager.lobby?.isOwner == true, pos);
            return playerSmallBox;
        }
    }
}
