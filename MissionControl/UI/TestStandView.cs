﻿using System;
using Gtk;
using MissionControl.Connection.Commands;
using MissionControl.Data;
using MissionControl.Data.Components;
using MissionControl.UI.Widgets;

namespace MissionControl.UI
{

    public interface ITestStandViewListener
    {
        void OnMenuSettingsPressed();
        void OnMenuPlotViewerPressed();
        void OnStatePressed(State state);
        void OnServoPressed(ServoComponent servo, float value);
        void OnSolenoidPressed(SolenoidComponent solenoid, bool open);
        void OnLogStartPressed();
        void OnLogStopPressed();
        void OnEmergencyCombinationPressed();
        void OnConnectPressed();
        void OnRunAutoSequencePressed();
    }

    public partial class TestStandView : Window, ILockable, IValveControlListener, IStateControlListener
    {

        SVGView _svgWidget;
        ValveControlWidget _valveWidget;
        StateControlWidget _stateWidget;
        ITestStandViewListener _listener;
        Session _session;

        bool _logRunning = false;
        Button _btnLogStart, _btnLogStop;

        Button _btnRunSequence;

        Button _btnConnect;
        Label _lastConnection;

        DToggleButton _btnLock;

        Gdk.Color _clrGoodConnection = new Gdk.Color(0, 255, 0);
        Gdk.Color _clrBadConnection = new Gdk.Color(255, 0, 0);

        bool _spaceDown, _escDown;

        public TestStandView(ITestStandViewListener listener, ref Session session) :
                base(Gtk.WindowType.Toplevel)
        {
            Title = "Control Panel";
            SetPosition(WindowPosition.Center);

            _listener = listener;
            _session = session;

            Layout();
            DeleteEvent += OnDeleteEvent;
            KeyPressEvent += WindowKeyPress;
            KeyReleaseEvent += WindowKeyRelease;
            ShowAll();
                
        }

        public void Layout()
        {

            // Background color
            ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 0));

            _svgWidget = new SVGView(@"Resources/TestStand.svg", ref _session);
            _valveWidget = new ValveControlWidget(_session.Mapping.Components(), this);
            _stateWidget = new StateControlWidget(_session.Mapping.States(), this);
            _stateWidget.SetCurrentState(_session.State, false);

            VBox midPanel = new VBox(false, 8);
            VBox rightPanel = new VBox(false, 8);

            // Logging
            VBox logButtonContainer = new VBox(false, 8);
            DSectionTitle logTitle = new DSectionTitle("Logging");
            HBox logButtons= new HBox(true, 8);
            _btnLogStart = new Button
            {
                Label = "Start log",
                HeightRequest = 40
            };
            _btnLogStart.Pressed += LogStartPressed;
            _btnLogStart.ModifyBg(StateType.Insensitive, new Gdk.Color(70, 70, 70));

            _btnLogStop = new Button
            {
                Label = "Stop log",
                HeightRequest = 40
            };
            _btnLogStop.Pressed += LogStopPressed;
            _btnLogStop.Sensitive = false;
            _btnLogStop.ModifyBg(StateType.Insensitive, new Gdk.Color(70, 70, 70));

            logButtons.PackStart(_btnLogStart, false, true, 0);
            logButtons.PackStart(_btnLogStop, false, true, 0);

            logButtonContainer.PackStart(logTitle, false, false, 0);
            logButtonContainer.PackStart(logButtons, false, false, 0);

            // Connection
            VBox connectionContainer = new VBox(false, 8);
            connectionContainer.PackStart(_lastConnection, false, false, 0);

            _btnConnect = new Button
            {
                Label = "Connect",
                HeightRequest = 40
            };

            _btnConnect.Pressed += (sender, e) =>
            {
                _listener.OnConnectPressed();
                _btnConnect.Sensitive = false;
                _btnConnect.Label = "Connecting...";
            };

            _btnConnect.ModifyBg(StateType.Insensitive, new Gdk.Color(70, 70, 70));

            _lastConnection = new Label { Text = "\n\n" };
            _lastConnection.ModifyFg(StateType.Normal, new Gdk.Color(255, 255, 255));
            _lastConnection.SetAlignment(0, 0.5f);

            DSectionTitle connectionTitle = new DSectionTitle("Connection");
            connectionContainer.PackStart(connectionTitle, false, false, 0);
            connectionContainer.PackStart(_btnConnect, false, false, 0);
            connectionContainer.PackStart(_lastConnection, false, false, 0);

            _btnLock = new DToggleButton(100, 40, "Enable controls", "Disable controls", DToggleButton.ToggleState.Inactive);
            _btnLock.Pressed += LockButtonPressed;
            _btnLock.ModifyBg(StateType.Insensitive, new Gdk.Color(140, 140, 140));

            _btnRunSequence = new Button
            {
                Label = "Run Auto Sequence",
                HeightRequest = 40
            };
            _btnRunSequence.Pressed += RunSequenceButtonPressed;
            _btnRunSequence.ModifyBg(StateType.Insensitive, new Gdk.Color(140, 140, 140));

            // Mid panel
            DSectionTitle valvesTitle = new DSectionTitle("Valves");
            midPanel.PackStart(valvesTitle, false, false, 0);
            midPanel.PackStart(_valveWidget, false, false, 0);
            midPanel.PackStart(logButtonContainer, false, false, 20);
            midPanel.PackStart(_btnLock, false, false, 20);

