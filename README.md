# VehicleSalesFIAP

API para uma plataforma de revenda de veiculos desenvolvida para o Tech Challenge FIAP/SOAT - Fase 3.

O sistema permite cadastrar veiculos para venda, editar veiculos disponiveis, listar veiculos por preco, efetivar compras por compradores autenticados e consultar veiculos vendidos. A autenticacao dos compradores e feita em um servico separado usando Keycloak, mantendo dados cadastrais fora do banco transacional da API.

Repositorio: https://github.com/elienairparronchi/VehicleSalesFIAP

## Requisitos Atendidos

| Requisito | Implementacao |
| --- | --- |
| Cadastrar veiculo para venda | `POST /api/v1/vehicles` |
| Editar dados do veiculo | `PUT /api/v1/vehicles/{id}` |
| Permitir compra por pessoa cadastrada | Cadastro no Keycloak e `POST /api/v1/vehicles/{id}/purchase` protegido por JWT |
| Listar veiculos a venda por preco crescente | `GET /api/v1/vehicles/available` |
| Listar veiculos vendidos por preco crescente | `GET /api/v1/vehicles/sold` |
| Separar cadastro/autorizacao de compradores dos dados transacionais | Keycloak separado da API e do SQL Server |
| Usar CI/CD | GitHub Actions com build, testes, validacoes e imagem Docker |

## Tecnologias

- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Keycloak
- Docker e Docker Compose
- GitHub Actions
- xUnit
- Swagger/OpenAPI

## Arquitetura

O projeto segue Clean Architecture:

```text
src/
  VehicleSalesFIAP.Domain
  VehicleSalesFIAP.Application
  VehicleSalesFIAP.Infrastructure
  VehicleSalesFIAP.Api

tests/
  VehicleSalesFIAP.Tests
```

Responsabilidades:

- `Domain`: entidades, value objects e regras centrais do negocio.
- `Application`: casos de uso, DTOs e contratos de persistencia.
- `Infrastructure`: Entity Framework Core, SQL Server, repositories e migrations.
- `Api`: controllers, autenticacao, autorizacao, Swagger e middleware.
- `Tests`: testes unitarios, integracao HTTP e fluxo fim-a-fim com a stack Docker real.

Regra de dependencia:

```text
Domain
Application -> Domain
Infrastructure -> Application, Domain
Api -> Application, Infrastructure
```

## Modelo De Dados

O banco transacional guarda veiculos e vendas.

Tabela `Vehicles`:

- marca;
- modelo;
- ano;
- cor;
- preco;
- moeda;
- status `Available` ou `Sold`;
- data de criacao;
- data de atualizacao;
- data de venda;
- controle de concorrencia.

Tabela `Sales`:

- veiculo vendido;
- identificador externo do comprador;
- preco capturado no momento da compra;
- moeda;
- data da compra.

Os dados cadastrais completos dos compradores ficam no Keycloak. A API armazena apenas o identificador externo recebido no claim `sub` do token JWT.

## Regras De Negocio

- Veiculo cadastrado entra com status `Available`.
- Marca, modelo e cor sao obrigatorios.
- Ano deve estar entre 1886 e o proximo ano calendario.
- Preco deve ser maior que zero.
- Veiculo vendido nao pode ser editado.
- Veiculo vendido nao pode ser removido.
- Veiculo vendido nao pode ser comprado novamente.
- A venda captura o preco vigente no momento da compra.

## Como Executar Localmente

### Pre-requisitos

- .NET SDK 10
- Docker Desktop
- Git

### Restaurar, Compilar E Testar

```bash
dotnet restore VehicleSalesFIAP.slnx
dotnet build VehicleSalesFIAP.slnx
dotnet test VehicleSalesFIAP.slnx
```

### Executar Com Docker Compose

```bash
docker compose up -d --build
```

Servicos expostos:

