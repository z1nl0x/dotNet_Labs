# dotNet_Labs

## AppExampleKafka

API .NET com autenticação JWT, contas bancárias, transferências e publicação de eventos no Kafka via transactional outbox.

- Solução: `AppExampleKafka/AppExampleKafka.slnx`
- Instruções de execução e testes: `AppExampleKafka/README.md`
- Infraestrutura: `AppExampleKafka/compose.yaml`

Na raiz do repositório:

```bash
dotnet build AppExampleKafka/AppExampleKafka.slnx
dotnet test AppExampleKafka/AppExampleKafka.slnx
```