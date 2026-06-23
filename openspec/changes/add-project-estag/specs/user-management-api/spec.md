## ADDED Requirements

### Requirement: API limitada ao recurso usuários
O sistema SHALL expor como domínio funcional somente o cadastro e gerenciamento de usuários na rota versionada `/api/v1/usuarios`; o endpoint técnico de saúde MAY permanecer disponível.

#### Scenario: Descoberta das rotas funcionais
- **WHEN** um cliente consulta a documentação OpenAPI
- **THEN** encontra somente as cinco operações CRUD de usuários como operações de negócio

### Requirement: Listagem paginada e filtrável
O sistema SHALL listar usuários por `GET /api/v1/usuarios`, aceitar `pagina`, `tamanhoPagina`, `nome` e `email`, limitar o tamanho da página a 100 e ordenar o resultado deterministicamente.

#### Scenario: Listagem padrão
- **WHEN** o cliente envia GET sem parâmetros
- **THEN** o sistema retorna `200 OK` com a primeira página, metadados de paginação e no máximo 10 usuários

#### Scenario: Filtros combinados
- **WHEN** o cliente informa filtros válidos de nome e e-mail
- **THEN** o sistema retorna `200 OK` somente com usuários compatíveis com ambos os filtros

### Requirement: Consulta individual
O sistema SHALL obter um usuário por `GET /api/v1/usuarios/{id}`.

#### Scenario: Usuário existente
- **WHEN** o cliente consulta um ID existente
- **THEN** o sistema retorna `200 OK` com os detalhes do usuário

#### Scenario: Usuário inexistente
- **WHEN** o cliente consulta um ID válido que não existe
- **THEN** o sistema retorna `404 Not Found` no contrato padronizado

### Requirement: Cadastro de usuário
O sistema SHALL cadastrar usuário por `POST /api/v1/usuarios` e garantir unicidade de e-mail.

#### Scenario: Cadastro válido
- **WHEN** o cliente envia todos os campos obrigatórios com valores válidos e e-mail ainda não cadastrado
- **THEN** o sistema persiste o usuário e retorna `201 Created`, dados criados e header `Location` para a consulta individual

#### Scenario: E-mail já cadastrado
- **WHEN** o cliente envia um e-mail pertencente a outro usuário
- **THEN** o sistema não cria registro e retorna `409 Conflict`

### Requirement: Atualização integral de usuário
O sistema SHALL atualizar os dados de usuário por `PUT /api/v1/usuarios/{id}` aplicando as mesmas regras do cadastro.

#### Scenario: Atualização válida
- **WHEN** o cliente envia dados válidos para um ID existente
- **THEN** o sistema persiste todos os campos atualizáveis e retorna `200 OK`

#### Scenario: Atualização de usuário inexistente
- **WHEN** o cliente envia PUT para um ID válido inexistente
- **THEN** o sistema retorna `404 Not Found` sem criar registro

### Requirement: Exclusão de usuário
O sistema SHALL excluir usuário por `DELETE /api/v1/usuarios/{id}`.

#### Scenario: Exclusão válida
- **WHEN** o cliente exclui um ID existente
- **THEN** o sistema remove o registro e retorna `204 No Content`

#### Scenario: Exclusão de usuário inexistente
- **WHEN** o cliente exclui um ID válido inexistente
- **THEN** o sistema retorna `404 Not Found`

