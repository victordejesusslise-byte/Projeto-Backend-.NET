# UsuariosAPI

API REST em .NET 8 para cadastro e gerenciamento de usuários.

O projeto também possui uma página web simples em **Razor Pages/C#** para testar cadastro, busca por ID e atualização de usuários. A área privada possui um painel com todos os endpoints do CRUD.

## Sobre o projeto

O **UsuariosAPI** é um sistema backend criado para demonstrar uma API REST completa de cadastro de usuários.

A aplicação permite registrar, consultar, listar, atualizar e remover usuários usando endpoints HTTP. Além da API, existe uma tela simples para cadastro e consulta, mais uma área privada para testar os endpoints e verificar a tabela de usuários.

O objetivo do projeto é apresentar boas práticas de desenvolvimento backend, incluindo separação em camadas, validação de entrada, tratamento de erros, documentação Swagger, persistência em SQL Server, testes automatizados e execução com Docker.

Principais partes do sistema:

- **Site de gerenciamento**: página Razor Pages/C# acessada pelo navegador.
- **API REST**: endpoints usados para manipular usuários.
- **Banco de dados SQL Server**: armazena os usuários cadastrados.
- **Swagger**: documentação interativa para testar a API.
- **Docker**: facilita a execução da aplicação.

## O que o sistema faz

- Cadastra usuários pelo site ou pela API.
- Lista usuários com filtros e paginação pela API e pela área privada.
- Consulta usuário por ID.
- Atualiza dados de usuário.
- Remove usuário pela API e pela área privada.
- Valida os dados enviados.
- Retorna erros padronizados.
- Usa SQL Server com Entity Framework Core.
- Possui Swagger para testar os endpoints.
- Pode ser executado com Docker.

## Requisitos para rodar

- Docker Desktop instalado.
- SQL Server Express instalado.
- SQL Server Management Studio instalado.
- PowerShell.
- Porta `1433` disponível para o SQL Server.

## Precisa subir Node.js?

Não. Este projeto não precisa de Node.js para rodar.

A tela do site foi feita com **Razor Pages/C#**, então ela sobe junto com a aplicação .NET pelo Docker:

```powershell
docker compose up -d --build
```

Não existe comando `npm start`, `npm run dev` ou `node server.js` neste projeto.

Se alguém perguntar "como subir o Node?", a resposta correta é:

```text
Não precisa subir Node. O frontend é Razor Pages/C# e roda dentro da própria API .NET.
```

O Node.js só seria necessário se o projeto tivesse um frontend separado em React, Angular, Vue ou JavaScript puro com servidor próprio. Este não é o caso.

## Passo a passo para rodar o sistema

### 1. Abrir a pasta do projeto

Abra o PowerShell e entre na pasta:

```powershell
cd "C:\Users\victo\Documents\UsuariosAPI\UsuariosAPI"
```

### 2. Abrir o Docker Desktop

Abra o **Docker Desktop** pelo menu iniciar do Windows.

Espere ele terminar de carregar. Depois confira no PowerShell:

```powershell
docker version
```

Se esse comando falhar, o Docker ainda não está pronto.

### 3. Ligar o SQL Server correto

Este projeto usa a instância:

```text
SQLEXPRESS03
```

No PowerShell como administrador, confira os serviços SQL:

```powershell
Get-Service | Where-Object { $_.Name -like "MSSQL*" -or $_.Name -like "SQLBrowser" } | Select-Object Name, Status, DisplayName
```

Se `MSSQL$SQLEXPRESS03` estiver parado, inicie:

```powershell
Start-Service 'MSSQL$SQLEXPRESS03'
```

Se aparecer erro `10048`, existe conflito de porta. Alguma outra instância SQL está usando a porta `1433`.

Para descobrir:

```powershell
netstat -ano | findstr :1433
```

Depois pare a instância conflitante e tente iniciar o `SQLEXPRESS03` novamente.

Exemplo:

```powershell
Stop-Service 'MSSQL$SQLEXPRESS01'
Start-Service 'MSSQL$SQLEXPRESS03'
```

### 4. Habilitar TCP/IP no SQL Server

Abra o **SQL Server Configuration Manager**.

Vá em:

```text
SQL Server Network Configuration
Protocols for SQLEXPRESS03
```

Faça:

1. Habilite `TCP/IP`.
2. Abra as propriedades de `TCP/IP`.
3. Entre na aba `IP Addresses`.
4. Em `IPAll`, limpe o campo `TCP Dynamic Ports`.
5. Em `TCP Port`, coloque `1433`.
6. Salve.
7. Reinicie o serviço `SQL Server (SQLEXPRESS03)`.

Depois teste:

```powershell
Test-NetConnection localhost -Port 1433
```

O esperado é:

```text
TcpTestSucceeded : True
```

### 5. Habilitar autenticação mista

No **SQL Server Management Studio**, conecte em:

```text
localhost\SQLEXPRESS03
```

Use autenticação do Windows.

Depois:

1. Clique com o botão direito no servidor.
2. Abra `Properties`.
3. Entre em `Security`.
4. Marque `SQL Server and Windows Authentication mode`.
5. Salve.
6. Reinicie o serviço `SQL Server (SQLEXPRESS03)`.

