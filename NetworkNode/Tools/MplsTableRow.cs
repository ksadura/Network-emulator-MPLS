using System.Net;
using System;

namespace Tools
{
    /// <summary>
    /// Represents an entry in MPLS-FIB table.
    /// </summary>
    public class MplsTableRow
    {
        /// <summary>
        /// Package destination adress.
        /// </summary>
        public IPAddress DestAddress { get; set; }

        public string InLabel { get; set; }
        public string OutLabel { get; set; }
        public string OutPort { get; set; }
        public string InPort { get; set; }
        public string index { get; set; }
        public string prev_index { get; set; }

        /// <summary>
        /// Name of a router to which an MPLS-FIB entry belogs to.
        /// </summary>
        public string RouterName { get; set; }

        /// <summary>
        /// Creates a row from an entry like this:
        /// DestAddress InLabel OutLabel OutPort InPort RouterName prev_index
        /// </summary>
        public MplsTableRow(string serializedString, bool isRemove)
        {
            var parts = serializedString.Split(" ");
            DestAddress = IPAddress.Parse(parts[0]);
            InLabel = parts[1];
            OutLabel = parts[2];
            OutPort = parts[3];
            InPort = parts[4];
            RouterName = parts[5];
            if (isRemove)
            {
                index = parts[6];
                prev_index = parts[7];
            }
            else
            {
                prev_index = parts[6];
            }
        }

        /// <summary>
        /// Serializes object into a string containing all of its public variables.
        /// </summary>
        /// <returns>Serialized object as a string.</returns>
        public string Serialize()
        {
            return $"{DestAddress} {InLabel} {OutLabel} {OutPort} {InPort} {RouterName} {index} {prev_index}";
        }

        /// <summary>
        /// Checks if two MPLS-FIB entries are equal.
        /// </summary>
        /// <param name="other">An MPLS-FIB entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        protected bool Equals(MplsTableRow other)
        {
            return Equals(DestAddress, other.DestAddress) && InLabel == other.InLabel && OutLabel == other.OutLabel && OutPort == other.OutPort && InPort == other.InPort && RouterName == other.RouterName && prev_index == other.prev_index && index == other.index;
        }

        public bool shouldBeUpdated(MplsTableRow other)
        {
            return Equals(DestAddress, other.DestAddress) && InLabel == other.InLabel && InPort == other.InPort && RouterName == other.RouterName && index == other.index;
        }

        /// <summary>
        /// Checks if two MPLS-FIB entries are equal.
        /// </summary>
        /// <param name="obj">An MPLS-FIB entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MplsTableRow)obj);
        }

        /// <summary>
        /// Generates a hashcode of the entry.
        /// </summary>
        /// <returns>Generated hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1;
                hashCode = (hashCode * 397) ^ (DestAddress != null ? DestAddress.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ InLabel.GetHashCode();
                hashCode = (hashCode * 397) ^ OutLabel.GetHashCode();
                hashCode = (hashCode * 397) ^ OutPort.GetHashCode();
                hashCode = (hashCode * 397) ^ InPort.GetHashCode();
                hashCode = (hashCode * 397) ^ RouterName.GetHashCode();
                hashCode = (hashCode * 397) ^ index.GetHashCode();
                hashCode = (hashCode * 397) ^ prev_index.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(DestAddress)}: {DestAddress}, {nameof(InLabel)}: {InLabel}, {nameof(OutLabel)}: {OutLabel}, {nameof(OutPort)}: {OutPort}, {nameof(InPort)}: {InPort}, {nameof(RouterName)}: {RouterName}, {nameof(index)}: {index}, {nameof(prev_index)}: {prev_index}";
        }
    }
}
