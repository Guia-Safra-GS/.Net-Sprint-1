# AgroMonitor API - Cadastro

API .NET do projeto **AgroMonitor**, desenvolvida como parte da Global Solution FIAP 2026/1 (2TDS, Advanced Business Development with .NET). É a metade de **cadastro** de uma solução de monitoramento agrícola inteligente para pequenos e médios produtores — o tema da GS parte da economia espacial (dados climáticos derivados de satélite) aplicada a um problema real no campo: saber quando e quanto irrigar, e prever risco de geada.

Esta API cuida do catálogo do sistema: as **especies** (espécies de plantas, com suas faixas ideais de umidade e sensibilidade a geada) e os **slots** (as vagas de plantio monitoradas). O projeto tem uma irmã em Java (**GuiaSafra**, Java Advanced) que cuida da operação — leituras dos sensores ESP32, previsão climática, regas e alertas. As duas APIs compartilham o mesmo banco Oracle da FIAP via um schema único que serve como contrato entre elas: cada uma é dona da escrita das suas próprias tabelas e apenas lê as do outro contexto quando precisa.

## Links da entrega

- **Repositório:** `https://github.com/Guia-Safra-GS/.Net-Sprint-1`
- **Vídeo demonstração (até 8 min):** `https://youtu.be/X3w-u3gO9AU`
- **Vídeo pitch (até 3 min):** `https://youtu.be/ZNpZ35j8NtU`
- **Documentação da API (Swagger):** roda localmente em `http://localhost:5128/` (ver seção "Como executar")

## Sobre a divisão por domínio

A escolha foi separar as duas APIs por **bounded context** (contexto delimitado), e não por questões técnicas como performance. A ideia é simples: uma API cuida do *cadastro* (o que existe na fazenda — especies e vagas), a outra cuida da *operação* (o que acontece com elas — medições, regas, alertas). Essa fronteira fica explícita na divisão das tabelas: cada lado escreve só no que é seu.

Esta API (.NET) é dona de escrita das seguintes tabelas:

- `TB_CAD_SPECIES` — catálogo de especies; guarda a **regra de negócio agronômica** (faixa de umidade tolerada, volume de rega, temperatura de risco de geada)
- `TB_CAD_SLOT` — cada vaga de plantio monitorada; relação 1:N com espécie

A API Java escreve em `TB_CAD_USER`, `TB_MON_CLIMATE_FORECAST`, `TB_MON_READING`, `TB_MON_WATERING_EVENT` e `TB_MON_ALERT`, e **lê** a `TB_CAD_SLOT` (somente leitura) para validar a FK antes de registrar uma leitura ou um alerta. A integridade entre os dois domínios é garantida pelas FKs do próprio Oracle.

## Stack técnica

| Camada | Tecnologia |
|---|---|
| Linguagem | C# 13 (.NET 9) |
| Framework | ASP.NET Core 9 (Web API com controllers) |
| Arquitetura | Clean Architecture (Domain / Application / Infrastructure / API) |
| Persistência | Entity Framework Core 9 |
| Provider | Oracle.EntityFrameworkCore 9.23 (ODP.NET managed) |
| Banco de dados | Oracle 19c (FIAP) |
| Validação | DataAnnotations + ModelState |
| Tratamento de erros | `IExceptionHandler` + ProblemDetails (RFC 7807) |
| Documentação | Swagger / Swashbuckle |
| Migrations | EF Core Migrations (code-first) |
| Build | .NET SDK / MSBuild |

## Como executar

Pré-requisitos no ambiente:

- .NET SDK 9 instalado e disponível no `PATH`
- Acesso à rede da FIAP (necessário para alcançar `oracle.fiap.com.br`)
- Credenciais do banco Oracle FIAP (RM e senha)
- Schema da FIAP populado executando o arquivo `gs_agromonitor.sql` no SQL Developer logado com seu RM (cria as tabelas, as procedures e a carga de dados)

Configuração de credenciais. As credenciais **não ficam versionadas**. O `appsettings.json` traz só placeholders; os dados reais vão no `appsettings.Development.json` (que está no `.gitignore`):

```json
{
  "ConnectionStrings": {
    "AgroMonitorOracle": "User Id=SEU_RM;Password=SUA_SENHA;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SID=ORCL)))"
  }
}
```

