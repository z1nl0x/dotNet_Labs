# AppExampleKafka — onboarding e transferências bancárias

## Nome da aplicação e compatibilidade local

A solução e todos os projetos/assemblies usam o prefixo `AppExampleKafka`. Abra `AppExampleKafka.slnx` no Rider. As variáveis de ferramentas/testes usam `APPEXAMPLEKAFKA_CONNECTION`, `APPEXAMPLEKAFKA_TEST_POSTGRES` e `APPEXAMPLEKAFKA_TEST_KAFKA`; a conexão da API continua usando `ConnectionStrings__DefaultConnection`.

Os identificadores físicos da infraestrutura existente (`apptest_bancario`, usuário `apptest`, projetos Compose `apptest-banking`/`apptest-banking-check` e cluster visual `apptest-local`) foram preservados intencionalmente para não criar volumes/bancos paralelos nem perder acesso aos dados. Referências a `AppTest_Pgsql` e `db_apptest` identificam a infraestrutura antiga e permanecem apenas como instruções/avisos históricos. Nenhum container, volume, tópico ou banco é renomeado por essa alteração.

O issuer e a audience JWT agora usam `AppExampleKafka.API` e `AppExampleKafka.Client`. Tokens de acesso emitidos com os valores antigos precisam ser substituídos por um novo login (ou renovação de sessão).


API .NET 10 / PostgreSQL / Apache Kafka, mantendo **Clean Architecture, CQRS com MediatR, FluentValidation, Repository e Unit of Work** no mesmo conjunto de projetos. Produtos e Categorias foram removidos de todas as camadas e do modelo EF.

## Organização do backend

A organização segue os casos de uso do projeto original, sem DTOs ou contratos de integração dentro de CQRS:

```text
AppExampleKafka.Application/
  DTOs/
    Auth/                  # MeuPerfilDto e contratos de autenticação
    Contas/                # ContaBancariaDto
    Transacoes/            # TransacaoDto
    Usuarios/              # CadastroBancarioDto e seus blocos PF/PJ/endereço
  Features/
    Auth/Register/         # Command, handler e cada validator em arquivo próprio
    Auth/Perfil/           # Query e handler em arquivos próprios
    Contas/{Create,GetById,GetList}/
    Transacoes/{Create,GetById,GetList}/
  Mappings/
    Contas/                # ContaBancariaProfile
    Transacoes/            # TransacaoProfile
    Usuarios/              # UsuarioProfile e CadastroBancarioProfile
  Contracts/Events/        # TransacaoRealizadaV1, separado dos handlers
AppExampleKafka.Domain/
  Entities/Bancario/       # ContaBancaria, Transacao e OutboxMensagem separados
  Entities/Usuarios/       # Usuario e cada tipo cadastral em arquivo próprio
  Enums/                  # TipoPessoa e UserRole
AppExampleKafka.Infraestructure/
  Messaging/              # Publisher, worker, dispatcher e KafkaOptions separados
```

Handlers usam `IMapper`; os profiles são registrados pelo `AddApplication`. Cadastro de entrada e perfil de saída usam DTOs, não instâncias das entidades do domínio. O domínio não depende de Application. A separação preserva os nomes JSON, valores dos enums, regras de validação, rotas e payload PascalCase do evento. Não altera o modelo EF nem exige migration.

`FeatureStructureTests` protege a separação de arquivos/pastas e impede DTOs/eventos nas features. `MappingTests` valida a configuração real do AutoMapper e a fronteira de DTOs. A procura da raiz usa a localização do assembly de testes, não o diretório de execução do comando.

## Escopo e garantias

