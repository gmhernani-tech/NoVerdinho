USE [master]
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'NoVerdinhoDB')
BEGIN
    CREATE DATABASE [NoVerdinhoDB]
END
GO

USE [NoVerdinhoDB]
GO

-- Tabelas principais (sem as constraints de FK para não dar erro de ordem)
CREATE TABLE [dbo].[usuarios](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [nome] [varchar](100) NOT NULL,
    [cpfcnpj] [varchar](14) NOT NULL,
    [email] [varchar](100) NOT NULL UNIQUE,
    [senha] [varchar](45) NOT NULL,
    [regime_trabalho] [varchar](3) DEFAULT ('CLT') CHECK ([regime_trabalho] IN ('PJ', 'CLT')),
    [salario_bruto] [decimal](10, 2) DEFAULT ((0.00)),
    [salario_liquido] [decimal](10, 2) DEFAULT ((0.00))
)
GO

CREATE TABLE [dbo].[mercados](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [nome] [varchar](100) NOT NULL,
    [usuario_id] [int] NOT NULL
)
GO

CREATE TABLE [dbo].[produtos](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [nome] [varchar](100) NOT NULL,
    [usuario_id] [int] NOT NULL
)
GO

CREATE TABLE [dbo].[compras](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [usuario_id] [int] NULL,
    [mercado_id] [int] NULL,
    [data_compra] [date] NOT NULL,
    [valor_total] [decimal](10, 2) DEFAULT ((0.00)),
    [descricao] [varchar](255) NULL
)
GO

CREATE TABLE [dbo].[compra_itens](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [compra_id] [int] NULL,
    [produto_id] [int] NULL,
    [quantidade] [int] DEFAULT ((1)),
    [valor_unitario] [decimal](10, 2) NOT NULL,
    [mercado_id] [int] NOT NULL,
    [usuario_id] [int] NOT NULL
)
GO

CREATE TABLE [dbo].[gastos](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [usuario_id] [int] NOT NULL,
    [descricao] [varchar](255) NOT NULL,
    [categoria] [varchar](50) NOT NULL,
    [valor] [decimal](10, 2) NOT NULL,
    [data_gasto] [datetime] DEFAULT (getdate())
)
GO

CREATE TABLE [dbo].[descontos_perfil](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [usuario_id] [int] NULL,
    [nome_desconto] [varchar](50) NULL,
    [porcentagem] [decimal](5, 2) NULL,
    [valor_calculado] [decimal](10, 2) NULL
)
GO

CREATE TABLE [dbo].[logs_erro](
    [id] [int] IDENTITY(1,1) PRIMARY KEY,
    [usuario_id] [int] NULL,
    [origem] [varchar](255) NOT NULL,
    [mensagem] [varchar](max) NOT NULL,
    [stack_trace] [varchar](max) NULL,
    [data_erro] [datetime] DEFAULT (getdate())
)
GO

-- Adicionando as Foreign Keys
ALTER TABLE [dbo].[compras] ADD CONSTRAINT [FK_compras_usuarios] FOREIGN KEY([usuario_id]) REFERENCES [dbo].[usuarios] ([id])
ALTER TABLE [dbo].[compras] ADD CONSTRAINT [FK_compras_mercados] FOREIGN KEY([mercado_id]) REFERENCES [dbo].[mercados] ([id])

ALTER TABLE [dbo].[compra_itens] ADD CONSTRAINT [FK_compra_itens_compras] FOREIGN KEY([compra_id]) REFERENCES [dbo].[compras] ([id]) ON DELETE CASCADE
ALTER TABLE [dbo].[compra_itens] ADD CONSTRAINT [FK_compra_itens_produtos] FOREIGN KEY([produto_id]) REFERENCES [dbo].[produtos] ([id])

ALTER TABLE [dbo].[mercados] ADD CONSTRAINT [FK_mercados_usuarios] FOREIGN KEY([usuario_id]) REFERENCES [dbo].[usuarios] ([id])
ALTER TABLE [dbo].[produtos] ADD CONSTRAINT [FK_produtos_usuarios] FOREIGN KEY([usuario_id]) REFERENCES [dbo].[usuarios] ([id])

ALTER TABLE [dbo].[gastos] ADD CONSTRAINT [FK_gastos_usuarios] FOREIGN KEY([usuario_id]) REFERENCES [dbo].[usuarios] ([id])
ALTER TABLE [dbo].[descontos_perfil] ADD CONSTRAINT [FK_descontos_perfil_usuarios] FOREIGN KEY([usuario_id]) REFERENCES [dbo].[usuarios] ([id]) ON DELETE CASCADE
ALTER TABLE [dbo].[logs_erro] ADD CONSTRAINT [FK_logs_erro_usuarios] FOREIGN KEY([usuario_id]) REFERENCES [dbo].[usuarios] ([id]) ON DELETE SET NULL
GO