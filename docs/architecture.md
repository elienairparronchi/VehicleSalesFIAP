# Architecture

Este documento descreve as decisoes tecnicas da API VehicleSalesFIAP para o Tech Challenge FIAP/SOAT - Fase 3.

## Objetivo Da Solucao

A API atende uma plataforma de revenda de veiculos. O frontend ainda sera desenvolvido por outro time, entao esta entrega concentra o backend, os contratos HTTP, a persistencia transacional e a integracao com um provedor de identidade separado.

Requisitos de negocio cobertos:

- cadastrar veiculo para venda com marca, modelo, ano, cor e preco;
- editar dados de veiculo disponivel;
- listar veiculos disponiveis por preco crescente;
- permitir compra por comprador cadastrado;
- listar veiculos vendidos por preco crescente;
- manter cadastro/autenticacao de clientes separado dos dados de venda.

## Visao Geral

```mermaid
flowchart LR
    Client["Cliente HTTP / Swagger / Frontend futuro"]
    Api["VehicleSalesFIAP.Api"]
    App["VehicleSalesFIAP.Application"]
    Domain["VehicleSalesFIAP.Domain"]
    Infra["VehicleSalesFIAP.Infrastructure"]
    Sql["SQL Server"]
    Keycloak["Keycloak"]

    Client --> Api
    Api --> App
    App --> Domain
    Infra --> App
    Infra --> Domain
    Api --> Infra
    Infra --> Sql
    Client --> Keycloak
    Api --> Keycloak
```

O Keycloak e o responsavel pelo cadastro, login e emissao de tokens dos compradores. A API recebe apenas o JWT e persiste o identificador externo do comprador (`sub`) na venda.

## Clean Architecture

O projeto foi separado em quatro camadas principais.

| Projeto | Responsabilidade | Exemplos |
| --- | --- | --- |
| `VehicleSalesFIAP.Domain` | Regras centrais do dominio, entidades e value objects | `Vehicle`, `Sale`, `Money`, `VehicleStatus` |
| `VehicleSalesFIAP.Application` | Casos de uso, DTOs e contratos de persistencia | `RegisterVehicleForSaleUseCase`, `PurchaseVehicleUseCase`, `IVehicleRepository` |
| `VehicleSalesFIAP.Infrastructure` | Implementacoes tecnicas e acesso a dados | `VehicleSalesDbContext`, repositories, EF Core mappings |
| `VehicleSalesFIAP.Api` | Adaptador HTTP, autenticacao, autorizacao, Swagger e middleware | `VehiclesController`, policies, `ProblemDetails` |

Dependencias permitidas:

```text
Domain
Application -> Domain
Infrastructure -> Application, Domain
Api -> Application, Infrastructure
Tests -> Api, Application, Domain, Infrastructure
```

O dominio nao depende de ASP.NET Core, Entity Framework, SQL Server, Keycloak ou qualquer detalhe externo. Isso preserva as regras de negocio e facilita testes unitarios.

## Modelo De Dominio

```mermaid
classDiagram
    class Vehicle {
        +Guid Id
        +string Brand
        +string Model
        +int Year
        +string Color
        +Money Price
        +VehicleStatus Status
        +DateTimeOffset CreatedAt
        +DateTimeOffset? UpdatedAt
        +DateTimeOffset? SoldAt
        +RegisterForSale()
        +UpdateDetails()
        +SellTo()
        +EnsureCanBeDeleted()
    }

    class Sale {
        +Guid Id
        +Guid VehicleId
        +string BuyerId
        +Money PurchasePrice
        +DateTimeOffset PurchasedAt
    }

    class Money {
        +decimal Amount
        +string Currency
    }

    Vehicle --> Money
    Sale --> Money
    Vehicle --> Sale
```

Regras centrais:

- `Vehicle` nasce sempre como `Available`.
- marca, modelo e cor sao obrigatorios e normalizados.
- ano deve estar entre 1886 e o proximo ano calendario.
- preco deve ser maior que zero e e arredondado para duas casas.
- veiculo vendido nao pode ser editado, removido ou vendido novamente.
- `Sale` captura o preco no momento da compra para preservar historico.

## Persistencia

O SQL Server guarda apenas dados transacionais:

- `Vehicles`: dados do veiculo, preco, moeda, status e datas.
- `Sales`: venda efetivada, veiculo, comprador externo, preco capturado e data.

O cadastro completo de compradores nao fica no banco da API. Essa separacao atende a exigencia de manter identidade/cadastro apartados da operacao transacional.

O EF Core usa migrations versionadas em:

```text
src/VehicleSalesFIAP.Infrastructure/Persistence/Migrations
```

O Docker Compose possui um servico `migrations`, que aplica as migrations antes da API iniciar.

## Autenticacao E Autorizacao

```mermaid
sequenceDiagram
    participant Buyer as Comprador
    participant Keycloak as Keycloak
    participant Api as API
    participant Sql as SQL Server

    Buyer->>Keycloak: login
    Keycloak-->>Buyer: access token JWT
    Buyer->>Api: POST /api/v1/vehicles/{id}/purchase
    Api->>Api: valida assinatura, issuer, audience e role buyer
    Api->>Sql: registra Sale e altera Vehicle para Sold
    Api-->>Buyer: 201 Created
```

Roles usadas:

| Role | Uso |
| --- | --- |
| `vehicle-manager` | cadastrar, editar, excluir e listar vendidos |
| `buyer` | comprar veiculo |

Endpoints publicos:

- `GET /api/v1/vehicles/{id}`;
- `GET /api/v1/vehicles/available`;
- `GET /api/v1/health`;
- `GET /health`.

## Fluxo De Compra

```mermaid
sequenceDiagram
    participant Manager as Gestor
    participant Buyer as Comprador
    participant Api as API
    participant Db as SQL Server

    Manager->>Api: POST /api/v1/vehicles
    Api->>Db: cria Vehicle Available
    Api-->>Manager: 201 Created
    Buyer->>Api: GET /api/v1/vehicles/available
    Api-->>Buyer: lista ordenada por preco
    Buyer->>Api: POST /api/v1/vehicles/{id}/purchase
    Api->>Db: cria Sale e marca Vehicle como Sold
    Api-->>Buyer: 201 Created
    Manager->>Api: GET /api/v1/vehicles/sold
    Api-->>Manager: lista vendidos por preco
```

## Observabilidade Basica

A API expoe dois endpoints de saude:

- `/api/v1/health`: endpoint simples de contrato da API.
- `/health`: health check tecnico do ASP.NET Core, incluindo verificacao do `VehicleSalesDbContext`.

No Docker Compose, os healthchecks coordenam a subida de SQL Server, Keycloak, migrations e API.

## Alinhamento Com O Conteudo Da Pos

A solucao foi documentada considerando os pontos recorrentes do material consolidado da pos:

- Clean Architecture e SOLID para separar dominio, casos de uso, adaptadores e detalhes externos.
- OAuth 2.0/JWT para desacoplar autenticacao do servico transacional.
- Docker Compose para orquestrar dependencias locais.
- CI/CD com Pull Requests, testes automatizados, analise de formatacao e build de imagem.
- Testes unitarios, integracao e fluxo fim-a-fim para proteger as regras principais.
