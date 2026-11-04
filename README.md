<<<<<<< HEAD
# 💵 NoVerdinho - Sistema de Gestão Financeira Pessoal
=======
# 🥦 NoVerdinho - Sistema de Gestão Financeira Pessoal
>>>>>>> 81bb9bc (Atualização README)

> **Trabalho Prático Integrado — Disciplina: Banco de Dados Avançado (UNIP)**  
> **Tecnologias:** C# .NET 8, ASP.NET Core MVC, SQL Server, HTML5/CSS3, JavaScript, PowerShell.

---

## 📌 Visão Geral do Projeto

O **NoVerdinho** é uma aplicação Full Stack voltada para a gestão e otimização do orçamento doméstico, controlo de gastos diários e comparativo de preços de mantimentos de mercado. 

O projeto foi desenvolvido para atender a todos os requisitos de arquitetura computacional de ponta a ponta, abrangendo desde a automação de infraestrutura no **Microsoft SQL Server** via scripts **PowerShell**, até à implementação de lógica avançada em **T-SQL** (Stored Procedures, Views, Functions, Triggers e Índices), consumida por um ecossistema **.NET 8**.

---

## 🏗️ Estrutura do Repositório

```text
src_github/
 ├── README.md                   # Documentação principal do repositório
 ├── setup_database.ps1          # Script PowerShell de inicialização e deploy do banco
 ├── database/                   # Scripts T-SQL (DDL, DML e Objetos Avançados)
 │    └── objetos_avancados.sql
 ├── api/                        # Backend / Controllers / Services (.NET 8)
 │    ├── Controllers/
 │    ├── Models/
 │    └── Services/
 └── frontend/                   # Interface da aplicação (Views ASP.NET MVC / JS / Web)
      ├── Views/
      └── wwwroot/
```

<<<<<<< HEAD
## 🗄️ Arquitetura do Banco de Dados (SQL Server)
A modelagem relacional foi desenhada no Microsoft SQL Server, garantindo a integridade dos dados e o cumprimento das normas de normalização (3FN).

## 🛠️ Objetos Avançados Implementados
=======
🗄️ Arquitetura do Banco de Dados (SQL Server)
A modelagem relacional foi desenhada no Microsoft SQL Server, garantindo a integridade dos dados e o cumprimento das normas de normalização (3FN).

🛠️ Objetos Avançados Implementados
>>>>>>> 81bb9bc (Atualização README)
Stored Procedures (sp_RegistrarGastoSeguro): Encapsula a lógica de negócio para inserção de movimentações financeiras dentro de um bloco transacional explícito (BEGIN TRANSACTION / COMMIT / ROLLBACK), garantindo a propriedade ACID.

Views (vw_ResumoConsolidadoMensal): Unifica as despesas de gastos fixos/variáveis e compras de mercado por utilizador, otimizando relatórios e dashboards.

Functions (fn_CalculaComprometimentoRenda): Função escalar que calcula percentualmente o comprometimento da renda líquida do utilizador com base no histórico de despesas do mês selecionado.

Triggers (trg_AuditaAlteracaoSalario): Regra de auditoria automática acionada em operações de UPDATE na tabela usuarios, registando o histórico de alterações salariais na tabela audit_log_usuarios.

Índices de Otimização (IX_gastos_usuario_data, IX_compras_usuario_data, IX_compra_itens_produto): Índices Não-Agrupados (Non-Clustered) criados para aceleração de pesquisas em consultas frequentes de filtragem por data e utilizador.

<<<<<<< HEAD
## 🚀 Como Executar o Projeto
=======
🚀 Como Executar o Projeto
>>>>>>> 81bb9bc (Atualização README)
Prerequisitos
.NET 8 SDK instalado.

Microsoft SQL Server ou SQL Server Express LocalDB ((localdb)\MSSQLLocalDB).

PowerShell 5.1+.

1. Automating SQL Server Environment (PowerShell)
Para criar automaticamente o banco de dados NoVerdinhoDB, criar a estrutura de tabelas e aplicar todos os objetos avançados do T-SQL, executa o script PowerShell incluído no projeto:

Abre o PowerShell no diretório raiz (src_github).

Executa o seguinte comando:
PowerShell
.\setup_database.ps1
Nota: O script utiliza o utilitário sqlcmd para ligar à instância (localdb)\MSSQLLocalDB e aprovisionar o ambiente sem intervenção manual.

2. Configuração e Execução do Backend/Frontend (.NET 8)
Garante que a String de Conexão no ficheiro appsettings.json aponta para a tua instância local do SQL Server:
<<<<<<< HEAD
```text
=======

>>>>>>> 81bb9bc (Atualização README)
JSON
{
  "ConnectionStrings": {
    "NoVerdinhoDB": "Server=(localdb)\\MSSQLLocalDB;Database=NoVerdinhoDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
<<<<<<< HEAD
```
=======
>>>>>>> 81bb9bc (Atualização README)
Executa a aplicação através do terminal do .NET:

Bash
dotnet restore
dotnet run
Acede à aplicação no navegador: https://localhost:7198 (ou na porta configurada no teu ambiente).

<<<<<<< HEAD
## 📡 Chamadas e Testes de API / Endpoints
=======
📡 Chamadas e Testes de API / Endpoints
>>>>>>> 81bb9bc (Atualização README)
Abaixo estão listados exemplos de endpoints internos utilizados para integração das funcionalidades:

1. Registar Novo Mercado
HTTP Method: POST
<<<<<<< HEAD
```text
Endpoint: /Home/SalvarMercado
```
Payload:
```text
=======

Endpoint: /Home/SalvarMercado

Payload:

>>>>>>> 81bb9bc (Atualização README)
JSON
{
  "nome": "Carrefour Campinas"
}
<<<<<<< HEAD
```
2. Registar Compra com Itens
HTTP Method: POST
```text
Endpoint: /Home/SalvarCompra
```
Payload:
```text
=======
2. Registar Compra com Itens
HTTP Method: POST

Endpoint: /Home/SalvarCompra

Payload:

>>>>>>> 81bb9bc (Atualização README)
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
<<<<<<< HEAD
```
3. Consultar Compras do Mês (Filtro Dinâmico)
HTTP Method: GET
```text
Endpoint: /Home/ListarComprasMes?mes=11&ano=2026
```
Resposta:
```text
=======
3. Consultar Compras do Mês (Filtro Dinâmico)
HTTP Method: GET

Endpoint: /Home/ListarComprasMes?mes=11&ano=2026

Resposta:

>>>>>>> 81bb9bc (Atualização README)
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
<<<<<<< HEAD
```
## 📜 Licença e Autoria
Trabalho desenvolvido como requisito de avaliação da disciplina de Banco de Dados Avançado na Universidade Paulista (UNIP).
=======

📜 Licença e Autoria
Trabalho desenvolvido como requisito de avaliação da disciplina de Banco de Dados Avançado na Universidade Paulista (UNIP).
>>>>>>> 81bb9bc (Atualização README)
