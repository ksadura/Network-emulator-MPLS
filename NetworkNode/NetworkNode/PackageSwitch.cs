using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tools;
using static NetworkNodes.NetworkNode;
using static Tools.HashSetOperations;

namespace NetworkNodes
{
    /// <summary>
    /// Represents a switch inside an LSR
    /// </summary>
    public class PackageSwitch
    {
        private NetworkNodeRoutingTables RoutingTables { get; set; }

        public MPLSPackage RouteMPLSPackage(MPLSPackage package, NetworkNodeRoutingTables networkNodeRoutingTables)
        {
            RoutingTables = networkNodeRoutingTables;

            MPLSPackage routedPackage = null;

            try
            {
                routedPackage = RoutePackage(package);
            }
            catch (Exception e)
            {
                AddLog($"Exception: {e.StackTrace}", LogType.Error);
                return null;
            }

            if (routedPackage == null)
            {
                AddLog($"I don't know how to route package. Package discarded!", LogType.Error);
                return null;
            }
            return routedPackage;
        }

        private MPLSPackage RoutePackage(MPLSPackage package)
        {
            var packageNotLabeled = package.label_stack.IsEmpty();
            MPLSPackage routedPackage = null;

            if (packageNotLabeled)
            {
                return PerformNotLabeledPackageAction(package, routedPackage);
            }
            else
            {
                routedPackage = RouteByMPLS(package);
            }

            if (routedPackage == null)
            {
                return null;
            }
            return routedPackage;
        }


        private MPLSPackage PerformNotLabeledPackageAction(MPLSPackage package, MPLSPackage routedPackage)
        {
            HashSet<MplsTableRow> tmpRows = new HashSet<MplsTableRow>();
            routedPackage = package;

            foreach (var row in RoutingTables.mplsTable.Rows)
            {
                if (row.DestAddress.Equals(package.DestinationIP) && row.InLabel.Equals("-"))
                {
                    tmpRows.Add(row);
                }
            }
            AddLog($"Looking for matching rows in MPLS table...", LogType.Information);
            if (!tmpRows.Any())
            {
                AddLog($"Couldn't find matching entry!", LogType.Error);
                return null;
            }
            HashSet<MplsTableRow> sortedRowStack = HashSetOperations.sort(tmpRows);
            foreach (var row in sortedRowStack)
            {
                PerformLabelAction(ref routedPackage, row, LabelActions.PUSH);
                AddLog($"Action for packet_ID={routedPackage.ID} is {LabelActions.PUSH} with label {row.OutLabel}, index = {row.index}", LogType.Action);
            }
            routedPackage.Port = (ushort) Convert.ToUInt32(sortedRowStack.Last().OutPort);
            routedPackage.PrevIP = NetworkNodeConfig.NodeAddress;
            AddLog($"Package with packet_ID={routedPackage.ID} routed to output port: {routedPackage.Port}, output label: {routedPackage.checkLabel().ID}", LogType.Route);
            return routedPackage;
        }

        private MPLSPackage RouteByMPLS(MPLSPackage unprocessedPackage)
        {
            HashSet<MplsTableRow> tmpRows = new HashSet<MplsTableRow>();
            string tmpIndex = "-";
            MplsTableRow matchingRow = null;
            var processedPackage = unprocessedPackage;

            do
            {
                foreach (var row in RoutingTables.mplsTable.Rows)
                {
                    if (row.InPort.Equals(processedPackage.Port.ToString()) && row.InLabel.Equals(processedPackage.checkLabel().ID.ToString()))
                    {
                        tmpRows.Add(row);
                    }
                }
                if (tmpRows.Count() > 1)
                {
                    if (tmpIndex != "-")
                    {
                        matchingRow = tmpRows.FirstOrDefault(row => row.prev_index.Equals(tmpIndex));
                    }
                    if (matchingRow == null)
                    {
                        HashSet<MplsTableRow> sortedRowStack = HashSetOperations.sort(tmpRows);
                        matchingRow = sortedRowStack.Last();
                        AddLog($"Found matching row by last element in sorted stack", LogType.Information);
                    }
                    else
                    {
                        AddLog($"Found matching row by tmp_index", LogType.Information);
                    }
                }
                else if (tmpRows.Count() == 1)
                {
                    matchingRow = tmpRows.First();
                    AddLog($"Found matching row by single row", LogType.Information);
                }
                else
                {
                    AddLog($"Couldn't find matching entry!", LogType.Error);
                    return null;
                }

                if (matchingRow.OutLabel.Equals("-"))
                {
                    PerformLabelAction(ref processedPackage, matchingRow, LabelActions.POP);
                    AddLog($"Action for packet_ID={processedPackage.ID} is {LabelActions.POP}, output port: {matchingRow.OutPort}", LogType.Action);
                    if (processedPackage.checkLabel() == null)
                    {
                        AddLog($"No further action for this package", LogType.Information);
                        break;
                    }
                }
                else
                {
                    PerformLabelAction(ref processedPackage, matchingRow, LabelActions.SWAP);
                    AddLog($"Action for packet_ID={processedPackage.ID} is {LabelActions.SWAP}, output port: {matchingRow.OutPort}, output label: {matchingRow.OutLabel}", LogType.Action);
                    break;
                }
                tmpIndex = matchingRow.index;
            } while (hasNextRow(processedPackage));
            processedPackage.Port = (ushort)Int32.Parse(matchingRow.OutPort);
            processedPackage.PrevIP = NetworkNodeConfig.NodeAddress;
            if (processedPackage.checkLabel() == null)
            {
                AddLog($"Package with packet_ID={processedPackage.ID} routed to output port: {processedPackage.Port} to destination adress {matchingRow.DestAddress}", LogType.Route);
            }
            else
            {
                AddLog($"Package with packet_ID={processedPackage.ID} routed to output port: {processedPackage.Port}, output label: {processedPackage.checkLabel().ID}", LogType.Route);
            }
            return processedPackage;
        }

        private bool hasNextRow(MPLSPackage package)
        {
            var nextRow = RoutingTables.mplsTable.Rows.FirstOrDefault(row => row.InPort.Equals(package.Port.ToString()) && row.InLabel.Equals(package.checkLabel().ID.ToString()));
            if (nextRow == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void PerformLabelAction(ref MPLSPackage package, MplsTableRow row, string action)
        {
            switch (action)
            {
                case LabelActions.POP:
                    package.popLabel();
                    break;

                case LabelActions.SWAP:
                    package.popLabel();
                    package.pushLabel(new Label((short)Int32.Parse(row.OutLabel)));
                    break;

                case LabelActions.PUSH:
                    package.pushLabel(new Label((short)Int32.Parse(row.OutLabel)));
                    break;

                default:
                    break;
            }
        }

        private void AddLog(string log, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogType.Action:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogType.Route:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            log = $"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}";
            Console.WriteLine(log);
        }
    }
}
