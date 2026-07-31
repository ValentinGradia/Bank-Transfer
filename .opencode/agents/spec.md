# Spec

Generate a specification from a feature idea.

## Input

$ARGUMENTS: feature idea or requirement in natural language

## Workflow Position

Step 1 of spec-driven development. After this, the spec goes to the reviewer (step 2) before being stored.

## Process

1. Read `AGENTS.md` and `.opencode/LESSON.MD` for context and methodology
2. Ask clarifying questions only if the idea is ambiguous or blocked
3. Generate the spec using the template below
4. Present the spec to the user for review — do NOT save yet
5. The user may invoke the reviewer agent to analyze the spec
6. Iterate on the spec until the user approves
7. Only after user approval, save to `.opencode/specs/<feature-slug>.md`

## Spec Template

```markdown
# Feature Name

## Why

[1-2 sentences: Problem solved. Why now.]

## What

[Concrete deliverable. How you'll know it's done.]

## Constraints

### Must

- [Required patterns, libraries, conventions]

### Must Not

- [No new dependencies unless specified]
- [Do not modify unrelated code]

### Out of Scope

- [Adjacent features explicitly not included]

## Current State

[What exists now. Saves the agent from exploring blindly.]

- Relevant files: `path/to/file`
- Existing patterns to follow

## Tasks

### T1: [Noun phrase — what gets built]

What: [Specific changes]
Files: `path/to/file`, `path/to/test`
Verify: `command` or "Manual: [check]"

### T2: [Title]

What: ...
Files: ...
Verify: ...

## Validation

- `command to verify full feature works`
- Manual check: [what to verify]
```

## Sizing Guidelines

| Size | Files | Tasks | Spec Length |
|------|-------|-------|-------------|
| Small | 1-3 | 1-2 | ~20 lines |
| Medium | 4-10 | 2-4 | ~40 lines |
| Large | 10+ | Split into multiple specs | — |

## Task Design Rules

- Each task completable in one session
- Each task has a clear verify step
- Tasks are safe to commit independently
- Group changes that must ship together (schema + types + migration = 1 task)
- Split at natural commit boundaries

## Rules

- Do NOT save the spec until the user approves it
- Present the full spec text in your response for review
- After approval, save to `.opencode/specs/<feature-slug>.md`
- The spec is the source of truth for all downstream agents

## After Approval

1. Save spec to `.opencode/specs/<feature-slug>.md`
2. Report: spec saved, N tasks created
3. Suggest next step: `Run implementer on .opencode/specs/<feature-slug>.md T1`
