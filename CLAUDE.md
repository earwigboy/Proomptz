# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a **specification-driven development workflow framework** that provides structured slash commands for feature development. The workflow guides users through specification → planning → task generation → implementation phases, with built-in quality checks and constitution-based governance.

## Core Workflow Commands

The framework provides 7 main slash commands (all prefixed with `/speckit.`):

1. **`/speckit.specify <description>`** - Create/update feature specification from natural language
   - Generates branch name, creates spec file in `specs/###-feature-name/`
   - Extracts user stories, requirements, success criteria
   - Validates spec quality with auto-generated checklist
   - Handles up to 3 clarification questions interactively

2. **`/speckit.clarify [context]`** - Reduce ambiguity in the specification (run BEFORE planning)
   - Sequential questioning (max 5 questions)
   - Updates spec incrementally after each answer
   - Focuses on high-impact ambiguities

3. **`/speckit.plan`** - Generate implementation plan with technical decisions
   - Phase 0: Research & technology decisions
   - Phase 1: Data model, API contracts, quickstart scenarios
   - Updates agent context files (`.claude/`, `.cursor/`, etc.)
   - Validates against constitution

4. **`/speckit.tasks`** - Generate dependency-ordered task list organized by user story
   - Tasks grouped by user story (US1, US2, US3...) for independent delivery
   - Checklist format: `- [ ] [TaskID] [P?] [Story?] Description with file path`
   - MVP = User Story 1 (P1 priority)
   - Tests are OPTIONAL unless explicitly requested

5. **`/speckit.checklist <domain>`** - Generate requirements quality checklist for a domain
   - Creates "unit tests for requirements" (NOT implementation tests)
   - Validates completeness, clarity, consistency, measurability, coverage
   - Examples: `ux.md`, `api.md`, `security.md`, `performance.md`
   - Each run creates a new domain-specific checklist file

6. **`/speckit.analyze`** - Cross-artifact consistency analysis (read-only)
   - Runs AFTER `/speckit.tasks` completes
   - Detects duplications, ambiguities, coverage gaps, constitution violations
   - Reports findings with severity levels (CRITICAL/HIGH/MEDIUM/LOW)

7. **`/speckit.implement`** - Execute task-by-task implementation
   - Checks prerequisite checklists before starting
   - Follows TDD if tests requested
   - Marks tasks complete in `tasks.md` as work progresses
   - Creates/verifies ignore files (`.gitignore`, `.dockerignore`, etc.)

## Typical Development Flow

```bash
# 1. Create specification
/speckit.specify "Add user authentication with OAuth2"

# 2. (Optional) Clarify ambiguities
/speckit.clarify

# 3. Generate implementation plan
/speckit.plan

# 4. Generate task list
/speckit.tasks

# 5. (Optional) Quality analysis
/speckit.analyze

# 6. (Optional) Create domain checklists
/speckit.checklist ux
/speckit.checklist security

# 7. Execute implementation
/speckit.implement
```

## Repository Structure

```
.claude/commands/          # Slash command definitions (speckit.*.md)
.specify/
├── memory/
│   └── constitution.md    # Project governance rules (customize per project)
├── templates/             # Templates for spec, plan, tasks, checklists
│   ├── spec-template.md
│   ├── plan-template.md
│   ├── tasks-template.md
│   └── checklist-template.md
└── scripts/bash/          # Workflow automation scripts
    ├── create-new-feature.sh
    ├── setup-plan.sh
    ├── check-prerequisites.sh
    └── update-agent-context.sh

specs/###-feature-name/    # Generated per feature
├── spec.md               # Requirements & user stories
├── plan.md               # Technical implementation plan
├── tasks.md              # Executable task checklist
├── research.md           # Technical research (Phase 0)
├── data-model.md         # Entity definitions (Phase 1)
├── quickstart.md         # Integration scenarios (Phase 1)
├── contracts/            # API specifications (Phase 1)
└── checklists/           # Domain-specific quality checks
```

## Key Architectural Patterns

### User Story Organization

All tasks are organized by **prioritized user stories** (P1, P2, P3...) for:
- Independent implementation (each story is a complete vertical slice)
- Independent testing (each story can be validated standalone)
- MVP delivery (User Story 1 = MVP)
- Incremental deployment

### Constitution-Based Governance

The `.specify/memory/constitution.md` file defines non-negotiable principles that are validated during planning. Common principles:
- Library-first architecture
- CLI interface requirements
- Test-first development (TDD)
- Integration testing contracts
- Observability standards

**CRITICAL**: Constitution violations are always flagged as CRITICAL severity and must be justified or resolved.

### Checklist Philosophy: "Unit Tests for Requirements"

Checklists test **requirement quality**, NOT implementation:
- ❌ WRONG: "Verify button clicks correctly" (tests implementation)
- ✅ CORRECT: "Are button interaction requirements defined with measurable criteria?" (tests requirements)

### Task Format Convention

Every task MUST follow:
```
- [ ] [TaskID] [P?] [Story?] Description with file path
```

- `TaskID`: Sequential (T001, T002...)
- `[P]`: Parallel-safe marker (different files, no dependencies)
- `[Story]`: User story label ([US1], [US2], [US3]...) for story phases only
- Include exact file paths in description

## Scripts & Automation

All bash scripts support:
- `--json` flag for machine-readable output
- `--help` for usage information
- Single quotes in args: use escape syntax `'I'\''m Groot'` or double-quote `"I'm Groot"`

**Key scripts**:
- `.specify/scripts/bash/create-new-feature.sh --json --short-name "feature-name" "description"`
- `.specify/scripts/bash/check-prerequisites.sh --json [--require-tasks] [--include-tasks]`
- `.specify/scripts/bash/setup-plan.sh --json`
- `.specify/scripts/bash/update-agent-context.sh claude` (updates `.claude/` context)

## Important Workflow Rules

1. **Clarification BEFORE Planning**: Run `/speckit.clarify` before `/speckit.plan` to reduce rework risk
2. **Constitution Authority**: Constitution principles are non-negotiable within workflow scope
3. **Max 3 Clarifications in Spec**: Specification generation limits to 3 [NEEDS CLARIFICATION] markers
4. **Max 5 Clarification Questions**: Interactive clarification limited to 5 questions
5. **Tests are Optional**: Only generate test tasks if explicitly requested or TDD approach specified
6. **Foundational Phase Blocks Stories**: Phase 2 (Foundational) MUST complete before ANY user story work
7. **Checklist Gating**: `/speckit.implement` checks all checklists before proceeding (can override)

## Customization Points

When setting up a new project:

1. **Edit `.specify/memory/constitution.md`** with project-specific principles
2. **Adjust templates** in `.specify/templates/` for project conventions
3. **Modify agent context** files (`.claude/`, `.cursor/`, etc.) for AI assistant guidance
4. **Configure ignore patterns** based on tech stack (auto-detected during implementation)

## Success Criteria Patterns

Success criteria MUST be:
- **Measurable**: Specific metrics (time, percentage, count)
- **Technology-agnostic**: No frameworks/languages/tools mentioned
- **User-focused**: Outcomes from user/business perspective
- **Verifiable**: Testable without implementation knowledge

✅ GOOD: "Users can complete checkout in under 2 minutes"
❌ BAD: "API response time is under 200ms" (too technical)

## Common Pitfalls

- Don't run `/speckit.analyze` before `/speckit.tasks` completes
- Don't skip `/speckit.clarify` for complex features
- Don't modify `tasks.md` format - strictly follow checklist pattern
- Don't create implementation-focused checklist items
- Don't exceed question limits (3 in spec, 5 in clarify)
- Don't violate constitution without explicit justification
