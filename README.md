# AML Transaction Monitoring System

Transaction monitoring for a bank. A pending operation is checked by a set of rules against the client's history, an alert with the triggered checks is created, and a compliance officer makes the final decision.

`.NET 10` · `ASP.NET Core` · `EF Core` · `SQL Server` · `Blazor` · `xUnit`

UI is in English. The domain is a Kazakhstani bank (AIFC, local MCC and counterparty lists).

## Architecture

| Project | Role | Depends on |
|---|---|---|
| `Aml.Domain` | Entities and enums | - |
| `Aml.Application` | Rules, screening service, alert factory, interfaces | Domain |
| `Aml.Infrastructure` | EF Core, reference-data cache, context builder, seeders | Application |
| `Aml.Api` | Controllers, JWT, DI | Infrastructure |
| `Aml.AdminPanel` | Blazor UI for officers and admins | calls the API |
| `Aml.Simulator` | Synthetic clients and transactions | Domain, Application, Infrastructure |
| `Aml.Tests` | xUnit tests for rules and screening logic | Application, Domain |

Rules don't know about the database. They work on a prepared `RuleContext`, so they are unit-tested without SQL Server. The infrastructure layer stays thin: it only loads data and computes time windows.

## How it works

### Rule types

| Type | Behavior | Why |
|---|---|---|
| **Hard** | One match blocks the transaction, account and client. Adds 0 to the score. | Signals that are conclusive on their own: sanctions, critical-risk country or merchant. |
| **Typology** | Collects its own points from several checks and triggers only at a confidence threshold. Blocks on its own, not added to the total score. | Behavioral patterns (dropper, drug, crypto, gambling) only mean something when most of the pattern matches. |
| **Scoring** | Weak signals with weights. The transaction is blocked when the sum reaches the threshold (70). | Signals that mean little alone but matter together. |

**Why typologies are kept out of the total score:** if partial typology scores were summed, a client with a few unrelated checks from different typologies could be blocked with no coherent pattern behind it. A typology must match its own pattern to count.

### Rules

| Code | Type | Detects |
|---|---|---|
| `WATCHLIST` | Hard | Client is on an active watchlist |
| `CRITICAL_RISK_COUNTRY` | Hard | Transaction, merchant or counterparty country rated Critical |
| `CRITICAL_RISK_MERCHANT` | Hard | Merchant category (MCC) rated Critical |
| `DROPPER` | Typology | New client, many P2P from different counterparties, immediate withdrawal, empty balance |
| `DRUG` | Typology | Young client, repeated amounts in a typical dose price range, many incoming cards and counterparties, quick cash-out |
| `CRYPTO` | Typology | Uneven amounts, many non-resident counterparties, intrabank transfers, exchanges outside AIFC |
| `GAMBLING` | Typology | Fractional amounts, doubling-stake pattern, foreign merchants (NG/GH), payouts from gambling merchants |
| `HIGH_RISK_COUNTRY` | Scoring | High-risk country plus repeated activity within 3 days |
| `HIGH_RISK_MERCHANT` | Scoring | Several high-risk merchants within 1 hour |
| `HIGH_FREQUENCY` | Scoring | Many operations within 1 hour |

### Two-level context

`RuleContext` is split into `ClientActivity` and `AccountActivity`.

- A balance belongs to one account, so "balance is below 20% of incoming transfers" is checked **per account**.
- A behavioral pattern is checked **per client**. A dropper deliberately splits flows across accounts, so a per-account count would never reach any threshold.

### Pipeline

```
Pending transaction
  -> EfRuleContextBuilder         (one DB load, windows computed in memory)
  -> TransactionScreeningService  (runs 10 rules)
  -> AlertFactory
  -> Alert in DB
  -> Compliance officer decision
```

If any rule type blocks the transaction, the transaction, account and client stay blocked until the officer resolves the alert as FalsePositive.

## Technical decisions

- **Dynamic thresholds.** Rule parameters live in `Rule.ParametersJson`, are loaded into a cache at startup and read by the rules. Constants in the `*Thresholds` classes are a fallback if a parameter is missing or the JSON is broken. Changing a threshold via the API refreshes the cache.
- **Reference-data cache.** Countries, merchants, MCC codes and counterparty lists are kept in memory.
- **Counterparty matching by name.** An incoming P2P transfer has only a name string. Names are normalized on save and looked up in a `HashSet`, so matching is O(1).
- **Audit trail.** Every officer decision, screening run and rule update is written to `AuditLog` with the user and old/new values. In AML this is a regulatory requirement.
- **Roles and alert workflow.** JWT with `Officer` and `Admin` roles. An alert goes `New → UnderReview → Blocked | FalsePositive`. Only the officer who took an alert can resolve it. A false positive restores the client, account and transaction, unless the client has other open alerts. Admin manages rules, runs screening and sees the audit log.
- **Idempotent screening.** Re-screening a transaction with an open alert reuses that alert instead of creating a duplicate.

## API

