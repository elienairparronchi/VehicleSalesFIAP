# Test Plan

Este documento descreve como validar a API VehicleSalesFIAP localmente, no CI e durante a demonstracao final.

## Estrategia

A suite foi organizada para cobrir os niveis principais do trabalho:

| Nivel | Objetivo | Tecnologia |
| --- | --- | --- |
| Unitario de dominio | Validar regras centrais sem infraestrutura | xUnit |
| Unitario de aplicacao | Validar casos de uso com repositorios em memoria | xUnit |
| Seguranca | Validar transformacao de roles do Keycloak | xUnit |
| Integracao HTTP | Validar controllers, middleware e policies | `WebApplicationFactory` |
| Fim-a-fim local | Validar API, SQL Server, Keycloak e migrations em Docker Compose | Docker Compose + chamadas HTTP |

## Comandos De Validacao

Restaurar dependencias:

```bash
dotnet restore VehicleSalesFIAP.slnx
```

Compilar:

```bash
dotnet build VehicleSalesFIAP.slnx --configuration Release
```

Executar testes:

```bash
dotnet test VehicleSalesFIAP.slnx --configuration Release
```

Verificar formatacao:

```bash
dotnet format VehicleSalesFIAP.slnx --verify-no-changes --verbosity minimal
```

Verificar pacotes vulneraveis:

```bash
dotnet list VehicleSalesFIAP.slnx package --vulnerable --include-transitive
```

Validar Docker Compose:

```bash
docker compose config
docker compose build api
```

Subir a stack completa:

```bash
docker compose up -d --build
docker compose ps
```

Validar health:

```bash
curl http://localhost:5000/health
```

Encerrar a stack:

```bash
docker compose down
```

## Cenarios Automatizados

Os testes automatizados cobrem:

- criacao de `Money` somente com valor positivo e moeda valida;
- criacao de `Vehicle` com dados obrigatorios;
- bloqueio de ano invalido;
- atualizacao apenas de veiculos disponiveis;
- venda alterando status para `Sold`;
- bloqueio de segunda venda;
- bloqueio de remocao de veiculo vendido;
- cadastro de veiculo via caso de uso;
- listagem de disponiveis por preco crescente;
- listagem de vendidos por preco crescente;
- compra de veiculo e persistencia da venda;
- transformacao de roles do Keycloak para roles da API;
- endpoint de vendidos exigindo role `vehicle-manager`;
- endpoint de compra exigindo role `buyer`;
- fluxo HTTP fim-a-fim com cadastro, listagem, compra e listagem de vendidos.

## Smoke Test Manual

Use este roteiro para a gravacao e para validacao local com Docker.

### 1. Subir infraestrutura

```bash
docker compose up -d --build
docker compose ps
```

Esperado:

- `vehiclesalesfiap-sqlserver` healthy;
- `vehiclesalesfiap-keycloak` healthy;
- `vehiclesalesfiap-migrations` exited com codigo 0;
- `vehiclesalesfiap-api` healthy.

### 2. Conferir health da API

```bash
curl http://localhost:5000/health
```

Esperado:

```text
Healthy
```

### 3. Obter token de gestor

```powershell
$managerTokenResponse = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8081/realms/vehiclesalesfiap/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    grant_type = "password"
    client_id = "vehiclesalesfiap-api"
    username = "vehicle.manager"
    password = "VehicleManager123!"
  }

$managerToken = $managerTokenResponse.access_token
```

### 4. Obter token de comprador

```powershell
$buyerTokenResponse = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8081/realms/vehiclesalesfiap/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    grant_type = "password"
    client_id = "vehiclesalesfiap-api"
    username = "buyer.user"
    password = "Buyer123!"
  }

$buyerToken = $buyerTokenResponse.access_token
```

### 5. Cadastrar veiculo

```powershell
$vehicle = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/v1/vehicles" `
  -ContentType "application/json" `
  -Headers @{ Authorization = "Bearer $managerToken" } `
  -Body (@{
    brand = "Toyota"
    model = "Corolla"
    year = 2022
    color = "Silver"
    price = 95000
  } | ConvertTo-Json)

$vehicle
```

Esperado:

- HTTP `201 Created`;
- `status` igual a `Available`.

### 6. Listar disponiveis

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/available"
```

Esperado:

- veiculo criado aparece na lista;
- lista ordenada por `price` crescente.

### 7. Comprar veiculo

```powershell
$sale = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)/purchase" `
  -Headers @{ Authorization = "Bearer $buyerToken" }

$sale
```

Esperado:

- HTTP `201 Created`;
- retorno contem `saleId`, `vehicleId`, `buyerId`, `purchasePrice`, `currency` e `purchasedAt`;
- `buyerId` vem do Keycloak.

### 8. Confirmar veiculo vendido

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)"
```

Esperado:

- `status` igual a `Sold`;
- `soldAt` preenchido.

### 9. Listar vendidos

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/sold" `
  -Headers @{ Authorization = "Bearer $managerToken" }
```

Esperado:

- veiculo vendido aparece na lista;
- lista ordenada por `price` crescente.

### 10. Validar bloqueio de segunda compra

```powershell
Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)/purchase" `
  -Headers @{ Authorization = "Bearer $buyerToken" }
```

Esperado:

- erro `400 Bad Request`;
- mensagem indicando que o veiculo ja foi vendido.

## Evidencias Para Entrega

Guarde prints ou trechos de terminal com:

- `docker compose ps`;
- `/health` retornando `Healthy`;
- Keycloak com realm `vehiclesalesfiap`;
- Swagger com endpoints;
- cadastro de veiculo retornando `201`;
- compra retornando `201`;
- vendido aparecendo em `/api/v1/vehicles/sold`;
- GitHub Actions com workflow verde;
- pacote publicado no GHCR, se o push para `main` ja tiver sido realizado.
