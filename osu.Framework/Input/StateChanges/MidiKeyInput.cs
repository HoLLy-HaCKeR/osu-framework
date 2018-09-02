// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Input.States;

namespace osu.Framework.Input.StateChanges
{
    public class MidiKeyInput : ButtonInput<MidiKey>
    {
        public MidiKeyInput(IEnumerable<ButtonInputEntry<MidiKey>> entries)
            : base(entries)
        {
        }

        public MidiKeyInput(MidiKey button, bool isPressed)
            : base(button, isPressed)
        {
        }

        public MidiKeyInput(ButtonStates<MidiKey> current, ButtonStates<MidiKey> previous)
            : base(current, previous)
        {
        }

        protected override ButtonStates<MidiKey> GetButtonStates(InputState state) => state.Midi.Keys;

        protected override void Handle(IInputStateChangeHandler handler, InputState state, MidiKey key, ButtonStateChangeKind kind) =>
            handler.HandleMidiKeyStateChange(state, key, kind);
    }
}
