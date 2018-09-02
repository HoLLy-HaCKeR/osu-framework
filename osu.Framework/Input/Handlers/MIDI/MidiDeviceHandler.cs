// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;

namespace osu.Framework.Input.Handlers.MIDI
{
    public class MidiDeviceHandler : InputHandler
    {
        private ScheduledDelegate scheduledRefreshDevices;

        private readonly List<MidiDevice> devices = new List<MidiDevice>();

        public override bool Initialize(GameHost host)
        {
            Enabled.BindValueChanged(enabled =>
            {
                if (enabled)
                {
                    host.InputThread.Scheduler.Add(scheduledRefreshDevices = new ScheduledDelegate(refreshDevices, 0, 500));
                }
                else
                {
                    scheduledRefreshDevices?.Cancel();

                    foreach (var device in devices)
                    {
                        if (device.LastState != null)
                            handleState(device, new MidiState());
                    }

                    devices.Clear();
                }
            }, true);

            return true;
        }

        private void handleState(MidiDevice device, MidiState newState)
        {
            PendingInputs.Enqueue(new MidiKeyInput(newState.Keys, device.LastState?.Keys));

            device.LastState = (MidiState)newState.Clone();

            FrameStatistics.Increment(StatisticsCounterType.MidiEvents);
        }

        private void refreshDevices()
        {
            var allDevices = new List<(int idx, MidiInCapabilities cap)>(MidiIn.NumberOfDevices);
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                allDevices.Add((i, MidiIn.DeviceInfo(i)));

            // Remove old devices
            for (var i = devices.Count - 1; i >= 0; i--) {
                var dev = devices[i];

                if (!allDevices.Any(x => Equals(x.cap, dev.Capabilities))) {
                    // Device is no longer present, push the last state and remove it
                    handleState(dev, new MidiState());

                    Logger.Log($"Disconnected MIDI device: {dev.Capabilities.ProductName}");

                    dev.NewState -= handleState;
                    dev.Dispose();

                    devices.Remove(dev);
                } else {
                    // This device still exists, remove it from allDevices so we're only left with new device ones.
                    allDevices.RemoveAll(x => Equals(x.cap, dev.Capabilities));
                }
            }

            // Add new devices
            foreach ((int idx, MidiInCapabilities cap) in allDevices) {
                var newDevice = new MidiDevice(idx, cap);
                newDevice.NewState += handleState;
                Logger.Log($"Connected MIDI device: {cap.ProductName}");
                devices.Add(newDevice);
            }
        }

        public override bool IsActive => true;
        public override int Priority => 0;

        private class MidiDevice : IDisposable
        {
            public MidiState LastState { get; set; } = new MidiState();
            public MidiState State { get; set; } = new MidiState();

            public event Action<MidiDevice, MidiState> NewState;

            public readonly MidiIn Device;
            public readonly MidiInCapabilities Capabilities;

            public MidiDevice(int idx, MidiInCapabilities capabilities)
            {
                Capabilities = capabilities;
                Device = new MidiIn(idx);
                Device.MessageReceived += onMessageReceived;
                Device.Start();
            }

            private void onMessageReceived(object sender, MidiInMessageEventArgs e)
            {
                var ev = e.MidiEvent;
                switch (ev) {
                    case NoteOnEvent noteOn when noteOn.Velocity != 0:
                        State.Keys.Add((MidiKey)noteOn.NoteNumber);
                        NewState?.Invoke(this, State);
                        break;
                    case NoteEvent noteOff:
                        State.Keys.SetPressed((MidiKey)noteOff.NoteNumber, false);
                        NewState?.Invoke(this, State);
                        break;
                }
            }

            public void Dispose()
            {
                Device.MessageReceived -= onMessageReceived;
                Device?.Dispose();
            }
        }
    }
}
