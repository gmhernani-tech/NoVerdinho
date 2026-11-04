namespace NoVerdinho.Models
{
    public class TratamentoNoVerdinho
    {
        public List<ItemDesconto> ListaDescontos { get; set; } = new List<ItemDesconto>();

        public int UsuarioID { get; set; }
        public string regime_Trabalho { get; set; }
        public decimal salario_bruto { get; set; }
        public decimal salario_liquido { get; set; }

        public TratamentoNoVerdinho()
        {
            regime_Trabalho = "CLT";
            salario_bruto = 0;
            salario_liquido = 0;
        }
    }

    public class ItemDesconto
    {
        public string Nome { get; set; }
        public decimal Porcentagem { get; set; }
        public decimal Valor { get; set; }
    }

    public class ExcluirGastoRequest
    {
        public int Id { get; set; }
    }

    public class Mercado
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public class GastoItem
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
    }

    public class CompraRequest
    {
        public int MercadoId { get; set; }
        public DateTime DataCompra { get; set; }
        public string Descricao { get; set; }
        public List<ItemCompraRequest> Itens { get; set; }
    }

    public class ProdutoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int UsuarioId { get; set; } 
    }

    public class ItemCompraRequest
    {
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
    }

    public class CompraVM
    {
        public int MercadoId { get; set; }
        public DateTime DataCompra { get; set; }
        public string Descricao { get; set; }
        public List<CompraItemVM> Itens { get; set; }
    }

    public class CompraItemVM
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
    }
}
