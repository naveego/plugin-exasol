using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using PluginExasol.API.Factory;

namespace PluginExasol.API.Discover
{
    public static partial class Discover
    {

        private const string GetTableAndColumnsQuery = @"SELECT 
            c.COLUMN_TABLE,
        c.COLUMN_SCHEMA,
        c.COLUMN_NAME,
        c.COLUMN_TYPE,
        c.COLUMN_IS_DISTRIBUTION_KEY,
        c.COLUMN_IS_NULLABLE,
        c.COLUMN_MAXSIZE,
        s.CONSTRAINT_TYPE
            FROM SYS.EXA_ALL_COLUMNS as c
            LEFT JOIN SYS.EXA_ALL_CONSTRAINT_COLUMNS as s ON 
            (c.COLUMN_TABLE = s.CONSTRAINT_TABLE AND 
        c.COLUMN_NAME= s.COLUMN_NAME AND
        c.COLUMN_SCHEMA = s.CONSTRAINT_SCHEMA)
        WHERE (s.CONSTRAINT_TYPE IS NULL
        OR s.CONSTRAINT_TYPE =  'PRIMARY KEY')
        AND (c.COLUMN_SCHEMA = '{0}'
        AND c.COLUMN_TABLE = '{1}')
        ORDER BY c.COLUMN_SCHEMA, c.COLUMN_TABLE";
        
        

        public static async Task<Schema> GetRefreshSchemaForTable(IConnectionFactory connFactory, Schema schema,
            int sampleSize = 5)
        {
            var decomposed = DecomposeSafeName(schema.Id).TrimEscape();
            var conn = connFactory.GetConnection();

            try
            {
                await conn.OpenAsync();

                var cmd = connFactory.GetCommand(
                    string.Format(GetTableAndColumnsQuery, decomposed.Schema.Trim('\"'), decomposed.Table.Trim('\"')), conn);
                var reader = await cmd.ExecuteReaderAsync();
                var refreshProperties = new List<Property>();
                
                while (await reader.ReadAsync())
                {
                    // add column to refreshProperties
                    var property = new Property
                    {
                        //Id = Utility.Utility.GetSafeName(reader.GetValueById(ColumnName).ToString(), '"'),
                        Id = $"{reader.GetValueById(ColumnName)}",
                        Name = reader.GetValueById(ColumnName).ToString(),
                        IsKey = reader.GetValueById(ColumnKey)?.ToString() == "PRIMARY KEY",
                        IsNullable = reader.GetValueById(IsNullable).ToString() == "YES",
                        Type = GetType(reader.GetValueById(DataType).ToString()),
                        TypeAtSource = GetTypeAtSource(reader.GetValueById(DataType).ToString(),
                            reader.GetValueById(CharacterMaxLength))
                    };
                    refreshProperties.Add(property);
                }

                // add properties
                schema.Properties.Clear();
                schema.Properties.AddRange(refreshProperties);

            

                // get sample and count
                return await AddSampleAndCount(connFactory, schema, sampleSize);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private static DecomposeResponse DecomposeSafeName(string schemaId)
        {
            var response = new DecomposeResponse
            {
                Database = "",
                Schema = "",
                Table = ""
            };
            var parts = schemaId.Split('.');

            switch (parts.Length)
            {
                case 0:
                    return response;
                case 1:
                    response.Table = parts[0];
                    return response;
                case 2:
                    response.Schema = parts[0];
                    response.Table = parts[1];
                    return response;
                case 3:
                    response.Database = parts[0];
                    response.Schema = parts[1];
                    response.Table = parts[2];
                    return response;
                default:
                    return response;
            }
        }

        private static DecomposeResponse TrimEscape(this DecomposeResponse response, char escape = '`')
        {
            response.Database = response.Database.Trim(escape);
            response.Schema = response.Schema.Trim(escape);
            response.Table = response.Table.Trim(escape);

            return response;
        }
    }

    class DecomposeResponse
    {
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
    }
}