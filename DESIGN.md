# Arquitetura e decisões - UsuariosAPI

Este arquivo resume o design atual. A análise detalhada está em [docs/DOCUMENTACAO_TECNICA.md](docs/DOCUMENTACAO_TECNICA.md).

## Arquitetura

```text
Site / Cliente HTTP
        |
        v
UsuariosAPI.API
        |
        v
UsuariosAPI.Application
        |
        v
UsuariosAPI.Domain
        ^
        |
UsuariosAPI.Infrastructure -> SQL Server SQLEXPRESS03
```

| Projeto | Responsabilidade |
|---|---|
| Domain | Entidade, enum e contrato do repositório |
| Application | Casos de uso, DTOs, validações e erros de negócio |
| Infrastructure | EF Core, DbContext, migrations e repositório |
| API | HTTP, Swagger, middleware, DI e tela Razor Pages em C# |

## Decisões

- API versionada em `/api/v1`.
- Recurso plural `/usuarios`.
- DTOs separados da entidade.
- Controllers dependem de services; services dependem de interfaces.
- EF Core 8.0.28 com SQL Server e consultas LINQ parametrizadas.
- FluentValidation para regras de entrada.
- Middleware global para respostas de erro.
- Swagger em `/swagger` apenas em Development.
- Interface web em Razor Pages/C# para testar cadastro, listagem, busca por ID, atualização e exclusão.
- Docker conectado ao SQL Server instalado no Windows.
- User Secrets para execução local e `.env` apenas para Compose.
- Container não root, somente leitura e sem capabilities.

## Limites atuais

- Sem autenticação e autorização.
- Sem rate limiting.
- Sem histórico persistente de operações.
- Migrations automáticas na inicialização.
- HTTPS depende do ambiente de hospedagem.
