namespace Revistone
{
    namespace Functions
    {
        public static class RevistoneExtras
        {
            public static string ArrayToString<T>(T[] t)
            {
                if (t.Length == 0) return "[]";

                string s = "[";

                foreach (T element in t)
                {
                    if (element != null) s += $"{element.ToString()}, ";
                }

                s = s.Substring(0, s.Length - 2) + "]";

                return s;
            }

            public static string ListToString<T>(List<T> t) { return ArrayToString(t.ToArray());}
        }
    }
}