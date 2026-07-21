# API Reference

Base local com Docker Compose:

```text
http://localhost:5000
```

Base local com `dotnet run`:

```text
http://localhost:5008
```

Swagger em desenvolvimento:

```text
/swagger
```

## Autenticacao

Use um token Bearer emitido pelo Keycloak local.

Realm:

```text
vehiclesalesfiap
```

Client:

```text
vehiclesalesfiap-api
```

Roles:

| Role | Descricao |
| --- | --- |
| `vehicle-manager` | Gestao de veiculos |
| `buyer` | Compra de veiculos |

Formato do header:

```http
Authorization: Bearer <access-token>
```

## Health

### `GET /api/v1/health`

Endpoint simples da API.

Resposta `200 OK`:

```json
{
  "status": "Healthy",
  "service": "VehicleSalesFIAP.Api",
  "checkedAt": "2026-07-20T21:00:00Z"
}
```

### `GET /health`

Health check tecnico usado pelo Docker Compose. Valida tambem o `VehicleSalesDbContext`.

Resposta esperada:

```text
Healthy
```

## Vehicles

### `POST /api/v1/vehicles`

Cadastra um veiculo para venda.

Autorizacao: `vehicle-manager`.

Request:

```json
{
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "color": "Silver",
  "price": 95000
}
```

Resposta `201 Created`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "color": "Silver",
  "price": 95000,
  "currency": "BRL",
  "status": "Available",
  "createdAt": "2026-07-20T21:00:00Z",
  "updatedAt": null,
  "soldAt": null
}
```

Status possiveis:

- `201 Created`;
- `400 Bad Request`;
- `401 Unauthorized`;
- `403 Forbidden`.

### `GET /api/v1/vehicles/{id}`

Consulta um veiculo por identificador.

Autorizacao: publico.

Status possiveis:

- `200 OK`;
- `404 Not Found`.

### `PUT /api/v1/vehicles/{id}`

Edita os dados de um veiculo disponivel.

Autorizacao: `vehicle-manager`.

Request:

```json
{
  "brand": "Toyota",
  "model": "Corolla XEi",
  "year": 2022,
  "color": "Black",
  "price": 99000
}
```

Status possiveis:

- `200 OK`;
- `400 Bad Request`;
- `401 Unauthorized`;
- `403 Forbidden`;
- `404 Not Found`.

Observacao: veiculos vendidos nao podem ser editados.

### `DELETE /api/v1/vehicles/{id}`

Remove um veiculo disponivel.

Autorizacao: `vehicle-manager`.

Status possiveis:

- `204 No Content`;
- `400 Bad Request`;
- `401 Unauthorized`;
- `403 Forbidden`;
- `404 Not Found`.

Observacao: veiculos vendidos nao podem ser removidos.

### `GET /api/v1/vehicles/available`

Lista veiculos disponiveis, ordenados por preco crescente.

Autorizacao: publico.

Resposta `200 OK`:

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "brand": "Toyota",
    "model": "Corolla",
    "year": 2022,
    "color": "Silver",
    "price": 95000,
    "currency": "BRL",
    "status": "Available",
    "createdAt": "2026-07-20T21:00:00Z",
    "updatedAt": null,
    "soldAt": null
  }
]
```

### `POST /api/v1/vehicles/{id}/purchase`

Efetiva a compra de um veiculo disponivel.

Autorizacao: `buyer`.

O comprador vem do claim `sub` do JWT. A API nao recebe dados cadastrais no corpo da requisicao.

Resposta `201 Created`:

```json
{
  "saleId": "11111111-1111-1111-1111-111111111111",
  "vehicleId": "00000000-0000-0000-0000-000000000000",
  "buyerId": "keycloak-user-sub",
  "purchasePrice": 95000,
  "currency": "BRL",
  "purchasedAt": "2026-07-20T21:10:00Z"
}
```

Status possiveis:

- `201 Created`;
- `400 Bad Request`;
- `401 Unauthorized`;
- `403 Forbidden`;
- `404 Not Found`.

Observacao: uma segunda compra do mesmo veiculo retorna erro de dominio.

### `GET /api/v1/vehicles/sold`

Lista veiculos vendidos, ordenados por preco crescente.

Autorizacao: `vehicle-manager`.

Status possiveis:

- `200 OK`;
- `401 Unauthorized`;
- `403 Forbidden`.

## Erros

Erros de dominio e aplicacao sao retornados como `ProblemDetails`.

Exemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Vehicle is already sold."
}
```

## Arquivo HTTP

O arquivo abaixo contem exemplos prontos para executar no Visual Studio, Rider ou extensoes REST Client:

```text
src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.http
```
