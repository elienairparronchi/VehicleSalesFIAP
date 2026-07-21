# VehicleSalesFIAP

API desenvolvida para o Tech Challenge FIAP/SOAT - Fase 3. O projeto representa uma plataforma de revenda de veiculos, com cadastro de veiculos, listagem por preco, venda online para compradores cadastrados e separacao entre autenticacao de clientes e dados transacionais.

## Status atual

### Parte 1 - Base tecnica

Entregue:

- Solution .NET 10.
- Estrutura inicial em Clean Architecture.
- API ASP.NET Core com endpoint de saude.
- Dockerfile inicial da API.
- Docker Compose com API, SQL Server e Keycloak.
- GitHub Actions com restore, build, testes e build da imagem Docker.
- Configuracao NuGet local usando apenas nuget.org.

### Parte 2 - Dominio, banco e migrations

Entregue:

- Entidade `Vehicle` com regras de cadastro, edicao e venda.
- Entidade `Sale` capturando comprador externo, veiculo, preco e data da compra.
- Value Object `Money` para preco em BRL.
- Enum `VehicleStatus` para `Available` e `Sold`.
- Contratos de persistencia na camada `Application`.
- `VehicleSalesDbContext` no projeto `Infrastructure`.
- Repositorios iniciais de veiculos e vendas.
- Mapeamento EF Core explicito para SQL Server.
- Migration inicial `InitialCreate`.
- Validacao da migration contra SQL Server via Docker Compose.

### Parte 3 - CRUD e listagem de veiculos

Entregue:

- `POST /api/v1/vehicles` para cadastrar veiculo para venda.
- `GET /api/v1/vehicles/{id}` para consultar veiculo por identificador.
- `PUT /api/v1/vehicles/{id}` para editar dados de veiculo disponivel.
- `DELETE /api/v1/vehicles/{id}` para excluir veiculo disponivel.
- `GET /api/v1/vehicles/available` para listar veiculos a venda por preco crescente.
- `GET /api/v1/vehicles/sold` para listar veiculos vendidos por preco crescente.
- Middleware de tratamento de erros com `ProblemDetails`.
- Casos de uso na camada `Application`, mantendo controllers como adaptadores HTTP.
- Testes de dominio e casos de uso.

### Parte 4 - Keycloak e protecao dos endpoints

Entregue:

- Realm Keycloak importado via Docker Compose.
- Client OpenID Connect `vehiclesalesfiap-api`.
- Roles `vehicle-manager` e `buyer`.
- Usuarios locais de exemplo para testes de autenticacao.
- Autenticacao JWT Bearer na API.
- Transformacao de roles do Keycloak para roles ASP.NET Core.
- Policies de autorizacao por perfil.
- Swagger/OpenAPI configurado com Bearer token.
- Endpoints administrativos de veiculos protegidos.
- Testes para conversao de roles do Keycloak.

### Parte 5 - Compra e efetivacao de venda

Entregue:

- `POST /api/v1/vehicles/{id}/purchase` para comprador autenticado efetivar a compra.
- Protecao do endpoint de compra com role `buyer`.
- Uso do `sub` do JWT como identificador externo do comprador.
- Persistencia da venda na tabela `Sales` sem guardar dados cadastrais do cliente.
- Atualizacao do veiculo para status `Sold` no mesmo fluxo transacional.
- Bloqueio de compra de veiculo ja vendido.
- Testes do caso de uso de compra.

### Parte 6 - Testes unitarios, integracao e fluxo fim-a-fim

Entregue:

- Testes unitarios de dominio para regras de veiculo, venda e dinheiro.
- Testes unitarios de casos de uso da camada `Application`.
- Testes de transformacao das roles do Keycloak para roles ASP.NET Core.
- Testes de integracao da API com `WebApplicationFactory`.
- Banco isolado por teste usando EF Core InMemory.
- Autenticacao fake de teste por headers para validar as mesmas policies da API.
- Fluxo fim-a-fim automatizado: gestor cadastra veiculo, comprador compra, veiculo fica vendido e segunda compra e bloqueada.
- GitHub Actions coletando resultado dos testes e cobertura com `XPlat Code Coverage`.

### Parte 7 - Docker Compose completo e refinamentos operacionais

Entregue:

