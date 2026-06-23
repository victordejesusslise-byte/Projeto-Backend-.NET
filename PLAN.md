# Plano e situação atual - UsuariosAPI

## Concluído

- [x] Solução .NET 8 em quatro camadas.
- [x] CRUD REST completo e versionado.
- [x] SQL Server SQLEXPRESS03 com EF Core 8.0.28.
- [x] Migrations e script SQL alternativo.
- [x] Validações com FluentValidation.
- [x] Erros padronizados com traceId.
- [x] Swagger/OpenAPI.
- [x] Site simples somente para cadastro.
- [x] 18 testes unitários.
- [x] 11 testes de integração.
- [x] Docker com health check e usuário não root.
- [x] Auditoria NuGet sem vulnerabilidades conhecidas.
- [x] README, documentação técnica e PDF.

## Próximo pacote recomendado antes de expor na internet

- [ ] Autenticação para GET, PUT e DELETE.
- [ ] Autorização administrativa.
- [ ] Rate limiting para POST.
- [ ] Logs estruturados de cadastro, alteração e exclusão.
- [ ] Usuário SQL com privilégio mínimo.
- [ ] HTTPS no ambiente de hospedagem.

## Evoluções opcionais

- [ ] Testes de integração com SQL Server real ou Testcontainers.
- [ ] RowVersion para concorrência.
- [ ] Soft delete.
- [ ] CI/CD com GitHub Actions.
- [ ] Health checks separados para liveness e readiness.

## Critério de liberação

O código está apto para avaliação e publicação no GitHub. A aplicação somente deve ser exposta à internet após a conclusão do pacote de segurança acima.

