# 💸 NoVerdinho - Saúde Financeira Integrada

> Transforme sua relação com o dinheiro: do controle de centavos no mercado ao cálculo real do seu salário líquido.

**NoVerdinho** é uma solução completa de gestão financeira pessoal desenvolvida para o **PIM III (UNIP)**. O foco do projeto é ir além de uma simples planilha de gastos, oferecendo ferramentas inteligentes para entender descontos em folha de pagamento e otimizar o custo da cesta básica através de histórico de preços.

---

## 🚀 Diferenciais do Projeto

* **Calculadora CLT/PJ Inteligente:** Entenda exatamente para onde vai seu dinheiro antes mesmo dele cair na conta (INSS, IRRF, FGTS).
* **Gestão de Compras de Mercado:** Cadastro de itens com histórico de preços por estabelecimento para identificar onde sua compra rende mais.
* **Previsibilidade com IA:** Utiliza **ML.NET** para prever o fechamento do mês com base no comportamento de gastos anteriores.
* **Acessibilidade Nativa:** Interface responsiva com suporte a componentes de tradução para **LIBRAS**.

---

## 🛠️ Stack Tecnológica

* **Backend:** .NET 8 (C#) com Arquitetura em Camadas.
* **Frontend:** ASP.NET Core MVC + Bootstrap 5 + JavaScript.
* **Banco de Dados:** * **Relacional:** SQL Server (Transações, Usuários e Orçamentos).
    * **NoSQL (Conceitual):** MongoDB para logs de variação de preços de produtos.
* **IA/Dados:** ML.NET para análise preditiva de gastos.
* **Versionamento:** Git & GitHub.

---

## 🏗️ Arquitetura e Boas Práticas (POO)

O projeto foi construído seguindo os princípios de **Clean Code** e **SOLID**:

- **Encapsulamento:** Regras de cálculo de impostos protegidas em serviços de domínio.
- **Herança e Polimorfismo:** Implementação de diferentes tipos de lançamentos (Fixos, Variáveis, Parcelados) herdando de uma estrutura base.
- **Injeção de Dependência:** Desacoplamento de serviços para facilitar testes unitários.

---

## 📱 Interface Responsiva (UX/UI)

O design foi pensado na **Jornada do Usuário "No Mercado"**, onde a inserção de dados precisa ser rápida e adaptada para dispositivos móveis, garantindo usabilidade mesmo em telas pequenas.

---
