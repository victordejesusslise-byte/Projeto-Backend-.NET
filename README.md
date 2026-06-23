# UsuariosAPI

API REST em .NET 8 para cadastro e gerenciamento de usuários, acompanhada de uma página web simples dedicada apenas ao cadastro.

Estado atual: build aprovado, 29 testes automatizados passando, auditoria NuGet sem vulnerabilidades conhecidas e container Docker saudável.

## Acessos

Com o Docker em execução:

| Endereço | Finalidade |
|---|---|
| `http://localhost:8080` | Site simples de cadastro |
| `http://localhost:8080/swagger` | Swagger, disponível em Development |
| `http://localhost:8080/health` | Estado de saúde da aplicação |
| `http://localhost:8080/api/v1/usuarios` | API REST de usuários |

Esses endereços pertencem à mesma aplicação. O site mostra somente o formulário de cadastro; os demais recursos são acessados pela API ou pelo Swagger.

## Funcionalidades

- Cadastro de usuário.
- Consulta por identificador.
- Listagem com filtros e paginação.
- Atualização completa dos dados.
- Exclusão de usuário.
- Validações e mensagens de erro padronizadas.
- Documentação Swagger/OpenAPI.
- SQL Server com Entity Framework Core e migrations.
- Testes unitários e de integração.
- Docker com usuário não privilegiado e health check.

## Estrutura

```text
UsuariosAPI/
|-- src/
|   |-- UsuariosAPI.Domain/          Entidades, enums e contratos
|   |-- UsuariosAPI.Application/     Serviços, DTOs e validações
|   |-- UsuariosAPI.Infrastructure/  EF Core, SQL Server e repositórios
|   `-- UsuariosAPI.API/             Endpoints, middleware e site
|-- tests/
|   |-- UsuariosAPI.UnitTests/
|   `-- UsuariosAPI.IntegrationTests/
|-- database/init.sql                Criação manual opcional
|-- docs/DOCUMENTACAO_TECNICA.md     Documento completo para análise
|-- Dockerfile
|-- docker-compose.yml
`-- .env.example
```

## Como rodar com Docker - passo a passo

### 1. Confirme os programas necessários

- Docker Desktop em execução.
- SQL Server Express instalado como `SQLEXPRESS03`.
- SQL Server Management Studio para configuração do banco.
- PowerShell aberto na pasta do projeto.

```powershell
Set-Location "C:\Users\victo\Documents\UsuariosAPI\UsuariosAPI"
```

### 2. Habilite a conexão TCP do SQL Server

No SQL Server Configuration Manager:

1. Abra `SQL Server Network Configuration`.
2. Entre em `Protocols for SQLEXPRESS03`.
3. Habilite `TCP/IP`.
4. Abra as propriedades de `TCP/IP`.
5. Na aba `IP Addresses`, limpe `TCP Dynamic Ports` em `IPAll`.
6. Informe `1433` em `TCP Port`.
7. Reinicie o serviço `SQL Server (SQLEXPRESS03)`.

Confirme a porta no PowerShell:

```powershell
Test-NetConnection localhost -Port 1433
```

O resultado esperado é `TcpTestSucceeded : True`.

### 3. Habilite autenticação mista

No SQL Server Management Studio:

1. Conecte em `localhost\SQLEXPRESS03` usando autenticação do Windows.
2. Clique com o botão direito no servidor e abra `Properties`.
3. Em `Security`, selecione `SQL Server and Windows Authentication mode`.
4. Reinicie o serviço do SQL Server.

### 4. Crie o banco e o login da API

Execute no SQL Server Management Studio. Troque a senha do exemplo por uma senha forte.

```sql
USE master;
GO

IF DB_ID(N'usuarios_db') IS NULL
    CREATE DATABASE usuarios_db;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'usuarios_api')
    CREATE LOGIN usuarios_api
    WITH PASSWORD = N'TROQUE_POR_UMA_SENHA_FORTE';
GO

USE usuarios_db;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'usuarios_api')
    CREATE USER usuarios_api FOR LOGIN usuarios_api;
GO

-- Adequado somente ao desenvolvimento, pois a API aplica migrations ao iniciar.
ALTER ROLE db_owner ADD MEMBER usuarios_api;
GO
```

Em produção, utilize um usuário separado para migrations e retire `db_owner` do usuário de execução.

### 5. Configure o `.env`

Crie o arquivo local a partir do exemplo:

```powershell
Copy-Item .env.example .env
```

Abra o `.env` e ajuste apenas a senha:

```dotenv
SQLSERVER_DOCKER_CONNECTION=Server=host.docker.internal,1433;Database=usuarios_db;User Id=usuarios_api;Password=SUA_SENHA_REAL;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
ASPNETCORE_ENVIRONMENT=Development
ALLOWED_HOSTS=localhost;127.0.0.1
```

Nunca envie o `.env` ao GitHub. Ele já está bloqueado pelo `.gitignore` e pelo `.dockerignore`.

### 6. Construa e inicie

```powershell
docker compose up -d --build
docker compose ps
```

O status esperado é `Up ... (healthy)`.

Se quiser acompanhar a inicialização:

```powershell
docker compose logs --tail 100 api
```

### 7. Abra e teste

1. Acesse `http://localhost:8080`.
2. Preencha o formulário.
3. Clique em `Cadastrar`.
4. Confirme a mensagem `Cadastro realizado com sucesso!`.
5. Abra `http://localhost:8080/swagger` para testar a API completa.

### 8. Pare o sistema

```powershell
docker compose down
```

O banco não é removido, pois ele está instalado no Windows e não dentro do Compose.

## Como rodar sem Docker