- `Usuario` mantém senha BCrypt, role, JWT e refresh token rotativo em cookie HTTP-only. Registro público sempre cria `User`; não aceita role do cliente.
- Cadastro PF: CPF, nascimento, nacionalidade, estado civil, profissão e renda mensal.
- Cadastro PJ: CNPJ numérico ou alfanumérico, razão social, nome fantasia, constituição, natureza jurídica, CNAE, faturamento e representante legal (nome, CPF, nascimento).
- Ambos: e-mail, telefone brasileiro, endereço (logradouro, número, complemento, bairro, cidade, UF e CEP). CPF/CNPJ têm validação local dos verificadores; documentos, telefone e CEP chegam **sem máscara** (CNPJ com letras maiúsculas). PF e representante devem ter entre 18 e 120 anos. Valores monetários têm até duas casas.
- São verificações cadastrais locais, **não KYC, prova de identidade, autorização do representante, consulta à Receita ou garantia de conformidade regulatória**. Os exemplos são dados de teste; não use dados reais para experimentar.
- Contas pertencem à identidade do JWT e iniciam **ativas, em BRL e com saldo zero**, tipo `Corrente`, agência fixa `0001`. O servidor gera um número de **12 dígitos numéricos com RNG criptográfico**, preservando zeros à esquerda, e DV de um dígito pelo módulo 11 (pesos 2 a 9 da direita para a esquerda; restos 0/1 resultam em 0). Um índice único no número garante unicidade inclusive sob concorrência; eventual colisão retorna 409 e uma nova criação pode ser tentada. Esses identificadores são locais ao laboratório, não números registrados em instituição financeira. Não existe endpoint de aporte arbitrário, alteração de saldo, exclusão de conta ou exclusão de histórico. Uma integração de entrada de recursos verificada/reconciliação está fora deste exemplo.
- Transferências são internas, entre contas distintas, ativas, da mesma moeda, com valor positivo e saldo suficiente. Apenas o titular da origem pode transferir. Leituras de conta são exclusivas do titular; histórico é visível ao titular de qualquer uma das contas envolvidas. Recursos de terceiros retornam 404, inclusive para Admin.
- Débito, crédito, transação e outbox usam o mesmo DbContext e uma transação **SERIALIZABLE**. O saldo também é um token de concorrência otimista. Conflitos concorrentes retornam 409 em vez de permitir gasto duplo.
- `ChaveIdempotencia` é obrigatória (1–100 caracteres, letras ASCII, números, `.`, `_`, `:`, `-`), única por usuário. Repetição com origem/destino/valor iguais retorna a transação original; conteúdo diferente retorna 409. Sob corrida simultânea, pode retornar 409: o cliente deve repetir **com a mesma chave e conteúdo**. Timeout de rede não justifica gerar outra chave.

## Segurança da base existente — leia antes de migrar

Esta versão contém uma única migration, **`Inicial`**, que cria o esquema bancário completo, incluindo agência, número, dígito verificador, tipo de conta e os índices de unicidade. As migrations anteriores foram substituídas após reversão explícita até zero do banco local `apptest_bancario` em `localhost:15437`, com descarte dos dados autorizado pelo responsável. Não há migration incremental nem backfill do histórico anterior nesta versão.

Para uma base vazia, aplique `dotnet ef database update --project AppExampleKafka.Infraestructure --startup-project AppExampleKafka.API`. **Não aplique esta inicial sobre uma base que ainda tenha o esquema ou o histórico das migrations anteriores.** Consolidar migrations não preserva dados automaticamente. Para alterações futuras, gere migrations incrementais a partir desta `Inicial`; não repita a reversão até zero em uma base com dados que devam ser mantidos.

**Não execute `database update`, não habilite `Database__MigrateOnStartup` e não use o compose apontando para a base antiga `db_apptest` / container `AppTest_Pgsql` da porta 5437.** O compose cria outro PostgreSQL, outro banco (`apptest_bancario`), volumes próprios e porta 15437. Não compartilha o volume antigo. A API não migra automaticamente por padrão.

Para preservar dados existentes: faça backup verificado e desenhe/revise uma migração de adoção específica, incluindo preenchimento dos cadastros, índices de documentos, desativação/remoção controlada do catálogo e reconciliação de saldos. Não apague o histórico de migrations da base e não marque esta inicial como aplicada sem comparar os esquemas. Nada disso é feito automaticamente aqui.

