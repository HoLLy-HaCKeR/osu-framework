namespace osu.Framework.Input.States
{
    public interface IMidiState
    {
        /// <summary>
        /// The currently pressed keys.
        /// </summary>
        ButtonStates<MidiKey> Keys { get; }

        IMidiState Clone();
    }
}