### 6. Criar banco e usuário da API

No SQL Server Management Studio, conecte em:

```text
localhost\SQLEXPRESS03
```

Depois execute:

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

ALTER ROLE db_owner ADD MEMBER usuarios_api;
GO
```

Use no `.env` a mesma senha definida no `CREATE LOGIN`.

### 7. Configurar o arquivo `.env`

Crie o arquivo:

```powershell
Copy-Item .env.example .env
notepad .env
```

Conteúdo esperado:

```env
SQLSERVER_DOCKER_CONNECTION=Server=host.docker.internal,1433;Database=usuarios_db;User Id=usuarios_api;Password=SUA_SENHA_REAL;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
ASPNETCORE_ENVIRONMENT=Development
ALLOWED_HOSTS=localhost;127.0.0.1
ADMIN_PANEL_PASSWORD=SUA_SENHA_DA_AREA_PRIVADA
```

Não versionar o arquivo `.env`, pois ele contém senha do banco.
Use uma senha diferente da senha do SQL Server.

### 8. Subir o sistema com Docker

Execute:

```powershell
docker compose up -d --build
```

Confira se o container está rodando:

```powershell
docker compose ps
```

O esperado é o container `usuarios_api` aparecer como `Up`.

Se quiser ver os logs:

```powershell
docker compose logs --tail 100 api
```

### 9. Testar se funcionou

Teste o health check:

```powershell
Invoke-WebRequest http://localhost:8080/health
```

Se retornar status `200`, a API está funcionando.

Abra no navegador:

```text
http://localhost:8080/site
```

Também é possível abrir o Swagger:

```text
http://localhost:8080/swagger
```

### 10. Parar o sistema

Quando quiser parar:

```powershell
docker compose down
```

## Acessar

| URL | Função |
|---|---|
| `http://localhost:8080/site` | Site simples para cadastro, busca por ID e atualização |
| `http://localhost:8080/site/admin/login` | Login da área privada |
| `http://localhost:8080/site/admin/tabela` | Painel privado dos endpoints e verificação da tabela |
| `http://localhost:8080/swagger` | Swagger da API |
| `http://localhost:8080/health` | Health check |
| `http://localhost:8080/api/v1/usuarios` | Endpoint principal |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/v1/usuarios` | Lista usuários |
| GET | `/api/v1/usuarios/{id}` | Busca usuário por ID |
| POST | `/api/v1/usuarios` | Cadastra usuário |
| PUT | `/api/v1/usuarios/{id}` | Atualiza usuário |
| DELETE | `/api/v1/usuarios/{id}` | Remove usuário |

Filtros da listagem:

```text
pagina
tamanhoPagina
nome
email
```

Exemplo:

```text
http://localhost:8080/api/v1/usuarios?pagina=1&tamanhoPagina=10
```

## Testar pelo PowerShell

Cadastrar:

```powershell
$novoUsuario = @{
  nome = "Rodrigo"
  sobrenome = "Souza"
  email = "ex@exemplo.com"
  genero = "Masculino-Feminino"
  dataNascimento = "1998-07-14"
} | ConvertTo-Json

$criado = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/api/v1/usuarios" `
  -ContentType "application/json" `
  -Body $novoUsuario

$id = $criado.dados.id
$criado
```

Listar:

```powershell
Invoke-RestMethod "http://localhost:8080/api/v1/usuarios?pagina=1&tamanhoPagina=10"
```

Buscar por ID:

```powershell
Invoke-RestMethod "http://localhost:8080/api/v1/usuarios/$id"
```

Atualizar:

```powershell
$usuarioAlterado = @{
  nome = "Victor"
  sobrenome = "Silva"
  email = "victor@exemplo.com"
  genero = 1
  dataNascimento = "1998-07-14"
} | ConvertTo-Json

Invoke-RestMethod -Method Put `
  -Uri "http://localhost:8080/api/v1/usuarios/$id" `
  -ContentType "application/json" `
  -Body $usuarioAlterado
```

Remover:

```powershell
Invoke-RestMethod -Method Delete "http://localhost:8080/api/v1/usuarios/$id"
```

## Testes

```powershell
dotnet test UsuariosAPI.sln --configuration Release
```

## Problemas comuns

### Docker não conecta

Abra o Docker Desktop e tente novamente:

```powershell
docker version
docker compose up -d --build
```

### SQL Server não conecta

Confira:

```powershell
Get-Service 'MSSQL$SQLEXPRESS03'
Test-NetConnection localhost -Port 1433
```

### Erro `10048`

Existe conflito de porta. Alguma outra instância SQL está usando a porta `1433`.

Descubra o processo:

```powershell
netstat -ano | findstr :1433
```

### API não conecta no banco

Confira:

- SQL Server `SQLEXPRESS03` ligado.
- Porta `1433` funcionando.
- Banco `usuarios_db` criado.
- Login `usuarios_api` criado.
- Senha do `.env` correta.

## Documentação completa

- [Documentação técnica](docs/DOCUMENTACAO_TECNICA.md)
- [PDF da documentação](pdf/Documentacao_UsuariosAPI.pdf)
- [Design do projeto](DESIGN.md)
- [Plano do projeto](PLAN.md)