## Subir tudo com Docker

Na pasta `AppExampleKafka` (esta solução):

```bash
docker compose --profile app up -d --build
docker compose ps -a
```

- API: `http://localhost:5080`; Swagger: `http://localhost:5080/swagger` (Development).
- PostgreSQL do exemplo: `localhost:15437`, banco `apptest_bancario`, usuário `apptest`.
- Kafka do host: `localhost:9092`; dentro da rede Docker: `kafka:29092`.
- Kafbat UI: `http://localhost:8082`, cluster `apptest-local`, somente leitura e acessível apenas pela interface local.
- Broker oficial fixado em `apache/kafka:4.1.1`, KRaft combinado (sem ZooKeeper). `kafka-storage-init` prepara a permissão do volume para UID 1000; broker não roda como root. `kafka-init` aguarda healthcheck e cria `banco.transacoes.v1` com três partições. Serviços init terminarem com código 0 é esperado.
- `api` é um profile opcional; com ele, a migration é aplicada por opt-in **somente ao banco novo do compose**. A imagem da API executa com usuário não-root.

Os defaults de senha e chave são **apenas de desenvolvimento**. Opcionalmente, crie `.env` não versionado:

```dotenv
POSTGRES_PASSWORD=defina-uma-senha-local
JWT_SECRET=defina-uma-chave-aleatoria-longa-de-ao-menos-32-bytes
POSTGRES_PORT=15437
KAFKA_PORT=9092
KAFKA_UI_PORT=8082
API_PORT=5080
```

Alterar a senha depois de inicializar um volume PostgreSQL não muda a senha existente; altere-a de forma administrativa ou use **outro volume descartável**, sem apagar dados que queira preservar. Não publique os listeners locais na Internet. Para parar sem apagar dados:

```bash
docker compose --profile app down
```

Use `-p nome-do-ambiente` em **todos** os comandos para isolar projetos. Não execute `down -v` em ambiente com dados importantes.

## Painel web do Kafka

O serviço `kafka-ui` usa `ghcr.io/kafbat/kafka-ui:v1.5.0` e conecta ao listener interno `kafka:29092`. Não use `localhost:9092` como bootstrap dentro desse container. Acesse `http://localhost:8082`, selecione **apptest-local** e abra **Topics → banco.transacoes.v1 → Messages** para visualizar payload, chave, headers, partição e offset. Também estão disponíveis brokers, partições e consumer groups. O painel não depende de uma API .NET em execução.

Está configurado em **somente leitura**, sem edição dinâmica de configuração, com healthcheck e reinício `unless-stopped`. Não permite produzir/apagar mensagens ou alterar tópicos pela interface. Isso não substitui ACLs do broker. Não há login neste ambiente local: qualquer usuário/processo com acesso à porta local pode ler os eventos. Não publique a porta na rede/Internet; acesso remoto exige autenticação, TLS e autorização apropriadas. `KAFKA_UI_PORT` altera somente a porta local publicada.

O projeto Compose já em execução nesta máquina é `apptest-banking-check`. Na pasta `AppExampleKafka`, para iniciar apenas o painel sem recriar PostgreSQL/Kafka existentes:

```bash
docker compose -p apptest-banking-check up -d --no-deps --wait kafka-ui
docker compose -p apptest-banking-check ps kafka-ui
docker compose -p apptest-banking-check logs --tail=100 kafka-ui
```

Use esse mesmo `-p` nos demais comandos desse ambiente; não suba outro projeto nas mesmas portas. `--no-deps` pressupõe que Kafka já esteja saudável. Em um ambiente novo, use `docker compose -p nome-do-ambiente up -d postgres kafka kafka-init kafka-ui`, sem `--no-deps`, para preparar as dependências. Os serviços de inicialização são temporários; PostgreSQL, Kafka e o painel são os três serviços persistentes de infraestrutura. A API em Docker continua opcional.

