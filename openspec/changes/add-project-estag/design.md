## Context

A solução .NET 8 já possui quatro camadas, CRUD de usuários, FluentValidation, EF Core, Swagger, middleware de exceções e testes. Porém, a configuração está dividida: o provider em código e a migration são SQL Server, enquanto `docker-compose.yml`, `database/init.sql`, `.env.example` e partes do `README.md` ainda usam ou mencionam MySQL. O tratamento atual não padroniza erros produzidos pelo model binding e oculta detalhes em todos os ambientes, contrariando o pedido de debug explícito. A documentação também precisa explicar o uso real de cada método HTTP.

O usuário exige aprovação antes de cada alteração funcional. Por isso, a futura aplicação será dividida em pacotes pequenos e verificáveis, sem mudanças fora do pacote aprovado.

## Goals / Non-Goals

**Goals:**

- Entregar apenas o domínio de cadastro e gerenciamento de usuários, com cinco endpoints CRUD versionados.
- Tornar respostas, validações e fallback previsíveis, seguros e fáceis de depurar.
- Usar SQL Server de ponta a ponta, inclusive no Docker Compose e no script SQL.
- Preservar separação de responsabilidades, injeção de dependências e consultas parametrizadas.
- Tornar Swagger e Markdown suficientes para executar, testar e compreender o sistema.
- Adotar nomenclatura em português e orientada à responsabilidade sem quebrar convenções necessárias do ecossistema .NET.

**Non-Goals:**

- Criar frontend separado; a interface interativa continuará sendo o Swagger UI.
- Adicionar autenticação, autorização, e-mail, upload, cache ou outros domínios sem nova aprovação.
- Expor stack trace ou detalhes internos em Production.
- Alterar a rota pública para uma versão sem versionamento.
- Trocar SQL Server por outro banco.

## Decisions

### 1. Contrato REST canônico versionado

A rota pública será `/api/v1/usuarios`, mantendo pluralização e versionamento já presentes. `POST` retornará `201 Created` com `Location`, `GET` e `PUT` retornarão `200`, e `DELETE` retornará `204`. A forma sem prefixo `/usuarios` será apresentada como descrição conceitual, não como uma segunda rota duplicada.

Alternativa considerada: disponibilizar simultaneamente `/usuarios` e `/api/v1/usuarios`. Foi descartada por duplicar o contrato, a documentação e os testes sem benefício funcional.

### 2. Envelope único e model binding controlado

Respostas continuarão em português com um envelope coerente (`sucesso`, `mensagem`, `dados`, `erros`, `traceId` quando aplicável). Erros de sintaxe JSON/corpo ausente serão `400`; violações de regras de campo e paginação serão `422`; recurso ausente será `404`; e-mail em conflito será `409`; falhas inesperadas serão `500`. A resposta automática de `[ApiController]` será configurada para usar o mesmo contrato.

Alternativa considerada: migrar todo o contrato para `ProblemDetails`. Embora seja padrão HTTP, isso criaria dois formatos ou uma quebra desnecessária no projeto atual. O envelope existente será aprimorado e documentado.

### 3. Debug explícito condicionado ao ambiente

O middleware global registrará todas as falhas inesperadas e incluirá `traceId`. Em Development, a resposta de erro interno também incluirá um bloco `debug` com tipo, mensagem e stack trace. Em Production, retornará somente mensagem genérica e `traceId`.

Alternativa considerada: sempre retornar detalhes completos. Foi rejeitada porque expõe caminhos, nomes internos e dados potencialmente sensíveis.

### 4. SQL Server como única tecnologia de persistência

Será mantido `Microsoft.EntityFrameworkCore.SqlServer`. O Compose usará a imagem oficial do SQL Server, health check e volume próprio; a API usará uma connection string SQL Server. A migration será a fonte principal do schema, enquanto `database/init.sql` será um script T-SQL alternativo consistente. O banco será criado/aplicado pela migration na inicialização, com retry para aguardar o container.

Alternativa considerada: conectar o container da API ao SQLEXPRESS da máquina. Foi rejeitada como padrão porque depende do sistema operacional e da rede do host; continuará documentada apenas como opção de execução local.

### 5. Nomenclatura descritiva com convenções .NET preservadas

