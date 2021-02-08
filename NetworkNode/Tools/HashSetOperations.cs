using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Tools
{
    public static class HashSetOperations
    {
        public static HashSet<MplsTableRow> sort(HashSet<MplsTableRow> set)
        {
            IEnumerable<MplsTableRow> sortedRowStack = set.OrderBy(row => row.index);
            return sortedRowStack.ToHashSet();
        }
    }
}
