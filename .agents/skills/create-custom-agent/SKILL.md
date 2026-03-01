---
name: create-custom-agent
description: 'Create custom GitHub Copilot agents. Use when asked to create, scaffold, or configure a custom agent, declarative agent, or @-invokable chat participant for GitHub Copilot.'
---

# Create Custom Agent

This skill guides you through creating a custom GitHub Copilot agent — an `@`-invokable chat participant that extends Copilot with domain-specific expertise. Custom agents are distinct from Agent Skills: skills provide reusable instructions loaded on demand, while agents own the full conversational interaction and can orchestrate tools, call APIs, and maintain their own prompt strategies.

## When Not to Use

- Adding reusable, invokable workflows — use Agent Skills (`.agents/skills/`) instead
- Adding background coding guidelines — use file-based instructions (`.github/instructions/`) instead
- Adding project-wide context for Copilot — use `.github/copilot-instructions.md` instead
- Creating reusable prompts — use .prompt.md instead

## Workflow

### Step 1: Choose the agent type

| Type | Location | Best for |
|---|---|---|
| Declarative (prompt file) | `.github/agents/<name>.md` | Simple prompt-driven cross-surface agents with no code |
| Extension-based (chat participant) | VS Code extension project | Full control, tool calling, VS Code API access |
| GitHub App (Copilot Extension) | Hosted service + GitHub App | Cross-surface agents (github.com, VS Code, Visual Studio) |

If the agent only needs a scoped system prompt and doesn't require custom code, start with a declarative agent.

### Step 2: Create a declarative agent (prompt file)

Declarative agents are Markdown files in `.github/agents/`. VS Code and GitHub Copilot discover them automatically.

```
.github/agents/
└── <agent-name>.md        # Agent definition
```

Template:

```markdown
---
name: my-agent
description: A short description of what this agent does and when to use it.
---

# <Agent Title>

You are an expert in <domain>. Your job is to:
- <behavior 1>
- <behavior 2>

## Guidelines

- <guideline 1>
- <guideline 2>

## Workflow

1. <step 1>
2. <step 2>

## Constraints

- <constraint 1>
- <constraint 2>

```

Supported frontmatter fields:

| Field | Required | Description |
|---|---|---|
| `name` | Yes | Lowercase, hyphens allowed. Used for `@`-mention. |
| `description` | Yes | What the agent does and when to use it. Shown in the participant list. |
| `target` | No | Target environment: `vscode` or `github-copilot` (defaults to both) |
| `tools` | No | List of allowed tools/tool sets |
| `model` | No | LLM name or prioritized array of models |
| `user-invokable` | No | Show in agents dropdown (default: true) |
| `disable-model-invocation` | No | Prevent subagent invocation (default: false) |
| `mcp-servers` | No | MCP server configs for GitHub Copilot target |
| `metadata` | No | Key-value mapping for additional arbitrary metadata. |
| `argument-hint` | No | Hint text guiding user interaction (VS Code only) |
| `agents` | No | List of allowed subagents (`*` for all, `[]` for none, VS Code only) |
| `handoffs` | No | List of next-step agent transitions (VS Code only) |


Tips for instructions:
- Use Markdown links to reference other files
- Reference tools with `#tool:<tool-name>` syntax
- Be specific about agent behavior and constraints

### Step 3: Configure tools

Specify which tools the agent can use:

```yaml
tools:
  - search              # Built-in tool
  - fetch               # Built-in tool
  - codebase            # Tool set
  - myServer/*          # All tools from MCP server
```

Common tool patterns:
- **Read-only agents**: `['search', 'fetch', 'codebase']`
- **Full editing agents**: `['*']` or specific editing tools
- **Specialized agents**: Cherry-pick specific tools

### Step 4: Add handoffs (optional, VS Code only)

Configure transitions to other agents:

```yaml
handoffs:
  - label: Start Implementation
    agent: implementation
    prompt: Implement the plan outlined above.
    send: false
    model: GPT-5.2 (copilot)
```

Handoff fields:
- `label`: Button text displayed to user
- `agent`: Target agent identifier
- `prompt`: Pre-filled prompt for target agent
- `send`: Auto-submit prompt (default: false)
- `model`: Optional model override for handoff

### Step 5: Create an extension-based chat participant (VS Code only)

For full control, implement a VS Code extension with a chat participant:

1. **Define the participant** in `package.json`:

