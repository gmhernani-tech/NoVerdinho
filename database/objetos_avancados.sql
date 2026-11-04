USE [NoVerdinhoDB];
GO

-- =========================================================================
-- 1. VIEWS: Simplificação de Relatórios e Consultas Consolidadas
-- =========================================================================
CREATE OR ALTER VIEW vw_ResumoConsolidadoMensal AS
SELECT 
    u.id AS usuario_id,
    u.nome AS usuario_nome,
    MONTH(ISNULL(g.data_gasto, c.data_compra)) AS mes,
    YEAR(ISNULL(g.data_gasto, c.data_compra)) AS ano,
    ISNULL(SUM(g.valor), 0) AS total_gastos_gerais,
    ISNULL(SUM(c.valor_total), 0) AS total_compras_mercado,
    (ISNULL(SUM(g.valor), 0) + ISNULL(SUM(c.valor_total), 0)) AS despesa_total
FROM usuarios u
LEFT JOIN gastos g ON u.id = g.usuario_id
LEFT JOIN compras c ON u.id = c.usuario_id
GROUP BY u.id, u.nome, MONTH(ISNULL(g.data_gasto, c.data_compra)), YEAR(ISNULL(g.data_gasto, c.data_compra));
GO

-- =========================================================================
-- 2. FUNCTIONS: Lógica Reutilizável (Escalar e de Tabela)
-- =========================================================================
CREATE OR ALTER FUNCTION fn_CalculaComprometimentoRenda 
(
    @usuario_id INT,
    @mes INT,
    @ano INT
)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @salario_liquido DECIMAL(10,2);
    DECLARE @total_gastos DECIMAL(10,2);
    DECLARE @percentual DECIMAL(5,2);

    -- Pega o salário líquido do usuário
    SELECT @salario_liquido = ISNULL(salario_liquido, 0) 
    FROM usuarios 
    WHERE id = @usuario_id;

    IF @salario_liquido <= 0 
        RETURN 0;

    -- Soma os gastos gerais e compras do mês/ano específico
    SELECT @total_gastos = ISNULL(SUM(valor), 0)
    FROM (
        SELECT valor FROM gastos WHERE usuario_id = @usuario_id AND MONTH(data_gasto) = @mes AND YEAR(data_gasto) = @ano
        UNION ALL
        SELECT valor_total FROM compras WHERE usuario_id = @usuario_id AND MONTH(data_compra) = @mes AND YEAR(data_compra) = @ano
    ) AS despesas;

    SET @percentual = (@total_gastos / @salario_liquido) * 100;

    RETURN @percentual;
END;
GO

-- =========================================================================
-- 3. STORED PROCEDURES: Encapsulamento de Regra de Negócio com Transação
-- =========================================================================
CREATE OR ALTER PROCEDURE sp_RegistrarGastoSeguro
    @usuario_id INT,
    @descricao VARCHAR(255),
    @categoria VARCHAR(50),
    @valor DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO gastos (usuario_id, descricao, categoria, valor, data_gasto)
        VALUES (@usuario_id, @descricao, @categoria, @valor, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- =========================================================================
-- 4. TRIGGERS: Auditoria de Alterações Salariais
-- =========================================================================
-- Tabela de Auditoria para a Trigger
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'audit_log_usuarios')
BEGIN
    CREATE TABLE audit_log_usuarios (
        id INT IDENTITY(1,1) PRIMARY KEY,
        usuario_id INT,
        detalhes VARCHAR(MAX),
        data_alteracao DATETIME DEFAULT GETDATE()
    );
END;
GO

CREATE OR ALTER TRIGGER trg_AuditaAlteracaoSalario
ON usuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(salario_bruto) OR UPDATE(salario_liquido)
    BEGIN
        INSERT INTO audit_log_usuarios (usuario_id, detalhes)
        SELECT 
            inserted.id, 
            'Salário bruto alterado de ' + CAST(ISNULL(deleted.salario_bruto, 0) AS VARCHAR) + 
            ' para ' + CAST(ISNULL(inserted.salario_bruto, 0) AS VARCHAR)
        FROM inserted
        INNER JOIN deleted ON inserted.id = deleted.id;
    END
END;
GO

-- =========================================================================
-- 5. ÍNDICES DE OTIMIZAÇÃO DE PERFORMANCE
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_gastos_usuario_data')
    CREATE NONCLUSTERED INDEX IX_gastos_usuario_data ON gastos(usuario_id, data_gasto);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_compras_usuario_data')
    CREATE NONCLUSTERED INDEX IX_compras_usuario_data ON compras(usuario_id, data_compra);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_compra_itens_produto')
    CREATE NONCLUSTERED INDEX IX_compra_itens_produto ON compra_itens(produto_id, valor_unitario);
GO