O painel pode mostrar mensagens de testes anteriores: recriar o PostgreSQL não apaga o histórico do Kafka. Grupos de consumidores podem aparecer vazios e o lag indisponível quando não existem offsets commitados. Taxas de bytes podem ficar indisponíveis sem configuração de métricas/JMX; isso não impede consultar tópicos e mensagens.

## API no host, infraestrutura em containers

Requer SDK .NET 10 e ferramenta `dotnet-ef` compatível com EF Core 10:

```bash
docker compose up -d postgres kafka kafka-init
dotnet restore
dotnet build
export APPEXAMPLEKAFKA_CONNECTION='Host=localhost;Port=15437;Database=apptest_bancario;Username=apptest;Password=somente-dev-troque-esta-senha'
dotnet ef database update --project AppExampleKafka.Infraestructure --startup-project AppExampleKafka.API --connection "$APPEXAMPLEKAFKA_CONNECTION"
export ConnectionStrings__DefaultConnection="$APPEXAMPLEKAFKA_CONNECTION"
export Kafka__BootstrapServers=localhost:9092
export Jwt__SecretKey='somente-dev-chave-longa-com-mais-de-32-caracteres-123456789'
dotnet run --project AppExampleKafka.API --no-launch-profile --urls http://localhost:5080
```

`APPEXAMPLEKAFKA_CONNECTION` é usada pela factory do EF; a API em runtime lê `ConnectionStrings__DefaultConnection`. Defina ambas ao usar configurações próprias. `Database__MigrateOnStartup=false` é o padrão. Se só for testar a API sem broker, `Kafka__Enabled=false` pausa o worker, **não** o registro atômico dos eventos; a outbox fica pendente.

## Swagger e requisições

Com a API já disponível em `http://localhost:5080`, abra `http://localhost:5080/swagger` em Development. Esta seção não exige iniciar serviços nem executar migrations para consultar os exemplos.

### Roteiro no Swagger

1. Expanda `POST /api/auth/register`, clique em **Try it out** e use um dos corpos completos PF/PJ abaixo. Envie exatamente um bloco `pessoaFisica`/`pessoaJuridica`, coerente com `tipoPessoa`. Registro e login retornam **200**, com `accessToken`, `expiresAtUtc` e `user`; o refresh token não vem no JSON.
2. Para usuários já cadastrados, use `POST /api/auth/login`. Copie o `accessToken` retornado; no **Authorize** do esquema HTTP Bearer, cole somente o JWT, sem duplicar o prefixo `Bearer`. A requisição protegida deve conter `Authorization: Bearer {{token}}`. Não envie senha ou refresh token nesse campo.
3. Teste perfil, contas e histórico com o mesmo titular. O registro público cria somente `User`: `admin-only` retorna **403** nesse fluxo, e só retorna 200 com JWT de um Admin previamente provisionado por processo administrativo confiável. Não há endpoint público de promoção.
4. O refresh usa o cookie HTTP-only `refreshToken`, não um corpo JSON. Persistir e reenviar cookies automaticamente depende do cliente, da configuração do cookie jar e, no navegador, das regras de origem/SameSite. Não pressuponha que Swagger, Rider e REST Client compartilhem cookies. Se não houver cookie válido, o refresh retorna **401**; faça login no mesmo cliente e habilite o armazenamento/envio de cookies. O refresh rotaciona o cookie: **copie o novo `accessToken` e atualize tanto `@token` quanto o Authorize do Swagger**. O JWT não é atualizado automaticamente nos exemplos.
5. Ao alternar PF/PJ no mesmo cliente, login/registro substituem o cookie dessa sessão. Mantenha JWT e cookie do mesmo usuário; faça novo login ao trocar de titular. Execute logout por último, com JWT válido e cookie atual: retorna **204**, revoga o refresh e solicita exclusão do cookie, mas não invalida imediatamente um access token já emitido.

