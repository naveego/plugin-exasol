using Naveego.Sdk.Plugins;
using PluginExasol.API.Utility;
using PluginExasol.DataContracts;

namespace PluginExasol.API.Replication
{
    public static partial class Replication
    {
        public static ReplicationTable GetVersionReplicationTable(Schema schema, string safeSchemaName, string safeVersionTableName)
        {
            var versionTable = ConvertSchemaToReplicationTable(schema, safeSchemaName, safeVersionTableName);
            versionTable.Columns.Add(new ReplicationColumn
            {
                ColumnName = Constants.ReplicationVersionRecordId,
                DataType = "varchar(255)",
                PrimaryKey = true
            });
            versionTable.Columns.Add(new ReplicationColumn
            {
                ColumnName = Constants.ReplicationRecordId,
                DataType = "varchar(255)",
                PrimaryKey = false
            });

            return versionTable;
        }
    }
}