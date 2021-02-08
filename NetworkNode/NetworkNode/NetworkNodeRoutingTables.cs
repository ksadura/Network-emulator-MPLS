using System;
using System.Threading.Tasks;
using static NetworkNodes.NetworkNode;
using Tools;

namespace NetworkNodes
{
    public class NetworkNodeRoutingTables
    {
        public MplsTable mplsTable { get; set; }

        public NetworkNodeRoutingTables()
        {
            mplsTable = new MplsTable();
        }

        public string HandleModifyForwardingTable(string Action, MplsTableRow row)
        {
            string receivedRow = "";
            switch (Action.Trim())
            {
                case ManagementActions.ADD_MPLS_ENTRY:
                    bool is_updated = false;
                    foreach (MplsTableRow temp_row in mplsTable.Rows)
                    {
                        if (temp_row.shouldBeUpdated(row))
                        {
                            AddLog($"Updating entry: {temp_row} to {row}", LogType.Update);
                            row.index = temp_row.index;
                            mplsTable.Rows.Remove(temp_row);
                            mplsTable.Rows.Add(row);
                            is_updated = true;
                            break;
                        }
                    }

                    if (is_updated == false)
                    {
                        AddLog($"Adding new entry: {row}", LogType.Add);
                        row.index = mplsTable.row_index.ToString();
                        mplsTable.Rows.Add(row);
                        mplsTable.row_index++;
                    }
                    receivedRow = "RECEIVED " + row.Serialize();
                    break;

                case ManagementActions.REMOVE_MPLS_ENTRY:
                    bool succes = false;
                    foreach (MplsTableRow temp_row in mplsTable.Rows)
                    {
                        if (temp_row.Equals(row))
                        {
                            mplsTable.Rows.Remove(temp_row);
                            AddLog($"Entry removed: {temp_row}", LogType.Remove);
                            succes = true;
                            break;
                        }
                    }
                    if(!succes)
                    {
                        AddLog($"Nothing to remove", LogType.Information);
                    }
                    break;
            }
            return receivedRow;
        }

        private void AddLog(string log, LogType logType)
        {
            switch (logType)
            {
                case LogType.Update:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;
                case LogType.Add:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                case LogType.Remove:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            log = $"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}";
            Console.WriteLine(log);
        }
    }
}