Em produção/deploy, a mesma connection string pode vir da variável de ambiente `ConnectionStrings__AgroMonitorOracle`, que sobrescreve o arquivo.

Rodando a aplicação:

```bash
dotnet run --project AgroMonitor.API
```

Por padrão a API sobe em `http://localhost:5128`, e o **Swagger UI abre na raiz** (`http://localhost:5128/`). Para forçar outra porta, use `--urls "http://localhost:XXXX"`.

## Arquitetura da aplicação

A API segue **Clean Architecture**, dividida em quatro projetos com a regra de dependência apontando sempre para dentro (o domínio não conhece ninguém; a infraestrutura e a API dependem do domínio, nunca o contrário).

```
        API  ───►  Application  ───►  Domain
         │              ▲
         └──►  Infrastructure  ──────┘
   (Controllers)   (EF Core, Oracle)
```

- **Domain** — as entidades (`Species`, `Slot`), a `BaseEntity` e a `DomainException`. É a única camada sem nenhuma dependência de framework. As entidades são **ricas**: validam as próprias regras no construtor e expõem `set` privado, então não existe objeto inválido no sistema.
- **Application** — os casos de uso (`SpeciesService`, `SlotService`), os contratos de repositório (`IRepository<T>`, `ISpeciesRepository`) e os DTOs (records de Request/Response). É aqui que mora a regra de negócio e a orquestração.
- **Infrastructure** — o `DbContext` (EF Core), as configurações de mapeamento (`IEntityTypeConfiguration`), a implementação dos repositórios e as Migrations. É a única camada que conhece o Oracle.
- **API** — os Controllers REST, o `GlobalExceptionHandler`, a configuração de injeção de dependência e o Swagger.

A conversão DTO ↔ Entity acontece nas bordas: `request.ToDomain()` na entrada, `Response.FromDomain(entity)` na saída — sempre com records imutáveis.

Decisões que valem destacar:

**Id numérico (long) em vez de Guid.** O projeto de referência da disciplina usava `Guid`, mas aqui o banco é compartilhado: as PKs são `NUMBER(19) GENERATED BY DEFAULT ON NULL AS IDENTITY` e a API Java já lê esses ids como número. Manter `Guid` quebraria a integração real. Por isso a `BaseEntity` usa `long`, e o EF mapeia a coluna como identity (`ValueGeneratedOnAdd`) — o banco gera o id e o EF lê de volta via `RETURNING`.

**Remoção lógica do slot.** Deletar um slot **não apaga a linha** — o `SlotService.Delete` marca o `Status` como `INACTIVE` e salva. Isso preserva o histórico que o domínio Java referencia por FK (leituras, regas e alertas continuam apontando para o slot). Apagar de verdade quebraria esse histórico.

**Exclusão de espécie é restrita.** Já a espécie usa FK com `DeleteBehavior.Restrict`: tentar deletar uma especies que ainda tem slots vinculados devolve **409 Conflict**, porque apagar a regra agronômica deixaria os slots órfãos. É a resposta direta para a pergunta "o que acontece com os dados relacionados ao deletar um registro?": slot → desativa; espécie com filhos → bloqueia.

**Valor derivado, não confiado no cliente.** As regras de negócio das especies (faixa de umidade, volume de rega) são validadas no domínio: umidade entre 0 e 100, mínima menor que a máxima, volume maior que zero. Qualquer violação vira `DomainException` → 400, antes de tocar no banco.

**Validação em três camadas.** Uma entrada inválida é barrada o mais cedo possível:
1. **DataAnnotations + ModelState** no controller — formato (campo obrigatório, tamanho, faixa). Falhou → **400**.
2. **Service** — existência de FK (ex.: antes de criar um slot, confere se a espécie existe via `ExistsById`). Não existe → **404** com mensagem clara.
3. **Banco (FK/constraints)** — rede de segurança final; se algo escapar, o `GlobalExceptionHandler` captura e padroniza.

**Tratamento global de erros (ProblemDetails).** O `GlobalExceptionHandler` (um `IExceptionHandler`) converte exceções em respostas JSON consistentes no padrão RFC 7807:

- `DomainException` / `ArgumentException` → **400**
- `KeyNotFoundException` → **404**
- `InvalidOperationException` (ex.: nome de especies duplicado) → **409**
- `DbUpdateException` (violação de constraint do banco, como a FK de exclusão restrita) → **409**
- `OracleException` (banco indisponível) → **502**

