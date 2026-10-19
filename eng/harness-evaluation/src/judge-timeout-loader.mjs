import { judgeTimeoutLimits } from './judge-timeout.mjs';

let clientUrl;
let adapterUrl;
let promptGraderUrl;
let comparisonRunnerUrl;
let timeoutMs;
let limits;

export function initialize(options) {
  ({ clientUrl, adapterUrl, promptGraderUrl, comparisonRunnerUrl, timeoutMs } = options);
  limits = judgeTimeoutLimits(timeoutMs);
}

export async function resolve(specifier, context, nextResolve) {
  const resolved = await nextResolve(specifier, context);
  const originalUrl = new URL(clientUrl);
  originalUrl.searchParams.set('efcore-original', 'true');
  if (specifier === '@github/copilot-sdk' && context.parentURL === originalUrl.href) {
    const source = `
      export * from ${JSON.stringify(resolved.url)};
      import { CopilotClient as Client } from ${JSON.stringify(resolved.url)};
      export class CopilotClient extends Client {
        async createSession(options) {
          if (!options.provider && options.model !== 'auto') {
            const models = await this.listModels();
            if (!models.some((model) => model.id === options.model)) {
              throw new Error(\`Judge model '\${options.model}' is not available from Copilot. Select an id returned by CopilotClient.listModels().\`);
            }
          }
          return super.createSession(options);
        }
      }
    `;
    return { url: `data:text/javascript,${encodeURIComponent(source)}`, shortCircuit: true };
  }
  if (resolved.url !== clientUrl) {
    return resolved;
  }

  const source = `
    import { createCopilotLlmClient as createClient } from ${JSON.stringify(originalUrl.href)};
    import { withJudgeTimeout } from ${JSON.stringify(adapterUrl)};
    export async function createCopilotLlmClient() {
      return withJudgeTimeout(await createClient(), ${JSON.stringify(timeoutMs)});
    }
  `;
  return { url: `data:text/javascript,${encodeURIComponent(source)}`, shortCircuit: true };
}

export async function load(url, context, nextLoad) {
  const result = await nextLoad(url, context);
  const originalUrl = new URL(clientUrl);
  originalUrl.searchParams.set('efcore-original', 'true');
  const replacements = {
    [originalUrl.href]: [
      'const timeoutMs = Math.min(options.timeoutMs ?? 120_000, 120_000);',
      'const timeoutMs = options.timeoutMs ?? 120_000;',
    ],
    [promptGraderUrl]: [
      'const COMPARISON_DEADLINE_MS = 400_000;',
      `const COMPARISON_DEADLINE_MS = ${limits.comparisonDeadlineMs};`,
    ],
    [comparisonRunnerUrl]: [
      'const DEFAULT_RETRY_BUDGET_MS = 600_000;',
      `const DEFAULT_RETRY_BUDGET_MS = ${limits.retryBudgetMs};`,
    ],
  };
  const replacement = replacements[url];
  if (!replacement) {
    return result;
  }
  const [before, after] = replacement;
  const source = typeof result.source === 'string' ? result.source : Buffer.from(result.source).toString('utf8');
  if (source.split(before).length !== 2) {
    throw new Error(`Unsupported Vally timeout implementation in ${url}; review judge-timeout-loader.mjs before upgrading Vally.`);
  }
  return { ...result, source: source.replace(before, after) };
}