- Docker Compose com API, SQL Server, Keycloak e servico de migrations.
- Healthcheck do SQL Server usando `sqlcmd`.
- Healthcheck do Keycloak validando o realm `vehiclesalesfiap`.
- Healthcheck da API chamando `/health`.
- Endpoint `/health` da API validando tambem o `VehicleSalesDbContext`.
- Aplicacao automatica das migrations EF Core antes da API iniciar.
- Volume de cache NuGet para acelerar execucoes repetidas do servico de migrations.
- Parametrizacao por `.env` para ambiente, nome do banco e credenciais locais.
- Script Linux de migrations com finais de linha protegidos por `.gitattributes`.

### Parte 8 - Evolucao do CI/CD e estrategia de deploy

Entregue:

- Workflow `CI/CD` com validacao separada de build/publicacao de imagem.
- Validacao automatica de formatacao com `dotnet format`.
- Verificacao de pacotes vulneraveis no pipeline.
- Build Docker com cache via Docker Buildx.
- Publicacao da imagem da API no GitHub Container Registry em push para `main`, tags e execucao manual.
- Tags Docker rastreaveis por branch, Pull Request, commit SHA e versao semantica.
- Dependabot para NuGet, GitHub Actions e Docker.
- Documento de estrategia de deploy, rollout e rollback em `docs/deployment-strategy.md`.

### Parte 9 - Documentacao final

Entregue:

- Documento de arquitetura em `docs/architecture.md`.
- Referencia dos endpoints e contratos HTTP em `docs/api-reference.md`.
- Plano de testes automatizados e smoke test manual em `docs/test-plan.md`.
- Checklist de entrega FIAP em `docs/delivery-checklist.md`.
- README atualizado como indice principal da solucao.

### Parte 10 - PDF final e roteiro do video

Entregue:

- Documento fonte da entrega final em `docs/final-submission.md`.
- Roteiro detalhado do video demonstrativo em `docs/video-script.md`.
- PDF final/draft da entrega em `output/pdf/VehicleSalesFIAP-entrega-final.pdf`.
- Gerador do PDF final em `tools/generate-final-pdf.py`.
- PDF preparado com link do repositorio e campo do video marcado para substituicao apos publicacao.

## Documentacao

Use estes documentos como pacote de apoio para avaliacao, manutencao e gravacao do video:

| Documento | Conteudo |
| --- | --- |
| `docs/architecture.md` | Visao da Clean Architecture, dominio, persistencia, seguranca e fluxo de compra |
| `docs/api-reference.md` | Endpoints, payloads, autorizacao, respostas e erros |
| `docs/test-plan.md` | Testes automatizados, comandos de validacao e roteiro de smoke test |
| `docs/deployment-strategy.md` | CI/CD, GHCR, rollout, rollback e criterios de promocao |
| `docs/delivery-checklist.md` | Checklist do enunciado, pendencias manuais e roteiro curto de demonstracao |
| `docs/final-submission.md` | Conteudo fonte do PDF final de entrega |
| `docs/video-script.md` | Roteiro detalhado para gravacao do video |

## Arquitetura

O projeto segue Clean Architecture, conforme os principios estudados na pos:

```text
VehicleSalesFIAP.Api
  -> Adaptador de entrada HTTP, controllers, OpenAPI e configuracao da aplicacao.

VehicleSalesFIAP.Application
  -> Casos de uso, contratos, DTOs e orquestracao das regras de negocio.

VehicleSalesFIAP.Domain
  -> Entidades, value objects, agregados e regras centrais do dominio.

VehicleSalesFIAP.Infrastructure
  -> EF Core, SQL Server, servicos externos e implementacoes tecnicas.

VehicleSalesFIAP.Tests
  -> Testes unitarios e de integracao.
```

Regra principal de dependencia:

- `Domain` nao depende de nenhuma outra camada.
- `Application` depende de `Domain`.
- `Infrastructure` depende de `Application` e `Domain`.
- `Api` depende de `Application` e `Infrastructure`.

## Tecnologias

- .NET 10
- ASP.NET Core
- SQL Server via Docker Compose
- Keycloak via Docker Compose
- Entity Framework Core
- GitHub Actions
- Docker

## Como executar localmente

### Pre-requisitos

- .NET SDK 10
- Docker Desktop
- Git

### Restaurar, compilar e testar

