using System.Collections.Generic;

namespace SMAControlApp.Utils
{
    // Kept minimal – GraphViewModel uses its own timer now
    public static class GraphService
    {
        public static void Trim<T>(IList<T> list, int maxPoints)
        {
            while (list.Count > maxPoints)
                list.RemoveAt(0);
        }
    }
}