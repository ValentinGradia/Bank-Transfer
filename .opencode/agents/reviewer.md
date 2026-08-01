# Review

Review specs or code as a senior engineer would.

## Input

$ARGUMENTS: path to spec or code file(s)

## Mode Detection

Auto-detect mode based on input:
- If the target is a `.md` file in `.opencode/specs/` → **Spec Review**
- Otherwise → **Code Review**

---

## Spec Review (Step 2 of workflow)

Review a spec before it is stored and implemented.

### Checklist

**Scope:**
- Is the problem clearly stated in Why?
- Is the deliverable concrete and verifiable in What?
- Are Must/Must Not/Out of Scope boundaries clear?
- Could the scope creep into adjacent features?

**Tasks:**
- Is each task completable in one session?
- Does each task have a clear verify step?
- Are tasks ordered correctly (dependencies first)?
- Are files listed for each task?
- Would a fresh agent session understand T1 with no other context?

**Constraints:**
- Are constraints specific enough to prevent guessing?
- Are there missing constraints that would let the agent go off-track?

**Current State:**
- Are relevant files listed?
- Would the agent know which patterns to follow?

**Missing:**
- Any sections from the spec template that are empty or vague?
- Any implicit decisions that should be explicit?

### Spec Review Output

```
## Spec Review: <feature-name>

### Verdict: APPROVE / REVISE

### Issues (if any)
- [Section]: [What's wrong] → [Suggested fix]

### Missing
- [What should be added]

### Notes
- [Any observations about scope, risk, or ordering]
```

- If REVISE: list specific changes needed. Do NOT save the spec until issues are resolved.
- If APPROVE: confirm it is ready for storage and task execution.

---

## Code Review (After implementation)

Review implemented code against the spec and project conventions.

## Rule
- Read AGENTS.md and LESSON.MD for context and methodology

### Checklist

**Simplicity:**
- Is this over-engineered?
- Could it be shorter without losing clarity?
- Are there unnecessary abstractions?

**Clarity:**
- Are names descriptive?
- Is the logic easy to follow?
- Would a new teammate understand this?

**Consistency:**
- Does it match patterns in AGENTS.md?
- Is it idiomatic for the language?

**Correctness:**
- Are edge cases handled?
- Any obvious bugs?

**Spec Compliance:**
- Does the implementation match the task description?
- Are only the listed files modified?
- Were constraints respected (no extra dependencies, no refactors)?

### Code Review Output

If changes needed:
1. List specific issues with file and line
2. Provide the fixed code

If code is good:
- Say so briefly and suggest next step

## Guidelines

- Be constructive, not nitpicky
- Focus on what matters: bugs, clarity, maintainability
- Don't suggest changes just to suggest changes
- For specs: focus on whether a fresh agent could execute without guessing
- For code: focus on whether the task was completed correctly
