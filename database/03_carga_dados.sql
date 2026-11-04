USE [NoVerdinhoDB]
GO

-- Inserindo Usuário de Teste
INSERT INTO [dbo].[usuarios] (nome, cpfcnpj, email, senha, regime_trabalho, salario_bruto, salario_liquido)
VALUES ('Guilherme Hernani', '12345678901', 'guilherme@noverdinho.local', 'senha123', 'CLT', 6000.00, 5200.00);

DECLARE @UserId INT = SCOPE_IDENTITY();

-- Inserindo Mercados em Campinas
INSERT INTO [dbo].[mercados] (nome, usuario_id) VALUES ('Carrefour Campinas', @UserId);
INSERT INTO [dbo].[mercados] (nome, usuario_id) VALUES ('Supermercado Taquaral', @UserId);

-- Inserindo Produtos Base
INSERT INTO [dbo].[produtos] (nome, usuario_id) VALUES ('Arroz 5kg', @UserId);
INSERT INTO [dbo].[produtos] (nome, usuario_id) VALUES ('Feijão Carioca 1kg', @UserId);
INSERT INTO [dbo].[produtos] (nome, usuario_id) VALUES ('Café Torrado 500g', @UserId);

-- Inserindo Gastos Fixos via Stored Procedure
EXEC [dbo].[sp_RegistrarGastoSeguro] @UserId, 'Conta de Energia CPFL', 'Moradia', 185.50;
EXEC [dbo].[sp_RegistrarGastoSeguro] @UserId, 'Internet Fibra', 'Moradia', 99.90;
EXEC [dbo].[sp_RegistrarGastoSeguro] @UserId, 'Assinatura Spotify', 'Lazer', 21.90;

-- Inserindo uma Compra de Mercado e Itens
INSERT INTO [dbo].[compras] (usuario_id, mercado_id, data_compra, valor_total, descricao)
VALUES (@UserId, 1, GETDATE(), 75.40, 'Compra rápida semana');
DECLARE @CompraId INT = SCOPE_IDENTITY();

INSERT INTO [dbo].[compra_itens] (compra_id, produto_id, quantidade, valor_unitario, mercado_id, usuario_id)
VALUES (@CompraId, 1, 1, 28.50, 1, @UserId),
       (@CompraId, 2, 2, 8.50, 1, @UserId),
       (@CompraId, 3, 1, 29.90, 1, @UserId);
GO