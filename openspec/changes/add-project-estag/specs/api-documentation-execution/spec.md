## ADDED Requirements

### Requirement: OpenAPI completo
O sistema SHALL documentar no Swagger/OpenAPI cada endpoint, parâmetros, corpo, respostas, códigos HTTP e exemplos relevantes.

#### Scenario: Uso pelo Swagger UI
- **WHEN** um desenvolvedor abre o Swagger UI em Development
- **THEN** consegue compreender e executar GET, POST, PUT e DELETE sem consultar o código-fonte

### Requirement: Guia prático dos métodos HTTP
O `README.md` SHALL explicar a finalidade de GET, POST, PUT e DELETE e fornecer exemplos executáveis com URL, JSON e resposta esperada para os cinco endpoints.

#### Scenario: Primeiro uso por novo desenvolvedor
- **WHEN** uma pessoa segue somente o README
- **THEN** consegue cadastrar, obter, listar, atualizar e excluir um usuário na ordem correta

### Requirement: Instruções de execução reproduzíveis
A documentação SHALL explicar pré-requisitos e procedimentos separados para Docker e execução local com SQL Server, incluindo configuração, migrations, testes, Swagger e solução de erros comuns.

#### Scenario: Execução via Docker
- **WHEN** um desenvolvedor segue o procedimento Docker em uma máquina compatível
- **THEN** consegue iniciar o banco e a API usando os arquivos versionados

### Requirement: Documentação coerente
`README.md`, `PLAN.md`, `DESIGN.md`, OpenAPI e exemplos SHALL descrever as mesmas rotas, banco, campos e comportamentos observados no código.

#### Scenario: Revisão documental
- **WHEN** a implementação é concluída
- **THEN** não restam referências a MySQL, rotas antigas ou funcionalidades inexistentes nos documentos ativos

