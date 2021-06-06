using VRage;

namespace IngameScript
{
    internal partial class Program
    {
        private static string Float(MyFixedPoint value)
        {
            return $"{(float) value,11:F3}";
        }

        private static string Int(MyFixedPoint value)
        {
            return $"{(int) value,7}";
        }
    }
}