namespace Ezvcc.App.Audio;

public sealed class AudioLevelEventArgs : EventArgs
{
    public AudioLevelEventArgs(float peak)
    {
        Peak = peak;
    }

    public float Peak { get; }
}