**Repositório genérico + específico.** `IRepository<T>` cobre o CRUD comum (síncrono, seguindo o padrão da disciplina). Onde uma entidade precisa de uma consulta própria, há um contrato específico — `ISpeciesRepository.ExistsByCommonName`, usado para garantir que não existam duas especies com o mesmo nome.

## Endpoints principais

Documentação interativa completa no Swagger UI em `http://localhost:5128/`. Abaixo os fluxos mais relevantes. Os caminhos seguem a convenção `api/[controller]`.

### Cadastrar uma especie

```http
POST /api/Species
Content-Type: application/json

{
    "commonName": "Alface",
    "scientificName": "Lactuca sativa",
    "minHumidity": 60,
    "maxHumidity": 85,
    "frostMinTemp": 2,
    "wateringMl": 200
}
```

O nome comum é único — se já existir, retorna **409**. Faixas de umidade fora de 0–100, mínima ≥ máxima ou volume ≤ 0 retornam **400**. Resposta **201** com a especie criada:

```json
{
    "id": 1,
    "commonName": "Alface",
    "scientificName": "Lactuca sativa",
    "minHumidity": 60,
    "maxHumidity": 85,
    "frostMinTemp": 2,
    "wateringMl": 200,
    "createdAt": "2026-06-08T21:48:26Z"
}
```

### Listar e buscar especies

```http
GET /api/Species
GET /api/Species/1
```

`GET` de listagem devolve todas as especies ordenadas por id; `GET /{id}` devolve uma só (ou **404** se não existir).

### Atualizar e remover uma especie

```http
PUT /api/Species/1        # atualiza, revalidando as regras de domínio
DELETE /api/Species/1     # 409 se houver slots vinculados (FK Restrict); 204 se não houver
```

### Cadastrar um slot (vaga de plantio)

```http
POST /api/Slot
Content-Type: application/json

{
    "speciesId": 1,
    "position": "A1",
    "plantedAt": "2026-03-01"
}
```

O `speciesId` é a FK para a especie e é **validado antes** de inserir — se a espécie não existir, retorna **404** ("Espécie 999 não encontrada"). O slot nasce com `status = ACTIVE`. Resposta **201**:

```json
{
    "id": 1,
    "speciesId": 1,
    "position": "A1",
    "plantedAt": "2026-03-01",
    "status": "ACTIVE",
    "createdAt": "2026-06-08T21:48:26Z"
}
```

### Atualizar e remover um slot

```http
PUT /api/Slot/1           # atualiza posição/data/espécie (revalida a FK)
DELETE /api/Slot/1        # remoção LÓGICA: status vira INACTIVE, retorna 204
```

Depois do `DELETE`, um `GET /api/Slot/1` ainda retorna o slot, mas com `"status": "INACTIVE"` — o registro e o histórico ligado a ele são preservados.

## Testando localmente

Com a aplicação rodando, o caminho mais rápido para um teste end-to-end é o próprio Swagger UI (`http://localhost:5128/`), que lista todos os endpoints com schema e permite disparar requests.

Roteiro sugerido:

1. `GET /api/Species` — confirma as especies que vieram do seed (`gs_agromonitor.sql`)
2. `POST /api/Species` — cria uma especie nova (201)
3. `POST /api/Species` com o mesmo nome — confirma o **409** de duplicidade
4. `POST /api/Slot` com `speciesId` válido — cria a vaga (201)
5. `POST /api/Slot` com `speciesId` inexistente — confirma o **404**
6. `DELETE /api/Slot/{id}` e depois `GET` — confirma a **remoção lógica** (status `INACTIVE`)
7. `DELETE /api/Species/{id}` de uma especie com slots — confirma o **409** (FK Restrict)

Se os erros vierem como JSON limpo no formato ProblemDetails (`{ "title": "...", "status": 404, "detail": "..." }`), o tratamento global está funcionando. Se aparecer stack trace, algo escapou do `GlobalExceptionHandler`.

## Estrutura de pastas

