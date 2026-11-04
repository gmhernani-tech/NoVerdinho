using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace NoVerdinho.Services
{
    public class DbService
    {
        private readonly string _connectionString;

        public DbService(IConfiguration configuration)
        {
            // Busca a string que configuramos no appsettings.json
            _connectionString = configuration.GetConnectionString("NoVerdinhoDB");
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task RegistrarErroAsync(int? usuarioId, string origem, Exception ex)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            string sql = @"INSERT INTO logs_erro (usuario_id, origem, mensagem, stack_trace) 
                   VALUES (@usuarioId, @origem, @mensagem, @stackTrace)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@usuarioId", (object)usuarioId ?? DBNull.Value);
            command.Parameters.AddWithValue("@origem", origem);
            command.Parameters.AddWithValue("@mensagem", ex.Message);
            command.Parameters.AddWithValue("@stackTrace", ex.StackTrace);

            await command.ExecuteNonQueryAsync();
        }
    }
}