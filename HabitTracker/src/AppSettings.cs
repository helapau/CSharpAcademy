using System.IO;

namespace HabitTracker.src
{
    internal static class AppSettings
    {
        public static readonly string PROJECT_ROOT_DIR = Path.Combine($"C{Path.VolumeSeparatorChar}", "Users", "hepa", "source", "CSharpAcademy", "HabitTracker");
    }
}
