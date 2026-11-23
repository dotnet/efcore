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
  if (resolved.url !== clientUrl) {
    return resolved;
  }

  const originalUrl = new URL(clientUrl);
  originalUrl.searchParams.set('efcore-original', 'true');
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
  const replacements = {
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
  const source = typeof result.source === 'string' ? result.source : Buffer.from(result.source).toString('utf8');
  const [before, after] = replacement;
  if (source.split(before).length !== 2) {
    throw new Error(`Unsupported Vally timeout implementation in ${url}; review judge-timeout-loader.mjs before upgrading Vally.`);
  }
  return { ...result, source: source.replace(before, after) };
}