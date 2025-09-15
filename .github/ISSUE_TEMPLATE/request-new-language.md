---
name: Request a new language
about: Request a new programming language for BestPracticesMcp. Choose to attach your own draft or ask Copilot to generate a first draft.
title: "[Language request] %s"
labels: enhancement, needs-triage
assignees: ''
---

Please use this form to propose a new language. Keep answers short so the issue is easy to scan on GitHub.

## 1) Basic
- Language name:
- One-line reason to include this language:

## 2) How you'd like to contribute
- [ ] I will attach a draft best-practices Markdown file.
- [ ] Please generate a first-draft Markdown for me (Copilot).

If you checked "Please generate", give 2–4 bullets describing what to include (e.g. formatting, testing, packaging, security):
- 
- 
- 

Preferred length for generated draft: (choose one)
- short (~100 lines) / medium (~150 lines) / long (~250 lines)

Preferred tone: practical / detailed / beginner-friendly / reference

## 3) Licensing & attribution
- Is the content original? (yes / no)
- If no, list sources and license info or links:

## 4) Submitter checklist (please complete before filing)
- [ ] Chosen one of the contribution options in section 2
- [ ] Confirmed license/attribution is compatible with this repo
- [ ] Included a suggested README entry (or short sentence) listing the resource path and tool path

## 5) Maintainer acceptance checklist (for PR)
- [ ] Add Markdown: Languages/<Language>/<language>-best-practices.md
- [ ] Add MCP tool: Languages/<Language>/<Language>.cs (or Functions/<Language>Tools.cs) following existing patterns
- [ ] Update README "Included languages" with language and resource/tool paths in the same PR
- [ ] Run `dotnet build` and `dotnet format --verify-no-changes`
- [ ] Verify the new resource is accessible via the MCP endpoint

Thank you — a maintainer will triage this request. If you asked Copilot to generate a draft, please review the generated content for accuracy and licensing before merging.
