## ADDED Requirements

### Requirement: Histórico estruturado de operações de usuário
O sistema SHALL registrar em log estruturado cada cadastro, atualização e exclusão concluídos com sucesso.

#### Scenario: Cadastro concluído
- **WHEN** um novo usuário é persistido com sucesso
- **THEN** o sistema registra o evento `UsuarioCadastrado` com ID, timestamp, operação e correlação da requisição

#### Scenario: Atualização concluída
- **WHEN** os dados de um usuário são atualizados com sucesso
- **THEN** o sistema registra o evento `UsuarioAtualizado` com ID, timestamp, operação e correlação da requisição

#### Scenario: Exclusão concluída
- **WHEN** um usuário é removido com sucesso
- **THEN** o sistema registra o evento `UsuarioRemovido` com ID, timestamp, operação e correlação da requisição

### Requirement: Persistência simples e rotativa
O sistema SHALL escrever o histórico em console e arquivo com rotação diária e SHALL documentar a localização e retenção configuradas.

#### Scenario: Mudança de dia
- **WHEN** a aplicação permanece ativa após a data de rotação
- **THEN** novos eventos são gravados em um novo arquivo sem sobrescrever o arquivo anterior

### Requirement: Proteção de dados no histórico
O sistema SHALL evitar nome, e-mail, corpo de requisição, senha ou outros dados pessoais desnecessários nos eventos de histórico.

#### Scenario: Inspeção do histórico
- **WHEN** um operador lê um evento de usuário
- **THEN** identifica a operação e o registro afetado sem encontrar o payload ou e-mail do usuário

### Requirement: Falhas não geram sucesso falso
O sistema SHALL emitir o evento de histórico somente depois que a operação de persistência for concluída com sucesso.

#### Scenario: Persistência falha
- **WHEN** cadastro, atualização ou exclusão falha antes da confirmação no banco
- **THEN** nenhum evento de operação concluída é registrado
