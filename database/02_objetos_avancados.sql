USE [NoVerdinhoDB]
GO

-- =======================================================
-- 1. TABELA DE AUDITORIA (Necessária para a Trigger)
-- =======================================================
CREATE TABLE [dbo].[audit_log_usuarios] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [usuario_id] INT,
    [salario_antigo] DECIMAL(10,2),
    [salario_novo] DECIMAL(10,2),
    [data_alteracao] DATETIME DEFAULT GETDATE()
)
GO

-- =======================================================
-- 2. STORED PROCEDURE (Transação e Tratamento de Erros)
-- =======================================================
CREATE PROCEDURE [dbo].[sp_RegistrarGastoSeguro]
    @UsuarioId INT,
    @Descricao VARCHAR(255),
    @Categoria VARCHAR(50),
    @Valor DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
            
            INSERT INTO [dbo].[gastos] (usuario_id, descricao, categoria, valor, data_gasto)
            VALUES (@UsuarioId, @Descricao, @Categoria, @Valor, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        INSERT INTO [dbo].[logs_erro] (usuario_id, origem, mensagem, stack_trace)
        VALUES (@UsuarioId, 'sp_RegistrarGastoSeguro', ERROR_MESSAGE(), ERROR_PROCEDURE());
        
        THROW;
    END CATCH
END
GO

-- =======================================================
-- 3. VIEW (Relatório Consolidado)
-- =======================================================
CREATE VIEW [dbo].[vw_ResumoConsolidadoMensal] AS
SELECT 
    u.id AS usuario_id,
    u.nome AS usuario_nome,
    YEAR(g.data_gasto) AS Ano,
    MONTH(g.data_gasto) AS Mes,
    'Gasto Fixo/Variável' AS Tipo,
    g.categoria AS Categoria,
    SUM(g.valor) AS TotalGasto
FROM [dbo].[usuarios] u
JOIN [dbo].[gastos] g ON u.id = g.usuario_id
GROUP BY u.id, u.nome, YEAR(g.data_gasto), MONTH(g.data_gasto), g.categoria

UNION ALL

SELECT 
    u.id AS usuario_id,
    u.nome AS usuario_nome,
    YEAR(c.data_compra) AS Ano,
    MONTH(c.data_compra) AS Mes,
    'Mercado' AS Tipo,
    m.nome AS Categoria,
    SUM(c.valor_total) AS TotalGasto
FROM [dbo].[usuarios] u
JOIN [dbo].[compras] c ON u.id = c.usuario_id
JOIN [dbo].[mercados] m ON c.mercado_id = m.id
GROUP BY u.id, u.nome, YEAR(c.data_compra), MONTH(c.data_compra), m.nome;
GO

-- =======================================================
-- 4. FUNCTION (Lógica Reutilizável)
-- =======================================================
CREATE FUNCTION [dbo].[fn_CalculaComprometimentoRenda] (@UsuarioId INT, @Mes INT, @Ano INT)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @RendaLiquida DECIMAL(10,2);
    DECLARE @TotalGasto DECIMAL(10,2);
    DECLARE @Percentual DECIMAL(5,2);

    SELECT @RendaLiquida = salario_liquido FROM [dbo].[usuarios] WHERE id = @UsuarioId;
    
    IF @RendaLiquida IS NULL OR @RendaLiquida <= 0
        RETURN 0;

    SELECT @TotalGasto = ISNULL(SUM(TotalGasto), 0)
    FROM [dbo].[vw_ResumoConsolidadoMensal]
    WHERE usuario_id = @UsuarioId AND Mes = @Mes AND Ano = @Ano;

    SET @Percentual = (@TotalGasto / @RendaLiquida) * 100;
    
    RETURN @Percentual;
END
GO

-- =======================================================
-- 5. TRIGGER (Auditoria)
-- =======================================================
CREATE TRIGGER [dbo].[trg_AuditaAlteracaoSalario]
ON [dbo].[usuarios]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(salario_bruto) OR UPDATE(salario_liquido)
    BEGIN
        INSERT INTO [dbo].[audit_log_usuarios] (usuario_id, salario_antigo, salario_novo)
        SELECT 
            i.id, 
            d.salario_liquido, 
            i.salario_liquido
        FROM inserted i
        JOIN deleted d ON i.id = d.id
        WHERE i.salario_liquido <> d.salario_liquido;
    END
END
GO

-- =======================================================
-- 6. ÍNDICES DE OTIMIZAÇÃO (Desempenho)
-- =======================================================
CREATE NONCLUSTERED INDEX IX_gastos_usuario_data ON [dbo].[gastos](usuario_id, data_gasto) INCLUDE (valor);
CREATE NONCLUSTERED INDEX IX_compras_usuario_data ON [dbo].[compras](usuario_id, data_compra) INCLUDE (valor_total);
CREATE NONCLUSTERED INDEX IX_compra_itens_produto ON [dbo].[compra_itens](produto_id, mercado_id);
GO