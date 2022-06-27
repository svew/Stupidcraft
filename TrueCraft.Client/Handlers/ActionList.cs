using System;
using System.Collections.Generic;

namespace TrueCraft.Client.Handlers
{
    public static class ActionList
    {
        private static List<ActionConfirmation> _lst = new List<ActionConfirmation>();


        public static void Add(ActionConfirmation action)
        {
            _lst.Add(action);
        }

        public static ActionConfirmation? Get(int actionNumber)
        {
            int n = 0;
            while (n < _lst.Count && _lst[n].ActionNumber != actionNumber)
                n++;

            if (n == _lst.Count) return null;

            ActionConfirmation rv = _lst[n];
            _lst.RemoveAt(n);

            return rv;
        }
    }
}
