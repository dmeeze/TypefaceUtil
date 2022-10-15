using System.Collections.Generic;

namespace TypefaceUtil.OpenType
{
    public static partial class UnicodeData
    {
        public static Dictionary<int, string> CharCodeNameMap = loadCharCodeNameMap();

        // built by source generator in UnicodeDataGenerator
        private static partial Dictionary<int, string> loadCharCodeNameMap();
    }
}