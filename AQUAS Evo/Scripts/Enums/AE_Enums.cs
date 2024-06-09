namespace AquasEvo
{
    public enum FFTSize
    {
        Tiny32x32,
        Small64x64,
        Medium128x128,
        Large256x256,
        VeryLarge512x512
    }

    public enum ProjGridSize
    {
        Tiny32x32,
        Small64x64,
        Medium128x128,
        Large256x256,
        VeryLarge512x512
    }

    public enum Spectrum
    {
        Phillips,
        PiersonMoskowitz,
        JONSWAP,
        TMA
    }

    public enum FrequencyCharacterization
    {
        LowFrequency = 0,
        HighFrequency = 1
    }
}