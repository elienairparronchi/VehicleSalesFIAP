# Roteiro Do Video Demonstrativo

Roteiro sugerido para gravar a demonstracao final do Tech Challenge FIAP/SOAT - Fase 3.

Duracao sugerida: 8 a 12 minutos.

## Preparacao Antes De Gravar

1. Confirmar que o projeto esta atualizado localmente.
2. Executar `docker compose down -v` se quiser uma demonstracao totalmente limpa.
3. Deixar um terminal aberto na pasta do projeto.
4. Deixar o navegador pronto para GitHub, GitHub Actions, Keycloak e Swagger.
5. Se o video for publicado antes da entrega, atualizar o link em `docs/final-submission.md` e regenerar o PDF.

Comandos de preparacao:

```bash
dotnet build VehicleSalesFIAP.slnx --configuration Release
dotnet test VehicleSalesFIAP.slnx --configuration Release
docker compose down -v
docker compose up -d --build
docker compose ps
```

## 1. Abertura

Tempo sugerido: 0:00 a 0:40.

Fala sugerida:

```text
Ola, este video apresenta a solucao VehicleSalesFIAP, desenvolvida para o Tech Challenge FIAP/SOAT - Fase 3. A entrega e uma API para revenda de veiculos, usando .NET 10, Clean Architecture, SQL Server, Keycloak, Docker Compose e GitHub Actions.
```

Mostrar na tela:

- repositorio GitHub;
- arquivo `README.md`;
- pastas `src`, `tests`, `infra`, `docs` e `.github`.

## 2. Arquitetura E Organizacao Do Codigo

Tempo sugerido: 0:40 a 1:40.

Fala sugerida:

```text
A solucao foi organizada com Clean Architecture. O dominio concentra as regras centrais, a camada de aplicacao contem os casos de uso, a infraestrutura implementa EF Core e SQL Server, e a API expoe os endpoints HTTP, autenticacao, autorizacao e Swagger.
```

Mostrar na tela:

- `src/VehicleSalesFIAP.Domain`;
- `src/VehicleSalesFIAP.Application`;
- `src/VehicleSalesFIAP.Infrastructure`;
- `src/VehicleSalesFIAP.Api`;
- `tests/VehicleSalesFIAP.Tests`.

Pontos para destacar:

- `Vehicle` controla cadastro, edicao e venda.
- `Sale` registra a venda.
- `Money` valida preco.
- A API nao armazena dados cadastrais completos do comprador.

## 3. CI/CD E Deploy Automatizado

Tempo sugerido: 1:40 a 2:40.

Fala sugerida:

```text
O projeto possui pipeline de CI/CD no GitHub Actions. Em Pull Requests e pushes para a main, o workflow restaura dependencias, valida formatacao, compila, executa testes, verifica pacotes vulneraveis e builda a imagem Docker. Fora de Pull Request, a imagem e publicada no GitHub Container Registry.
```

Mostrar na tela:

- `.github/workflows/ci.yml`;
- aba Actions do GitHub, se ja houver execucao;
- `docs/deployment-strategy.md`.

## 4. Infraestrutura Local Com Docker Compose

Tempo sugerido: 2:40 a 3:40.

Fala sugerida:

```text
A infraestrutura local sobe com Docker Compose. Ela contem SQL Server, Keycloak, a API e um servico de migrations que aplica o schema do Entity Framework antes da API iniciar.
```

Executar ou mostrar:

```bash
docker compose up -d --build
docker compose ps
```

Esperado:

- SQL Server healthy;
- Keycloak healthy;
- migrations finalizado com sucesso;
- API healthy.

Validar:

```bash
curl http://localhost:5000/health
```

## 5. Cadastro Do Comprador No Keycloak

Tempo sugerido: 3:40 a 4:50.

Fala sugerida:

```text
O cadastro e a autorizacao de compradores ficam separados da API transacional, conforme solicitado no enunciado. Nesta solucao, o servico responsavel por identidade e o Keycloak. O comprador de teste ja e importado junto com o realm local para facilitar a demonstracao.
```

Mostrar na tela:

