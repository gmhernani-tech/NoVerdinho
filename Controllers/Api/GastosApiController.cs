using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace NoVerdinho.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class GastosApiController : ControllerBase
    {
        private readonly string _connectionString;

        public GastosApiController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NoVerdinhoDB");
        }

        // GET: api/gastosapi/5
        [HttpGet("{usuarioId}")]
        public IActionResult GetGastos(int usuarioId)
        {
            var gastos = new List<object>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT id, descricao, categoria, valor, data_gasto FROM gastos WHERE usuario_id = @usuarioId ORDER BY data_gasto DESC";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@usuarioId", usuarioId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gastos.Add(new
                        {
                            Id = reader.GetInt32(0),
                            Descricao = reader.GetString(1),
                            Categoria = reader.GetString(2),
                            Valor = reader.GetDecimal(3),
                            DataGasto = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("dd/MM/yyyy")
                        });
                    }
                }
            }
            return Ok(gastos);
        }

        // POST: api/gastosapi
        [HttpPost]
        public IActionResult PostGasto([FromBody] GastoDto request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Consumo da Stored Procedure com transação embutida
                    var command = new SqlCommand("sp_RegistrarGastoSeguro", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);
                    command.Parameters.AddWithValue("@Descricao", request.Descricao);
                    command.Parameters.AddWithValue("@Categoria", request.Categoria);
                    command.Parameters.AddWithValue("@Valor", request.Valor);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return StatusCode(201, new { mensagem = "Gasto registado com sucesso via Stored Procedure!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Falha ao registar gasto.", detalhe = ex.Message });
            }
        }
    }

    public class GastoDto
    {
        public int UsuarioId { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public decimal Valor { get; set; }
    }
}