            // Right panel
            DSectionTitle statesTitle = new DSectionTitle("States");
            DSectionTitle autoSequenceTitle = new DSectionTitle("Auto Sequence");
            rightPanel.PackStart(statesTitle, false, false, 0);
            rightPanel.PackStart(_stateWidget, false, false, 20);
            rightPanel.PackStart(autoSequenceTitle, false, false, 0);
            rightPanel.PackStart(_btnRunSequence, false, false, 0);
            rightPanel.PackStart(connectionContainer, false, false, 20);

            // Horizonal layout
            HBox horizontalLayout = new HBox(false, 0);
            horizontalLayout.PackStart(_svgWidget, true, true, 0);
            horizontalLayout.PackStart(midPanel, false, false, 20);
            horizontalLayout.PackStart(rightPanel, false, false, 20);

            // Window layout
            Alignment align = new Alignment(0.5f, 0.5f, 1, 1)
            {
                TopPadding = 20,
                LeftPadding = 20,
                RightPadding = 20,
                BottomPadding = 20
            };
            align.Add(horizontalLayout);

            VBox verticalLayout = new VBox(false, 0);
  
            MenuBar menu = new MenuBar();

            MenuItem settingsItem = new MenuItem("Settings");
            settingsItem.Activated += (sender, e) => _listener.OnMenuSettingsPressed();
            menu.Append(settingsItem);

            MenuItem plotItem = new MenuItem("Plot Viewer");
            plotItem.Activated += (sender, e) => _listener.OnMenuPlotViewerPressed();
            menu.Append(plotItem);

            verticalLayout.PackStart(menu, false, false, 0);
            verticalLayout.PackStart(align, true, true, 0);

            Add(verticalLayout);

        }

        void LogStartPressed(object sender, EventArgs e)
        {
            _listener.OnLogStartPressed();
            _logRunning = true;
            _btnLogStart.Sensitive = !_logRunning;
            _btnLogStop.Sensitive = _logRunning;
        }

        void LogStopPressed(object sender, EventArgs e)
        {
            _listener.OnLogStopPressed();
            _logRunning = false;
            _btnLogStart.Sensitive = !_logRunning;
            _btnLogStop.Sensitive = _logRunning;
        }

        void LockButtonPressed (object sender, EventArgs e)
        {
            _btnLock.Toggle();
            UpdateControls();
        }

        void RunSequenceButtonPressed (object sender, EventArgs e)
        {
            _listener.OnRunAutoSequencePressed();
        }


        public void UpdateControls() {
            UpdateLastConnectionLabel();
            UpdateConnectButton();

            _stateWidget.SetCurrentState(_session.State, _session.IsAutoSequence);
            _valveWidget.Sensitive = !_btnLock.Active && !_session.IsAutoSequence;
            _stateWidget.Sensitive = !_btnLock.Active && !_session.IsAutoSequence;
            _btnRunSequence.Sensitive = !_btnLock.Active && !_session.IsAutoSequence;
            _btnLock.Sensitive = !_session.IsAutoSequence;
        }

        public void UpdateSVG() {
            _svgWidget.Refresh();
        }

        public void UpdateLastConnectionLabel ()
        {
            double time = Math.Floor(10 * (DateTime.Now - _session.LastReceived).TotalMilliseconds / 1000.0) / 10;
            _lastConnection.Text = string.Format("Time since package:\n{0} s", time);
            _lastConnection.ModifyFg(StateType.Normal, (time < 4) ? _clrGoodConnection : _clrBadConnection);
        }

        public void UpdateConnectButton() { 
            if (_session.Connected)
            {
                _btnConnect.Sensitive = false;
                _btnConnect.Label = "Connected";
            }
            else
            {
                _btnConnect.Sensitive = true;
                _btnConnect.Label = "Connect";
            }
        }

        [GLib.ConnectBefore]
        protected void WindowKeyPress(object sender, KeyPressEventArgs args)
        {
            _spaceDown |= args.Event.Key == Gdk.Key.space;
            _escDown |= args.Event.Key == Gdk.Key.Escape;

            if (_spaceDown && _escDown)
            {
                _listener.OnEmergencyCombinationPressed();
            }
        }

        void WindowKeyRelease(object o, KeyReleaseEventArgs args)
        {
            _spaceDown &= args.Event.Key == Gdk.Key.space;
            _escDown &= args.Event.Key == Gdk.Key.Escape;
        }


        public void Lock()
        {
            throw new NotImplementedException();
        }

        public void Unlock()
        {
            throw new NotImplementedException();
        }

        protected void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            KeyPressEvent -= WindowKeyPress;
            Application.Quit();
            a.RetVal = true;
        }

        public void OnStatePressed(State state)
        {
            _listener.OnStatePressed(state);
        }

        public void OnServoPressed(ServoComponent servo, float value)
        {
            _listener.OnServoPressed(servo, value);

        }

        public void OnSolenoidPressed(SolenoidComponent solenoid, bool open)
        {
            _listener.OnSolenoidPressed(solenoid, open);
        }

        public void OnControlEnter(ValveComponent component)
        {
            _svgWidget.MarkValve(component);
        }

        public void OnControlLeave(ValveComponent component)
        {
            _svgWidget.UnmarkValve(component);
        }
    }
}
