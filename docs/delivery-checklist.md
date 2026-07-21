# Delivery Checklist

Checklist final do Tech Challenge FIAP/SOAT - Fase 3.

## Requisitos Do Enunciado

| Requisito | Status | Evidencia no projeto |
| --- | --- | --- |
| Cadastrar veiculo para venda | Entregue | `POST /api/v1/vehicles` |
| Informar marca, modelo, ano, cor e preco | Entregue | `CreateVehicleRequest` e entidade `Vehicle` |
| Editar dados do veiculo | Entregue | `PUT /api/v1/vehicles/{id}` |
| Comprar veiculo pela internet | Entregue | `POST /api/v1/vehicles/{id}/purchase` |
| Compra apenas por pessoa cadastrada | Entregue | endpoint protegido por role `buyer` no Keycloak |
| Listar veiculos a venda por preco crescente | Entregue | `GET /api/v1/vehicles/available` |
| Listar veiculos vendidos por preco crescente | Entregue | `GET /api/v1/vehicles/sold` |
| Separar cadastro/autorizacao de compradores dos dados transacionais | Entregue | Keycloak separado da API e banco transacional |
| Usar CI/CD e Pull Requests | Entregue | `.github/workflows/ci.yml` e estrategia em `docs/deployment-strategy.md` |
| README explicando projeto, uso e testes | Entregue | `README.md` |
| Codigo-fonte funcionando | Entregue | solution .NET 10, testes e Docker Compose |
| Deploy automatizado | Entregue | build e publicacao de imagem no GHCR pelo GitHub Actions |

## Entregaveis FIAP

O PDF final da entrega deve conter:

- link do repositorio GitHub;
- link do video demonstrativo;
- breve descricao da solucao;
- instrucoes para rodar localmente;
- usuarios de teste;
- evidencia de CI/CD;
- observacao sobre infraestrutura local com Docker Compose.

Campos para preencher na Parte 10:

```text
Repositorio: https://github.com/elienairparronchi/VehicleSalesFIAP
Video: PENDENTE - substituir pelo link do video apos gravacao e publicacao
Imagem GHCR: ghcr.io/elienairparronchi/vehiclesalesfiap
PDF: output/pdf/VehicleSalesFIAP-entrega-final.pdf
Gerador do PDF: tools/generate-final-pdf.py
```

## Criterios De Pronto Antes Do Video

- `dotnet build VehicleSalesFIAP.slnx --configuration Release` executa sem erros.
- `dotnet test VehicleSalesFIAP.slnx --configuration Release` executa sem erros.
- `dotnet format VehicleSalesFIAP.slnx --verify-no-changes --verbosity minimal` executa sem alteracoes pendentes.
- `dotnet list VehicleSalesFIAP.slnx package --vulnerable --include-transitive` nao aponta vulnerabilidades conhecidas.
- `docker compose up -d --build` sobe SQL Server, Keycloak, migrations e API.
- `http://localhost:5000/health` retorna `Healthy`.
- `http://localhost:5000/swagger` abre a documentacao interativa.
- Tokens de gestor e comprador sao gerados pelo Keycloak.
- Fluxo de cadastro, listagem, compra e listagem de vendidos funciona.

## Roteiro Curto Para Demonstracao

1. Mostrar rapidamente o repositorio no GitHub e a estrutura Clean Architecture.
2. Mostrar o workflow `CI/CD` no GitHub Actions.
3. Subir a stack com `docker compose up -d --build`.
4. Mostrar `docker compose ps` com containers saudaveis.
5. Abrir Keycloak e mostrar realm `vehiclesalesfiap`, client e usuarios de teste.
6. Abrir Swagger da API.
7. Obter token do gestor e autorizar no Swagger ou usar PowerShell.
8. Cadastrar veiculo.
9. Listar veiculos disponiveis.
10. Obter token do comprador.
11. Comprar o veiculo.
12. Consultar veiculo e mostrar status `Sold`.
13. Listar veiculos vendidos.
14. Tentar comprar novamente e mostrar erro de regra de negocio.

## Pendencias Manuais

Estas atividades dependem de acao fora do codigo:

- criar commit inicial;
- fazer push para o GitHub;
- abrir Pull Request, se quiser evidenciar o fluxo exigido;
- aguardar GitHub Actions executar no repositorio remoto;
- gravar e publicar o video;
- substituir o campo pendente do video no PDF final antes do envio oficial.
- apos alterar `docs/final-submission.md`, regenerar o PDF com `python tools/generate-final-pdf.py`.

## Documentos De Apoio

- `README.md`: indice principal e comandos essenciais.
- `docs/architecture.md`: arquitetura, dominio, seguranca e persistencia.
- `docs/api-reference.md`: contratos HTTP, exemplos e status codes.
- `docs/test-plan.md`: testes automatizados e smoke test manual.
- `docs/deployment-strategy.md`: CI/CD, publicacao de imagem e rollback.
- `docs/final-submission.md`: conteudo fonte do PDF final.
- `docs/video-script.md`: roteiro detalhado do video demonstrativo.
- `tools/generate-final-pdf.py`: gerador do PDF final a partir do Markdown.