Arquivos e pastas internos serão nomeados em português por responsabilidade, usando PascalCase para tipos C# (por exemplo, `ContextoBancoDados`, `ConfiguracaoServicos`, `ServicoUsuario`, `RepositorioUsuario`). `Program.cs`, nomes dos projetos/assemblies e o contrato JSON/REST serão preservados quando a convenção ou compatibilidade superar o benefício da renomeação. O mapa "nome → responsabilidade" será documentado.

Alternativa considerada: renomear solution, projetos, namespaces e todos os tipos de uma vez. Isso adiciona risco alto e pouco valor funcional; só será feito se o usuário aprovar explicitamente esse alcance ampliado.

### 6. Testes por comportamento observável

Testes unitários cobrirão regras de serviço e validação. Testes de integração cobrirão os cinco endpoints e o formato de erros usando banco isolado. O provider de teste deve respeitar o máximo possível as regras relacionais importantes, especialmente unicidade de e-mail.

Alternativa considerada: manter somente EF InMemory. Ele é rápido, mas não reproduz integralmente constraints e semântica relacional; SQLite in-memory é preferível se a mudança de dependência for aprovada.

### 7. Segurança proporcional ao escopo

O acesso ao banco será exclusivamente por LINQ/EF Core parametrizado; entradas terão limites; respostas usarão serialização JSON; segredos ficarão em variáveis de ambiente e `.env` permanecerá ignorado. Não será aplicada "sanitização" destrutiva de texto, pois validação, parametrização e encoding no consumidor são controles mais corretos para uma API JSON.

### 8. Histórico simples com logs estruturados

A API usará Serilog como provedor de logging e gravará eventos estruturados de usuário em console e arquivo com rotação diária. Após operações concluídas com sucesso serão registrados os eventos `UsuarioCadastrado`, `UsuarioAtualizado` e `UsuarioRemovido`, contendo ID do usuário, timestamp, tipo da operação e correlação da requisição. Nome, e-mail, corpo JSON e outros dados pessoais não serão gravados no histórico.

Alternativa considerada: criar uma tabela e endpoints de auditoria. Foi adiada porque amplia o modelo de dados e a superfície da API além do histórico simples solicitado; poderá ser proposta futuramente se houver necessidade de consulta transacional ou retenção obrigatória.

## Risks / Trade-offs

- [Senha forte obrigatória na imagem SQL Server dificulta valores de exemplo] → fornecer `.env.example` sem segredo real e validar variáveis antes da execução.
- [Migration automática pode disputar em múltiplas instâncias] → adequada ao projeto de estágio; documentar migração separada como evolução para produção.
- [Detalhes de debug podem vazar se o ambiente for configurado errado] → habilitar detalhes somente quando `IHostEnvironment.IsDevelopment()` for verdadeiro.
- [Renomeações extensas geram diffs grandes e erros de namespace] → aplicar por camada, compilar e testar após cada pacote aprovado.
- [EF InMemory mascara comportamento do SQL Server] → adicionar casos relacionais com SQLite ou integração SQL Server conforme aprovação.
- [Documentação divergir do código] → validar exemplos e OpenAPI nos testes/execução final.

## Migration Plan

1. Obter aprovação do pacote de banco e substituir Compose, `.env.example` e script por SQL Server.
2. Obter aprovação do pacote REST/erros e ajustar contrato, model binding, middleware e Swagger.
3. Obter aprovação do pacote de nomenclatura e aplicar renomeações por camada, compilando entre etapas.
4. Obter aprovação do pacote de testes e ampliar a suíte.
5. Obter aprovação do pacote documental e sincronizar README, PLAN e DESIGN com o comportamento verificado.
6. Executar build, testes, migration e smoke test dos cinco endpoints.

Rollback: cada pacote será mantido isolado para permitir reversão manual dos arquivos do pacote antes de prosseguir; nenhuma migration destrutiva de dados está planejada.

## Decisões confirmadas pelo usuário

- Renomear arquivos, pastas e tipos internos quando isso melhorar o entendimento; preservar nomes técnicos necessários e evitar renomear projetos/solution sem benefício claro.
- O Docker subirá somente a API e conectará ao `SQLEXPRESS02` existente no host.
- Implementar em pacote próprio um histórico simples de cadastro, atualização e exclusão com Serilog e arquivo rotativo.

## Open Questions

- Rate limiting e health check detalhado permanecem como extras opcionais para uma aprovação futura.
