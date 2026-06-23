## ADDED Requirements

### Requirement: Provider único SQL Server
O sistema SHALL usar o provider EF Core para SQL Server em todos os ambientes e SHALL remover configurações ativas e instruções que indiquem MySQL ou MariaDB.

#### Scenario: Inicialização da aplicação
- **WHEN** a API inicia com uma connection string SQL Server válida
- **THEN** registra o `DbContext` com `UseSqlServer`, aplica a política de retry e conecta sem dependência de provider MySQL

### Requirement: Modelo normalizado de usuários
O banco SHALL possuir a tabela `usuarios` com chave primária numérica gerada, campos obrigatórios com limites, e-mail único, gênero persistido de forma definida, data de nascimento opcional e timestamps de criação e atualização.

#### Scenario: Tentativa concorrente de e-mail duplicado
- **WHEN** duas gravações tentam persistir o mesmo e-mail
- **THEN** a constraint única do banco impede a duplicidade mesmo que a validação prévia não detecte a corrida

### Requirement: Evolução do schema
O sistema SHALL manter migrations EF Core válidas e um script T-SQL alternativo coerente com o mapeamento da entidade.

#### Scenario: Banco vazio
- **WHEN** as migrations são aplicadas em um banco SQL Server vazio
- **THEN** o schema necessário é criado sem comandos específicos de MySQL

### Requirement: Execução com Docker Compose
O Compose SHALL iniciar SQL Server e a API, persistir os dados em volume, aguardar o banco ficar saudável e fornecer a connection string por variável de ambiente.

#### Scenario: Subida limpa dos containers
- **WHEN** o operador fornece uma senha SQL Server válida e executa `docker compose up --build`
- **THEN** o banco fica saudável, a API aplica as migrations e passa a responder na porta documentada

### Requirement: Segredos fora do código
O sistema SHALL obter credenciais por variáveis de ambiente e SHALL manter valores reais fora do controle de versão.

#### Scenario: Preparação do ambiente
- **WHEN** um desenvolvedor copia `.env.example` para `.env`
- **THEN** encontra nomes e exemplos de variáveis SQL Server sem credencial real do usuário

