import { createRequire } from 'node:module';
import { pathToFileURL } from 'node:url';

const require = createRequire(import.meta.url);
const { parseTimerDuration } = await import(new URL('./utils/duration.js', pathToFileURL(require.resolve('@microsoft/vally'))));

export const defaultJudgeTimeoutMs = 600_000;

export function timeoutMilliseconds(value, label) {
  const parsed = parseTimerDuration(value);
  if (!parsed.ok || !Number.isSafeInteger(parsed.ms)) {
    throw new Error(`${label} must be a positive duration within Node's timer range, such as 30s or 10m: ${value}`);
  }
  return parsed.ms;
}

export function judgeTimeoutLimits(timeoutMs = defaultJudgeTimeoutMs) {
  const comparisonDeadlineMs = Math.max(400_000, timeoutMs * 3 + 40_000);
  const retryBudgetMs = comparisonDeadlineMs + 200_000;
  if (!Number.isSafeInteger(timeoutMs) || timeoutMs <= 0 || retryBudgetMs > 2_147_483_647) {
    throw new Error('Judge timeout must be a positive integer in milliseconds with timer-safe comparison and retry budgets.');
  }
  return { comparisonDeadlineMs, retryBudgetMs };
}

export function withJudgeTimeout(client, timeoutMs = defaultJudgeTimeoutMs) {
  judgeTimeoutLimits(timeoutMs);
  return {
    ...client,
    judge: (options) => client.judge({ ...options, timeoutMs: Math.min(options.timeoutMs ?? timeoutMs, timeoutMs) }),
    shutdown: () => client.shutdown(),
  };
}