### Exemplos executáveis (.http)

Abra `AppExampleKafka.API/AppExampleKafka.API.http` em um cliente compatível, ou copie o bloco abaixo para um arquivo `.http`. Execute **uma requisição por vez**, não o arquivo inteiro: há casos negativos e pré-condições deliberadas. Os dados são fictícios para teste, não prova de identidade/KYC. Em uma base onde já existam esses documentos/e-mails, repetir cadastro retorna **409**; use login em vez de cadastrar novamente.

- Substitua `@token` pelo `accessToken` real de registro/login/refresh, sem o prefixo `Bearer`.
- O roteiro volta ao login PF antes de criar duas contas do mesmo titular. Para repetir como PJ, faça login PJ e atualize token e IDs. Uma conta de destino também pode pertencer a outro usuário, mas sua consulta individual permanece restrita ao titular.
- Copie o `id` da primeira conta criada para `@contaOrigemId` e o da segunda para `@contaDestinoId`. Os GUIDs iniciais são apenas marcadores: sem substituí-los, a transferência não comprova saldo insuficiente e pode retornar 404.
- Ambas começam com **saldo zero**, não há aporte arbitrário nem endpoint oculto de crédito. Com IDs reais de contas novas, a transferência de 25,50 retorna **409 por saldo insuficiente**, não 201. Sem sucesso não há `transacaoId`; o histórico continua vazio. Só teste criação bem-sucedida, leitura por ID e conflito de chave já persistida em um ambiente com recursos legítimos previamente registrados; os testes automatizados usam fixtures controladas, não um seed público.
- Quando houver transferência 201, copie seu `id` para `@transacaoId`. Repita chave e conteúdo iguais para replay 201; altere o conteúdo mantendo a chave já persistida para 409. Em timeout/conflito concorrente, mantenha a mesma chave e conteúdo. Uma tentativa rejeitada por saldo insuficiente não persiste a chave.

