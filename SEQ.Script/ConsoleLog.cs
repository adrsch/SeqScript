using Stride.Engine;
using Stride.Games;
using Stride.UI;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Extensions;
using Stride.Core.Mathematics;
using SEQ.Script;
using Stride.Input;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public class ConsoleLog : SyncScript, IGameStateController
    {
        public UIPage page;

        TextBlock logBlock;
        
        ScrollViewer scroller;
        EditText editText;
        public InteractionState GetInteractionState()
        {
            return InteractionState.GUI;
        }

        public PointerState GetPointerState()
        {
            return PointerState.GUI;
        }

        public void OnGainControl()
        {
            Show();
        }

        public void OnLoseControl()
        {
            Hide();
        }
        public UIComponent PageComponent;
        public static ConsoleLog Inst;
        public override void Start()
        {
            Inst = this;
            logBlock = page.RootElement.FindVisualChildOfType<TextBlock>("log");
            scroller = page.RootElement.FindVisualChildOfType<ScrollViewer>("scroller");
            editText = page.RootElement.FindVisualChildOfType<EditText>("console");


            Logger.OnLogUpdated += () =>
                {
                    if (page.RootElement.IsEnabled)
                    {
                        logBlock.Text = Logger.GetLogs(new LogDisplayOptions
                        {
                            channelFilter = Channel.All,
                            priorityFilter = LogPriority.All,
                            showTimeStamp = true,
                        });

                        scroller.ScrollToEnd(Orientation.Vertical);
                    }
            };


            PageComponent.Enabled = true;
            Hide();
        }


        LinkedList<string> History = new LinkedList<string>();
        int historySpot = -1;

        void MoveToHistory()
        {
            if (historySpot < 0)
            {
                historySpot = -1;

                editText.Text = "";
            }
            else if (historySpot >= History.Count)
            {
                historySpot = History.Count - 1;
            }
            else
            {
                editText.Text = History.ElementAt(historySpot);
            }
        }

        bool usingController;

        bool buttonup = true;

        public override void Update()
        {
            if (editText.Text.StartsWith('`'))
                editText.Text = "";
            
            if (Input.IsGamePadButtonDownAny(Stride.Input.GamePadButton.Back))
            {
                if (buttonup)
                {
                    buttonup = false;
                    usingController = true;
                    if (!this.IsActive())
                    {
                        GameStateManager.Push(this);
                    }
                    else
                    {
                        editText.Text = "";
                        GameStateManager.Remove(this);
                    }
                }
            }
            else
            {
                buttonup = true;
            }
           if (Keybinds.GetKeyUp(Keybind.Console))
            {
                usingController = false;
                if (!this.IsActive())
                {
                    if (this.IsBuried())
                    {
                        editText.Text = "";
                        GameStateManager.Remove(this);
                        GameStateManager.Push(this);
                    }
                    else
                    {
                        GameStateManager.Push(this);
                    }
                }
                else
                {
                    editText.Text = "";
                    GameStateManager.Remove(this);
                }
           }
            if (this.IsActive())
            {
                var size = G.S.IsFullscreen ? Game.Window.PreferredFullscreenSize : Game.Window.PreferredWindowedSize;
                var asVector3 = new Vector3(size.X, size.Y, 512);
                PageComponent.Size = asVector3;
                PageComponent.Resolution = asVector3;

              editText.IsSelectionActive = true;
                if (Input.IsKeyPressed(Stride.Input.Keys.Up))
                {
                    historySpot++;
                    MoveToHistory();
                }
                if (Input.IsKeyPressed(Stride.Input.Keys.Down))
                {
                    historySpot--;
                    MoveToHistory();
                }
                if (Input.IsKeyReleased(Stride.Input.Keys.Enter))
                {

                    if (!string.IsNullOrWhiteSpace(editText.Text))
                    {
                        historySpot = -1;
                        History.AddFirst(editText.Text);
                        var cmd = editText.Text;
                        MoveToHistory();
                        Shell.Exec(cmd);
                    }
                }
                /*
                StringBuilder sb = new StringBuilder();
                foreach (var e in Input.KeyEvents)
                {
                    //editText.IsSelectionActive = true;
                    if (e.Key == (Stride.Input.Keys.Up))
                    {
                        historySpot++;
                        MoveToHistory();
                    }
                    else if (e.Key == (Stride.Input.Keys.Down))
                    {
                        historySpot--;
                        MoveToHistory();
                    }
                    else if (e.Key == (Stride.Input.Keys.Enter))
                    {

                        if (!string.IsNullOrWhiteSpace(editText.Text))
                        {
                            historySpot = -1;
                            History.AddFirst(editText.Text);
                            var cmd = editText.Text;
                            MoveToHistory();
                            Shell.Exec(cmd);
                        }
                    }
                    else if (e.Key == Keys.Left)
                    {
                        editText.CaretPosition--;

                    }
                    else if (e.Key == Keys.Right)
                    {
                        editText.CaretPosition++;
                    }
                    else if (e.Key == Keys.BackSpace)
                    {
                        editText.Text += e.Key;
                    }
                    else
                    {
                        editText.Text += e.Key;
                    }
                }*/
            }
            else
            {
                editText.IsSelectionActive = false;
            }
        }

        void Hide()
        {
            page.RootElement.Visibility = Visibility.Collapsed;

        }

        bool addedEvents = false;
        void Show()
        {
            /*
            var size = G.Inst.isFullscreen ? Game.Window.PreferredFullscreenSize : Game.Window.PreferredWindowedSize;
            var asVector3 = new Vector3(size.X, size.Y, 512);
            PageComponent.Size = asVector3;
            PageComponent.Resolution = asVector3;
            */
            editText.IsSelectionActive = true;
            Game.Window.AllowUserResizing = true;
            page.RootElement.Visibility = Visibility.Visible;
            editText.Text = "";
            var tes = Input.KeyEvents;
            logBlock.Text = Logger.GetLogs(new LogDisplayOptions
            {
                channelFilter = Channel.All,
                priorityFilter = LogPriority.All,
                showTimeStamp = true,
            });
            Game.Window.IsMouseVisible = true;
            Input.UnlockMousePosition();
            scroller.ScrollToEnd(Orientation.Vertical);
            
            if (G.S.GetModule<ITouchModule>() is ITouchModule touchModule && touchModule.TouchEnabled)
            {
                if (!addedEvents)
                {
                    editText.TouchDown += (x, y) =>
                    {
                        Logger.Log(Channel.Input, LogPriority.Info, "Opening keyboard...");
                        //   SteamUtils.ShowGamepadTextInput(GamepadTextInputMode.Normal, GamepadTextInputLineMode.MultipleLines, "Developer console", 512);
                        touchModule.OpenKeyboard("Developer console", true);
                    };

                    /*
                    SteamUtils.OnGamepadTextInputDismissed += didSubmit =>
                    {
                        if (didSubmit)
                        {
                            Shell.Exec(SteamUtils.GetEnteredGamepadText());
                        }
                    };*/
                }
                addedEvents = true;
                /*
                if (!SteamUtils.ShowFloatingGamepadTextInput(TextInputMode.MultipleLines, 0, 356, 1280, 33))
                {
                    //Logger.Log(Channel.Input, LogPriority.Error, "Failed to show floating keyboard");
                    //       SteamUtils.ShowGamepadTextInput(GamepadTextInputMode.Normal, GamepadTextInputLineMode.MultipleLines, "Developer console", 512);
                }
                */
                //  SteamUtils.ShowGamepadTextInput(GamepadTextInputMode.Normal, GamepadTextInputLineMode.MultipleLines, "Developer console", 512);
            }
        }

    }
}
