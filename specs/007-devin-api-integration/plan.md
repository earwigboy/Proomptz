# Implementation Plan: Devin API Integration

**Branch**: `007-devin-api-integration` | **Date**: 2025-10-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-devin-api-integration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This feature enables users to send prompt templates to Devin AI by clicking a "Send to Devin" button. The system creates a new Devin session via the Devin API and displays a clickable link to the session. The implementation replaces the existing stub `DevinClient` with a real HTTP-based client using .NET's `HttpClient`, adds secure API key configuration, implements error handling, and provides user-friendly feedback for success and error scenarios.

## Technical Context

**Language/Version**: C# .NET 9.0
**Primary Dependencies**: ASP.NET Core 9.0, HttpClient (built-in), FluentValidation 12.0.0
**Storage**: SQLite 9.0.10 (existing), filesystem-based template storage in `/app/data`
**Testing**: xUnit (standard .NET testing framework)
**Target Platform**: Linux/Docker (Alpine-based runtime), multi-stage containerized deployment
**Project Type**: Web application (React 19.1.1 frontend + ASP.NET Core backend with layered architecture)
**Performance Goals**: API response time <5 seconds for Devin session creation (external API-dependent), maintain existing <200ms p95 for internal endpoints
**Constraints**: API key must not be exposed in client-side code or logs, must handle Devin API rate limits gracefully, must work with existing stub interface `IDevinClient`
**Scale/Scope**: Single-user focused application, estimated <100 Devin API calls per day per instance, must support environment variable or appsettings.json configuration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate Evaluation

| Principle | Requirement | Status | Notes |
|-----------|-------------|--------|-------|
| **Code Quality** | Single responsibility, DRY, meaningful names, type safety, error handling | ✅ PASS | Will implement single-purpose `DevinClient` with typed DTOs, proper error handling for HTTP failures |
| **Testing Standards** | Independent tests, fast feedback, 80% coverage for new code, 100% for critical paths | ✅ PASS | Devin integration is NOT a critical path (no auth/payment/data integrity risk); standard 80% coverage applies |
| **UX Consistency** | Consistent error messages, loading states, responsive patterns | ✅ PASS | Will use existing error handling middleware, add loading state to button, follow existing toast notification patterns |
| **Performance** | <200ms p95 for internal APIs, monitoring on critical paths | ✅ PASS | External API call exempt from <200ms requirement; will implement timeout (30s), no performance impact on existing endpoints |
| **Security** | Secure credential storage, no secrets in client code or logs | ✅ PASS | API key in appsettings.json (server-side only), never sent to frontend, masked in logs |
| **Development Workflow** | Feature branch, PR required, CI validation, code review | ✅ PASS | Currently on feature branch `007-devin-api-integration`, will follow standard PR process |
| **Quality Gates** | All tests passing, linting, type checking, no security vulnerabilities | ✅ PASS | Will ensure all gates pass before merge |

### Gate Result: **✅ APPROVED TO PROCEED**

No constitution violations identified. This feature aligns with all established principles:
- Uses existing layered architecture (Core/Application/Infrastructure)
- Follows existing patterns (IDevinClient interface already defined)
- Maintains security requirements (server-side API key storage)
- Meets testing standards (non-critical path, 80% coverage target)
- Respects performance budgets (external API timeout, no blocking of internal APIs)

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
backend/src/
├── PromptTemplateManager.Api/
│   ├── Controllers/
│   │   └── TemplatesController.cs (existing - has SendPrompt endpoint)
│   ├── Program.cs (existing - DI registration)
│   ├── appsettings.json (UPDATE - add Devin config)
│   └── appsettings.Development.json (UPDATE - add dev config)
├── PromptTemplateManager.Application/
│   ├── DTOs/
│   │   ├── SendPromptResponse.cs (existing)
│   │   ├── DevinSessionResponse.cs (NEW)
│   │   └── DevinErrorResponse.cs (NEW)
│   └── Services/ (no changes for this feature)
├── PromptTemplateManager.Core/
│   ├── Interfaces/
│   │   └── IDevinClient.cs (existing - keep interface)
│   └── Exceptions/
│       └── DevinApiException.cs (NEW)
└── PromptTemplateManager.Infrastructure/
    └── DevinIntegration/
        ├── DevinClient.cs (UPDATE - replace stub with real implementation)
        ├── DevinApiOptions.cs (NEW - configuration model)
        └── DevinApiModels.cs (NEW - request/response models)