```http
# Execute uma requisição por vez; copie os valores reais das respostas.
# Dados fictícios de teste. Cadastros repetidos retornam 409: use login depois.
@host = http://localhost:5080
@token = cole-o-accessToken-sem-o-prefixo-Bearer
@contaOrigemId = 00000000-0000-0000-0000-000000000001
@contaDestinoId = 00000000-0000-0000-0000-000000000002
@transacaoId = 00000000-0000-0000-0000-000000000003
@chaveIdempotencia = pedido-2026-001

### Registro PF — 200; copie accessToken para @token
POST {{host}}/api/auth/register
Content-Type: application/json

{
  "nome": "Ana Teste",
  "email": "ana@example.com",
  "password": "SenhaForte123!",
  "cadastro": {
    "tipoPessoa": "PF",
    "documento": "52998224725",
    "telefone": "11987654321",
    "endereco": {
      "logradouro": "Rua de Teste", "numero": "10", "complemento": "Apto 1",
      "bairro": "Centro", "cidade": "São Paulo", "uf": "SP", "cep": "01001000"
    },
    "pessoaFisica": {
      "dataNascimento": "1990-01-01", "nacionalidade": "Brasileira",
      "estadoCivil": "Solteira", "profissao": "Engenheira", "rendaMensal": 5000.00
    }
  }
}

### Registro PJ — 200; muda o cookie da sessão para o usuário PJ
POST {{host}}/api/auth/register
Content-Type: application/json

{
  "nome": "Empresa de Teste",
  "email": "empresa@example.com",
  "password": "SenhaForte123!",
  "cadastro": {
    "tipoPessoa": "PJ",
    "documento": "11222333000181",
    "telefone": "1133334444",
    "endereco": {
      "logradouro": "Rua da Empresa", "numero": "20", "bairro": "Centro",
      "cidade": "São Paulo", "uf": "SP", "cep": "01001000"
    },
    "pessoaJuridica": {
      "razaoSocial": "Empresa de Teste Ltda", "nomeFantasia": "Empresa Teste",
      "dataConstituicao": "2020-01-01", "naturezaJuridica": "Sociedade limitada",
      "cnae": "6201501", "faturamentoMensal": 10000.00,
      "representanteLegal": {
        "nome": "Ana Teste", "cpf": "52998224725", "dataNascimento": "1990-01-01"
      }
    }
  }
}

### Login PJ — opcional; copie accessToken para @token para operar como PJ
POST {{host}}/api/auth/login
Content-Type: application/json

{ "email": "empresa@example.com", "password": "SenhaForte123!" }

### Login PF — 200; use este login para seguir o roteiro com duas contas PF
POST {{host}}/api/auth/login
Content-Type: application/json

{ "email": "ana@example.com", "password": "SenhaForte123!" }

### Refresh — 200 com cookie válido; sem cookie/expirado/revogado: 401
# Persistir/enviar cookies automaticamente depende do cliente e da configuração.
# Habilite o cookie jar; não envie o refresh token no JSON nem como Bearer.
# A resposta rotaciona o cookie: copie o novo accessToken para @token.
POST {{host}}/api/auth/refresh

### Perfil e cadastro completo do titular do JWT — 200
GET {{host}}/api/auth/me
Authorization: Bearer {{token}}

### Admin — 403 para PF/PJ registrados publicamente; 200 somente com JWT Admin
# Registro público sempre cria User; não há promoção por este roteiro.
GET {{host}}/api/auth/admin-only
Authorization: Bearer {{token}}

### Criar origem — 201; copie id para @contaOrigemId
# Sem corpo: titular vem do JWT; saldo ZERO, BRL, ativa e Corrente.
POST {{host}}/api/contas
Authorization: Bearer {{token}}

### Criar destino — 201; copie outro id para @contaDestinoId
# Mantendo o mesmo token, ambas as contas pertencem ao mesmo titular.
POST {{host}}/api/contas
Authorization: Bearer {{token}}

### Consultar conta do titular — 200 após atualizar @contaOrigemId
GET {{host}}/api/contas/{{contaOrigemId}}
Authorization: Bearer {{token}}

### Listar contas do titular — 200; pageSize de 1 a 100
GET {{host}}/api/contas?pageNumber=1&pageSize=20
Authorization: Bearer {{token}}

### Transferir — 409 saldo insuficiente em contas novas; 201 só com saldo legítimo
# Não existe endpoint de aporte, seed de saldo público ou crédito arbitrário.
# Após 201, copie id para @transacaoId; replay idêntico também retorna 201.
# Em timeout/conflito concorrente, repita a mesma chave e o mesmo conteúdo.
POST {{host}}/api/transacoes
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "contaOrigemId": "{{contaOrigemId}}",
  "contaDestinoId": "{{contaDestinoId}}",
  "valor": 25.50,
  "chaveIdempotencia": "{{chaveIdempotencia}}"
}

### Consultar transação — 200 apenas com id real e titular envolvido; senão 404
# Contas novas com saldo zero não geram transação nem id para esta consulta.
GET {{host}}/api/transacoes/{{transacaoId}}
Authorization: Bearer {{token}}

### Listar histórico do titular — 200; items vazio se não houve transferências
GET {{host}}/api/transacoes?pageNumber=1&pageSize=20
Authorization: Bearer {{token}}

### Erro 400 — valor zero, mesmo com contas existentes e distintas
POST {{host}}/api/transacoes
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "contaOrigemId": "{{contaOrigemId}}",
  "contaDestinoId": "{{contaDestinoId}}",
  "valor": 0,
  "chaveIdempotencia": "erro-valor-zero"
}

### Erro 400 — paginação inválida (máximo 100 por página)
GET {{host}}/api/contas?pageNumber=1&pageSize=101
Authorization: Bearer {{token}}

### Erro 400 — cadastro bancário obrigatório ausente
POST {{host}}/api/auth/register
Content-Type: application/json

{
  "nome": "Cadastro Incompleto",
  "email": "incompleto@example.com",
  "password": "SenhaForte123!"
}

### Erro 409 de idempotência — SOMENTE após a transferência original retornar 201
# Mesma chave já persistida e mesmo par de contas, mas valor diferente.
# Sem transferência anterior, este exemplo NÃO comprova conflito de chave.
POST {{host}}/api/transacoes
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "contaOrigemId": "{{contaOrigemId}}",
  "contaDestinoId": "{{contaDestinoId}}",
  "valor": 26.50,
  "chaveIdempotencia": "{{chaveIdempotencia}}"
}

### Logout — execute por último; 204 com JWT válido e cookie da mesma sessão
# Revoga o refresh do cookie e solicita sua exclusão; não invalida o JWT já emitido.
POST {{host}}/api/auth/logout
Authorization: Bearer {{token}}
```