Guarde a conexão com User Secrets do .NET:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS03;Database=usuarios_db;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;" --project src/UsuariosAPI.API
dotnet run --project src/UsuariosAPI.API
```

Use os endereços exibidos pelo terminal. Acrescente `/swagger` para abrir a documentação da API.

## Endpoints

| Método | Rota | Descrição | Sucesso |
|---|---|---|---|
| GET | `/api/v1/usuarios` | Listar com filtros e paginação | 200 |
| GET | `/api/v1/usuarios/{id}` | Consultar um usuário | 200 |
| POST | `/api/v1/usuarios` | Cadastrar | 201 |
| PUT | `/api/v1/usuarios/{id}` | Atualizar todos os dados | 200 |
| DELETE | `/api/v1/usuarios/{id}` | Excluir | 204 |

Filtros do GET: `pagina`, `tamanhoPagina`, `nome` e `email`.

## Testar GET, POST, PUT e DELETE

### POST

```powershell
$novoUsuario = @{
  nome = "Victor"
  sobrenome = "Souza"
  email = "victor@exemplo.com"
  genero = "Masculino"
  dataNascimento = "1998-07-14"
} | ConvertTo-Json

$criado = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/api/v1/usuarios" `
  -ContentType "application/json" `
  -Body $novoUsuario

$id = $criado.dados.id
$criado
```

### GET

```powershell
Invoke-RestMethod "http://localhost:8080/api/v1/usuarios?pagina=1&tamanhoPagina=10"
Invoke-RestMethod "http://localhost:8080/api/v1/usuarios/$id"
```

### PUT

```powershell
$alterado = @{
  nome = "Victor"
  sobrenome = "Silva"
  email = "victor@exemplo.com"
  genero = "Masculino"
  dataNascimento = "1998-07-14"
} | ConvertTo-Json

Invoke-RestMethod -Method Put `
  -Uri "http://localhost:8080/api/v1/usuarios/$id" `
  -ContentType "application/json" `
  -Body $alterado
```

### DELETE

```powershell
Invoke-RestMethod -Method Delete "http://localhost:8080/api/v1/usuarios/$id"
```

## Validações

- Nome obrigatório, máximo de 100 caracteres.
- Sobrenome obrigatório, máximo de 100 caracteres.
- E-mail obrigatório, formato válido, máximo de 150 caracteres e único.
- Gênero: `Masculino`, `Feminino` ou `Outro`.
- Data de nascimento opcional e não futura.
- Página maior ou igual a 1.
- Tamanho da página entre 1 e 100.

## Erros padronizados

```json
{
  "sucesso": false,
  "mensagem": "Não foi possível processar a requisição.",
  "dados": null,
  "erros": ["E-mail inválido."],
  "traceId": "identificador-da-requisicao"
}
```

| Código | Significado |
|---|---|
| 400 | JSON ausente, inválido ou com tipo incorreto |
| 404 | Usuário não encontrado |
| 409 | E-mail duplicado |
| 422 | Regra de validação não atendida |
| 500 | Falha interna |

Detalhes técnicos de erro são exibidos somente em Development.

## Testes e segurança

```powershell
dotnet test UsuariosAPI.sln --configuration Release
dotnet list UsuariosAPI.sln package --vulnerable --include-transitive
```

Última validação realizada:

- 18 testes unitários aprovados.
- 11 testes de integração aprovados.
- Zero pacotes vulneráveis segundo o feed atual do NuGet.
- Container executado pelo usuário não privilegiado `app`.
- Sistema de arquivos do container somente leitura.
- Health check respondendo com status saudável.

Proteções atuais: consultas parametrizadas pelo EF Core, validação de entrada, respostas sem HTML, CSP, cabeçalhos de segurança, segredos fora da imagem e container sem capabilities.

Antes de publicar na internet ainda são recomendados: autenticação para GET/PUT/DELETE, rate limiting no cadastro, HTTPS, logs de auditoria e usuário SQL com privilégios mínimos.

## Problemas comuns

### Variável `SQLSERVER_DOCKER_CONNECTION` ausente

Crie o `.env` e confirme que a variável não está vazia.

### Erro 26 ou servidor não encontrado

Confirme o nome `SQLEXPRESS03`, habilite TCP/IP, reinicie o serviço e teste a porta 1433.

### Erro de certificado

Mantenha `Encrypt=True;TrustServerCertificate=True` apenas no ambiente local. Em produção, use certificado confiável.

### Banco `usuarios_db` não existe

Crie o banco pelo script SQL acima ou execute `database/init.sql`. Não use `USE usuarios_db` antes de criar o banco.

### Container inicia, mas a API não conecta

Confira modo misto, login SQL, senha do `.env`, firewall e `Test-NetConnection localhost -Port 1433`.

## Enviar para o GitHub

Antes do primeiro envio:

```powershell
dotnet test UsuariosAPI.sln --configuration Release
dotnet list UsuariosAPI.sln package --vulnerable --include-transitive
docker compose config --quiet
```

Confira que `.env` não será incluído:

```powershell
git status --short
git check-ignore .env
```

Inicialize e publique:

```powershell
git init
git add .
git status
git commit -m "feat: adiciona API de usuarios documentada"
git branch -M main
git remote add origin https://github.com/SEU-USUARIO/UsuariosAPI.git
git push -u origin main
```

Leia o `git status` antes do commit e nunca publique senhas, arquivos `.env`, tokens ou connection strings reais.

## Documentação completa

- [Documentação técnica para análise](docs/DOCUMENTACAO_TECNICA.md)
- [PDF da documentação](output/pdf/Documentacao_UsuariosAPI.pdf)
- [Decisões de arquitetura](DESIGN.md)
- [Plano e situação atual](PLAN.md)

## Licença

Nenhuma licença foi definida. Antes de tornar o repositório público, escolha uma licença ou declare que o código permanece sem licença de reutilização.