- `http://localhost:8081/admin`;
- login `admin` / `admin`;
- realm `vehiclesalesfiap`;
- client `vehiclesalesfiap-api`;
- roles `buyer` e `vehicle-manager`;
- usuario `buyer.user`.

Opcional para reforcar o cadastro:

```text
Criar um novo usuario manualmente no Keycloak, definir senha e atribuir a role buyer. Depois usar esse usuario para obter token e comprar o veiculo.
```

## 6. Swagger E Tokens

Tempo sugerido: 4:50 a 5:50.

Abrir:

```text
http://localhost:5000/swagger
```

Obter token de gestor:

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

Obter token de comprador:

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

Fala sugerida:

```text
Os endpoints protegidos recebem Bearer Token. O gestor pode cadastrar, editar, remover e listar vendidos. O comprador pode efetivar compra.
```

## 7. Cadastro De Veiculo

Tempo sugerido: 5:50 a 6:50.

Executar:

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

Fala sugerida:

```text
Aqui o gestor cadastra um veiculo para venda. A resposta retorna 201 Created e o veiculo nasce com status Available.
```

## 8. Listagem De Disponiveis

Tempo sugerido: 6:50 a 7:30.

Executar:

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/available"
```

Fala sugerida:

```text
Este endpoint e publico e lista os veiculos disponiveis ordenados por preco crescente, do mais barato para o mais caro.
```

## 9. Compra E Efetivacao

Tempo sugerido: 7:30 a 8:50.

Executar:

```powershell
$sale = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)/purchase" `
  -Headers @{ Authorization = "Bearer $buyerToken" }

$sale
```

Fala sugerida:

```text
Agora o comprador autenticado efetiva a compra. A API usa o claim sub do JWT como identificador externo do comprador, registra a venda e altera o veiculo para Sold no mesmo fluxo transacional.
```

Confirmar veiculo vendido:

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)"
```

## 10. Listagem De Vendidos

Tempo sugerido: 8:50 a 9:40.

Executar:

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/v1/vehicles/sold" `
  -Headers @{ Authorization = "Bearer $managerToken" }
```

Fala sugerida:

```text
O gestor consegue consultar os veiculos vendidos, tambem ordenados por preco crescente.
```

## 11. Regras De Seguranca E Negocio

Tempo sugerido: 9:40 a 10:40.

Tentar comprar novamente:

```powershell
Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)/purchase" `
  -Headers @{ Authorization = "Bearer $buyerToken" }
```

Fala sugerida:

```text
A segunda compra e bloqueada porque o veiculo ja foi vendido. Isso mostra a regra de dominio protegendo a consistencia da venda.
```

Opcional: mostrar que gestor nao compra:

```powershell
Invoke-WebRequest -Method Post `
  -Uri "http://localhost:5000/api/v1/vehicles/$($vehicle.id)/purchase" `
  -Headers @{ Authorization = "Bearer $managerToken" }
```

Esperado:

```text
403 Forbidden
```

## 12. Testes Automatizados

Tempo sugerido: 10:40 a 11:30.

Executar ou mostrar resultado:

```bash
dotnet test VehicleSalesFIAP.slnx --configuration Release
```

Fala sugerida:

```text
A suite automatizada cobre dominio, casos de uso, seguranca e fluxo HTTP fim-a-fim. O teste principal cadastra veiculo, compra, confirma a venda, lista vendidos e valida o bloqueio de segunda compra.
```

## 13. Encerramento

Tempo sugerido: 11:30 a 12:00.

Fala sugerida:

```text
Com isso, a entrega cobre os requisitos do enunciado: API funcional, comprador autenticado em servico separado, banco transacional com vendas, listagens por preco, Docker Compose, testes automatizados, CI/CD e documentacao para execucao local.
```

## Checklist Do Video

- Mostrar repositorio.
- Mostrar README.
- Mostrar arquitetura em pastas.
- Mostrar GitHub Actions.
- Mostrar Docker Compose.
- Mostrar Keycloak e comprador cadastrado.
- Mostrar Swagger.
- Obter token de gestor.
- Obter token de comprador.
- Cadastrar veiculo.
- Listar disponiveis.
- Comprar veiculo.
- Consultar status vendido.
- Listar vendidos.
- Mostrar bloqueio de segunda compra.
- Mostrar testes passando.