### Respostas e identificação da conta

`POST /api/contas` funciona sem corpo (ou com `{}`); não recebe saldo, titular ou identificação. Conta e transação criadas retornam **201** com `Location`, inclusive replay idempotente de transação. Registro/login/refresh e leituras retornam 200; logout retorna 204. Validação: **400**; sem JWT válido em rota protegida: **401**; role insuficiente: **403**; recurso inexistente/de terceiro: **404**, inclusive para Admin; saldo/estado/idempotência/concorrência: **409**. Os casos negativos no bloco mostram valor zero, paginação acima de 100 e cadastro ausente (400), saldo zero e chave já persistida com outro conteúdo (409, observadas as pré-condições).

Criação, consulta individual e listagem de contas retornam `id`, `saldo`, `moeda`, `ativa`, `agencia`, `numero`, `digitoVerificador`, `tipoConta` e `criadoEm` (abertura UTC, reutilizando a data original da entidade). Agência, número e DV são strings para preservar zeros à esquerda. Contas são Corrente, agência `0001`, BRL e ativas; número e DV são gerados pelo servidor. Listagens de contas/transações retornam envelope com `items`, `pageNumber`, `pageSize`, `totalCount`, `totalPages`, `hasPreviousPage` e `hasNextPage`; `pageNumber` aceita 1–1000000 e `pageSize`, 1–100. Não há entrada pública para escolher identificação, tipo, titular ou saldo, nem endpoint de exclusão de contas/histórico.

## Kafka e transactional outbox

O handler **não chama Kafka durante a transferência**. Persiste um `TransacaoRealizadaV1` com `EventId` estável na mesma transação do débito/crédito. O `BackgroundService` lê somente registros commitados; PostgreSQL usa `FOR UPDATE SKIP LOCKED` e mantém o lock até ACK + gravação de `PublicadoEm`. Vários workers não publicam simultaneamente a mesma linha. Falhas preservam o pendente e repetem após `Kafka:PollIntervalSeconds`.

O producer usa **Confluent.Kafka real**, `Acks.All`, `EnableIdempotence=true` e criação automática de tópicos desabilitada. Chave Kafka: ID da transação. Headers: `event-id` e `event-type`. Payload JSON versionado (nomes PascalCase):

```json
{
  "EventId": "<guid>", "Versao": 1, "Tipo": "TransacaoRealizada",
  "TransacaoId": "<guid>", "ContaOrigemId": "<guid>", "ContaDestinoId": "<guid>",
  "Valor": 25.50, "Moeda": "BRL", "OcorridoEm": "2026-11-09T12:00:00Z"
}
```

Não inclui nome, e-mail, CPF/CNPJ, senha, telefone ou endereço. IDs de conta e valores continuam sendo dados financeiros sensíveis/pseudonimizados: restrinja acesso, retenção e auditoria do tópico.

