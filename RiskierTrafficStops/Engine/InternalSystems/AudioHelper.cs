using IrrKlang;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal class SoundPlayer
{
    internal string FileName;
    private ISoundEngine soundEngine = new();
        
    internal SoundPlayer(string FileName)
    {
        this.FileName = FileName;
    }

    internal void PlaySound2D()
    {
        soundEngine.Play2D(FileName);
    }
}