```bash
dotnet restore VehicleSalesFIAP.slnx
dotnet build VehicleSalesFIAP.slnx
dotnet test VehicleSalesFIAP.slnx
```

### Executar somente a API

```bash
dotnet run --project src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj
```

Endpoints iniciais:

- `GET /api/v1/health`
- `GET /health`
- `GET /swagger` em ambiente Development
- `GET /swagger/v1/swagger.json` em ambiente Development

Endpoints de veiculos:

- `POST /api/v1/vehicles` - protegido, role `vehicle-manager`
- `GET /api/v1/vehicles/{id}` - publico
- `PUT /api/v1/vehicles/{id}` - protegido, role `vehicle-manager`
- `DELETE /api/v1/vehicles/{id}` - protegido, role `vehicle-manager`
- `GET /api/v1/vehicles/available` - publico
- `POST /api/v1/vehicles/{id}/purchase` - protegido, role `buyer`
- `GET /api/v1/vehicles/sold` - protegido, role `vehicle-manager`

### Executar com Docker Compose

```bash
docker compose up --build
```

Para executar em segundo plano:

```bash
docker compose up -d --build
```

Servicos expostos:

- API: `http://localhost:5000`
- Keycloak: `http://localhost:8081`
- SQL Server: `localhost,1433`

Servicos internos:

- `migrations`: aplica as migrations EF Core no SQL Server e encerra com sucesso.
- `nuget_cache`: volume usado pelo container de migrations para reaproveitar pacotes NuGet.

Credenciais locais padrao:

- Keycloak admin: `admin`
- Keycloak password: `admin`
- SQL Server user: `sa`
- SQL Server password: `VehicleSalesFIAP@12345`

Para sobrescrever as credenciais locais, copie `.env.example` para `.env` e altere os valores.

### Operacao local com Docker

Verificar o estado dos containers:

```bash
docker compose ps
```

Acompanhar logs da API e das migrations:

```bash
docker compose logs -f api migrations
```

Parar a stack mantendo os volumes:

```bash
docker compose down
```

Recriar tudo do zero, removendo banco, realm Keycloak e cache NuGet locais:

```bash
docker compose down -v
docker compose up --build
```

Usar um banco local alternativo para uma demonstracao limpa:

```powershell
$env:SQLSERVER_DATABASE = "VehicleSalesFIAPDemo"
docker compose up --build
```

O servico `api` depende de `sqlserver` saudavel, `keycloak` saudavel e `migrations` finalizado com sucesso. Isso reduz falhas de inicializacao quando o SQL Server ou o Keycloak ainda estao subindo.

## Autenticacao com Keycloak

O Docker Compose importa automaticamente o realm local a partir de `infra/keycloak/realm-export.json`.

Configuracao local:

- URL Keycloak: `http://localhost:8081`
- Admin console: `http://localhost:8081/admin`
- Admin user: `admin`
- Admin password: `admin`
- Realm: `vehiclesalesfiap`
- Client: `vehiclesalesfiap-api`
- Role administrativa: `vehicle-manager`
- Role de comprador: `buyer`

Usuarios de teste:

- Gestor: `vehicle.manager` / `VehicleManager123!`
- Comprador: `buyer.user` / `Buyer123!`

Obter token de gestor com PowerShell:

```powershell
$tokenResponse = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8081/realms/vehiclesalesfiap/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    grant_type = "password"
    client_id = "vehiclesalesfiap-api"
    username = "vehicle.manager"
    password = "VehicleManager123!"
  }

$token = $tokenResponse.access_token
```

Obter token de comprador com PowerShell:

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

Usar token em endpoint protegido:

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/sold" `
  -Headers @{ Authorization = "Bearer $token" }
```

No Swagger, clique em `Authorize` e informe:

```text
Bearer <seu-token>
```

Se o container do Keycloak ja existir de uma execucao anterior e o realm nao aparecer, recrie os volumes locais:

```bash
docker compose down -v
docker compose up -d keycloak
```

Esse comando remove os dados locais dos containers, entao deve ser usado apenas no ambiente de desenvolvimento.

## Banco de dados e migrations

O banco transacional da API guarda apenas veiculos e vendas. Dados cadastrais de clientes ficam fora deste banco e serao gerenciados pelo Keycloak.

Tabelas iniciais:

- `Vehicles`
  - dados do veiculo
  - preco como `PriceAmount` e `PriceCurrency`
  - status `Available` ou `Sold`
  - `RowVersion` para controle de concorrencia no SQL Server
- `Sales`
  - veiculo vendido
  - `BuyerId` vindo do provedor de identidade
  - preco capturado no momento da compra
  - data da compra

Restaurar ferramentas locais:

```bash
dotnet tool restore
```

Criar uma nova migration:

```bash
dotnet tool run dotnet-ef migrations add NomeDaMigration \
  --project src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj \
  --startup-project src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj \
  --context VehicleSalesDbContext \
  --output-dir Persistence/Migrations
