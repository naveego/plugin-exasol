using System.Data;
using System.Threading.Tasks;
using Exasol.EXADataProvider;

namespace PluginExasol.API.Factory
{
    public class Reader : IReader
    {
        private readonly EXADataReader _reader;
        public Reader(IDataReader reader)
        {
            _reader = (EXADataReader) reader;
        }

        public async Task<bool> ReadAsync()
        {
            return await _reader.ReadAsync();
        }

        public async Task CloseAsync()
        {
            await _reader.CloseAsync();
        }

        public DataTable GetSchemaTable()
        {
            return _reader.GetSchemaTable();
        }

        public object GetValueById(string id, char trimChar = '`')
        {
            return _reader[id.Trim(trimChar)];
        }

        public bool HasRows()
        {
            return _reader.HasRows;
        }
    }
}