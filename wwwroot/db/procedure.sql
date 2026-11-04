-- Passo 1: Criação do Banco e Tabela de Usuários
CREATE DATABASE noverdinho_db;
USE noverdinho_db;

CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    senha VARCHAR(255) NOT NULL, -- Lembre-se de salvar o HASH no C#
    data_cadastro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Passo 2: Perfil Financeiro (A "Fotografia" do Cadastro)
CREATE TABLE perfil_financeiro (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    cpf VARCHAR(11),
    cnpj VARCHAR(14),
    tipo_regime ENUM('CLT', 'PJ_MEI', 'AMBOS') NOT NULL,
    salario_bruto DECIMAL(10,2) NOT NULL,
    perc_inss DECIMAL(5,2),
    perc_irrf DECIMAL(5,2),
    perc_outros DECIMAL(5,2),
    valor_liquido DECIMAL(10,2),
    data_calculo TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- Situação 1: Gastos Fixos
CREATE TABLE gastos_fixos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    descricao VARCHAR(100) NOT NULL,
    valor_estimado DECIMAL(10,2) NOT NULL,
    dia_vencimento INT NOT NULL,
    categoria ENUM('Saude', 'Lazer', 'Telecom', 'Educacao', 'Habitacao', 'Outros') DEFAULT 'Outros',
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- Situação 2: Gastos Variáveis
CREATE TABLE gastos_variaveis (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    descricao VARCHAR(100) NOT NULL, -- Ex: Conta de Luz
    valor_real DECIMAL(10,2) NOT NULL,
    mes_referencia INT NOT NULL,
    ano_referencia INT NOT NULL,
    data_pagamento DATE,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- Situação 3: Lista de Mercados
CREATE TABLE mercados (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    bairro VARCHAR(50)
);

-- Situação 4: Itens e Comparativo de Preços
CREATE TABLE compras_itens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    mercado_id INT NOT NULL,
    nome_produto VARCHAR(100) NOT NULL,
    valor_unitario DECIMAL(10,2) NOT NULL,
    quantidade DECIMAL(10,2) DEFAULT 1,
    data_compra DATE NOT NULL,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE,
    FOREIGN KEY (mercado_id) REFERENCES mercados(id)
);