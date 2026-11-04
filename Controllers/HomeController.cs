using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using NoVerdinho.Models;
using NoVerdinho.Models.Cadastro;
using NoVerdinho.Models.Login;
using NoVerdinho.Services;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace NoVerdinho.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbService _db;
        private readonly ILogger<HomeController> _logger;

        // UNIFICADO: O .NET injeta ambos os serviços de uma vez aqui
        public HomeController(DbService db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                // Trocado MySqlCommand por SqlCommand
                using var command = new SqlCommand("SELECT COUNT(*) FROM usuarios", connection);
                var total = await command.ExecuteScalarAsync();

                ViewBag.TotalUsuarios = total;
            }
            catch (Exception ex)
            {
                // Ajustada a mensagem de log para refletir o SQL Server
                _logger.LogError(ex, "Erro ao conectar no SQL Server");
                ViewBag.TotalUsuarios = "Erro";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TrocarSenha([FromBody] dynamic data)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            string senhaAntiga = data.GetProperty("SenhaAntiga").GetString();
            string senhaNova = data.GetProperty("SenhaNova").GetString();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // 1. Verificar se a senha antiga está correta
            string sqlVerifica = "SELECT id FROM usuarios WHERE id = @id AND senha = @senha";
            using (var cmd = new SqlCommand(sqlVerifica, conn)) // Trocado para SqlCommand
            {
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@senha", senhaAntiga);
                var result = await cmd.ExecuteScalarAsync();
                if (result == null) return BadRequest("Senha antiga incorreta.");
            }

            // 2. Atualizar para a nova senha
            string sqlUpdate = "UPDATE usuarios SET senha = @novaSenha WHERE id = @id";
            using (var cmd = new SqlCommand(sqlUpdate, conn)) // Trocado para SqlCommand
            {
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@novaSenha", senhaNova);
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok();
        }

        // PAGINA PRINCIPAL ATUALIZADA
        public async Task<IActionResult> NoVerdinho()
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return RedirectToAction("Login");

            var model = new TratamentoNoVerdinho();
            var listaMovimentacoes = new List<GastoItem>();

            // Inicialização preventiva para evitar erros de Null Reference na View
            ViewBag.NomeUsuario = "Usuário";
            ViewBag.TotalGastos = 0m;
            ViewBag.SaldoDisponivel = 0m;
            ViewBag.PercentualGastos = 0;
            ViewBag.Radar = new List<dynamic>();

            int mesAtual = DateTime.Now.Month;
            int anoAtual = DateTime.Now.Year;

            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                // 1. BUSCAR DADOS DO USUÁRIO
                string sqlUser = "SELECT nome, salario_liquido FROM usuarios WHERE id = @id";
                using (var cmd = new SqlCommand(sqlUser, connection))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        ViewBag.NomeUsuario = reader.IsDBNull(0) ? "Usuário" : reader.GetString(0).Split(' ')[0];
                        model.salario_liquido = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    }
                }

                // 2. BUSCAR GASTOS (TABELA GASTOS)
                string sqlLista = @"SELECT id, descricao, categoria, data_gasto, valor 
                            FROM gastos 
                            WHERE usuario_id = @id 
                            AND (categoria = 'Fixo' OR (MONTH(data_gasto) = @mes AND YEAR(data_gasto) = @ano)) 
                            ORDER BY categoria DESC, data_gasto DESC";

                using (var cmdLista = new SqlCommand(sqlLista, connection))
                {
                    cmdLista.Parameters.AddWithValue("@id", userId);
                    cmdLista.Parameters.AddWithValue("@mes", mesAtual);
                    cmdLista.Parameters.AddWithValue("@ano", anoAtual);

                    using var readerLista = await cmdLista.ExecuteReaderAsync();
                    while (await readerLista.ReadAsync())
                    {
                        listaMovimentacoes.Add(new GastoItem
                        {
                            Id = readerLista.GetInt32(readerLista.GetOrdinal("id")),
                            Descricao = readerLista.GetString(readerLista.GetOrdinal("descricao")),
                            Categoria = readerLista.GetString(readerLista.GetOrdinal("categoria")),
                            Data = readerLista.GetDateTime(readerLista.GetOrdinal("data_gasto")),
                            Valor = readerLista.GetDecimal(readerLista.GetOrdinal("valor"))
                        });
                    }
                }

                // 3. BUSCAR TOTAL DE MERCADO (TABELA COMPRAS)
                string sqlMercado = @"SELECT SUM(valor_total) FROM compras 
                             WHERE usuario_id = @id AND MONTH(data_compra) = @mes AND YEAR(data_compra) = @ano";

                decimal totalMercado = 0;
                using (var cmdMercado = new SqlCommand(sqlMercado, connection))
                {
                    cmdMercado.Parameters.AddWithValue("@id", userId);
                    cmdMercado.Parameters.AddWithValue("@mes", mesAtual);
                    cmdMercado.Parameters.AddWithValue("@ano", anoAtual);
                    var result = await cmdMercado.ExecuteScalarAsync();
                    totalMercado = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }

                // 4. BUSCAR DADOS DO RADAR (MELHORES PREÇOS) - Ajustado para SQL Server (TOP 2)
                string sqlRadar = @"SELECT TOP 2 p.nome as Produto, ci.valor_unitario as Preco, m.nome as Mercado 
                            FROM compra_itens ci 
                            JOIN produtos p ON ci.produto_id = p.id 
                            JOIN mercados m ON ci.mercado_id = m.id 
                            WHERE ci.usuario_id = @id 
                            AND ci.valor_unitario = (SELECT MIN(valor_unitario) FROM compra_itens WHERE produto_id = p.id) 
                            ORDER BY ci.id DESC";

                var listaRadar = new List<dynamic>();
                using (var cmdRadar = new SqlCommand(sqlRadar, connection))
                {
                    cmdRadar.Parameters.AddWithValue("@id", userId);
                    using var readerRadar = await cmdRadar.ExecuteReaderAsync();
                    while (await readerRadar.ReadAsync())
                    {
                        listaRadar.Add(new
                        {
                            Produto = readerRadar["Produto"].ToString(),
                            Preco = Convert.ToDecimal(readerRadar["Preco"]),
                            Mercado = readerRadar["Mercado"].ToString()
                        });
                    }
                }
                ViewBag.Radar = listaRadar;

                // 5. CÁLCULOS FINAIS PARA OS CARDS
                decimal totalGastosGerais = listaMovimentacoes.Sum(x => x.Valor);
                decimal totalFinal = totalGastosGerais + totalMercado;

                ViewBag.TotalGastos = totalFinal;
                ViewBag.SaldoDisponivel = model.salario_liquido - totalFinal;
                ViewBag.PercentualGastos = model.salario_liquido > 0 ? (totalFinal / model.salario_liquido) * 100 : 0;
                ViewBag.Movimentacoes = listaMovimentacoes;

                return View(model);
            }
            catch (Exception ex)
            {
                // Log de erro e retorno seguro
                await _db.RegistrarErroAsync(userId, "Erro no Dashboard NoVerdinho", ex);
                ViewBag.Movimentacoes = new List<GastoItem>();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarGasto(int? Id, string Descricao, string Categoria, decimal Valor)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return RedirectToAction("Login");

            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                if (Id.HasValue && Id > 0)
                {
                    // EDITAR
                    string sql = @"UPDATE gastos SET descricao = @desc, categoria = @cat, valor = @valor WHERE id = @id AND usuario_id = @userId";

                    using var cmd = new SqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@desc", Descricao);
                    cmd.Parameters.AddWithValue("@cat", Categoria);
                    cmd.Parameters.AddWithValue("@valor", Valor);

                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    // NOVO - Trocado NOW() por GETDATE() do SQL Server
                    string sql = @"INSERT INTO gastos (usuario_id, descricao, categoria, valor, data_gasto) VALUES (@userId, @desc, @cat, @valor, GETDATE())";

                    using var cmd = new SqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@desc", Descricao);
                    cmd.Parameters.AddWithValue("@cat", Categoria);
                    cmd.Parameters.AddWithValue("@valor", Valor);

                    await cmd.ExecuteNonQueryAsync();
                }

                return RedirectToAction("NoVerdinho");
            }
            catch (Exception ex)
            {
                await _db.RegistrarErroAsync(userId, "Erro ao Salvar/Editar Gasto", ex);
                return RedirectToAction("NoVerdinho");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirGasto([FromBody] ExcluirGastoRequest data)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return Unauthorized();

            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                string sql = "DELETE FROM gastos WHERE id = @id AND usuario_id = @userId";

                using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", data.Id);
                cmd.Parameters.AddWithValue("@userId", userId);

                await cmd.ExecuteNonQueryAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await _db.RegistrarErroAsync(userId, "Erro ao Excluir Gasto", ex);
                return StatusCode(500);
            }
        }

        //MERCADO
        //MERCADO
        public IActionResult Mercado()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SalvarMercado([FromBody] Mercado model)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return Unauthorized();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = "INSERT INTO mercados (nome, usuario_id) VALUES (@nome, @usuario_id)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", model.Nome);
            cmd.Parameters.AddWithValue("@usuario_id", userId);

            await cmd.ExecuteNonQueryAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SalvarProduto([FromBody] ProdutoModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "INSERT INTO produtos (nome, usuario_id) VALUES (@nome, @userId)";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nome", model.Nome);
                cmd.Parameters.AddWithValue("@userId", userId);

                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ListarMercados()
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return Unauthorized();

            var lista = new List<Mercado>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = "SELECT id, nome FROM mercados WHERE usuario_id = @userId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Mercado
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome"))
                });
            }

            return Json(lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListarProdutos()
        {
            var lista = new List<ProdutoModel>();
            int? userId = HttpContext.Session.GetInt32("UsuarioID");

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                string sql = "SELECT id, nome FROM produtos WHERE usuario_id = @userId ORDER BY nome ASC";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new ProdutoModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Nome = reader.GetString(reader.GetOrdinal("nome"))
                    });
                }
            }
            return Json(lista);
        }

        [HttpPost]
        public async Task<IActionResult> SalvarCompra([FromBody] CompraVM model)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return Unauthorized();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var transaction = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                // SQL Pai: Adaptado LAST_INSERT_ID() para SCOPE_IDENTITY() do SQL Server
                string sqlCompra = @"INSERT INTO compras (usuario_id, mercado_id, data_compra, valor_total, descricao) 
                             VALUES (@userId, @mercadoId, @data, @valorTotal, @desc);
                             SELECT SCOPE_IDENTITY();";

                decimal totalGeral = model.Itens.Sum(i => i.Quantidade * i.ValorUnitario);

                long compraId;
                using (var cmd = new SqlCommand(sqlCompra, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@mercadoId", model.MercadoId);
                    cmd.Parameters.AddWithValue("@data", model.DataCompra);
                    cmd.Parameters.AddWithValue("@valorTotal", totalGeral);
                    cmd.Parameters.AddWithValue("@desc", model.Descricao ?? "");
                    compraId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                string sqlItens = @"INSERT INTO compra_itens (compra_id, produto_id, quantidade, valor_unitario, mercado_id, usuario_id) 
                            VALUES (@compraId, @produtoId, @qtd, @valor, @mercadoId, @userId)";

                foreach (var item in model.Itens)
                {
                    using (var cmdItem = new SqlCommand(sqlItens, conn, transaction))
                    {
                        cmdItem.Parameters.AddWithValue("@compraId", compraId);
                        cmdItem.Parameters.AddWithValue("@produtoId", item.ProdutoId);
                        cmdItem.Parameters.AddWithValue("@qtd", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@valor", item.ValorUnitario);
                        cmdItem.Parameters.AddWithValue("@mercadoId", model.MercadoId);
                        cmdItem.Parameters.AddWithValue("@userId", userId);
                        await cmdItem.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine("ERRO SALVAR COMPRA: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarComprasMes(int mes, int ano)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return Unauthorized();

            var lista = new List<object>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        SELECT p.nome as produto, m.nome as mercado, ci.valor_unitario, ci.quantidade, c.data_compra, ci.id
        FROM compra_itens ci
        JOIN compras c ON ci.compra_id = c.id
        JOIN produtos p ON ci.produto_id = p.id
        JOIN mercados m ON ci.mercado_id = m.id
        WHERE c.usuario_id = @userId 
        AND MONTH(c.data_compra) = @mes 
        AND YEAR(c.data_compra) = @ano
        ORDER BY c.data_compra DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@mes", mes);
            cmd.Parameters.AddWithValue("@ano", ano);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    produto = reader.GetString(reader.GetOrdinal("produto")),
                    mercado = reader.GetString(reader.GetOrdinal("mercado")),
                    valor = reader.GetDecimal(reader.GetOrdinal("valor_unitario")),
                    qtd = reader.GetInt32(reader.GetOrdinal("quantidade")),
                    data = reader.GetDateTime(reader.GetOrdinal("data_compra")).ToString("dd/MM/yyyy")
                });
            }

            return Json(lista);
        }

        //CADASTRO
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CadastroNovoUsuario(TratamentoCadastro model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string documentolimpo = new string(model.CpfCnpj.Where(char.IsDigit).ToArray());
                    string enumregimetrabalho = model.Regime_Trabalho == 0 ? "CLT" : "PJ";

                    string sql = "INSERT INTO usuarios (nome, cpfcnpj, email, regime_trabalho, senha) VALUES (@Nome, @CpfCnpj, @Email, @Regime_Trabalho, @Senha)";

                    using var connection = _db.GetConnection();
                    await connection.OpenAsync(); // Ajustado para await

                    using var command = new SqlCommand(sql, connection);

                    command.Parameters.AddWithValue("@Nome", model.Nome);
                    command.Parameters.AddWithValue("@CpfCnpj", documentolimpo);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@Regime_Trabalho", enumregimetrabalho);
                    command.Parameters.AddWithValue("@Senha", model.Senha);

                    await command.ExecuteNonQueryAsync();

                    TempData["Sucesso"] = "Usuário cadastrado com sucesso!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await _db.RegistrarErroAsync(null, "HomeController >> CadastroNovoUsuario", ex);
                    ViewBag.Erro = "Ocorreu um erro interno. Nossa equipe já foi notificada.";
                }

                return View("Cadastro", model);
            }

            return View("Cadastro", model);
        }

        //LOGIN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidarLogin(TratamentoLogin model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = _db.GetConnection();
                    await connection.OpenAsync(); // Ajustado para await

                    // Adaptado LIMIT 1 para TOP 1 do SQL Server
                    string sql = "SELECT TOP 1 * FROM usuarios WHERE email = @Email AND senha = @Senha";

                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@Senha", model.Senha);

                    using var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string nome = reader.GetString(reader.GetOrdinal("nome"));

                        HttpContext.Session.SetInt32("UsuarioID", id);
                        HttpContext.Session.SetString("Nome", nome);

                        return RedirectToAction("NoVerdinho", "Home");
                    }
                    else
                    {
                        return View("Login", model);
                    }
                }
                catch (Exception ex)
                {
                    await _db.RegistrarErroAsync(null, "HomeController >> ValidarLogin", ex);
                    ViewBag.Erro = "Ocorreu um erro interno. Nossa equipe já foi notificada.";
                }

                return View("Login", model);
            }

            return View("Login", model);
        }

        //CALCULO
        public async Task<IActionResult> CalculoCLTePJ(int id)
        {
            var informacoes = new TratamentoNoVerdinho();
            informacoes.UsuarioID = id;

            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                // 1. BUSCAR DADOS DO USUÁRIO
                string sqlUsuario = "SELECT * FROM usuarios WHERE id = @Id";
                using (var cmdUser = new SqlCommand(sqlUsuario, connection))
                {
                    cmdUser.Parameters.AddWithValue("@Id", id);
                    using var reader = await cmdUser.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        informacoes.regime_Trabalho = reader.IsDBNull(reader.GetOrdinal("regime_trabalho")) ? "" : reader.GetString(reader.GetOrdinal("regime_trabalho"));
                        informacoes.salario_bruto = reader.IsDBNull(reader.GetOrdinal("salario_bruto")) ? 0 : reader.GetDecimal(reader.GetOrdinal("salario_bruto"));
                        informacoes.salario_liquido = reader.IsDBNull(reader.GetOrdinal("salario_liquido")) ? 0 : reader.GetDecimal(reader.GetOrdinal("salario_liquido"));
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }

                // 2. BUSCAR OS DESCONTOS DO PERFIL
                string sqlDescontos = "SELECT nome_desconto, porcentagem, valor_calculado FROM descontos_perfil WHERE usuario_id = @Id";

                using (var cmdDesc = new SqlCommand(sqlDescontos, connection))
                {
                    cmdDesc.Parameters.AddWithValue("@Id", id);
                    using var readerDesc = await cmdDesc.ExecuteReaderAsync();

                    while (await readerDesc.ReadAsync())
                    {
                        informacoes.ListaDescontos.Add(new ItemDesconto
                        {
                            Nome = readerDesc.GetString(readerDesc.GetOrdinal("nome_desconto")),
                            Porcentagem = readerDesc.GetDecimal(readerDesc.GetOrdinal("porcentagem")),
                            Valor = readerDesc.GetDecimal(readerDesc.GetOrdinal("valor_calculado"))
                        });
                    }
                }

                return View("CalculoCLTePJ", informacoes);
            }
            catch (Exception ex)
            {
                await _db.RegistrarErroAsync(id, "HomeController >> CalculoCLTePJ", ex);
                ViewBag.Erro = "Ocorreu um erro ao carregar seus dados.";
                return View("CalculoCLTePJ", informacoes);
            }
        }

        public async Task<IActionResult> UpdateSalarioBruto(decimal salario_bruto, int id)
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                string sql = "UPDATE usuarios SET salario_bruto = @Salario_bruto WHERE id = @Id";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Salario_bruto", salario_bruto);
                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();

                TempData["Sucesso"] = "Salário Bruto Atualizado!";
                return Empty;
            }
            catch (Exception ex)
            {
                await _db.RegistrarErroAsync(null, "HomeController >> UpdateSalarioBruto", ex);
                ViewBag.Erro = "Ocorreu um erro interno. Nossa equipe já foi notificada.";
            }

            return Empty;
        }

        public async Task<IActionResult> UpdateRegime(string regime_trabalho, int id)
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                string sql = "UPDATE usuarios SET regime_trabalho = @Regime_trabalho WHERE id = @Id";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Regime_trabalho", regime_trabalho);
                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();

                TempData["Sucesso"] = "Regime de Trabalho Atualizado!";
                return Empty;
            }
            catch (Exception ex)
            {
                await _db.RegistrarErroAsync(null, "HomeController >> UpdateRegime", ex);
                ViewBag.Erro = "Ocorreu um erro interno. Nossa equipe já foi notificada.";
            }

            return Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CalculoCLTePJ(TratamentoNoVerdinho model)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioID");
            if (userId == null) return RedirectToAction("Login");

            decimal bruto = model.salario_bruto;
            decimal liquido = bruto;

            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();
                using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                try
                {
                    // Limpa os descontos anteriores
                    string sqlDel = "DELETE FROM descontos_perfil WHERE usuario_id = @id";
                    using (var cmd = new SqlCommand(sqlDel, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // --- REGRA 1: INSS (Tabela Progressiva Simplificada) ---
                    if (model.regime_Trabalho == "CLT")
                    {
                        decimal valorInss = CalcularInss(bruto);
                        decimal percInss = bruto > 0 ? (valorInss / bruto) * 100 : 0;

                        await SalvarDescontoNoBanco(userId.Value, "INSS (Previdência)", percInss, valorInss, connection, transaction);
                        liquido -= valorInss;

                        // --- REGRA 2: IRRF (Base = Bruto - INSS) ---
                        decimal baseIrrf = bruto - valorInss;
                        decimal valorIrrf = CalcularIrrf(baseIrrf);

                        if (valorIrrf > 0)
                        {
                            decimal percIrrf = bruto > 0 ? (valorIrrf / bruto) * 100 : 0;
                            await SalvarDescontoNoBanco(userId.Value, "IRRF (Imposto de Renda)", percIrrf, valorIrrf, connection, transaction);
                            liquido -= valorIrrf;
                        }
                    }

                    // --- REGRA 3: DESCONTOS PERSONALIZADOS ---
                    if (model.ListaDescontos != null && model.ListaDescontos.Any())
                    {
                        foreach (var item in model.ListaDescontos.Where(d => !string.IsNullOrEmpty(d.Nome)))
                        {
                            decimal valorDesc = item.Porcentagem > 0 ? (bruto * item.Porcentagem) / 100 : item.Valor;

                            await SalvarDescontoNoBanco(userId.Value, item.Nome, item.Porcentagem, valorDesc, connection, transaction);
                            liquido -= valorDesc;
                        }
                    }

                    // --- SALVAR RESULTADO FINAL ---
                    string sqlUpdate = "UPDATE usuarios SET salario_liquido = @liq, salario_bruto = @brut WHERE id = @id";
                    using (var cmdUpd = new SqlCommand(sqlUpdate, connection, transaction))
                    {
                        cmdUpd.Parameters.AddWithValue("@liq", liquido < 0 ? 0 : liquido);
                        cmdUpd.Parameters.AddWithValue("@brut", bruto);
                        cmdUpd.Parameters.AddWithValue("@id", userId);
                        await cmdUpd.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                    return RedirectToAction("CalculoCLTePJ", new { id = userId.Value });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _db.RegistrarErroAsync(userId, "Erro de Cálculo", ex);
                return View("Error");
            }
        }

        private decimal CalcularInss(decimal salario)
        {
            if (salario <= 1412.00m) return salario * 0.075m;
            if (salario <= 2666.68m) return (salario * 0.09m) - 21.18m;
            if (salario <= 4000.03m) return (salario * 0.12m) - 101.18m;
            if (salario <= 7786.02m) return (salario * 0.14m) - 181.18m;
            return 908.85m;
        }

        private decimal CalcularIrrf(decimal baseCalculo)
        {
            if (baseCalculo <= 2259.20m) return 0;
            if (baseCalculo <= 2826.65m) return (baseCalculo * 0.075m) - 169.44m;
            if (baseCalculo <= 3751.05m) return (baseCalculo * 0.15m) - 381.44m;
            if (baseCalculo <= 4664.68m) return (baseCalculo * 0.225m) - 662.77m;
            return (baseCalculo * 0.275m) - 896.00m;
        }

        private async Task SalvarDescontoNoBanco(int userId, string nome, decimal porcento, decimal valor, SqlConnection conn, SqlTransaction trans)
        {
            string sql = "INSERT INTO descontos_perfil (usuario_id, nome_desconto, porcentagem, valor_calculado) VALUES (@u, @n, @p, @v)";
            using var cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@p", porcento);
            cmd.Parameters.AddWithValue("@v", valor);
            await cmd.ExecuteNonQueryAsync();
        }

        // ERRO
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}