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

        // POST: api/gastosapi (Consome a Stored Procedure com Transação)
        [HttpPost]
        public IActionResult PostGasto([FromBody] GastoDto request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
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

        // PUT: api/gastosapi/5
        [HttpPut("{id}")]
        public IActionResult PutGasto(int id, [FromBody] GastoDto request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = "UPDATE gastos SET descricao = @Descricao, categoria = @Categoria, valor = @Valor WHERE id = @Id AND usuario_id = @UsuarioId";
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);
                    command.Parameters.AddWithValue("@Descricao", request.Descricao);
                    command.Parameters.AddWithValue("@Categoria", request.Categoria);
                    command.Parameters.AddWithValue("@Valor", request.Valor);

                    connection.Open();
                    int linhasAfetadas = command.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                        return NotFound(new { erro = "Gasto não encontrado para atualização." });
                }
                return Ok(new { mensagem = "Gasto atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Falha ao atualizar gasto.", detalhe = ex.Message });
            }
        }

        // DELETE: api/gastosapi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteGasto(int id, [FromQuery] int usuarioId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = "DELETE FROM gastos WHERE id = @Id AND usuario_id = @UsuarioId";
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                    connection.Open();
                    int linhasAfetadas = command.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                        return NotFound(new { erro = "Gasto não encontrado para exclusão." });
                }
                return Ok(new { mensagem = "Gasto excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Falha ao excluir gasto.", detalhe = ex.Message });
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