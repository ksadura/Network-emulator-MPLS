using System.Collections.Generic;
using Tools;

namespace NetworkNodes
{
    public class MplsTable
    {
        public HashSet<MplsTableRow> Rows { get; set; }
        public int row_index { get; set; }

        public MplsTable()
        {
            Rows = new HashSet<MplsTableRow>();
            row_index = 0;
        }
    }
}