| Servico | URL |
| --- | --- |
| API | `http://localhost:5000` |
| Swagger | `http://localhost:5000/swagger` |
| Health check | `http://localhost:5000/health` |
| Keycloak | `http://localhost:8081` |
| SQL Server | `localhost,1433` |

Verificar containers:

```bash
docker compose ps
```

Parar containers:

```bash
docker compose down
```

Recriar ambiente local do zero:

```bash
docker compose down -v
docker compose up -d --build
```

## Configuracao Local

O Docker Compose sobe quatro servicos:

- `api`: API ASP.NET Core.
- `sqlserver`: banco transacional.
- `keycloak`: provedor de identidade.
- `migrations`: aplica as migrations do EF Core antes da API iniciar.

As imagens de .NET, SQL Server e Keycloak usam versoes fixas no `compose.yml` e nos `Dockerfile`. O servico de migrations executa um EF Core bundle isolado e nao grava artefatos de build no diretorio do host.

Credenciais locais padrao:

| Recurso | Usuario | Senha |
| --- | --- | --- |
| Keycloak admin | `admin` | `admin` |
| SQL Server | `sa` | `VehicleSalesFIAP@12345` |

Variaveis de ambiente podem ser sobrescritas criando um arquivo `.env` a partir de `.env.example`.

Essas credenciais existem apenas para desenvolvimento e demonstracao local. Em outro ambiente, defina senhas proprias por secrets ou variaveis protegidas.

Em producao, configure tambem `ConnectionStrings__VehicleSales`, `Authentication__Authority` com uma URL HTTPS, `Authentication__Audience` e os emissores permitidos em `Authentication__ValidIssuers`. A aplicacao falha na inicializacao se a autoridade nao for fornecida.

### Seguranca Das Credenciais

- O repositorio nao deve conter arquivos `.env`, chaves privadas, tokens ou senhas reais.
- `.env`, suas variantes e formatos comuns de chave privada sao ignorados pelo Git.
- Os valores presentes em `.env.example`, no realm do Keycloak e nos exemplos deste README sao publicos e exclusivos da demonstracao local.
- O Docker Compose local nao deve ser exposto diretamente na internet com essas credenciais.
- O CI verifica todo o historico versionado com Gitleaks e bloqueia o pipeline se detectar um segredo.
- O `GITHUB_TOKEN` usado para publicar no GHCR e temporario, limitado ao job e fornecido automaticamente pelo GitHub Actions.

## Autenticacao

O Keycloak e importado automaticamente a partir de `infra/keycloak/realm-export.json`.

Configuracao local:

- Realm: `vehiclesalesfiap`
- Client: `vehiclesalesfiap-api`
- Role de gestor: `vehicle-manager`
- Role de comprador: `buyer`
- Grupo padrao de novos usuarios: `buyers`

### Cadastrar Um Novo Comprador

O cadastro permanece totalmente no Keycloak. Para testar o autoatendimento:

1. Acesse `http://localhost:8081/realms/vehiclesalesfiap/account`.
2. Na tela de login, selecione `Register`.
3. Informe os dados do novo usuario e uma senha valida.
4. O Keycloak inclui o usuario no grupo padrao `buyers`, que concede a role `buyer`.

Se o realm ja existia antes dessa configuracao, recrie somente o ambiente local com `docker compose down -v` e `docker compose up -d --build` para que o arquivo de importacao seja aplicado novamente. O primeiro comando remove os dados locais dos containers.

Usuarios de teste:

| Perfil | Usuario | Senha |
| --- | --- | --- |
| Gestor | `vehicle.manager` | `VehicleManager123!` |
| Comprador | `buyer.user` | `Buyer123!` |

Obter token de gestor:

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

$managerToken = $tokenResponse.access_token
```

Obter token de comprador:

```powershell
$tokenResponse = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8081/realms/vehiclesalesfiap/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    grant_type = "password"
    client_id = "vehiclesalesfiap-api"
    username = "buyer.user"
    password = "Buyer123!"
  }