Entrega é **at-least-once, não exactly-once**. Se Kafka confirmar e o processo morrer antes do commit de `PublicadoEm`, a mesma mensagem pode ser reenviada. A idempotência do producer não resolve a fronteira banco↔Kafka nem deduplicação após reinício. Consumidores externos devem deduplicar persistentemente por `EventId`. Não há ordem global nem ordem garantida por conta; o particionamento é por transação e o contrato só anuncia transações já concluídas.

Não há consumidor na API: Kafka é a saída de eventos para assinantes externos (notificações, auditoria, integrações). Consumir a própria mensagem para alterar saldo duplicaria um trabalho já commitado. Inspecione o tópico local:

```bash
docker compose exec kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server kafka:29092 --topic banco.transacoes.v1 --from-beginning \
  --property print.key=true --property print.headers=true
```

Monitorar tamanho/idade do backlog da outbox, falhas/retries, latência de ACK e espaço em disco é responsabilidade operacional. Uma mensagem permanentemente inválida requer intervenção (não é descartada silenciosamente). A implementação não inclui expurgo automático, dead-letter, métricas, reconciliação nem ledger contábil de partidas dobradas.

## Testes

Os ciclos verticais iniciaram com testes vermelhos antes de implementar as funcionalidades (domínio, criação/leitura CQRS, autorização, idempotência, onboarding, outbox, producer/worker, endpoints e tratamento de concorrência PostgreSQL). Testes adicionais verificam rollback após escrita e locks concorrentes da outbox.

```bash
# Domínio, aplicação, persistência SQLite e HTTP in-process. Integrações são explicitamente skipped sem opt-in.
dotnet test AppExampleKafka.Tests

# PostgreSQL e broker reais; sobe somente o ambiente novo do compose.
docker compose up -d postgres kafka kafka-init
export APPEXAMPLEKAFKA_TEST_POSTGRES='Host=localhost;Port=15437;Database=apptest_bancario;Username=apptest;Password=somente-dev-troque-esta-senha'
export APPEXAMPLEKAFKA_TEST_KAFKA=true
dotnet test AppExampleKafka.Tests
```

Os testes PostgreSQL criam e removem **somente schemas aleatórios `banking_test_*`** no banco novo `apptest_bancario`, aplicam a migration e abrem contextos separados. Recusam portas 5432/5437. Não usam `EnsureDeleted` nem apagam a base. A concorrência é sincronizada antes do SaveChanges para forçar colisão real, não uma simulação sequencial. O teste de Kafka inicia o worker real, produz e consome a mensagem no broker, e lê a confirmação de volta no PostgreSQL. O tópico pode conservar eventos desses schemas de teste depois da limpeza.

## Produção

Este é um laboratório funcional, não um core bancário pronto para produção. Além da migração revisada, precisam ser tratados: onboarding verificado/KYC e regras legais aplicáveis, ledger e reconciliação, funding autorizado, antifraude/limites, privacidade/LGPD, rate limiting, gestão de credenciais, revogação/concorrência de refresh tokens, backup/restore, métricas, retenção e disaster recovery.

O compose é single-broker de desenvolvimento, com fator de replicação 1, PLAINTEXT e credenciais de exemplo: `Acks.All` nesse ambiente não oferece redundância entre brokers. Produção exige TLS/SASL, ACLs, múltiplos brokers/réplicas, `min.insync.replicas` adequado, serviços separados, segredos externos e HTTPS na borda. Configure `Kafka__SecurityProtocol`, `Kafka__SaslMechanism`, `Kafka__SaslUsername`, `Kafka__SaslPassword` e/ou `Kafka__SslCaLocation` conforme o cluster. Nunca reutilize a chave JWT do exemplo. Migrations devem ser executadas por um processo de deploy controlado, não com usuário da API com permissões de DDL.

**Aviso de dependência preexistente:** AutoMapper 14.0.0 gera NU1903 (GHSA-rvv3-g6hj-g44x). Não foi suprimido nem atualizado automaticamente nesta refatoração; revise atualização/remoção e compatibilidade/licenciamento antes de produção.
