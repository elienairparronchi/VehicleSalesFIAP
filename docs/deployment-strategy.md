# Deployment Strategy

Este documento descreve a estrategia de CI/CD e deploy da API VehicleSalesFIAP para o Tech Challenge FIAP/SOAT - Fase 3.

## Objetivo

Garantir que toda alteracao passe por Pull Request, validacao automatizada e publicacao reprodutivel da imagem Docker antes de ser promovida para um ambiente de execucao.

## Ambientes

| Ambiente | Objetivo | Origem da imagem | Banco | Identidade |
| --- | --- | --- | --- | --- |
| Local | Desenvolvimento e demonstracao | Build local via Docker Compose | SQL Server em container | Keycloak em container |
| CI | Validacao automatizada | Build em GitHub Actions | EF Core InMemory nos testes | Handler de teste |
| Staging | Homologacao futura | `ghcr.io/<owner>/<repo>:sha-<commit>` | SQL Server gerenciado ou containerizado | Keycloak/Auth0/Cognito |
| Production | Producao futura | Tag semantica `vX.Y.Z` | SQL Server gerenciado | Provedor OIDC dedicado |

## Pipeline

O workflow `.github/workflows/ci.yml` possui dois jobs principais.

### Validate Solution

Executado em Pull Requests, pushes para `main`, tags e execucao manual:

- restaura ferramentas locais;
- restaura pacotes NuGet;
- valida formatacao com `dotnet format`;
- compila em Release;
- executa testes unitarios e de integracao;
- coleta cobertura com `XPlat Code Coverage`;
- verifica pacotes vulneraveis;
- publica artefato `test-results`.

### Build And Publish Container Image

Executado apos a validacao:

- normaliza o nome da imagem para minusculo;
- configura Docker Buildx;
- gera tags e labels OCI com `docker/metadata-action`;
- builda a imagem Docker em Pull Requests;
- publica a imagem no GitHub Container Registry em pushes para `main`, tags e execucoes manuais.

## Registro De Imagens

Registro:

```text
ghcr.io
```

Imagem:

```text
ghcr.io/<owner>/<repo>
```

Tags geradas:

- `main`, para a branch principal;
- `pr-<numero>`, apenas para build de Pull Request;
- `sha-<commit>`, para rastreabilidade precisa;
- `vX.Y.Z`, quando uma tag semantica for publicada;
- `vX.Y`, como alias de minor version;
- `latest`, apenas na branch padrao.

## Publicacao De Release

Fluxo recomendado:

1. Criar branch de trabalho.
2. Abrir Pull Request para `main`.
3. Aguardar CI passar.
4. Fazer merge em `main`.
5. Criar tag semantica:

```bash
git tag v1.0.0
git push origin v1.0.0
```

6. Usar a imagem `ghcr.io/<owner>/<repo>:v1.0.0` como artefato de deploy.

## Rollout

Para promover uma versao:

1. Confirmar que a imagem existe no GHCR.
2. Atualizar o ambiente de destino para a tag desejada.
3. Aplicar migrations antes de liberar trafego para a nova API.
4. Validar `/health`.
5. Executar smoke test do fluxo: autenticar, cadastrar veiculo, listar disponiveis, comprar e listar vendidos.

## Rollback

Para voltar uma versao:

1. Identificar a ultima tag estavel no GHCR.
2. Atualizar o ambiente de destino para essa tag.
3. Validar `/health`.
4. Executar o smoke test principal.
5. Avaliar se migrations aplicadas exigem script compensatorio.

## Secrets E Permissoes

O pipeline usa `GITHUB_TOKEN` para publicar no GHCR. O job de container declara:

```yaml
permissions:
  contents: read
  packages: write
```

Para um deploy real em cloud, os secrets devem ser criados por ambiente no GitHub, por exemplo:

- `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`;
- `DATABASE_CONNECTION_STRING`;
- `KEYCLOAK_ADMIN_PASSWORD`;
- `OIDC_AUTHORITY`.

## Criterios De Aceitacao

Uma versao esta pronta para promocao quando:

- Pull Request foi aprovado;
- workflow `CI/CD` passou;
- testes unitarios e de integracao passaram;
- imagem Docker foi publicada no GHCR;
- pacote nao possui vulnerabilidades conhecidas pelas fontes atuais;
- ambiente responde `Healthy` em `/health`;
- smoke test de compra foi executado com sucesso.
