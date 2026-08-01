# Task

Execute a single task from a spec file.

## Input

$ARGUMENTS: `path/to/spec.md TN`

Where `TN` is the task number (e.g., `T1`, `T2`).

## Workflow Position

Step 4 of spec-driven development. The spec has been reviewed and approved (steps 1-3). This agent implements one task in a clean session.

## Process

1. Read the spec file
2. Parse Why, What, Constraints, and Current State for context
3. Find task TN in the Tasks section
4. Implement exactly what the task describes — nothing more
5. Run the Verify step from the task
6. Report results

## Rules

- Read AGENTS.md and LESSON.MD for context and methodology
- **Only this task** — ignore all other tasks in the spec
- **Only files listed in the task** — do not touch unlisted files
- **No drive-by refactors** or additions
- **Follow constraints strictly** — especially Must Not
- **Write tests only if specified** in the task
- **Do NOT add dependencies** unless explicitly allowed in Constraints
- **Do NOT modify** files outside the task scope

## Output

After completion, report:

```
## Task TN Complete

### What was implemented
[1-2 sentences describing the changes]

### Files modified
- `path/to/file` — [what changed]
- `path/to/test` — [what was added]

### Verification
[Paste the verification output — test results, build output, etc.]

### Issues or blockers
[Any problems encountered, or "None"]

### Next step
[If more tasks remain: "Run implementer on spec.md TN+1"
 If all tasks complete: "Run validation section from spec.md"]
```

## Failure

If the task fails (build error, test failure, blocked):

1. Report the error clearly
2. Do NOT attempt to fix things outside the task scope
3. Suggest: "Needs reviewer attention" or "Blocked: [reason]"
