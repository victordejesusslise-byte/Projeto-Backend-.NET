## ADDED Requirements

### Requirement: Separação de responsabilidades
O sistema SHALL separar regras de domínio, casos de uso, persistência e transporte HTTP em camadas com dependências direcionadas para abstrações.

#### Scenario: Requisição de cadastro
- **WHEN** o controller recebe um POST válido
- **THEN** delega o caso de uso ao serviço por interface e o serviço delega persistência ao repositório por interface

### Requirement: Injeção de dependências
Serviços, validadores, contexto e repositórios SHALL ser registrados no container e recebidos por construtor, sem instanciação direta nas camadas consumidoras.

#### Scenario: Substituição em teste
- **WHEN** um teste unitário executa um serviço
- **THEN** consegue substituir o repositório por um mock sem acessar SQL Server

### Requirement: Nomenclatura descritiva
Arquivos, pastas e tipos internos SHALL usar nomes em português que indiquem sua responsabilidade, respeitando PascalCase e convenções obrigatórias do .NET; exceções aprovadas SHALL ser explicadas no mapa de arquitetura.

#### Scenario: Navegação pelo repositório
- **WHEN** um novo desenvolvedor consulta a árvore e o mapa de componentes
- **THEN** consegue identificar onde ficam banco de dados, configurações, regras, serviços, controllers e testes

### Requirement: Controles básicos de segurança
O sistema SHALL usar consultas parametrizadas pelo EF Core, limitar entradas, serializar respostas como JSON e carregar segredos fora do código.

#### Scenario: Entrada semelhante a SQL ou HTML
- **WHEN** campos textuais contêm metacaracteres SQL ou marcação HTML dentro dos limites permitidos
- **THEN** o sistema trata o conteúdo como dado, sem executar SQL nem produzir conteúdo HTML executável na resposta da API

### Requirement: Cobertura automatizada dos comportamentos principais
O sistema SHALL possuir testes unitários e de integração para fluxos de sucesso e principais falhas do CRUD, filtros, paginação, validações e middleware.

#### Scenario: Verificação antes da entrega
- **WHEN** o comando de testes documentado é executado
- **THEN** todos os testes passam e falhas produzem diagnóstico suficiente para localizar o comportamento quebrado

### Requirement: Alterações condicionadas à aprovação
A implementação SHALL ser dividida em pacotes de alteração apresentados ao usuário antes de editar os arquivos funcionais correspondentes.

#### Scenario: Início de um pacote
- **WHEN** um novo pacote de implementação está pronto para começar
- **THEN** o agente descreve os arquivos e comportamentos afetados e aguarda aprovação explícita