$buyerToken = $tokenResponse.access_token
```

Usar token no Swagger:

```text
Bearer <access-token>
```

O grant de senha usado nos exemplos esta habilitado para facilitar a demonstracao local. Uma aplicacao web real deve usar Authorization Code com PKCE.

## Endpoints

| Metodo | Rota | Acesso |
| --- | --- | --- |
| `GET` | `/api/v1/health` | Publico |
| `GET` | `/health` | Publico |
| `POST` | `/api/v1/vehicles` | `vehicle-manager` |
| `GET` | `/api/v1/vehicles/{id}` | Publico |
| `PUT` | `/api/v1/vehicles/{id}` | `vehicle-manager` |
| `DELETE` | `/api/v1/vehicles/{id}` | `vehicle-manager` |
| `GET` | `/api/v1/vehicles/available` | Publico |
| `POST` | `/api/v1/vehicles/{id}/purchase` | `buyer` |
| `GET` | `/api/v1/vehicles/sold` | `vehicle-manager` |

## Exemplos De Uso

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

## Banco De Dados E Migrations

Restaurar ferramentas locais:

```bash
dotnet tool restore
```

Criar migration:

```bash
dotnet tool run dotnet-ef migrations add NomeDaMigration \
  --project src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj \
  --startup-project src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj \
  --context VehicleSalesDbContext \
  --output-dir Persistence/Migrations
```

Aplicar migrations manualmente:

```bash
dotnet tool run dotnet-ef database update \
  --project src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj \
  --startup-project src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj \
  --context VehicleSalesDbContext
```

Ao executar com Docker Compose, as migrations sao aplicadas automaticamente pelo servico `migrations`.

## Testes

Executar todos os testes:

```bash
dotnet test VehicleSalesFIAP.slnx --configuration Release
```

Executar testes de integracao:

```bash
dotnet test VehicleSalesFIAP.slnx --configuration Release --filter "FullyQualifiedName~Integration"
```

Executar o smoke test completo com a stack Docker ja iniciada:

```powershell
.\tests\e2e\docker-compose-smoke.ps1
```

Esse teste cria um comprador temporario no Keycloak, valida as roles reais, cadastra veiculos na API com SQL Server, verifica as duas listas em ordem crescente, efetiva compras e bloqueia uma segunda compra.

Coletar cobertura:

```bash
dotnet test VehicleSalesFIAP.slnx \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults
```

A suite cobre:

- regras de dominio;
- casos de uso;
- autorizacao por roles;
- transformacao de roles do Keycloak;
- limites de dependencia da Clean Architecture;
- fluxo HTTP de cadastro, compra e venda com autenticacao simulada;
- fluxo fim-a-fim com Keycloak, SQL Server, migrations e API em Docker Compose.

## CI/CD

O workflow `.github/workflows/ci.yml` executa em Pull Requests, pushes para `main`, tags `v*.*.*` e execucao manual.

Validacoes do pipeline:

- varredura de segredos no historico com Gitleaks;
- restore;
- verificacao de formatacao;
- build em Release;
- testes automatizados;
- coleta de cobertura;
- verificacao bloqueante de pacotes vulneraveis;
- teste fim-a-fim da stack Docker com cadastro de comprador no Keycloak;
- build da imagem Docker;
- publicacao da imagem no GitHub Container Registry quando aplicavel.

O projeto tambem possui Dependabot para atualizacoes de NuGet, SDK .NET, GitHub Actions, Dockerfiles e Docker Compose.

## Fluxo De Pull Request

As alteracoes devem sair de uma branch de trabalho e entrar em `main` por Pull Request. O template em `.github/pull_request_template.md` registra validacoes, riscos e impacto da mudanca. A imagem do GHCR so e publicada depois que os testes unitarios, de integracao e o fluxo Docker fim-a-fim passam.
