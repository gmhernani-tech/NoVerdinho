# =====================================================================
# Script de Automação de Deploy da Base de Dados - NoVerdinhoDB
# Disciplina: Banco de Dados Avançado
# =====================================================================

$ServerName = "(localdb)\MSSQLLocalDB"
$DatabaseName = "NoVerdinhoDB"
$DatabaseFolder = ".\database"

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host " A iniciar o Setup da Base de Dados: $DatabaseName " -ForegroundColor Cyan
Write-Host " Servidor Alvo: $ServerName " -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""

# Função auxiliar para executar scripts via SQLCMD e verificar erros
function Run-SqlScript {
    param (
        [string]$FilePath,
        [string]$DbName = "master"
    )

    if (Test-Path $FilePath) {
        Write-Host "[A EXECUTAR] $FilePath..." -ForegroundColor Yellow
        # O parâmetro -b força o sqlcmd a abortar e devolver erro em caso de falha
        $sqlcmdArgs = "-S", $ServerName, "-d", $DbName, "-E", "-i", $FilePath, "-b", "-W"
        
        # Executa o sqlcmd
        & sqlcmd @sqlcmdArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[SUCESSO] Script executado sem erros.`n" -ForegroundColor Green
        } else {
            Write-Host "[ERRO] Falha ao executar $FilePath. Verifica os logs acima." -ForegroundColor Red
            exit $LASTEXITCODE
        }
    } else {
        Write-Host "[ERRO] Ficheiro não encontrado: $FilePath" -ForegroundColor Red
        exit 1
    }
}

try {
    # 1. Criação da Base de Dados e Tabelas (DDL)
    # Executa no 'master' para criar a NoVerdinhoDB
    Run-SqlScript -FilePath "$DatabaseFolder\01_schema.sql" -DbName "master"

    # 2. Criação dos Objetos Avançados (Procedures, Views, Triggers, Functions)
    Run-SqlScript -FilePath "$DatabaseFolder\02_objetos_avancados.sql" -DbName $DatabaseName

    # 3. Carga de Dados de Teste (DML - População)
    Run-SqlScript -FilePath "$DatabaseFolder\03_carga_dados.sql" -DbName $DatabaseName

    Write-Host "=======================================================" -ForegroundColor Cyan
    Write-Host " Setup finalizado com SUCESSO! A Base de Dados está pronta. " -ForegroundColor Green
    Write-Host "=======================================================" -ForegroundColor Cyan

} catch {
    Write-Host "[ERRO CRÍTICO] Ocorreu uma exceção inesperada durante o setup." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}