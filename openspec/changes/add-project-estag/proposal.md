## Why

O projeto precisa consolidar uma API de cadastro e gerenciamento de usuários pronta para avaliação técnica, eliminando inconsistências atuais entre SQL Server e referências legadas a MySQL e tornando explícitos o contrato REST, os erros, as validações, a execução e a organização do código. A mudança também deve tornar o sistema compreensível para quem nunca viu o repositório, sem ampliar o domínio além de usuários.

## What Changes

- Manter o escopo funcional exclusivamente no cadastro e gerenciamento de usuários, com CRUD completo em rotas REST versionadas sob `/api/v1/usuarios`.
- Garantir listagem com filtros por nome/e-mail e paginação, consulta por ID, criação, atualização integral e exclusão.
- Padronizar códigos HTTP e corpos de sucesso/erro, distinguindo validação, regra de negócio, recurso inexistente, conflito e falha interna.
- Implementar fallback global seguro: detalhes explícitos para debug apenas em Development e mensagem genérica com identificador de rastreamento nos demais ambientes.
- Completar validações amigáveis de rota, query string e corpo JSON, inclusive JSON inválido e campos obrigatórios.
- Consolidar EF Core com SQL Server em código, migrations, script SQL, variáveis de ambiente e `docker-compose.yml`, removendo referências funcionais e documentais a MySQL.
- Revisar Swagger/OpenAPI com parâmetros, exemplos, respostas e códigos documentados para todos os endpoints.
- Reorganizar e renomear arquivos/componentes com nomes em português que descrevam sua responsabilidade (por exemplo, dados/banco, configurações, serviços e repositórios), preservando as convenções obrigatórias do .NET e nomes públicos REST.
- Cobrir os fluxos principais e erros com testes unitários e de integração, incluindo paginação, filtros, validação, conflito, não encontrado e fallback.
- Atualizar `README.md`, `PLAN.md` e documentação técnica para explicar arquitetura, banco, execução, Swagger e uso prático de GET, POST, PUT e DELETE.
- Manter consultas parametrizadas pelo EF Core, limites de entrada e serialização JSON segura como controles contra SQL Injection e XSS.

## Capabilities

### New Capabilities

- `user-management-api`: Contrato REST versionado do CRUD de usuários, incluindo filtros, paginação, payloads e códigos HTTP.
- `error-validation-contract`: Validações de entrada e respostas de erro padronizadas, com separação de falhas de negócio e internas e fallback adequado ao ambiente.
- `sql-server-persistence`: Persistência normalizada em SQL Server com EF Core, chaves, índices, migrations, script de criação e execução em Docker.
- `api-documentation-execution`: Swagger/OpenAPI e documentação textual completa para executar, testar e entender todas as funcionalidades e métodos HTTP.
- `maintainable-api-architecture`: Organização em camadas, injeção de dependências, abstrações, nomenclatura descritiva, segurança básica e testes automatizados.
- `user-operation-history`: Histórico estruturado e persistido em arquivo das operações de cadastro, atualização e exclusão de usuários, sem registrar dados pessoais desnecessários.

### Modified Capabilities

Nenhuma. O repositório ainda não possui especificações OpenSpec base publicadas.

## Impact

- Código afetado: projetos `UsuariosAPI.API`, `UsuariosAPI.Application`, `UsuariosAPI.Domain` e `UsuariosAPI.Infrastructure`, além dos testes.
- Configuração afetada: `docker-compose.yml`, `Dockerfile`, `.env.example`, configurações ASP.NET Core e conexão EF Core.
- Dados afetados: migration inicial, modelo EF Core e `database/init.sql`, todos alinhados ao SQL Server.
- Documentação afetada: Swagger/OpenAPI, `README.md`, `PLAN.md` e `DESIGN.md`.
- API pública preservada na rota canônica atual `/api/v1/usuarios`; não será introduzido outro domínio funcional nem frontend nesta mudança.
- Implementação condicionada à aprovação do usuário por pacote de alterações; estes artefatos constituem apenas o planejamento.
