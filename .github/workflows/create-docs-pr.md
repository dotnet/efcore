---
description: This workflow creates documentation PRs for the EF documentation side when new features are implemented.

on:
  slash_command:
    name: "doc"
    events: [issue_comment]  # Only trigger in issue comments (not PR)
  workflow_dispatch:

permissions:
  contents: read
  issues: read
  pull-requests: read

safe-outputs:
  create-pull-request:
    target-repo: "dotnet/EntityFramework.Docs"
    reviewers: [roji]
    draft: true
    fallback-as-issue: false
    # TEMPORARY, until we get an org token
    github-token: ${{ secrets.ROJI_TOKEN }}

  # Allow adding a comment in case of failure
  add-comment:
    hide-older-comments: true    # hide previous comments from same workflow
    allowed-reasons: [outdated]
---

# Document new EF features

This workflow generates documentation for new EF features, submitting a PR to the EF docs repo (dotnet/EntityFramework.Docs).

## Target branch

* The EF repo has automation to automatically add a label indicating in which preview/rc has been completed; the label is applied to the issue (not PR), and has the form `preview-3` or `rc-2` with the number adjusted. 
* The docs repo should have a corresponding branch, containing documentation to be published live when that preview/rc is published.
* When the workflow is launched, check the issue, find the preview/rc label, and submit the PR against the corresponding branch in the doc repo (dotnet/EntityFramework.Docs).
* If the label is missing, abort and post a comment to the triggering issue.

## Preparation and information gathering

* Take into account any additional context provided by the user in the triggering comment where the `/doc` command was triggered; this may contain specific instructions on how to write the documentation.
* Fully read the conversation history of the issue, as well as any linked PRs or relevant issues linked from it, to gain good context on the feature, APIs introduced, etc.
* Read also the `.github/copilot-instructions.md` file in the target dotnet/EntityFramework.Docs repo for practices and standards in that repo.

## Writing the documentation

* Add documentation in the appropriate section of the docs, depending on what the feature is.
* Fully document the feature, but keep it brief - do not add edge-case documentation in the name of exhaustivity that wouldn't be relevant to the majority of users.
* Before the new documentation, add the following note (adjusting for the major version):

```
> [!NOTE]
> This feature is being introduced in EF Core 11, which is currently in preview.
```

* Find the "what's new" page for the latest major release (typically `core/what-is-new/ef-core-11.0`, adjusting for the version), and add a **brief** section on the feature - just the minimum needed to make the user understand what it's about; include a minimal code sample as well if relevant. At the bottom, add a line such as "For more information on X, see the documentation" linking to the full docs added above.
* For both the full docs and the what's new documentation, do not simply create a new section; first check to see if there's an existing section that already covers related/similar functionality; if there is, either merge the new content into it or place the new section new to it.
* If the issue adds a function translation, add the appropriate entry (or entries) in the provider's functions page.

## Additional instructions

* The commit in the submitted PR should have a title of the form "Document X", where X is the name of the feature as it appears in the title of the originating issue. If the title is too long, for a git commit, make it shorter. The commit body should be of the form "Document Y", where Y is a link to the originating issue.
* If you encounter any error or issue, post a comment on the triggering PR detailing the problem(s) you encountered.
