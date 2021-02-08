using System;
using Tools;

namespace NetworkNodes
{
    public class ForwardingTableEventArgs
    {
        public string Action { get; set; }

        public MplsTableRow Row { get; set; }

        public ForwardingTableEventArgs(string action, MplsTableRow row)
        {
            Action = action;
            Row = row;
        }

    }
}
