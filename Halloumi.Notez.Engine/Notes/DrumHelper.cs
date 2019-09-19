namespace Halloumi.Notez.Engine.Notes
{
    public static class DrumHelper
    {
        public static bool IsBassDrum(int note)
        {
            var drumType = (DrumType) note;
            return drumType == DrumType.AcousticBassDrum || drumType == DrumType.BassDrum1;
        }

        public static bool IsSnareDrum(int note)
        {
            var drumType = (DrumType)note;
            return drumType == DrumType.AcousticSnare || drumType == DrumType.ElectricSnare;
        }

    }
}