| Method | Route | Role | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | - | Get a JWT |
| GET | `/api/alerts` | any | Alerts, filter by status |
| GET | `/api/alerts/{id}` | any | Alert details: client, transaction, triggered rules |
| PATCH | `/api/alerts/{id}/take` | officer | Take an alert to review |
| PATCH | `/api/alerts/{id}/resolve` | officer | Resolve as `Blocked` / `FalsePositive` with a comment |
| GET | `/api/rules` | any | Rules with parameters and weights |
| PATCH | `/api/rules/{id}` | admin | Change weight, parameters, active flag |
| GET | `/api/audit` | admin | Audit log with filters |
| POST | `/api/screening/{transactionId}` | admin | Run screening for a transaction |

## Quick start

Requirements: .NET 10 SDK, SQL Server.

```bash
# 1. Set your connection string in Aml/src/Aml.Api/appsettings.json

# 2. Set the JWT key (at least 32 characters)
cd Aml/src/Aml.Api
dotnet user-secrets set "Jwt:Key" "<your-jwt-key>"

# 3. Apply migrations
dotnet ef database update --project ../Aml.Infrastructure --startup-project .

# 4. Run the API (reference data and users are seeded)
dotnet run

# 5. Generate demo clients and transactions
cd ../Aml.Simulator
dotnet run

# 6. Run the admin panel
cd ../Aml.AdminPanel
dotnet run

# Tests
cd ../Aml.Tests
dotnet test
```

To screen a transaction, get an id from the database (e.g. `SELECT t.Id, a.ClientId
FROM Transactions t
JOIN Accounts a ON t.FromAccountId = a.Id OR t.ToAccountId = a.Id
WHERE t.Status = 2
ORDER BY a.ClientId;`), log in as admin and call `POST /api/screening/{transactionId}`.

**Demo accounts** (passwords are hardcoded in the seeder for demonstration only; a real system would never do this):

| User | Password | Role |
|---|---|---|
| `Administrator` | `Admin123!` | Admin |
| `ComplianceOfficer` | `Officer123!` | Officer |

## Demo scenario

The simulator creates one client per scenario:

| Scenario | Rule triggered | Result |
|---|---|---|
| Clean profile | - | Passes |
| Crypto arbitrage | `CRYPTO` | Blocked |
| Dropper | `DROPPER` | Blocked |
| Drug | `DRUG` | Blocked |
| Gambling | `GAMBLING` | Blocked |
| Watchlist | `WATCHLIST` | Blocked |
| Critical-risk country | `CRITICAL_RISK_COUNTRY` | Blocked |
| Critical-risk merchant | `CRITICAL_RISK_MERCHANT` | Blocked |
| High-risk country | `HIGH_RISK_COUNTRY` | Blocked |
| High-risk merchant | `HIGH_RISK_MERCHANT` | Passes (below the threshold) |
| High frequency | `HIGH_FREQUENCY` | Passes (below the threshold) |
| High frequency + high-risk merchant | both | Blocked (sum reaches the threshold) |

**Dynamic thresholds.** Raise `transactionCountLastHourMinRange` for `HIGH_FREQUENCY` and re-screen the same transaction: the rule stops triggering. Set it back and it triggers again.

```http
PATCH /api/rules/{id}
{ "parametersJson": "{\"transactionCountLastHourMinRange\": 50}" }
```

## Deliberate simplifications

These are conscious scope decisions.

- **No fuzzy name matching.** `WatchList` references an already confirmed client; matching happens before a record gets there. Fuzzy matching is a large separate field.
- **No historical balance.** `Account.Balance` is a current snapshot. Reconstructing history would require a ledger.
- **Synthetic reference data.** Made up for the demo, does not reflect official data.
- **No regional risk inside a country.** `Client` has only a free-text address. Geography is covered by country-level rules.
- **Manual screening.** Triggered via `POST /api/screening/{transactionId}`. A background service with a transaction queue is not implemented.
- **Sub-threshold signals are not saved.** A scoring total below the threshold creates no alert. A real system would route these to a watch queue.
- **Alert state and audit use separate `SaveChanges` calls**, without a shared DB transaction. If the audit write fails, the alert decision is already saved.
- **Demo data is tied to `DateTime.Now`.** After a while the 24-hour windows empty out and typologies stop triggering; re-run the simulator.
- **No logging or integration tests.** Covered by 60 unit tests for rules and domain logic.

## Roadmap

- Background screening service with a transaction queue
- Shared DB transaction for alert and audit
- Structured logging
- Integration tests
- Watch queue for sub-threshold signals
- Fuzzy name matching for watchlist screening
- Ledger for historical balance reconstruction
- Structured address and regional risk
- Real reference data (country risk, FATF lists) instead of synthetic
- Event-based alert data (store the context snapshot at alert time instead of relying on `DateTime.Now` windows)
- More typologies and rules, and continued improvements to existing ones

## Screenshots

![Dashboard](docs/dashboard.png)
![Alert details](docs/alert-details.png)
![Rules](docs/rules.png)
