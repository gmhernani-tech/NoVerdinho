
# 💵 NoVerdinho - Sistema de Gestão Financeira Pessoal

> **Trabalho Prático Integrado — Disciplina: Banco de Dados Avançado (UNIP)**  
> **Tecnologias:** C# .NET 8, ASP.NET Core MVC, SQL Server, HTML5/CSS3, JavaScript, PowerShell.

---

## 📌 Visão Geral do Projeto

O **NoVerdinho** é uma aplicação Full Stack voltada para a gestão e otimização do orçamento doméstico, controle de gastos diários e comparativo de preços de mantimentos de mercado. 

O projeto foi desenvolvido para atender a todos os requisitos de arquitetura computacional de ponta a ponta, abrangendo desde a automação de infraestrutura no **Microsoft SQL Server** via scripts **PowerShell**, até a implementação de lógica avançada em **T-SQL** (Stored Procedures, Views, Functions, Triggers e Índices), consumida por um ecossistema **.NET 8**.

---

## 🏗️ Estrutura do Repositório

```text
 ├── README.md                   # Documentação principal do repositório
 ├── setup_database.ps1          # Script PowerShell de inicialização e deploy do banco
 ├── database/                   # Scripts T-SQL (DDL, DML e Objetos Avançados)
 │    ├── 01_schema.sql
 │    ├── 02_objetos_avancados.sql
 │    └── 03_carga_dados.sql
 ├── Controllers/                # Backend / Endpoints e Lógica de Negócio (.NET 8)
 ├── Models/                     # Classes de Domínio e Entidades de Dados
 ├── Views/                      # Interface da aplicação Front-End (Páginas Razor/HTML)
 ├── wwwroot/                    # Arquivos estáticos do Front-End (CSS, JS, Imagens)
 ├── Program.cs                  # Ponto de entrada, configuração do servidor e serviços
 └── appsettings.json            # Configurações de ambiente e String de Conexão
```

🗄️ Arquitetura do Banco de Dados (SQL Server)
A modelagem relacional foi desenhada no Microsoft SQL Server, garantindo a integridade dos dados e o cumprimento das regras de normalização (3FN).

🛠️ Objetos Avançados Implementados
Stored Procedures (sp_RegistrarGastoSeguro): Encapsula a lógica de negócio para inserção de movimentações financeiras dentro de um bloco transacional explícito (BEGIN TRANSACTION / COMMIT / ROLLBACK), garantindo a propriedade ACID.

Views (vw_ResumoConsolidadoMensal): Unifica as despesas de gastos fixos/variáveis e compras de mercado por usuário, otimizando relatórios e dashboards.

Functions (fn_CalculaComprometimentoRenda): Função escalar que calcula percentualmente o comprometimento da renda líquida do usuário com base no histórico de despesas do mês selecionado.

Triggers (trg_AuditaAlteracaoSalario): Regra de auditoria automática acionada em operações de UPDATE na tabela de usuários, registrando o histórico de alterações salariais na tabela audit_log_usuarios.

Índices de Otimização (IX_gastos_usuario_data, IX_compras_usuario_data, IX_compra_itens_produto): Índices Não-Agrupados (Non-Clustered) criados para aceleração de pesquisas em consultas frequentes de filtragem por data e usuário.

🚀 Como Executar o Projeto
Pré-requisitos

.NET 8 SDK instalado.

Microsoft SQL Server ou SQL Server Express LocalDB ((localdb)\MSSQLLocalDB).

PowerShell 5.1+.

1. Automação do Banco de Dados (PowerShell)
Para criar automaticamente o banco de dados NoVerdinhoDB, criar a estrutura de tabelas e aplicar todos os objetos avançados do T-SQL, execute o script PowerShell incluído no projeto:

Abra o PowerShell no diretório raiz (src_github) e execute o seguinte comando:

PowerShell
.\setup_database.ps1
Nota: O script utiliza o utilitário sqlcmd para conectar à instância (localdb)\MSSQLLocalDB e provisionar o ambiente sem intervenção manual.

2. Configuração e Execução do Backend/Frontend (.NET 8)
Garanta que a String de Conexão no arquivo appsettings.json aponta para a sua instância local do SQL Server:
```
JSON
{
  "ConnectionStrings": {
    "NoVerdinhoDB": "Server=(localdb)\\MSSQLLocalDB;Database=NoVerdinhoDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```
Execute a aplicação através do terminal do .NET:

Bash
dotnet restore
dotnet run
Acesse a aplicação no navegador: https://localhost:7198 (ou na porta configurada no seu ambiente).

📡 Chamadas e Testes de API / Endpoints
Abaixo estão listados exemplos de endpoints internos utilizados para integração das funcionalidades:

1. Registrar Novo Mercado
HTTP Method: POST
```
Endpoint: /Home/SalvarMercado
```
Payload:
```
JSON
{
  "nome": "Carrefour Campinas"
}
```

2. Registrar Compra com Itens
HTTP Method: POST
```
Endpoint: /Home/SalvarCompra
```
Payload:
```
JSON
{
  "mercadoId": 1,
  "dataCompra": "2026-11-04",
  "descricao": "Compras Mensais de Mantimentos",
  "itens": [
    {
      "produtoId": 2,
      "quantidade": 3,
      "valorUnitario": 28.50
    }
  ]
}
```

3. Consultar Compras do Mês (Filtro Dinâmico)
HTTP Method: GET
```
Endpoint: /Home/ListarComprasMes?mes=11&ano=2026
```
Resposta:
```
JSON
[
  {
    "id": 1002,
    "produto": "Arroz 5kg",
    "mercado": "Supermercado Taquaral",
    "valor": 28.50,
    "qtd": 3,
    "data": "04/11/2026"
  }
]
```
📜 Licença e Autoria
Trabalho desenvolvido como requisito de avaliação da disciplina de Banco de Dados Avançado na Universidade Paulista (UNIP).