backend/tests/ (to be created during implementation)
├── PromptTemplateManager.Tests.Unit/
│   └── DevinClientTests.cs (NEW)
└── PromptTemplateManager.Tests.Integration/
    └── DevinIntegrationTests.cs (NEW)

frontend/src/
├── lib/api/ (auto-generated from OpenAPI)
│   └── services/
│       └── TemplatesService.ts (existing - already has sendPrompt)
└── pages/
    └── TemplatesPage.tsx (UPDATE - add loading state, error handling)
```

**Structure Decision**: This is a web application with separate backend and frontend directories. The backend follows a layered architecture pattern (Api → Application → Core ← Infrastructure). The Devin integration lives in the Infrastructure layer (`DevinIntegration/` folder) which is the correct location for external service integrations. The existing `IDevinClient` interface in Core provides the abstraction boundary.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

No constitution violations identified - this section is empty.

---

## Post-Design Constitution Re-Check

*Re-evaluation after Phase 1 design artifacts (data-model.md, contracts/, quickstart.md) completed*

### Design Review Against Constitution

After completing the technical design, we re-evaluate all constitution principles:

| Principle | Requirement | Status | Design Validation |
|-----------|-------------|--------|-------------------|
| **Code Quality** | Single responsibility, DRY, type safety, error handling | ✅ PASS | Design includes: single-purpose DevinClient, typed DTOs (DevinSessionRequest/Response), DevinApiException for structured errors, no code duplication |
| **Testing Standards** | 80% coverage, independent tests, fast feedback | ✅ PASS | Test strategy documented in research.md: unit tests with mocked HttpMessageHandler, 80% target, xUnit + Moq + FluentAssertions |
| **UX Consistency** | Error messages, loading states, consistent patterns | ✅ PASS | Error mapping table defined (research.md): user-friendly messages for all HTTP statuses, loading state in frontend, toast notifications |
| **Performance** | <200ms p95 internal, monitoring | ✅ PASS | Design includes 30s timeout for external API, no impact on internal endpoints, performance benchmarks defined (quickstart.md) |
| **Security** | Secure credentials, no client exposure | ✅ PASS | DevinApiOptions with environment variable support, API key never logged (research.md), server-side only, HTTPS enforced |
| **Development Workflow** | Feature branch, PR, CI, review | ✅ PASS | On feature branch, contracts defined for API validation, integration scenarios documented |
| **Quality Gates** | Tests passing, linting, type checking | ✅ PASS | Checklist included in research.md, security checklist in quickstart.md |

### Design Artifacts Review

| Artifact | Status | Constitution Alignment |
|----------|--------|------------------------|
| `research.md` | ✅ Complete | Follows DRY (IHttpClientFactory), error handling strategy, security considerations, logging patterns |
| `data-model.md` | ✅ Complete | Type-safe DTOs, validation rules, clear entity relationships, no database changes (minimal complexity) |
| `contracts/internal-api-contract.yaml` | ✅ Complete | Documents existing endpoint, OpenAPI 3.0 standard, clear error responses |
| `contracts/devin-api-contract.yaml` | ✅ Complete | Documents external dependency, all error scenarios covered |
| `quickstart.md` | ✅ Complete | Testing scenarios, security checklist, performance benchmarks, troubleshooting guide |

### Post-Design Gate Result: **✅ APPROVED FOR IMPLEMENTATION**

**Summary**:
- Zero constitution violations
- All design artifacts follow established patterns
- Security requirements fully addressed in design
- Testing strategy aligns with 80% coverage requirement
- Error handling provides user-friendly experience
- Performance budgets respected (timeout prevents blocking)

**Justifications**: None required - full constitution compliance.

**Ready for Phase 2**: Task generation with `/speckit.tasks`

---

## Planning Complete - Summary

### Implementation Plan Status: ✅ COMPLETE

**Branch**: `007-devin-api-integration`

**Generated Artifacts**:
- ✅ `plan.md` - This file (comprehensive implementation plan)
- ✅ `research.md` - Phase 0 technical research and decisions
- ✅ `data-model.md` - Phase 1 entity definitions and DTOs
- ✅ `contracts/internal-api-contract.yaml` - Internal API specification
- ✅ `contracts/devin-api-contract.yaml` - External Devin API specification
- ✅ `quickstart.md` - Integration scenarios and testing guide
- ✅ `.claude/context.md` - Updated agent context (if applicable)

**Constitution Compliance**: ✅ FULL COMPLIANCE
- Pre-research gate: PASSED
- Post-design gate: PASSED
- Zero violations requiring justification

**Key Technical Decisions**:
1. **HTTP Client**: IHttpClientFactory with typed HttpClient (built-in .NET)
2. **Configuration**: ASP.NET Core Options pattern with environment variable support
3. **Error Handling**: DevinApiException with user-friendly messages
4. **Testing**: xUnit + Moq + FluentAssertions, 80% coverage target
5. **Security**: API key server-side only, environment variable injection, masked logging

**Files to Modify During Implementation**:
- `backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinClient.cs` (UPDATE)
- `backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinApiOptions.cs` (NEW)
- `backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinApiModels.cs` (NEW)
- `backend/src/PromptTemplateManager.Core/Exceptions/DevinApiException.cs` (NEW)
- `backend/src/PromptTemplateManager.Application/DTOs/SendPromptResponse.cs` (UPDATE - add SessionUrl)
- `backend/src/PromptTemplateManager.Application/DTOs/DevinSessionResponse.cs` (NEW)
- `backend/src/PromptTemplateManager.Application/DTOs/DevinErrorResponse.cs` (NEW)
- `backend/src/PromptTemplateManager.Api/appsettings.json` (UPDATE)
- `backend/src/PromptTemplateManager.Api/appsettings.Development.json` (UPDATE)
- `backend/src/PromptTemplateManager.Api/Program.cs` (UPDATE - HttpClient registration)
- `backend/src/PromptTemplateManager.Api/Controllers/TemplatesController.cs` (UPDATE - error handling)
- `frontend/src/pages/TemplatesPage.tsx` (UPDATE - UI for session URL)
- `backend/tests/PromptTemplateManager.Tests.Unit/DevinClientTests.cs` (NEW)
- `backend/tests/PromptTemplateManager.Tests.Integration/DevinIntegrationTests.cs` (NEW - optional)

**Next Steps**:
1. Run `/speckit.tasks` to generate dependency-ordered task list
2. Run `/speckit.analyze` (optional) for cross-artifact consistency check
3. Run `/speckit.checklist <domain>` (optional) for domain-specific quality checklists
4. Run `/speckit.implement` to execute tasks

**Estimated Complexity**: LOW-MEDIUM
- Replaces stub with real implementation (existing interface)
- No database migrations required
- No breaking changes to existing API
- Standard HTTP client patterns
- Well-defined error scenarios

**Risk Assessment**: LOW
- ✅ Uses existing interface (IDevinClient already defined)
- ✅ No impact on existing features (pure addition)
- ✅ External API well-documented (Devin API reference available)
- ✅ Timeouts prevent hanging requests
- ✅ Comprehensive error handling designed
- ⚠️ External dependency (Devin API availability) - mitigated by clear error messages

---

**Planning Phase Complete**: 2025-10-26

This implementation plan is ready for task generation and implementation. All research complete, all design artifacts generated, and all constitution gates passed.
