// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Input.States;

namespace osu.Framework.Input
{
    public class MidiState : IMidiState
    {
        public ButtonStates<MidiKey> Keys { get; private set; } = new ButtonStates<MidiKey>();
        
        public IMidiState Clone()
        {
            var clone = (MidiState)MemberwiseClone();
            clone.Keys = Keys.Clone();

            return clone;
        }
    }
}
