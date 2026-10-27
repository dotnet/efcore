import { register, createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { pathToFileURL } from 'node:url';
import { defaultJudgeTimeoutMs, judgeTimeoutLimits, timeoutMilliseconds } from './judge-timeout.mjs';

const require = createRequire(import.meta.url);
const entryUrl = pathToFileURL(join(dirname(require.resolve('@microsoft/vally-cli/package.json')), 'dist/index.js'));
let timeoutMs = defaultJudgeTimeoutMs;
if (process.argv[2] === '--judge-timeout') {
  timeoutMs = timeoutMilliseconds(process.argv[3], '--judge-timeout');
  process.argv.splice(2, 2);
}
judgeTimeoutLimits(timeoutMs);

register(new URL('./judge-timeout-loader.mjs', import.meta.url), import.meta.url, {
  data: {
    clientUrl: new URL('./graders/llm/llm-client.js', import.meta.resolve('@microsoft/vally')).href,
    adapterUrl: new URL('./judge-timeout.mjs', import.meta.url).href,
    promptGraderUrl: new URL('./graders/llm/prompt-grader.js', import.meta.resolve('@microsoft/vally')).href,
    comparisonRunnerUrl: new URL('./compare-runner.js', entryUrl).href,
    timeoutMs,
  },
});
await import(entryUrl.href);