```json
"contributes": {
    "chatParticipants": [
        {
            "id": "my-extension.my-agent",
            "name": "my-agent",
            "fullName": "My Agent",
            "description": "Short description shown in chat input",
            "isSticky": false,
            "commands": [
                {
                    "name": "explain",
                    "description": "Explain the selected code"
                }
            ]
        }
    ]
}
```

2. **Register and implement the request handler** in `extension.ts`:

```typescript
export function activate(context: vscode.ExtensionContext) {
    const agent = vscode.chat.createChatParticipant('my-extension.my-agent', handler);
    agent.iconPath = vscode.Uri.joinPath(context.extensionUri, 'icon.png');
}

const handler: vscode.ChatRequestHandler = async (
    request: vscode.ChatRequest,
    context: vscode.ChatContext,
    stream: vscode.ChatResponseStream,
    token: vscode.CancellationToken
) => {
    const model = request.model;
    const messages = [
        vscode.LanguageModelChatMessage.User(request.prompt)
    ];
    const response = await model.sendRequest(messages, {}, token);
    for await (const fragment of response.text) {
        stream.markdown(fragment);
    }
};
```

3. **Declare the extension dependency** in `package.json`:

```json
"extensionDependencies": ["github.copilot-chat"]
```

4. **Add tool calling (optional)**

Agents can invoke language model tools registered by other extensions:

```typescript
const tools = vscode.lm.tools.filter(tool => tool.tags.includes('my-domain'));
const result = await chatUtils.sendChatParticipantRequest(request, context, {
    prompt: 'You are an expert in <domain>.',
    tools,
    responseStreamOptions: { stream, references: true, responseText: true }
}, token);
return await result.result;
```

### Step 6: Create a GitHub App (Copilot Extension) for cross-surface availability (optional)

If the agent should be available on GitHub.com, Visual Studio, JetBrains, and VS Code simultaneously, implement a GitHub App that acts as a Copilot Extension. The app registers a webhook endpoint, receives chat requests, and streams responses back.

Key considerations:
- The GitHub App must be installed on the user's account or organization
- Responses are streamed via Server-Sent Events (SSE)
- Use the [GitHub Copilot Extensions documentation](https://docs.github.com/en/copilot/building-copilot-extensions/about-building-copilot-extensions) for the full integration guide
- For VS Code-specific features (editor access, file trees, command buttons), prefer an extension-based participant instead

### Step 7: Validate

After creating or modifying an agent, verify:

- [ ] `name` is lowercase, uses hyphens (no spaces), and is unique
- [ ] `description` clearly describes what the agent does and when to invoke it
- [ ] Frontmatter YAML is valid (no syntax errors)
- [ ] Declarative agent file is in `.github/agents/`
- [ ] Tools list contains only available tools
- [ ] Extension-based agent: participant ID matches in `package.json` and `createChatParticipant` call
- [ ] Agent does not duplicate functionality of built-in agents (`@workspace`, `@vscode`, `@terminal`)
- [ ] Handoff agent names match existing agents
- [ ] Agent instructions don't include secrets, tokens, or internal URLs

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| Agent name conflicts with built-in participants | Use a unique prefix (domain name) |
| Description is too vague | Include specific keywords users would naturally say |
| System prompt is too long | Keep instructions to essential behaviors; move reference material to Agent Skills |
| Agent requires VS Code API but is authored as declarative | Switch to extension-based participant |
| Using `isSticky: true` unnecessarily | Only set sticky if the agent should persist between turns by default |
| No `extensionDependencies` on `github.copilot-chat` | Add it; otherwise the contribution point may not be available |
| Agent invoked as subagent unexpectedly | Set `disable-model-invocation: true` |
| Subagent appears in the dropdown | Set `user-invokable: false` |

## References

- [VS Code Chat Participant API](https://code.visualstudio.com/api/extension-guides/ai/chat)
- [VS Code AI Extensibility Overview](https://code.visualstudio.com/api/extension-guides/ai/ai-extensibility-overview)
- [VS Code Extension Samples – chat-sample](https://github.com/microsoft/vscode-extension-samples/tree/main/chat-sample)
- [GitHub Copilot Extensions documentation](https://docs.github.com/en/copilot/building-copilot-extensions/about-building-copilot-extensions)
- [GitHub Copilot Custom agents configuration](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
- [Agent Skills Specification](https://agentskills.io/specification)
- [make-skill](../make-skill/SKILL.md)
- [make-instructions](../make-instructions/SKILL.md)
