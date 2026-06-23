## 1. Aprovações e linha de base

- [x] 1.1 Confirmar com o usuário se as renomeações abrangem apenas arquivos, pastas e tipos internos ou também projetos, namespaces e solution
- [x] 1.2 Confirmar com o usuário se o Docker Compose deve subir SQL Server próprio ou conectar ao `SQLEXPRESS02` do host
- [x] 1.3 Perguntar e registrar se algum extra opcional será tratado em mudança separada: Serilog, rate limiting ou health check detalhado
- [x] 1.4 Executar build e testes atuais, registrar falhas preexistentes e conferir que `.env` permanece ignorado

## 2. Pacote SQL Server e execução

- [x] 2.1 Apresentar ao usuário a lista exata de arquivos do pacote SQL Server e obter aprovação antes das edições
- [x] 2.2 Remover o serviço MySQL do `docker-compose.yml` e configurar a API para conectar ao SQL Server do host
- [x] 2.3 Corrigir `.env.example`, configurações e instruções de ambiente para SQL Server sem incluir credenciais reais
- [x] 2.4 Converter `database/init.sql` para T-SQL e alinhá-lo ao modelo e à migration EF Core
- [x] 2.5 Revisar mapeamento, índice único e migration para garantir consistência de tipos, limites e constraints
- [ ] 2.6 Validar build, migration e subida dos containers do pacote aprovado

## 3. Pacote REST, validações e fallback

- [x] 3.1 Apresentar ao usuário a lista exata de arquivos do pacote REST/erros e obter aprovação antes das edições
- [x] 3.2 Padronizar o envelope de resposta com `traceId` em erros e bloco `debug` opcional
- [x] 3.3 Configurar respostas de model binding, corpo ausente e JSON malformado no mesmo contrato da API
- [x] 3.4 Completar validações de IDs, paginação, filtros e DTOs com mensagens amigáveis em português
- [x] 3.5 Aprimorar o middleware para separar erros de negócio de falhas internas e expor debug somente em Development
- [x] 3.6 Mapear violação concorrente da constraint única de e-mail para `409 Conflict`
- [x] 3.7 Revisar os cinco endpoints, códigos HTTP, header `Location`, cancelamento e rota `/api/v1/usuarios`
- [ ] 3.8 Completar metadados e exemplos OpenAPI dos parâmetros, payloads e respostas
- [x] 3.9 Executar build e testes focados no contrato REST e no fallback

## 4. Pacote de nomenclatura e arquitetura

- [ ] 4.1 Apresentar ao usuário o mapa completo de renomeações e obter aprovação antes de mover ou renomear qualquer arquivo funcional
- [ ] 4.2 Aplicar somente o alcance aprovado, usando nomes em português e PascalCase por responsabilidade
- [ ] 4.3 Atualizar namespaces, referências, registros de injeção e testes afetados pelas renomeações
- [ ] 4.4 Preservar `Program.cs`, contratos REST/JSON e demais exceções de convenção aprovadas
- [ ] 4.5 Compilar a solution e executar os testes após as renomeações

## 5. Pacote de testes e segurança

- [x] 5.1 Apresentar ao usuário os novos casos e eventual troca do banco de teste por SQLite in-memory e obter aprovação
- [ ] 5.2 Ampliar testes unitários para filtros, paginação, atualização, exclusão, validações e classificação de erros
- [ ] 5.3 Ampliar testes de integração para todos os sucessos e principais códigos `400`, `404`, `409`, `422` e `500`
- [x] 5.4 Testar que detalhes de debug aparecem em Development e não aparecem em Production
- [ ] 5.5 Testar unicidade de e-mail e tratamento inerte de entradas com metacaracteres SQL ou marcação HTML
- [ ] 5.6 Executar toda a suíte e gerar relatório de cobertura quando a ferramenta estiver disponível

## 6. Pacote de histórico de operações

- [ ] 6.1 Apresentar ao usuário os arquivos e o formato do histórico Serilog e obter aprovação antes das edições funcionais
- [ ] 6.2 Configurar Serilog com saída em console e arquivo rotativo ignorado pelo controle de versão
- [ ] 6.3 Registrar eventos estruturados após cadastro, atualização e exclusão concluídos, sem dados pessoais desnecessários
- [ ] 6.4 Adicionar testes que confirmem a emissão dos eventos apenas após operações bem-sucedidas
- [ ] 6.5 Documentar localização, formato, retenção e leitura dos arquivos de histórico

## 7. Pacote de documentação

- [ ] 7.1 Apresentar ao usuário o índice das mudanças documentais e obter aprovação antes das edições
- [ ] 7.2 Reescrever o `README.md` com execução Docker/local em SQL Server e tutorial de GET, POST, PUT e DELETE via Swagger e linha de comando
- [ ] 7.3 Atualizar `PLAN.md` com status real, critérios de aceite e itens ainda pendentes
- [ ] 7.4 Atualizar `DESIGN.md` com fluxo por camadas, mapa nome-responsabilidade, modelo SQL Server e contrato de erros
- [ ] 7.5 Remover referências restantes a MySQL e conferir rotas, campos, portas e exemplos contra o código executado

## 8. Verificação final

- [ ] 8.1 Executar restore, build e todos os testes sem falhas
- [ ] 8.2 Subir a solução conforme a opção de banco aprovada e confirmar health check e Swagger
- [ ] 8.3 Realizar smoke test sequencial de POST, GET lista/filtros, GET por ID, PUT e DELETE
- [ ] 8.4 Verificar respostas de validação, conflito, não encontrado e fallback nos ambientes Development e Production
- [ ] 8.5 Apresentar ao usuário o relatório final de arquivos alterados, comandos de uso e resultados de verificação