```

Aplicar migrations no SQL Server local:

```bash
docker compose up -d sqlserver

dotnet tool run dotnet-ef database update \
  --project src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj \
  --startup-project src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj \
  --context VehicleSalesDbContext
```

Ao usar `docker compose up --build`, esse passo e executado automaticamente pelo servico `migrations`.

## Exemplos de uso da API

Cadastrar veiculo:

```http
POST /api/v1/vehicles HTTP/1.1
Content-Type: application/json
Authorization: Bearer <token-vehicle-manager>

{
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "color": "Silver",
  "price": 95000
}
```

Editar veiculo:

```http
PUT /api/v1/vehicles/{id} HTTP/1.1
Content-Type: application/json
Authorization: Bearer <token-vehicle-manager>

{
  "brand": "Toyota",
  "model": "Corolla XEi",
  "year": 2022,
  "color": "Black",
  "price": 99000
}
```

Listar veiculos disponiveis:

```http
GET /api/v1/vehicles/available HTTP/1.1
Accept: application/json
```

Comprar veiculo:

```http
POST /api/v1/vehicles/{id}/purchase HTTP/1.1
Authorization: Bearer <token-buyer>
```

Listar veiculos vendidos:

```http
GET /api/v1/vehicles/sold HTTP/1.1
Accept: application/json
Authorization: Bearer <token-vehicle-manager>
```

## Testes automatizados

A suite de testes cobre regras de dominio, casos de uso, seguranca e fluxo HTTP da API.

Executar todos os testes:

```bash
dotnet test VehicleSalesFIAP.slnx --configuration Release
```

Executar somente os testes de integracao/fim-a-fim:

```bash
dotnet test VehicleSalesFIAP.slnx --configuration Release --filter "FullyQualifiedName~Integration"
```

Coletar cobertura local:

```bash
dotnet test VehicleSalesFIAP.slnx \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults
```

Nos testes de integracao, a API e executada em memoria com `WebApplicationFactory`. O SQL Server e substituido por EF Core InMemory e a autenticacao JWT real e substituida por um handler de teste que usa os headers `X-Test-UserId` e `X-Test-Roles`. Isso mantem os testes rapidos e independentes de Docker, enquanto as policies reais da API continuam sendo exercitadas.

## CI/CD

O workflow `.github/workflows/ci.yml` executa em Pull Requests, pushes para `main`, tags `v*.*.*` e execucao manual.

Job `validate`:

- restore
- verificacao de formatacao
- build em Release
- testes com resultado `.trx`
- cobertura com `XPlat Code Coverage`
- verificacao de pacotes vulneraveis
- upload do artefato `test-results`

Job `container`:

- build da imagem Docker da API com Docker Buildx
- cache de camadas Docker no GitHub Actions
- publicacao no GitHub Container Registry quando nao for Pull Request

Imagem publicada:

```text
ghcr.io/<owner>/<repo>
```

Tags principais:

- `main`
- `latest`
- `sha-<commit>`
- `vX.Y.Z`
- `vX.Y`

A estrategia de deploy, promocao e rollback esta documentada em `docs/deployment-strategy.md`.

O fluxo recomendado para o trabalho e GitHub Flow:

1. Criar uma branch curta a partir de `main`.
2. Fazer commits pequenos e rastreaveis.
3. Abrir Pull Request.
4. Aguardar o CI passar.
5. Fazer merge para `main`.

## Proximos passos manuais

- Criar commit inicial.
- Fazer push para o GitHub.
- Abrir Pull Request, caso queira evidenciar o fluxo de PR exigido.
- Aguardar o GitHub Actions executar no repositorio remoto.
- Gravar e publicar o video demonstrativo.
- Substituir o campo pendente do video no PDF final antes do envio oficial.
