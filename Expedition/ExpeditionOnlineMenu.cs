using HarmonyLib;
using Menu;
using Menu.Remix.MixedUI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using RainMeadow.UI.Components;









namespace RainMeadow
{
    // Minimal Expedition menu scaffold. Expand UI as needed.
    public class ExpeditionOnlineMenu : SmartMenu
    {
        EventfulHoldButton startButton;

        private SlugcatCustomization personaSettings;
        public ChatMenuBox? chatMenuBox;
        public PlayerDisplayer? playerDisplayer;
        private MenuLabel? lobbyLabel, slugcatLabel;
        private ButtonScroller? playerScrollBox;
        public override MenuScene.SceneID GetScene => null;
        public BigArrowButton prevButton;
        public BigArrowButton nextButton;


        //Chat constants
        private const int maxVisibleMessages = 10;
        //Chat variables
        private List<MenuObject> chatSubObjects = [];
        private List<(string, string)> chatLog = [];
        private int currentLogIndex = 0;
        private bool isChatToggled = false;
        private ChatTextBox chatTextBox;
        private Vector2 chatTextBoxPos;


        public static int MaxVisibleOnList => 8;
        public static float ButtonSpacingOffset => 8;
        public static float ButtonSizeWithSpacing => ButtonSize + ButtonSpacingOffset;
        public static float ButtonSize => 30;




        private void SetupOnlineCustomization()
        {
            // personaSettings = ExpeditionGameMode.avatarSettings[0];
        }


        private void SetupClientOptions()
        {
            //restartCheckbox = new CheckBox(this, pages[0], this, restartCheckboxPos, 70f, Translate("Match save"), "CLIENTSAVERESET", false);
            //restartCheckbox.displayText = "Match save";
            //restartCheckbox.label.text = "Match save";
            //restartCheckbox.IDString = "CLIENTSAVERESET";
            //pages[0].subObjects.Add(clientWantsToOverwriteSave);
        }

        public void ToggleChat(bool toggled)
        {
            this.isChatToggled = toggled;
            this.ResetChatInput();
            this.UpdateLogDisplay();
        }

        internal void ResetChatInput()
        {
            this.chatTextBox?.DelayedUnload(0.1f);
            pages[0].ClearMenuObject(ref this.chatTextBox);
            if (this.isChatToggled && this.chatTextBox is null)
            {
                this.chatTextBox = new ChatTextBox(this, pages[0], "", new Vector2(this.chatTextBoxPos.x + 24, 0), new(575, 30));
                pages[0].subObjects.Add(this.chatTextBox);
            }
        }

