# =========================================================================
# Script de Inicialização Automatizada do Banco de Dados - NoVerdinho
# =========================================================================

$serverInstance = "(localdb)\MSSQLLocalDB"
$dbName = "NoVerdinhoDB"
$databaseFolder = "$PSScriptRoot\database"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " Iniciando configuracao do Banco de Dados: $dbName" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# 1. Criação do Banco de Dados se não existir
$queryCreateDb = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '$dbName') BEGIN CREATE DATABASE [$dbName]; END;"
Write-Host "[1/2] Verificando e criando o banco de dados..." -ForegroundColor Yellow
sqlcmd -S $serverInstance -Q $queryCreateDb

if ($LASTEXITCODE -eq 0) {
    Write-Host " Banco de dados '$dbName' verificado/criado com sucesso!" -ForegroundColor Green
} else {
    Write-Host " Erro ao criar o banco de dados. Verifique se o SQL Server LocalDB está ativo." -ForegroundColor Red
    exit 1
}

# 2. Executando os scripts de objetos avançados
$scriptAvancado = "$databaseFolder\objetos_avancados.sql"
if (Test-Path $scriptAvancado) {
    Write-Host "[2/2] Aplicando Stored Procedures, Views, Functions, Triggers e Índices..." -ForegroundColor Yellow
    sqlcmd -S $serverInstance -d $dbName -i $scriptAvancado
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " Objetos avançados aplicados com sucesso!" -ForegroundColor Green
    } else {
        Write-Host " Aviso: Ocorreu um erro ao aplicar alguns objetos SQL." -ForegroundColor Yellow
    }
} else {
    Write-Host " Arquivo 'objetos_avancados.sql' não encontrado na pasta database." -ForegroundColor Red
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " Configuração do Banco de Dados concluída!" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan