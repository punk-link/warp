---
mode: 'agent'
description: 'Create GitHub Issues.'
tools: ['codebase', 'search', 'github', 'create_issue', 'search_issues', 'update_issue']
---
# Create GitHub Issues for Unmet Specification Requirements

Create GitHub Issues for unimplemented requirements in the specification at `${file}`.

## Process

1. Analyze specification file to extract all requirements
2. Check codebase implementation status for each requirement
3. Search existing issues using `search_issues` to avoid duplicates
4. Create new issue per unimplemented requirement using `create_issue`
5. Use `feature_request.yml` template (fallback to default)

## Requirements

- One issue per unimplemented requirement from specification
- Clear requirement ID and description mapping
- Include implementation guidance and acceptance criteria
- Verify against existing issues before creation

## Issue Content

- Title: Requirement ID and brief description
- Description: Detailed requirement, implementation method, and context
- Labels: feature, enhancement (as appropriate)

## Implementation Check

- Search codebase for related code patterns
- Check related specification files in `/spec/` directory
- Verify requirement isn't partially implemented


# Create GitHub Issue from Implementation Plan

Create GitHub Issues for the implementation plan at `${file}`.

## Process

1. Analyze plan file to identify phases
2. Check existing issues using `search_issues`
3. Create new issue per phase using `create_issue` or update existing with `update_issue`
4. Use `feature_request.yml` or `chore_request.yml` templates (fallback to default)

## Requirements

- One issue per implementation phase
- Clear, structured titles and descriptions
- Include only changes required by the plan
- Verify against existing issues before creation

## Issue Content

- Title: Phase name from implementation plan
- Description: Phase details, requirements, and context
- Labels: Appropriate for issue type (feature/chore)


# Create GitHub Issue from Specification

Create GitHub Issue for the specification at `${file}`.

## Process

1. Analyze specification file to extract requirements
2. Check existing issues using `search_issues`
3. Create new issue using `create_issue` or update existing with `update_issue`
4. Use `feature_request.yml` template (fallback to default)

## Requirements

- Single issue for the complete specification
- Clear title identifying the specification
- Include only changes required by the specification
- Verify against existing issues before creation

## Issue Content

- Title: Feature name from specification
- Description: Problem statement, proposed solution, and context
- Labels: feature, enhancement (as appropriate)


# How to Write GitHub Issues for the Warp Project

When creating GitHub issues for the Warp project, follow these guidelines to ensure they are clear, actionable, and facilitate efficient resolution:


### Issue Structure

```md
## Issue Title
[Brief, specific description of the problem or feature request]


### Description
Clear explanation of the issue or feature request.


### Steps to Reproduce
1. Navigate to [page]
2. Click on [element]
3. Observe [behavior]


### Expected Behavior
What should happen.


### Actual Behavior
What actually happens.


### Screenshots
[Attach relevant screenshots showing the issue]


### Additional Context
Any other relevant information.
```


## Best Practices

1. Be Specific and Concise
    * Write clear titles that summarize the issue in a few words
    * Use technical terminology appropriate for the Warp project
2. Provide Complete Information
    * Include all steps necessary to reproduce the issue
    * Specify which part of the application is affected (Preview page, editor, etc.)
3. Use Markdown Formatting
    * Format code snippets with appropriate syntax highlighting
    * Use lists, headers, and quotes for readability
4. One Issue Per Report
    * Keep each issue focused on a single problem or feature
    * Create separate issues if you identify multiple problems
5. Reference Related Issues
    * Link to related issues with #[issue-number]
    * Mention if this issue blocks or depends on other work
6. Add Appropriate Labels
    * Tag with bug, enhancement, documentation, etc.
    * Indicate priority if applicable


## Rules

1. Return the issue as a formatted markdown. Do not add anithing, except the issue to the answer.
2. Include the Screenshots section only then a user provides screenshots.


---

Following these guidelines will help the Warp team efficiently process and address your reported issues.