        public void AddMessage(string user, string message)
        {
            if (OnlineManager.lobby == null) return;
            if (OnlineManager.lobby.gameMode.mutedPlayers.Contains(user)) return;
            MatchmakingManager.currentInstance.FilterMessage(ref message);
            this.chatLog.Add((user, message));
            this.UpdateLogDisplay();
        }
        private void SetupOnlineMenuItems()
        {
            // Player lobby label
            lobbyLabel = new MenuLabel(this, pages[0], Translate("LOBBY"), new Vector2(194, 553), new(110, 30), true);
            pages[0].subObjects.Add(lobbyLabel);

            var invite = new SimplerButton(this, pages[0], Translate("Invite Friends"), new(nextButton.pos.x + 80f, 50f), new(110, 35));
            invite.OnClick += (_) => MatchmakingManager.currentInstance.OpenInvitationOverlay();
            pages[0].subObjects.Add(invite);

            //this.chatTextBoxPos = new Vector2(this.manager.rainWorld.options.ScreenSize.x * 0.001f + (1366f - this.manager.rainWorld.options.ScreenSize.x) / 2f, 0);
            //var toggleChat = new SimplerSymbolButton(this, pages[0], "Kill_Slugcat", "", this.chatTextBoxPos);
            //toggleChat.OnClick += (_) =>
            {
                //ToggleChat(!this.isChatToggled);
                if (input.controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer)
                {
                    selectedObject = null;
                }
            }
            ;
            //pages[0].subObjects.Add(toggleChat);

            // var sameSpotOtherSide = restartCheckboxPos.x - startButton.pos.x;
            //friendlyFire = new CheckBox(this, pages[0], this, new Vector2(startButton.pos.x - sameSpotOtherSide, restartCheckboxPos.y + 30), 70f, Translate("Friendly Fire"), "ONLINEFRIENDLYFIRE", false);
            //reqCampaignSlug = new CheckBox(this, pages[0], this, new Vector2(startButton.pos.x - sameSpotOtherSide, restartCheckboxPos.y), 150f, Translate("Require Campaign Slugcat"), "CAMPAIGNSLUGONLY", false);
            if (!OnlineManager.lobby.isOwner)
            {
                //   friendlyFire.buttonBehav.greyedOut = true;
                //   reqCampaignSlug.buttonBehav.greyedOut = true;
            }
            //pages[0].subObjects.Add(friendlyFire);
            //pages[0].subObjects.Add(reqCampaignSlug);
        }
        private void ModifyExistingMenuItems()
        {
            foreach (var obj in pages[0].subObjects) // unfortunate locally declared variable.
            {
                if (obj is SimpleButton button && button.signalText == "BACK")
                {
                    button.pos = new Vector2(prevButton.pos.x - 140f, 50f);
                }
            }
        }
        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager, RainMeadow.Ext_ProcessID.ExpeditionMenu)
        {
            backTarget = RainMeadow.Ext_ProcessID.LobbySelectMenu;
            RainMeadow.Warn("GARRA TEST PAGES:" + pages.Count);


            this.startButton = new EventfulHoldButton(this, this.pages[0], this.Translate("ENTER"), new UnityEngine.Vector2(683f, 85f), 40f);
            this.startButton.OnClick += (_) => { StartGame(); };
            this.pages[0].subObjects.Add(this.startButton);

            // Chat and player list (mirrors Story/Arena lobby patterns)
            try
            {
                chatMenuBox = new ChatMenuBox(this, this.pages[0], new Vector2(100f, 125f), new Vector2(300, 425));
                chatMenuBox.roundedRect.size.y = 475f;
                pages[0].subObjects.Add(chatMenuBox);
                ChatLogManager.Subscribe(chatMenuBox);

                //playerDisplayer = new PlayerDisplayer(this, pages[0], new Vector2(960f, 130f), [.. OnlineManager.players.OrderByDescending(x => x.isMe)], GetPlayerButton, 4, ArenaPlayerBox.DefaultSize.x, new(ArenaPlayerBox.DefaultSize.y, 0), new(ArenaPlayerSmallBox.DefaultSize.y, 10));
                //pages[0].subObjects.Add(playerDisplayer);
                //playerDisplayer.CallForRefresh();

                ModifyExistingMenuItems();
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
                //testing menus
                if (OnlineManager.lobby.isOwner)
                {
                    RainMeadow.Warn("garra test Im the lobby owner :D");
                    lobbyLabel = new MenuLabel(this, pages[0], Translate("LOBBY"), new Vector2(194, 553), new(110, 30), true);
                    RainMeadow.Warn("garra test page 0 Count" + pages[0].subObjects.Count());

                    pages[0].subObjects.Add(lobbyLabel);

                    RainMeadow.Warn("garra test page 0 Count" + pages[0].subObjects.Count());
                }
                else
                {
                    RainMeadow.Warn("garra test Im NOT the lobby owner :D");
                    MatchmakingManager.OnPlayerListReceived += OnlineManager_OnPlayerListReceived;

                    SetupClientOptions();
                }


                SetupOnlineCustomization();

                SetupOnlineMenuItems();
                UpdatePlayerList();

                //slugcatPageIndex = indexFromColor(storyGameMode.currentCampaign);


                //UpdateSelectedSlugcatInMiscProg();

                MatchmakingManager.OnPlayerListReceived += OnlineManager_OnPlayerListReceived;

                //ChatTextBox.OnShutDownRequest += ResetChatInput;
                //ChatLogManager.Subscribe(this);


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
            // For a minimal scaffold, just transition to game start like MeadowMenu does.
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
                playerDisplayer?.UpdatePlayerList([.. OnlineManager.players.OrderByDescending(x => x.isMe)]);
            }
            catch { }
        }

        public override void Update()
        {
            var jollyallowed = false;
            if (ModManager.JollyCoop)
            {

            }


            if (ChatTextBox.blockInput)
            {
                //ChatTextBox.blockInput = false;
                //if ((RWInput.CheckPauseButton(0) || Input.GetKeyDown(KeyCode.Escape)) && !lastPauseButton)
                //{
                //    PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                //    ToggleChat(false);
                //    lastPauseButton = true;
                //}
                //ChatTextBox.blockInput = true;
            }

            //if (ExpeditionGameMode.needMenuSaveUpdate)
            //{
            //    RainMeadow.Debug("page refresh");
            //    RefreshPages();
            //}


            base.Update();

            if (ModManager.JollyCoop)
            {
                //this.storyGameMode.friendlyFire = manager.rainWorld.options.friendlyFire;
                // if (jollyallowed)
                // {
                //     this.jollyPlayerCountLabel.text = base.Translate("Players: <num_p>").Replace("<num_p>", Custom.rainWorld.options.JollyPlayerCount.ToString());
                //     this.RefreshJollySummary();
                // }

            }

            /*
                        if (this.isChatToggled)
                        {
                            if (Input.GetKey(KeyCode.UpArrow))
                            {
                                if (currentLogIndex < chatLog.Count - 1)
                                {
                                    currentLogIndex++;
                                    UpdateLogDisplay();
                                }
                            }
                            else if (Input.GetKey(KeyCode.DownArrow))
                            {
                                if (currentLogIndex > 0)
                                {
                                    currentLogIndex--;
                                    UpdateLogDisplay();
                                }
                            }
                        }*/

            if (OnlineManager.lobby == null) return;
            if (OnlineManager.lobby.isOwner)
            {
                nextButton.buttonBehav.greyedOut = false;
                prevButton.buttonBehav.greyedOut = false;

                if (startButton != null)
                {
                    startButton.buttonBehav.greyedOut = OnlineManager.lobby.clientSettings.Values.Any(cs => cs.inGame);
                }

            }
            else
            {

                nextButton.buttonBehav.greyedOut = true;
                prevButton.buttonBehav.greyedOut = true;

                // if (onlineDifficultyLabel == null)
                // {
                //     onlineDifficultyLabel = new MenuLabel(this, pages[0], $"{GetCurrentCampaignName()}", new Vector2(startButton.pos.x - 100f, startButton.pos.y + 100f), new Vector2(200f, 30f), bigText: true);
                //     onlineDifficultyLabel.label.alignment = FLabelAlignment.Center;
                //     onlineDifficultyLabel.label.alpha = 0.5f;
                //     pages[0].subObjects.Add(onlineDifficultyLabel);
                // }


                // if (onlineDifficultyLabel != null)
                // {
                //     onlineDifficultyLabel.text = GetCurrentCampaignName() + (string.IsNullOrEmpty(storyGameMode.region) ? Translate(" - New Game") : " - " + Translate(storyGameMode.region));
                // }

            }

        }


