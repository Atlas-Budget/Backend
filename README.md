# Backend

## Visão Geral

Este projeto é a **API backend** do aplicativo de economia doméstica focado em planejamento financeiro futuro.

A API é responsável por:

* Autenticação de usuários
* Persistência de dados financeiros
* Cálculos de previsões mensais
* Fornecer dados prontos para visualização

---

## Stack Tecnológica

* **.NET 8**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server**
* **JWT + Refresh Token**
* **FluentValidation**
* **Swagger / OpenAPI**

---

## Banco de Dados

### Tabelas principais

```txt
Users
Accounts
Transactions
Reserves
```

### Convenções

* PK: `uniqueidentifier`
* Valores monetários: `decimal(18,2)`
* Datas: `datetimeoffset`
* Sem soft delete na V1

---

## Autenticação

### Fluxo

1. Cadastro / Login
2. Retorno de Access Token (JWT)
3. Refresh Token para renovação

### Claims do JWT

```txt
sub (UserId)
email
```

---

## Estrutura do Projeto

```txt
/Api
 ├─ Controllers
 ├─ Dtos
 ├─ Entities
 ├─ Enums
 ├─ Services
 ├─ Auth
 └─ Program.cs
```

---

## Regras de Negócio (V1)

* Cada usuário possui **uma conta financeira**
* Movimentações podem ser:

  * Ganho
  * Gasto
  * Aporte na reserva
* Subtipos:

  * Comum
  * Futuro
  * Constante (mensal)

---

## Cálculos

Todos os cálculos financeiros são feitos no backend:

* Total de ganhos previstos
* Total de gastos previstos
* Saldo final do mês
* Média diária disponível
* Crescimento previsto da reserva

O frontend **não executa lógica financeira**.

---

## Como rodar o projeto

```bash
dotnet restore
dotnet ef database update
dotnet run
```

A API sobe por padrão em:

```
http://localhost:5000
```

Swagger:

```
http://localhost:5000/swagger
```

---

## Deploy (V1)

* Azure App Service
* Azure SQL Database
* Application Insights

---

## Status

🚧 MVP em desenvolvimento (V1)
