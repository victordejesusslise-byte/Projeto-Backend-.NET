## ADDED Requirements

### Requirement: Validação amigável de entrada
O sistema SHALL validar tipos, campos obrigatórios, comprimentos, formato de e-mail, enum de gênero, data de nascimento, IDs, filtros e paginação antes de executar alterações no banco.

#### Scenario: Campo semanticamente inválido
- **WHEN** o cliente envia um JSON bem-formado que viola uma ou mais regras de campo
- **THEN** o sistema retorna `422 Unprocessable Entity` com todas as mensagens de validação aplicáveis em português

#### Scenario: JSON malformado ou corpo ausente
- **WHEN** o endpoint exige corpo e o cliente envia JSON malformado ou nenhum corpo
- **THEN** o sistema retorna `400 Bad Request` no mesmo formato padronizado dos demais erros

#### Scenario: Paginação inválida
- **WHEN** `pagina` é menor que 1 ou `tamanhoPagina` está fora do intervalo de 1 a 100
- **THEN** o sistema retorna `422 Unprocessable Entity` com mensagem que explica os limites aceitos

### Requirement: Envelope de resposta consistente
O sistema SHALL retornar respostas JSON consistentes contendo indicação de sucesso, mensagem, dados e erros quando aplicável; respostas de erro SHALL incluir um `traceId` para correlação.

#### Scenario: Erro de negócio
- **WHEN** ocorre validação, conflito ou recurso não encontrado
- **THEN** o corpo retorna `sucesso: false`, mensagem amigável, dados nulos, erros quando existentes e `traceId`

### Requirement: Diferenciação de falhas
O sistema SHALL mapear erros de validação para `400` ou `422`, recurso inexistente para `404`, conflito de unicidade para `409`, outras regras de negócio para seu código declarado e falhas não tratadas para `500`.

#### Scenario: Exceção de negócio conhecida
- **WHEN** uma exceção de negócio conhecida atravessa a camada de aplicação
- **THEN** o middleware retorna seu código HTTP e mensagem pública sem classificá-la como falha interna

#### Scenario: Exceção inesperada
- **WHEN** ocorre uma exceção não classificada
- **THEN** o middleware registra a exceção e retorna `500 Internal Server Error`

### Requirement: Fallback seguro e depurável
O sistema SHALL fornecer detalhes explícitos de exceção somente no ambiente Development e SHALL ocultá-los nos demais ambientes.

#### Scenario: Falha interna em Development
- **WHEN** ocorre falha inesperada com ambiente Development
- **THEN** a resposta `500` contém `traceId` e bloco `debug` com tipo, mensagem e stack trace

#### Scenario: Falha interna fora de Development
- **WHEN** ocorre falha inesperada em Production ou outro ambiente
- **THEN** a resposta `500` contém mensagem genérica e `traceId`, sem tipo, mensagem interna ou stack trace