        internal void UpdateLogDisplay()
        {
            if (!this.isChatToggled)
            {
                var list = new List<MenuObject>();
                foreach (var e in chatSubObjects)
                {
                    e.RemoveSprites();
                    list.Add(e);
                }
                foreach (var e in list) pages[0].RemoveSubObject(e);
                chatSubObjects.Clear(); //do not keep gc stuff!
                return;
            }
            if (chatLog.Count > 0)
            {
                int startIndex = Mathf.Clamp(chatLog.Count - maxVisibleMessages - currentLogIndex, 0, chatLog.Count - maxVisibleMessages);
                var logsToRemove = new List<MenuObject>();

                // First, collect all the logs to remove
                foreach (var log in chatSubObjects)
                {
                    log.RemoveSprites();
                    logsToRemove.Add(log);
                }

                // Now remove the logs from the original collection
                foreach (var log in logsToRemove)
                {
                    chatSubObjects.Remove(log);
                    pages[0].RemoveSubObject(log);
                }

                ChatLogManager.UpdatePlayerColors();

                float yOffSet = 0;
                var visibleLog = chatLog.Skip(startIndex).Take(maxVisibleMessages);
                foreach (var (username, message) in visibleLog)
                {
                    if (username is null or "")
                    {
                        // system message
                        var messageLabel = new MenuLabel(this, pages[0], message,
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        messageLabel.label.alignment = FLabelAlignment.Left;
                        messageLabel.label.color = ChatLogManager.defaultSystemColor;
                        chatSubObjects.Add(messageLabel);
                        pages[0].subObjects.Add(messageLabel);
                    }
                    else
                    {
                        var color = ChatLogManager.GetDisplayPlayerColor(username);

                        var usernameLabel = new MenuLabel(this, pages[0], username,
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        usernameLabel.label.alignment = FLabelAlignment.Left;
                        usernameLabel.label.color = color;
                        chatSubObjects.Add(usernameLabel);
                        pages[0].subObjects.Add(usernameLabel);

                        var usernameWidth = LabelTest.GetWidth(usernameLabel.label.text);
                        var messageLabel = new MenuLabel(this, pages[0], $": {message}",
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f + usernameWidth + 2f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        messageLabel.label.alignment = FLabelAlignment.Left;
                        chatSubObjects.Add(messageLabel);
                        pages[0].subObjects.Add(messageLabel);
                    }
                    yOffSet += 20f;
                }
            }
        }

        private void UpdatePlayerList()
        {
            playerScrollBox?.RemoveAllButtons(false);
            if (playerScrollBox == null)
            {
                playerScrollBox = new(this, pages[0], new(194, 553 - 30 - ButtonScroller.CalculateHeightBasedOnAmtOfButtons(MaxVisibleOnList, ButtonSize, ButtonSpacingOffset)), MaxVisibleOnList, 200, new(ButtonSize, ButtonSpacingOffset));
                pages[0].subObjects.Add(playerScrollBox);
            }
            foreach (OnlinePlayer player in OnlineManager.players)
            {
                StoryMenuPlayerButton playerButton = new(this, playerScrollBox, player, OnlineManager.lobby.isOwner && player != OnlineManager.lobby.owner);
                playerScrollBox.AddScrollObjects(playerButton);
            }
            playerScrollBox.ConstrainScroll();

        }
        void RefreshPages()
        {
            UpdatePlayerList();

            pages[0].mouseCursor.BumToFront(); //add cursor container back
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