```
AgroMonitor/
├── AgroMonitor.sln
├── global.json
├── AgroMonitor.Domain/
│   ├── Common/BaseEntity.cs
│   ├── Entities/Species.cs, Slot.cs
│   └── Exceptions/DomainException.cs
├── AgroMonitor.Application/
│   ├── DTOs/ (SpeciesRequest/Response, SlotRequest/Response)
│   ├── Repositories/ (IRepository, ISpeciesRepository)
│   └── Services/ (Interfaces + Implementations)
├── AgroMonitor.Infrastructure/
│   └── Persistence/
│       ├── AgroMonitorContext.cs
│       ├── AgroMonitorContextFactory.cs   (design-time, para o EF Tools)
│       ├── Configurations/ (SpeciesConfiguration, SlotConfiguration)
│       ├── Repositories/ (Repository<T>, SpeciesRepository)
│       └── Migrations/ (InitialCreate)
└── AgroMonitor.API/
    ├── Controllers/ (SpeciesController, SlotController)
    ├── Exceptions/GlobalExceptionHandler.cs
    ├── Extensions/AgroMonitorServiceCollectionExtensions.cs
    ├── Program.cs
    └── appsettings.json
```

## Schema do banco e Migrations

As tabelas do AgroMonitor são criadas pelo script `gs_agromonitor.sql` (compartilhado com as disciplinas de banco e Java), que contém o DDL das tabelas, procedures, carga de dados e a automação PL/SQL.

O projeto também versiona uma **Migration code-first** (`InitialCreate`), gerada via EF Core, que descreve as tabelas `TB_CAD_SPECIES` e `TB_CAD_SLOT` — com o identity, a FK 1:N e o índice único do nome. O papel dela é documentar e versionar o schema deste domínio: num banco limpo, um `dotnet ef database update` cria as tabelas; no banco compartilhado da FIAP, elas já vêm do script da disciplina de banco, então a migration serve como registro do modelo.

> Detalhe de compatibilidade: o provider Oracle é configurado com `UseOracleSQLCompatibility(DatabaseVersion19)`. Sem isso, o EF Core 9 gera literais booleanos `TRUE/FALSE` em consultas com `EXISTS`, que o Oracle 19c não entende (ele só ganhou tipo booleano em SQL na versão 23). Com o ajuste, o SQL sai compatível (`1`/`0`).

## Requisitos da disciplina cobertos

A entrega cumpre, dentro desta API:

- **API REST com boas práticas e arquitetura** — Clean Architecture em 4 projetos, controllers finos, separação Request DTO / Response DTO / Entity, domínio rico
- **Persistência em banco relacional** — EF Core 9 com provider Oracle
- **Relacionamento 1:N** — `Species` 1—N `Slot` (FK `SPECIES_ID`), com exclusão restrita
- **Uso correto de Migration** — Migration `InitialCreate` code-first versionada
- **CRUD completo nas duas entidades** com status HTTP corretos (200, 201, 204, 400, 404, 409) — validado endpoint a endpoint
- **Validação de entrada** — DataAnnotations + validação de FK no service + constraints do banco
- **Tratamento global de exceções** com JSON consistente (ProblemDetails / RFC 7807)
- **Documentação Swagger/OpenAPI** com comentários XML, summaries e ApiResponses

## Limitações conhecidas

- **Sem autenticação.** A API de cadastro não tem login nem token — o escopo da disciplina foca em REST, EF Core, relacionamento e Migration. Autenticação seria uma sprint à parte (e, na solução completa, o domínio de usuários/JWT vive na API Java).
- **Migration não aplicada no banco compartilhado.** Como o schema da FIAP é criado pelo script PL/SQL, a migration não é executada lá (`update-database`) para não duplicar o schema — ela existe como artefato code-first. Num banco próprio, seria aplicada normalmente.
- **`createdAt` em UTC.** O timestamp de criação é gravado em UTC (`DateTime.UtcNow`), aparecendo com sufixo `Z`. É consistente, mas não converte para o fuso local.

## Próximos passos

Itens que não entraram nesta entrega mas fariam sentido evoluir:

- Paginação e filtros nas listagens (hoje devolvem a lista completa)
- Autenticação e autorização
- Endpoint para reativar um slot (hoje a remoção lógica só desativa)
- Testes automatizados (unitários nos services, integração nos controllers) com um provider em memória
- Coleção de testes (Insomnia/Bruno) versionada em `docs/`, como na API Java
