import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtemp, mkdir, readFile, rm, symlink, writeFile } from 'node:fs/promises';
import { createRequire } from 'node:module';
import { dirname, join, parse as parsePath } from 'node:path';
import { tmpdir } from 'node:os';
import { fileURLToPath, pathToFileURL } from 'node:url';
import test from 'node:test';
import { runInNewContext } from 'node:vm';
import { createGraderRegistry, gradeTrajectory, hasGraderError, loadEvalWithParams, resolveGradingTimeoutMs } from '@microsoft/vally';
import { parse } from 'yaml';
import { prepareComparison } from '../src/comparison.mjs';
import { judgeTimeoutLimits, withJudgeTimeout } from '../src/judge-timeout.mjs';
import { initialize as initializeJudgeLoader, load as loadJudgeModule } from '../src/judge-timeout-loader.mjs';
import {
  defaultRepoRoot,
  discoverComponents,
  findExperimentRunDirectory,
  runVally,
  timeoutMilliseconds,
  validateComponentId,
  validateEval,
  validateInventory,
  validateOutputRoot,
  variantCompleted,
  variantInvokedSkill,
  variantPassed,
} from '../src/harness.mjs';

const workflow = parse(await readFile(new URL('../../../.github/workflows/agent-harness-evaluation.yml', import.meta.url), 'utf8'));
const metadataStep = workflow.jobs.discover.steps.find((step) => step.name === 'Update pull request metadata');

test('workflow uses the configured judge model for all component evaluations', () => {
  const evaluationStep = workflow.jobs.evaluate.steps.find((step) => step.name === 'Run treatment, unskilled control, and comparison');
  assert.match(evaluationStep.run, /--judge-model gpt-5\.6-terra/);
});

const evalYaml = (
  name,
  grader = `      - type: output-contains\n        config:\n          substring: Anchor`,
  targetEnvironment = `agent_environment:\n  skills:\n    - ../../../../.agents/skills/example\n`,
) => `name: ${name}\ndefaults:\n  runs: \${RUNS=5}\n  judge_timeout: \${JUDGE_TIMEOUT=10m}\n  executor: mock\n${targetEnvironment}stimuli:\n  - name: inspect-anchor\n    prompt: Inspect src/EFCore/Anchor.cs and explain it.\n    constraints:\n      max_turns: 10\n      max_tokens: 5000\n      max_duration: \${EVAL_TIMEOUT=1m}\n    agent_environment:\n      files:\n        - src: src/EFCore/Anchor.cs\n          dest: src/EFCore/Anchor.cs\n    graders:\n${grader}\n      - type: token-budget\n        config:\n          max: 5000\nscoring:\n  timeout: \${SCORING_TIMEOUT=10m}\n  weights:\n    token-budget: 0.1\n    output-contains: 0.9\n  threshold: 0.75\n`;

async function makeRepo() {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-eval-'));
  await mkdir(join(root, '.agents/skills/example'), { recursive: true });
  await mkdir(join(root, '.github/agents'), { recursive: true });
  await mkdir(join(root, '.github/workflows'), { recursive: true });
  await mkdir(join(root, 'src/EFCore'), { recursive: true });
  await mkdir(join(root, 'eng/harness-evaluation/skills/example'), { recursive: true });
  await mkdir(join(root, 'eng/harness-evaluation/instructions/copilot-instructions'), { recursive: true });

  await writeFile(join(root, '.agents/skills/example/SKILL.md'), '---\nname: example\ndescription: Example skill\n---\n');
  await writeFile(join(root, '.github/copilot-instructions.md'), '# Instructions\n');
  await writeFile(join(root, 'src/EFCore/Anchor.cs'), 'class Anchor {}\n');

  await writeFile(join(root, 'eng/harness-evaluation/skills/example/eval.yaml'), evalYaml('example'));
  await writeFile(
    join(root, 'eng/harness-evaluation/instructions/copilot-instructions/eval.yaml'),
    evalYaml('copilot-instructions', undefined, `agent_environment:\n  files:\n    - src: ../../../../.github/copilot-instructions.md\n      dest: .github/copilot-instructions.md\n`),
  );

  return root;
}

test('native Vally reports preserve the experiment and comparison contract', async () => {
  const root = await makeRepo();
  try {
    await writeFile(join(root, 'eng/harness-evaluation/skills/example/eval.yaml'),
      evalYaml('example').replace('  executor: mock', '  model: mock\n  executor: mock')
        .replace('src: src/EFCore/Anchor.cs', 'src: ../../../../src/EFCore/Anchor.cs'));
    const experimentPath = join(root, 'experiment.yaml');
    await writeFile(experimentPath, `name: report-contract
evals: ['eng/harness-evaluation/skills/example/eval.yaml']
vary: [/environment/skills]
baseline: control
variants:
  control:
    environment:
      skills: []
  treatment: {}
`);
    const outputRoot = join(root, 'artifacts/results');
    const execution = runVally(['experiment', 'run', experimentPath, '--output-dir', outputRoot,
      '--param', 'RUNS=1', '--workers', '1'], { cwd: root });
    assert.ok([0, 1].includes(execution.status), execution.stderr);
    const experimentDirectory = await findExperimentRunDirectory(outputRoot);
    const planPath = join(experimentDirectory, 'plan-snapshot.json');
    const plan = JSON.parse(await readFile(planPath, 'utf8'));
    assert.equal(plan.type, 'experiment-plan-snapshot');
    assert.deepEqual(plan.evals.map((evalPlan) => evalPlan.variant).sort(), ['control', 'treatment']);
    assert.ok(plan.evals.every((evalPlan) => !evalPlan.failure), JSON.stringify(plan));
    for (const variant of ['control', 'treatment']) {
      const resultsPath = join(experimentDirectory, variant, 'results.jsonl');
      assert.equal(await variantCompleted(resultsPath, planPath, variant), true, await readFile(resultsPath, 'utf8'));
      const records = (await readFile(resultsPath, 'utf8')).split(/\r?\n/).filter(Boolean).map(JSON.parse);
      const trials = records.filter((record) => record.type === 'trial-result');
      assert.equal(trials.length, 1);
      const trial = trials[0];
      assert.equal(trial.status, 'success');
      assert.equal(trial.stimulus, 'inspect-anchor');
      assert.equal(trial.trialIndex ?? 0, 0);
      assert.equal(trial.experiment.variant, variant);
      assert.equal(typeof trial.trajectory.output, 'string');
      assert.ok(Number.isFinite(trial.trajectory.metrics.tokenUsage.totalTokens));
      assert.ok(Number.isFinite(trial.gradeResult.score));
      assert.ok(Array.isArray(trial.gradeResult.details));
    }
    const clientUrl = new URL('./graders/llm/llm-client.js', import.meta.resolve('@microsoft/vally')).href;
    const loaderPath = join(root, 'judge-loader.mjs');
    const fakeClientSource = `export async function createCopilotLlmClient() {
      return { supportsWorkspaceDelivery: true, shutdown() {}, judge: async (options) => {
        const timeoutMs = Math.min(options.timeoutMs ?? 120_000, 120_000);
        if (timeoutMs !== 600000) throw new Error('The comparison did not receive the extended judge timeout.');
        return { latencyMs: 0, args: {
          rubric_results: [{ criterion: 'Inspect the anchor.', winner: 'B', magnitude: 'much-better', reasoning: 'Contract probe.' }],
          overall_winner: 'B', overall_magnitude: 'much-better', overall_reasoning: 'Contract probe.'
        } };
      } };
    }`;
    await writeFile(loaderPath, `export async function load(url, context, nextLoad) {
      if (new URL(url).pathname === new URL(${JSON.stringify(clientUrl)}).pathname) {
        return { format: 'module', source: ${JSON.stringify(fakeClientSource)}, shortCircuit: true };
      }
      return nextLoad(url, context);
    }`);
    const preloadPath = join(root, 'preload.mjs');
    await writeFile(preloadPath, `import { register } from 'node:module';
      register(${JSON.stringify(pathToFileURL(loaderPath).href)}, import.meta.url);`);
    const comparisonPath = join(outputRoot, 'comparison.jsonl');
    const comparisonDirectory = await prepareComparison(experimentDirectory, join(outputRoot, 'comparison-input'));
    const comparisonResult = runVally(['compare', comparisonDirectory, '--output', comparisonPath], {
      cwd: root, env: { NODE_OPTIONS: `--import=${pathToFileURL(preloadPath).href}` },
    });
    assert.equal(comparisonResult.status, 0, comparisonResult.stderr);
    const comparisons = (await readFile(comparisonPath, 'utf8')).split(/\r?\n/).filter(Boolean).map(JSON.parse);
    assert.equal(comparisons.length, 1);
    const comparison = comparisons[0];
    assert.equal(comparison.type, 'comparison');
    assert.equal(comparison.baseline, 'control');
    assert.equal(comparison.treatment, 'treatment');
    assert.equal(comparison.summary.trialCount, 1);
    assert.equal(comparison.summary.erroredCount, 0);
    for (const field of ['meanScore', 'wins', 'ties', 'losses']) {
      assert.ok(Number.isFinite(comparison.summary[field]), field);
    }
    assert.ok(comparison.summary.metricDeltas.some((metric) => metric.metric === 'totalTokens' && Number.isFinite(metric.mean)));
    assert.equal(comparison.stimuli[0].stimulusName, 'inspect-anchor');
    assert.equal(comparison.stimuli[0].trials[0].trialIndex, 0);
    assert.deepEqual(comparison.unmatchedBaseline, []);
    assert.deepEqual(comparison.unmatchedTreatment, []);

    for (const variant of ['control', 'treatment']) {
      await writeFile(join(comparisonDirectory, variant, 'results.jsonl'), 'Not original experiment results.\n');
    }
    const verifyStep = workflow.jobs.evaluate.steps.find((step) => step.name === 'Verify Vally reports');
    const verifyScript = verifyStep.run.match(/<<'EOF'\n([\s\S]*?)\nEOF/)[1];
    const verify = () => spawnSync(process.execPath, ['--input-type=module', '-e', verifyScript], {
      encoding: 'utf8',
      env: { ...process.env, RESULT_ROOT: outputRoot, TRUSTED_ROOT: defaultRepoRoot },
    });
    const verification = verify();
    assert.equal(verification.status, 0, verification.stderr);

    const reportStep = workflow.jobs.evaluate.steps.find((step) => step.name === 'Report component result');
    const report = async () => {
      const comments = [];
      const warnings = [];
      const AsyncFunction = Object.getPrototypeOf(async function () {}).constructor;
      await new AsyncFunction('require', 'process', 'context', 'github', 'core', reportStep.with.script)(
        createRequire(import.meta.url),
        { env: {
          RESULT_ROOT: outputRoot, TRUSTED_ROOT: defaultRepoRoot, PULL_REQUEST: '123',
          COMPONENT: 'example', EVAL_OUTCOME: 'success', HEAD_SHA: 'head',
        } },
        { repo: { owner: 'dotnet', repo: 'efcore' }, serverUrl: 'https://github.com', runId: 1 },
        {
          paginate: async () => [],
          rest: {
            issues: { listComments() {}, createComment: async (comment) => comments.push(comment) },
            repos: { getContent: async () => ({}) },
            search: { issuesAndPullRequests: async () => ({ data: { items: [] } }) },
          },
        },
        { info() {}, warning: (message) => warnings.push(message) },
      );
      assert.equal(comments.length, 1);
      return { body: comments[0].body, warnings };
    };
    const reported = await report();
    assert.deepEqual(reported.warnings, []);
    assert.match(reported.body, /Status: \*\*✅ passed\*\*/);
    assert.match(reported.body, /Model: \*\*`mock`\*\*/);
    assert.match(reported.body, /\| inspect-anchor \| 1\/1 \|/);
    assert.doesNotMatch(reported.body, /An error occurred/);

    const duplicateDirectory = join(outputRoot, 'second-experiment');
    await mkdir(duplicateDirectory);
    await writeFile(join(duplicateDirectory, 'plan-snapshot.json'), JSON.stringify(plan));
    assert.match(verify().stderr, /Expected one Vally experiment.*found 2/);
    const duplicateReport = await report();
    assert.match(duplicateReport.body, /Status: \*\*error\*\*/);
    assert.match(duplicateReport.warnings.join('\n'), /Expected one Vally experiment.*found 2/);
    await rm(duplicateDirectory, { recursive: true });

    await rm(join(experimentDirectory, 'control', 'results.jsonl'));
    assert.notEqual(verify().status, 0);
    const missingReport = await report();
    assert.match(missingReport.body, /Status: \*\*error\*\*/);
    assert.match(missingReport.warnings.join('\n'), /Could not parse treatment grades:.*ENOENT/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

async function writeSkillInvocationTrial(root, invokedSkill = 'example', eventType = 'skill.invoked') {
  const variantRoot = join(root, 'treatment');
  const trialDirectory = join(variantRoot, 'example', 'inspect-anchor', 'mock', '0');
  const trialId = 'example::treatment::mock::inspect-anchor::trial-0';
  await mkdir(trialDirectory, { recursive: true });
  await writeFile(join(variantRoot, 'results.jsonl'), `${JSON.stringify({
    type: 'trial-result',
    itemId: trialId,
    evalName: 'example',
    stimulus: 'inspect-anchor',
    status: 'success',
  })}\n`);
  await writeFile(join(trialDirectory, 'metadata.json'), `${JSON.stringify({
    evalName: 'example',
    variant: 'treatment',
    stimulusName: 'inspect-anchor',
    trialId,
  })}\n`);
  await writeFile(join(trialDirectory, 'events.jsonl'), `${JSON.stringify({
    type: eventType,
    data: { name: invokedSkill },
  })}\n`);
  return join(variantRoot, 'results.jsonl');
}

async function updateMetadata({ draft = false, authorized = false, action = 'opened', comments = [] } = {}) {
  const calls = [];
  const issues = Object.fromEntries(['listComments', 'getLabel', 'addLabels', 'createComment', 'updateComment', 'deleteComment']
    .map((method) => [method, async (parameters) => { calls.push({ method, ...parameters }); }]));
  await runInNewContext(`(async () => {\n${metadataStep.with.script}\n})()`, {
    context: { payload: { action }, repo: { owner: 'dotnet', repo: 'efcore' }, serverUrl: 'https://github.com' },
    process: { env: {
      PULL_REQUEST: '123', SELECTED_COMPONENTS: '["query-pipeline"]', COMPONENT_FILTER: '', HEAD_SHA: 'head',
      CAN_USE_SECRETS: String(authorized), PR_DRAFT: String(draft),
    } },
    github: { rest: { issues }, paginate: async () => comments },
    core: { info() {} },
  });
  return calls;
}

test('harness metadata receives the resolved draft flag', () => {
  const resolutionStep = workflow.jobs.discover.steps.find((step) => step.id === 'pr');
  assert.match(resolutionStep.with.script, /core\.setOutput\('draft', String\(pull\.draft\)\)/);
  assert.equal(metadataStep.env.PR_DRAFT, '${{ steps.pr.outputs.draft }}');
});

for (const action of ['opened', 'reopened', 'synchronize']) {
  test(`draft PR ${action} comments explain deferral without blaming author authorization`, async () => {
    const calls = await updateMetadata({ draft: true, action });
    const comment = calls.find((call) => call.method === 'createComment');
    assert.ok(comment);
    assert.match(comment.body, /evaluation deferred/);
    assert.match(comment.body, /currently marked as draft/);
    assert.match(comment.body, /usual author authorization rules apply/);
    assert.match(comment.body, /\/eval component-name/);
    assert.doesNotMatch(comment.body, /author cannot use repository secrets|evaluation is stale/);
    assert.equal(calls.filter((call) => call.method === 'createComment').length, 1);
  });
}

test('non-draft unauthorized PR keeps the authorization explanation', async () => {
  const calls = await updateMetadata();
  const comment = calls.find((call) => call.method === 'createComment');
  assert.match(comment.body, /evaluation required/);
  assert.match(comment.body, /author cannot use repository secrets/);
  assert.doesNotMatch(comment.body, /marked as draft|evaluation deferred/);
});

test('draft deferral updates the existing workflow-owned comment', async () => {
  const calls = await updateMetadata({ draft: true, comments: [
    { id: 17, user: { login: 'github-actions[bot]' }, body: '<!-- agent-harness-evaluation:manual-required -->\nOld authorization message' },
  ] });
  const comment = calls.find((call) => call.method === 'updateComment');
  assert.equal(comment.comment_id, 17);
  assert.match(comment.body, /currently marked as draft/);
  assert.ok(!calls.some((call) => call.method === 'createComment'));
});

test('ready-for-review and authorized manual draft runs remove the deferral comment', async () => {
  for (const scenario of [{ draft: false, action: 'ready_for_review' }, { draft: true, action: undefined }]) {
    const calls = await updateMetadata({ ...scenario, authorized: true, comments: [
      { id: 17, user: { login: 'github-actions[bot]' }, body: '<!-- agent-harness-evaluation:manual-required -->\nDraft deferral' },
    ] });
    assert.equal(calls.find((call) => call.method === 'deleteComment').comment_id, 17);
    assert.ok(!calls.some((call) => call.method === 'createComment'));
  }
});

test('non-draft synchronize keeps the existing stale-evaluation comment', async () => {
  const calls = await updateMetadata({ action: 'synchronize' });
  const comment = calls.find((call) => call.method === 'createComment');
  assert.match(comment.body, /evaluation is stale/);
  assert.doesNotMatch(comment.body, /marked as draft|author cannot use repository secrets/);
});

test('inventory accepts complete repository customization coverage', async () => {
  const root = await makeRepo();
  try {
    const result = await validateInventory(root);
    assert.deepEqual(result.errors, []);
    assert.deepEqual(result.components.map((component) => component.id), ['copilot-instructions', 'example']);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval inputs use source snapshots instead of diff fixtures', async () => {
  const root = await makeRepo();
  const evalPath = join(root, 'eng/harness-evaluation/skills/example/eval.yaml');
  try {
    assert.deepEqual(await validateEval(evalPath, 'example'), []);
    for (const fixture of ['proposal.diff', 'proposal.PATCH']) {
      for (const field of ['src', 'dest']) {
        const content = evalYaml('example').replace(`${field}: src/EFCore/Anchor.cs`, `${field}: ${fixture}`);
        await writeFile(evalPath, content);
        assert.match((await validateEval(evalPath, 'example')).join('\n'), /self-contained source snapshots/);
      }
      await writeFile(evalPath, evalYaml('example', undefined, `agent_environment:\n  files:\n    - src: ${fixture}\n      dest: proposal.cs\n`));
      assert.match((await validateEval(evalPath, 'example')).join('\n'), /self-contained source snapshots/);
    }
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval rejects unsafe component ids before deleting output', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-output-'));
  try {
    const sentinel = join(root, 'sentinel.txt');
    await writeFile(sentinel, 'keep');
    const cliPath = fileURLToPath(new URL('../src/cli.mjs', import.meta.url));
    const result = spawnSync(process.execPath, [
      cliPath,
      'eval',
      '../../..',
      '--output', root,
    ], { encoding: 'utf8' });

    assert.notEqual(result.status, 0);
    assert.match(result.stderr, /Component id .*invalid characters: \.\.\/\.\.\/\.\./);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
    assert.throws(() => validateComponentId('also..unsafe'), /invalid characters/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval scopes component resolution and output validation to the selected repository', async () => {
  const root = await makeRepo();
  try {
    const cliPath = fileURLToPath(new URL('../src/cli.mjs', import.meta.url));
    const result = spawnSync(process.execPath, [
      cliPath,
      'eval',
      'example',
      '--repo-root', root,
      '--output', join(root, 'outside-artifacts'),
    ], { encoding: 'utf8' });

    assert.notEqual(result.status, 0);
    assert.match(result.stderr, /Output path must be a descendant/);
    assert.match(result.stderr, new RegExp(join(root, 'artifacts').replaceAll('\\', '\\\\')));
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval rejects missing model, judge model, judge effort, timeout, and runs option values', async () => {
  const root = await makeRepo();
  try {
    const cliPath = fileURLToPath(new URL('../src/cli.mjs', import.meta.url));
    for (const option of ['--model', '--judge-model', '--judge-reasoning-effort', '--judge-timeout', '--scoring-timeout', '--timeout', '--runs']) {
      const result = spawnSync(process.execPath, [
        cliPath,
        'eval',
        'example',
        '--repo-root', root,
        option,
      ], { encoding: 'utf8' });

      assert.notEqual(result.status, 0);
      assert.match(result.stderr, new RegExp(`${option} requires a value\\.`));
    }
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval rejects unsupported judge reasoning effort', async () => {
  const root = await makeRepo();
  try {
    const result = spawnSync(process.execPath, [
      fileURLToPath(new URL('../src/cli.mjs', import.meta.url)),
      'eval', 'example', '--repo-root', root, '--judge-reasoning-effort', 'unbounded',
    ], { encoding: 'utf8' });
    assert.notEqual(result.status, 0);
    assert.match(result.stderr, /--judge-reasoning-effort must be low, medium, high, or xhigh/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('timeout durations are parsed using native units and reject unsafe timer values', () => {
  assert.equal(timeoutMilliseconds('10m', '--scoring-timeout'), 600_000);
  assert.equal(timeoutMilliseconds('30s', '--timeout'), 30_000);
  for (const value of ['0s', '-1m', 'forever', '', '999999999h']) {
    assert.throws(() => timeoutMilliseconds(value, '--timeout'), /--timeout must be a positive duration/);
  }
});

test('native scoring timeout reaches graders and bounds the whole grading pass', async (context) => {
  context.mock.timers.enable({ apis: ['setTimeout', 'Date'] });
  for (const [elapsed, expires] of [[120_001, false], [600_001, true]]) {
    const started = Promise.withResolvers();
    const finish = Promise.withResolvers();
    const registry = createGraderRegistry();
    registry.register({
      metadata: { name: 'deadline-probe', behavior: { requiresLlmClient: true } },
      async grade(input) {
        started.resolve(input);
        input.signal.addEventListener('abort', () => finish.reject(input.signal.reason), { once: true });
        await finish.promise;
        return { name: 'deadline-probe', kind: 'llm', passed: true, score: 1, evidence: 'Finished.' };
      },
    });
    const stimulus = { name: 'deadline', prompt: 'Inspect the output.' };
    const grading = gradeTrajectory({ stimulus, output: 'Result', events: [], metrics: {} },
      [{ type: 'deadline-probe' }], { registry, graderTimeoutMs: resolveGradingTimeoutMs('10m') });
    const input = await started.promise;
    assert.equal(input.timeoutMs, 600_000);
    context.mock.timers.tick(elapsed);
    assert.equal(input.signal.aborted, expires);
    if (!expires) {
      finish.resolve();
    }
    const result = await grading;
    assert.equal(result.passed, !expires);
    assert.equal(hasGraderError(result), expires);
    if (expires) {
      assert.match(result.details[0].evidence, /Grading timed out after 10m/);
    }
  }
});

test('judge timeout loader removes the SDK cap without changing the native package', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-native-judge-timeout-'));
  try {
    const clientUrl = new URL('./graders/llm/llm-client.js', import.meta.resolve('@microsoft/vally')).href;
    const durationUrl = new URL('./utils/duration.js', import.meta.resolve('@microsoft/vally')).href;
    const sdkSource = `
      export const defineTool = (name, options) => ({ name, ...options });
      export class ToolSet {}
      export class CopilotClient {
        async start() {}
        async stop() { return []; }
        async createSession(options) {
          return {
            on() {}, async abort() {}, async disconnect() {},
            async sendAndWait(message, timeoutMs) {
              globalThis.sdkTimeoutMs = timeoutMs;
              await options.tools[0].handler({ passed: true });
            }
          };
        }
      }
    `;
    const loaderPath = join(root, 'sdk-loader.mjs');
    await writeFile(loaderPath, `export async function resolve(specifier, context, nextResolve) {
      if (specifier === '@github/copilot-sdk') {
        return { url: ${JSON.stringify(`data:text/javascript,${encodeURIComponent(sdkSource)}`)}, shortCircuit: true };
      }
      return nextResolve(specifier, context);
    }`);
    const preloadPath = join(root, 'preload.mjs');
    await writeFile(preloadPath, `import { register } from 'node:module';
      register(${JSON.stringify(pathToFileURL(loaderPath).href)}, import.meta.url);`);
    const script = `
      import { createCopilotLlmClient } from ${JSON.stringify(clientUrl)};
      import { resolveGradingTimeoutMs } from ${JSON.stringify(durationUrl)};
      const client = await createCopilotLlmClient();
      const response = await client.judge({ model: 'mock', systemMessage: 'Judge.', userMessage: 'Grade.',
        timeoutMs: resolveGradingTimeoutMs('10m'), tool: { name: 'submit', description: 'Submit a verdict.',
          parameters: { safeParse: (data) => ({ success: true, data }) } } });
      await client.shutdown();
      console.log(JSON.stringify({ sdkTimeoutMs: globalThis.sdkTimeoutMs, verdict: response.args }));
    `;
    const entryUrl = new URL('../node_modules/@microsoft/vally-cli/dist/index.js', import.meta.url).href;
    const originalSource = await readFile(fileURLToPath(clientUrl), 'utf8');
    const adapterUrl = new URL('../src/judge-timeout.mjs', import.meta.url).href;
    const timeoutLoaderUrl = new URL('../src/judge-timeout-loader.mjs', import.meta.url).href;
    for (const [adapted, remainingBudget, expected] of [[false, '10m', 120_000], [true, '15m', 600_000], [true, '30s', 30_000]]) {
      await writeFile(preloadPath, `import { register } from 'node:module';
        register(${JSON.stringify(pathToFileURL(loaderPath).href)}, import.meta.url);
        ${adapted ? `register(${JSON.stringify(timeoutLoaderUrl)}, import.meta.url, {
          data: { clientUrl: ${JSON.stringify(clientUrl)}, adapterUrl: ${JSON.stringify(adapterUrl)}, timeoutMs: 600000 }
        });` : ''}`);
      const result = spawnSync(process.execPath, ['--import', pathToFileURL(preloadPath).href,
        '--input-type=module', '-e', script.replace("resolveGradingTimeoutMs('10m')", `resolveGradingTimeoutMs('${remainingBudget}')`)],
      { encoding: 'utf8' });
      assert.equal(result.status, 0, result.stderr);
      assert.deepEqual(JSON.parse(result.stdout), { sdkTimeoutMs: expected, verdict: { passed: true } });
    }
    await writeFile(loaderPath, `export async function resolve(specifier, context, nextResolve) {
      if (specifier === '@github/copilot-sdk') {
        return { url: ${JSON.stringify(`data:text/javascript,${encodeURIComponent(sdkSource)}`)}, shortCircuit: true };
      }
      return nextResolve(specifier, context);
    }
    export async function load(url, context, nextLoad) {
      if (url === ${JSON.stringify(entryUrl)}) {
        return { format: 'module', source: ${JSON.stringify(`${script.replace("resolveGradingTimeoutMs('10m')", "resolveGradingTimeoutMs('30m')")}
          console.log(JSON.stringify(process.argv.slice(2)));`)}, shortCircuit: true };
      }
      return nextLoad(url, context);
    }`);
    await writeFile(preloadPath, `import { register } from 'node:module';
      register(${JSON.stringify(pathToFileURL(loaderPath).href)}, import.meta.url);`);
    for (const command of ['experiment', 'compare']) {
      for (const [timeout, expected] of [[undefined, 600_000], ['15m', 900_000], ['30s', 30_000]]) {
        const result = spawnSync(process.execPath, ['--import', pathToFileURL(preloadPath).href,
          fileURLToPath(new URL('../src/vally.mjs', import.meta.url)),
          ...(timeout ? ['--judge-timeout', timeout] : []), command, 'fixture'],
        { encoding: 'utf8', env: { ...process.env, EF_HARNESS_JUDGE_TIMEOUT_MS: '1' } });
        assert.equal(result.status, 0, result.stderr);
        const lines = result.stdout.trim().split(/\r?\n/).map(JSON.parse);
        assert.deepEqual(lines, [
          { sdkTimeoutMs: expected, verdict: { passed: true } },
          [command, 'fixture'],
        ]);
      }
    }
    assert.equal(await readFile(fileURLToPath(clientUrl), 'utf8'), originalSource);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('judge timeout adapter preserves tighter scoring budgets, evidence and lifecycle', async () => {
  const evidence = { workingDirectory: 'evidence', evidenceFiles: ['output.txt'] };
  const signal = new AbortController().signal;
  const response = { args: { passed: true } };
  const received = [];
  let shutdowns = 0;
  const client = withJudgeTimeout({
    supportsWorkspaceDelivery: true,
    judge(options) { received.push(options); return response; },
    shutdown() { shutdowns++; },
  });
  for (const timeoutMs of [undefined, 30_000, 900_000]) {
    assert.equal(await client.judge({ workspace: evidence, signal, timeoutMs }), response);
  }
  assert.equal(client.supportsWorkspaceDelivery, true);
  assert.deepEqual(received.map((options) => options.timeoutMs), [600_000, 30_000, 600_000]);
  assert.ok(received.every((options) => options.workspace === evidence && options.signal === signal));
  await client.shutdown();
  assert.equal(shutdowns, 1);
  assert.deepEqual(judgeTimeoutLimits(), { comparisonDeadlineMs: 1_840_000, retryBudgetMs: 2_040_000 });
  for (const value of [0, -1, NaN, Infinity, 2_147_483_647]) {
    assert.throws(() => withJudgeTimeout({}, value), /Judge timeout must be a positive integer/);
  }
});

test('judge timeout loader scales comparison deadlines and fails closed on native drift', async () => {
  const entryUrl = new URL('../node_modules/@microsoft/vally-cli/dist/index.js', import.meta.url);
  const promptGraderUrl = new URL('./graders/llm/prompt-grader.js', import.meta.resolve('@microsoft/vally')).href;
  const comparisonRunnerUrl = new URL('./compare-runner.js', entryUrl).href;
  const clientUrl = new URL('./graders/llm/llm-client.js', import.meta.resolve('@microsoft/vally')).href;
  initializeJudgeLoader({ clientUrl, promptGraderUrl, comparisonRunnerUrl, timeoutMs: 600_000 });
  for (const [url, expected] of [[promptGraderUrl, 'const COMPARISON_DEADLINE_MS = 1840000;'],
    [comparisonRunnerUrl, 'const DEFAULT_RETRY_BUDGET_MS = 2040000;']]) {
    const original = await readFile(fileURLToPath(url), 'utf8');
    const loaded = await loadJudgeModule(url, {}, async () => ({ source: Buffer.from(original), format: 'module' }));
    assert.ok(loaded.source.includes(expected));
    assert.equal(await readFile(fileURLToPath(url), 'utf8'), original);
    await assert.rejects(loadJudgeModule(url, {}, async () => ({ source: 'changed upstream', format: 'module' })),
      /Unsupported Vally timeout implementation/);
  }
  assert.deepEqual(judgeTimeoutLimits(900_000), { comparisonDeadlineMs: 2_740_000, retryBudgetMs: 2_940_000 });
  assert.deepEqual(judgeTimeoutLimits(30_000), { comparisonDeadlineMs: 400_000, retryBudgetMs: 600_000 });
  for (const value of [0, -1, 0.5, NaN, Infinity, 2_147_483_647]) {
    assert.throws(() => judgeTimeoutLimits(value), /timer-safe comparison and retry budgets/);
  }
});

test('all evals specify and resolve judge defaults and overrides through Vally', async () => {
  const components = await discoverComponents();
  assert.ok(components.length > 0);
  for (const component of components) {
    const evalPath = join(defaultRepoRoot, component.eval);
    const authored = parse(await readFile(evalPath, 'utf8'));
    assert.equal(authored.defaults.judge_reasoning_effort, '${JUDGE_REASONING_EFFORT=medium}', component.id);
    assert.equal(authored.defaults.judge_timeout, '${JUDGE_TIMEOUT=10m}', component.id);
    assert.equal(authored.scoring.timeout, '${SCORING_TIMEOUT=10m}', component.id);
    const defaults = (await loadEvalWithParams(evalPath)).spec.defaults;
    assert.equal(defaults.judge_reasoning_effort, 'medium', component.id);
    assert.equal(defaults.judge_model, 'gpt-6.1-terra');
    assert.equal(defaults.judge_timeout, '10m', component.id);
    assert.equal(authored.defaults.timeout, undefined, component.id);
    assert.equal(defaults.timeout, undefined, component.id);
    const resolved = (await loadEvalWithParams(evalPath)).spec;
    assert.equal(resolved.scoring.timeout, '10m', component.id);
    for (const [index, stimulus] of resolved.stimuli.entries()) {
      assert.equal(stimulus.constraints.max_duration,
        authored.stimuli[index].constraints.max_duration.match(/^\$\{EVAL_TIMEOUT=([^}]+)\}$/)[1],
        `${component.id}/${stimulus.name}`);
    }
    const overridden = (await loadEvalWithParams(evalPath, {
      cliParams: { JUDGE_REASONING_EFFORT: 'high', JUDGE_MODEL: 'gpt-6.1-sol', JUDGE_TIMEOUT: '12m', SCORING_TIMEOUT: '15m', EVAL_TIMEOUT: '45m' },
    })).spec;
    assert.equal(overridden.defaults.judge_reasoning_effort, 'high', component.id);
    assert.equal(overridden.defaults.judge_model, 'gpt-6.1-sol');
    assert.equal(overridden.defaults.judge_timeout, '12m', component.id);
    assert.equal(overridden.scoring.timeout, '15m', component.id);
    assert.equal(overridden.defaults.timeout, undefined, component.id);
    for (const stimulus of overridden.stimuli) {
      assert.equal(stimulus.constraints.max_duration, '45m', `${component.id}/${stimulus.name}`);
    }
  }
});

test('query placement eval stages competing ownership hierarchies without prompt hints', async () => {
  const evalUrl = new URL('../skills/query-pipeline/eval.yaml', import.meta.url);
  assert.deepEqual(await validateEval(fileURLToPath(evalUrl), 'query-pipeline'), []);
  const spec = parse(await readFile(evalUrl, 'utf8'));
  const stimulus = spec.stimuli.find((item) => item.name === 'move-provider-query-regression-to-specifications');
  assert.doesNotMatch(stimulus.prompt, /OwnedEntityQuery|relational.only|core base|null.safe|empty collection/i);
  const destinations = new Set(stimulus.agent_environment.files.map((file) => file.dest));
  for (const snapshot of ['before', 'proposed']) {
    for (const [project, type] of [
      ['EFCore.Specification.Tests', 'OwnedEntityQueryTestBase'],
      ['EFCore.Relational.Specification.Tests', 'OwnedEntityQueryRelationalTestBase'],
      ['EFCore.SqlServer.FunctionalTests', 'OwnedEntityQuerySqlServerTest'],
      ['EFCore.Sqlite.FunctionalTests', 'OwnedEntityQuerySqliteTest'],
    ]) {
      assert.ok(destinations.has(`${snapshot}/test/${project}/Query/${type}.cs`));
    }
  }
  const proposal = stimulus.agent_environment.files.find((file) =>
    file.dest === 'proposed/test/EFCore.SqlServer.FunctionalTests/Query/NorthwindMiscellaneousQuerySqlServerTest.cs');
  const source = await readFile(new URL(proposal.src, evalUrl), 'utf8');
  assert.match(source, /Owned_collection_projection_preserves_rows/);
  assert.match(source, /OwnsMany/);
  assert.match(source, /Assert\.Equal\(2, rows.Count\)/);
});

test('review rubrics reject passes with failed mandatory criteria and speculative parallelism', async () => {
  const review = parse(await readFile(new URL('../skills/code-review/eval.yaml', import.meta.url), 'utf8'));
  for (const stimulus of review.stimuli) {
    const grader = stimulus.graders.find((item) => item.type === 'prompt');
    assert.match(grader.config.prompt, /Every rubric criterion is mandatory/);
    assert.match(grader.config.prompt, /pass must not coexist with a failed rubric subcriterion/);
  }
  const query = parse(await readFile(new URL('../skills/query-pipeline/eval.yaml', import.meta.url), 'utf8'));
  const count = query.stimuli.find((item) => item.name === 'review-split-query-count-change');
  assert.match(count.rubric.join('\n'), /does not establish parallel theory rows/);
  assert.match(count.rubric.join('\n'), /child IDs and names/);
});

test('Comment review eval stages both snapshots and the SQL path without diagnosis hints', async () => {
  const evalUrl = new URL('../skills/code-review/eval.yaml', import.meta.url);
  const evalPath = fileURLToPath(evalUrl);
  assert.deepEqual(await validateEval(evalPath, 'code-review'), []);
  const spec = parse(await readFile(evalUrl, 'utf8'));
  const stimulus = spec.stimuli.find((item) => item.name === 'review-linq-comment-addition');
  assert.ok(stimulus);
  assert.doesNotMatch(stimulus.prompt, /inject|normaliz|unicode|full.?width|delimiter|escape|security-review|conventions\.md/i);
  assert.equal(stimulus.graders.find((grader) => grader.type === 'file-matches').config.path, 'review.md');
  assert.equal(stimulus.graders.find((grader) => grader.type === 'prompt').config.scoring, 'binary');

  const destinations = new Set(stimulus.agent_environment.files.map((file) => file.dest));
  for (const snapshot of ['before', 'proposed']) {
    for (const path of [
      'src/EFCore/Extensions/EntityFrameworkQueryableExtensions.cs',
      'src/EFCore/Query/QueryMetadata.cs',
      'src/EFCore.Relational/Query/RelationalQueryMetadata.cs',
      'src/EFCore.Relational/Query/QuerySqlGenerator.cs',
      'test/EFCore.SqlServer.FunctionalTests/Query/QueryCommentTest.cs',
    ]) {
      assert.ok(destinations.has(`${snapshot}/${path}`), `${snapshot}/${path} is staged`);
    }
  }
  for (const path of [
    'proposed/src/EFCore/Query/Internal/QueryComment.cs',
    'proposed/src/EFCore.Relational/Query/Internal/QueryCommentFormatter.cs',
    'proposed/src/EFCore.Relational/Query/Internal/QueryCommentWriter.cs',
    'context/src/EFCore.Relational/Storage/RelationalSqlGenerationHelper.cs',
    'context/src/EFCore.Relational/Storage/RelationalCommand.cs',
    'context/Application/OrderSearch.cs',
    'context/review-context.md',
    'docs/threat_model.md',
  ]) {
    assert.ok(destinations.has(path), `${path} is staged`);
  }
  for (const file of stimulus.agent_environment.files) {
    assert.doesNotMatch(file.src, /\.(?:diff|patch)$/i);
    assert.ok((await readFile(new URL(file.src, evalUrl), 'utf8')).length > 0, `${file.src} exists`);
  }
});

test('triage sandbox evals stage self-contained reports and grade safety without diagnosis hints', async () => {
  const evalUrl = new URL('../skills/triage/eval.yaml', import.meta.url);
  const spec = parse(await readFile(evalUrl, 'utf8'));
  assert.deepEqual(await validateEval(fileURLToPath(evalUrl), 'triage'), []);
  assert.deepEqual(spec.agent_environment, { skills: ['../../../../.agents/skills/triage'] });
  const stimuli = spec.stimuli.filter((stimulus) => stimulus.name.startsWith('sandbox-triage-'));
  assert.equal(stimuli.length, 3);
  for (const stimulus of stimuli) {
    assert.match(stimulus.prompt, /sandbox mode/);
    assert.match(stimulus.prompt, /Write triage-report\.md/);
    assert.doesNotMatch(stimulus.prompt, /malicious|injection|credential|unsafe|blocked|vulnerability/i);
    assert.deepEqual(stimulus.agent_environment.files.map((file) => file.dest), ['issue.md', 'issue-context.md']);
    for (const file of stimulus.agent_environment.files) {
      assert.match(file.src, /^fixtures\/sandbox\/.*\.md$/);
      assert.ok((await readFile(new URL(file.src, evalUrl), 'utf8')).length > 0);
    }
    const artifactGrader = stimulus.graders.find((grader) => grader.type === 'file-matches');
    assert.equal(artifactGrader.config.path, 'triage-report.md');
    const promptGrader = stimulus.graders.find((grader) => grader.type === 'prompt');
    assert.deepEqual(promptGrader.config.evidence, ['trajectory', 'repo']);
    assert.equal(promptGrader.config.scoring, 'binary');
    assert.equal(promptGrader.config.threshold, 1);
    assert.ok(stimulus.graders.some((grader) => grader.type === 'token-budget'));
  }
  const securityStop = spec.stimuli.find((stimulus) => stimulus.name === 'stop-on-a-security-sensitive-relational-report');
  assert.equal(securityStop.graders.find((grader) => grader.type === 'file-not-exists').config.path, 'triage-report.md');
});

test('eval forwards judge effort and independent timeouts to the adapted launcher and parameters', async () => {
  const root = await makeRepo();
  try {
    const capturePath = join(root, 'vally-call.json');
    const preloadPath = join(root, 'capture-vally.mjs');
    await writeFile(preloadPath, `
import { createRequire, syncBuiltinESMExports } from 'node:module';
import { writeFileSync } from 'node:fs';
const childProcess = createRequire(import.meta.url)('node:child_process');
childProcess.spawnSync = (executable, args, options) => {
  writeFileSync(${JSON.stringify(capturePath)}, JSON.stringify({ executable, args, effort: options.env.EVAL_JUDGE_REASONING_EFFORT }));
  throw new Error('Intercepted Vally invocation');
};
syncBuiltinESMExports();
`);
    await writeFile(join(root, 'eng/harness-evaluation/skills/example/eval.yaml'),
      evalYaml('example').replace('${SCORING_TIMEOUT=10m}', '${SCORING_TIMEOUT=12m}')
        .replace('${JUDGE_TIMEOUT=10m}', '${JUDGE_TIMEOUT=11m}'));
    for (const options of [[], ['--judge-timeout', '20m'], ['--scoring-timeout', '15m', '--timeout', '30m']]) {
      const result = spawnSync(process.execPath, [
        '--import', pathToFileURL(preloadPath).href,
        fileURLToPath(new URL('../src/cli.mjs', import.meta.url)),
        'eval', 'example', '--repo-root', root, '--judge-reasoning-effort', 'high', ...options,
      ], { encoding: 'utf8', env: { ...process.env, EF_HARNESS_JUDGE_TIMEOUT_MS: '1' } });
      assert.match(result.stderr, /Intercepted Vally invocation/);
      const invocation = JSON.parse(await readFile(capturePath, 'utf8'));
      assert.equal(invocation.executable, process.execPath);
      assert.equal(invocation.args[0], fileURLToPath(new URL('../src/vally.mjs', import.meta.url)));
      assert.deepEqual(invocation.args.slice(1, 5), ['--judge-timeout', options[0] === '--judge-timeout' ? '20m' : '11m', 'experiment', 'run']);
      assert.ok(invocation.args.includes('JUDGE_REASONING_EFFORT=high'));
      assert.equal(invocation.effort, 'high');
      if (options[0] === '--judge-timeout') {
        assert.ok(invocation.args.includes('JUDGE_TIMEOUT=20m'));
        assert.ok(!invocation.args.some((arg) => arg.startsWith('SCORING_TIMEOUT=') || arg.startsWith('EVAL_TIMEOUT=')));
      }
      if (options[0] === '--scoring-timeout') {
        assert.ok(invocation.args.includes('SCORING_TIMEOUT=15m'));
        assert.ok(invocation.args.includes('EVAL_TIMEOUT=30m'));
        assert.ok(!invocation.args.some((arg) => arg.startsWith('JUDGE_TIMEOUT=')));
      }
    }
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval rejects invalid timeouts and unsupported execution overrides before deleting output', async () => {
  const root = await makeRepo();
  try {
    const output = join(root, 'artifacts/results');
    await mkdir(output, { recursive: true });
    const sentinel = join(output, 'keep.txt');
    await writeFile(sentinel, 'keep');
    const cliPath = fileURLToPath(new URL('../src/cli.mjs', import.meta.url));
    for (const option of ['--judge-timeout', '--scoring-timeout', '--timeout']) {
      for (const value of ['0s', 'forever', '999999999h']) {
        const result = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
          '--output', output, option, value], { encoding: 'utf8' });
        assert.notEqual(result.status, 0);
        assert.ok(result.stderr.includes(`${option} must be a positive duration`), result.stderr);
        assert.equal(await readFile(sentinel, 'utf8'), 'keep');
      }
    }
    const evalPath = join(root, 'eng/harness-evaluation/skills/example/eval.yaml');
    await writeFile(evalPath, evalYaml('example').replace('max_duration: ${EVAL_TIMEOUT=1m}', 'max_duration: 1m'));
    const unsupported = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
      '--output', output, '--timeout', '30m'], { encoding: 'utf8' });
    assert.notEqual(unsupported.status, 0);
    assert.match(unsupported.stderr, /All stimulus duration limits must use the EVAL_TIMEOUT parameter/);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
    await writeFile(evalPath, evalYaml('example').replace('${SCORING_TIMEOUT=10m}', '10m'));
    const unsupportedScoring = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
      '--output', output, '--scoring-timeout', '15m'], { encoding: 'utf8' });
    assert.notEqual(unsupportedScoring.status, 0);
    assert.match(unsupportedScoring.stderr, /scoring.timeout must use the SCORING_TIMEOUT parameter/);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
    await writeFile(evalPath, evalYaml('example').replace('${JUDGE_TIMEOUT=10m}', '10m'));
    const unsupportedJudge = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
      '--output', output, '--judge-timeout', '15m'], { encoding: 'utf8' });
    assert.notEqual(unsupportedJudge.status, 0);
    assert.match(unsupportedJudge.stderr, /defaults.judge_timeout must use the JUDGE_TIMEOUT parameter/);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
    await writeFile(evalPath, evalYaml('example').replace('${SCORING_TIMEOUT=10m}', 'forever'));
    const invalidDefault = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
      '--output', output], { encoding: 'utf8' });
    assert.notEqual(invalidDefault.status, 0);
    assert.match(invalidDefault.stderr, /scoring.timeout must be a positive duration/);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
    await writeFile(evalPath, evalYaml('example').replace('${JUDGE_TIMEOUT=10m}', 'forever'));
    const invalidJudgeDefault = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
      '--output', output], { encoding: 'utf8' });
    assert.notEqual(invalidJudgeDefault.status, 0);
    assert.match(invalidJudgeDefault.stderr, /defaults.judge_timeout must be a positive duration/);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
    await writeFile(evalPath, evalYaml('example'));
    const overflow = spawnSync(process.execPath, [cliPath, 'eval', 'example', '--repo-root', root,
      '--output', output, '--judge-timeout', '300h'], { encoding: 'utf8' });
    assert.notEqual(overflow.status, 0);
    assert.match(overflow.stderr, /timer-safe comparison and retry budgets/);
    assert.equal(await readFile(sentinel, 'utf8'), 'keep');
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval output is constrained to an artifacts descendant', async () => {
  const repoRoot = await mkdtemp(join(tmpdir(), 'efcore-agent-output-'));
  const outside = await mkdtemp(join(tmpdir(), 'efcore-agent-output-outside-'));
  try {
    const unsafePaths = [
      parsePath(repoRoot).root,
      dirname(repoRoot),
      repoRoot,
      join(repoRoot, 'src'),
      join(repoRoot, 'artifacts'),
      join(tmpdir(), 'unrelated-output'),
    ];
    for (const unsafePath of unsafePaths) {
      assert.throws(
        () => validateOutputRoot(unsafePath, repoRoot),
        /Output path must be a descendant/,
      );
    }
    assert.doesNotThrow(() => validateOutputRoot(join(repoRoot, 'artifacts', 'harness-evaluation'), repoRoot));

    await mkdir(join(repoRoot, 'artifacts'), { recursive: true });
    await symlink(outside, join(repoRoot, 'artifacts', 'escape'), 'junction');
    assert.throws(
      () => validateOutputRoot(join(repoRoot, 'artifacts', 'escape', 'unrelated'), repoRoot),
      /Output path must not contain symbolic links or junctions/,
    );

    await rm(join(repoRoot, 'artifacts', 'escape'));
    const missingTarget = join(outside, 'missing');
    await symlink(
      missingTarget,
      join(repoRoot, 'artifacts', 'dangling'),
      process.platform === 'win32' ? 'junction' : 'dir',
    );
    assert.throws(
      () => validateOutputRoot(join(repoRoot, 'artifacts', 'dangling', 'unrelated'), repoRoot),
      /Output path must not contain symbolic links or junctions/,
    );

    await rm(join(repoRoot, 'artifacts', 'dangling'));
    await rm(join(repoRoot, 'artifacts'), { recursive: true });
    await symlink(missingTarget, join(repoRoot, 'artifacts'), process.platform === 'win32' ? 'junction' : 'dir');
    assert.throws(
      () => validateOutputRoot(join(repoRoot, 'artifacts', 'harness-evaluation'), repoRoot),
      /Output path must not contain symbolic links or junctions/,
    );
  } finally {
    await rm(repoRoot, { recursive: true, force: true });
    await rm(outside, { recursive: true, force: true });
  }
});

test('inventory rejects an eval without a matching component', async () => {
  const root = await makeRepo();
  try {
    await mkdir(join(root, 'eng/harness-evaluation/skills/orphan'), { recursive: true });
    await writeFile(join(root, 'eng/harness-evaluation/skills/orphan/eval.yaml'), 'name: orphan\n');
    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /Evaluation has no matching harness component.*orphan/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory does not require coverage for plugins', async () => {
  const root = await makeRepo();
  try {
    await mkdir(join(root, '.github/copilot'), { recursive: true });
    await writeFile(join(root, '.github/copilot/settings.json'), JSON.stringify({
      enabledPlugins: {
        'example@example-marketplace': true,
      },
    }));
    const result = await validateInventory(root);
    assert.deepEqual(result.errors, []);
    assert.deepEqual(result.components.map((component) => component.id), ['copilot-instructions', 'example']);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory fails closed for malformed eval YAML', async () => {
  const root = await makeRepo();
  try {
    await writeFile(join(root, 'eng/harness-evaluation/skills/example/eval.yaml'), 'name: [');
    await assert.rejects(validateInventory(root));
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory requires eval names to match component ids', async () => {
  const root = await makeRepo();
  try {
    const evalPath = join(root, 'eng/harness-evaluation/skills/example/eval.yaml');
    await writeFile(evalPath, evalYaml('other'));
    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /example: eval name 'other' must match component id 'example'/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory rejects skill invocation from shared variant scoring', async () => {
  const root = await makeRepo();
  try {
    await writeFile(
      join(root, 'eng/harness-evaluation/skills/example/eval.yaml'),
      evalYaml('example', `      - type: skill-invocation\n        config:\n          required: [example]`),
    );
    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /skill-invocation grader must not be used in shared control\/treatment scoring/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory requires an isolated root treatment environment', async () => {
  const root = await makeRepo();
  try {
    const evalPath = join(root, 'eng/harness-evaluation/skills/example/eval.yaml');
    const grader = `      - type: output-contains\n        config:\n          substring: Anchor`;
    await writeFile(evalPath, evalYaml('example', grader, ''));
    let result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /eval root agent_environment must declare the evaluated customization/);

    const leaked = evalYaml('example', grader).replace(
      '    agent_environment:\n      files:',
      '    agent_environment:\n      skills:\n        - ../../../../.agents/skills/example\n      files:',
    );
    await writeFile(evalPath, leaked);
    result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /evaluated customization must not be declared per stimulus/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory discovers declarative agents with plain and agent-qualified names', async () => {
  const root = await makeRepo();
  try {
    await writeFile(join(root, '.github/agents/plain.md'), '---\nname: plain\ndescription: Plain agent\n---\n');
    await writeFile(join(root, '.github/agents/qualified.agent.md'), '---\nname: qualified\ndescription: Qualified agent\n---\n');
    await mkdir(join(root, 'eng/harness-evaluation/agents/plain'), { recursive: true });
    await mkdir(join(root, 'eng/harness-evaluation/agents/qualified'), { recursive: true });
    await writeFile(
      join(root, 'eng/harness-evaluation/agents/plain/eval.yaml'),
      evalYaml('plain', undefined, `agent_environment:\n  files:\n    - src: ../../../../.github/agents/plain.md\n      dest: .github/agents/plain.md\n`),
    );
    await writeFile(
      join(root, 'eng/harness-evaluation/agents/qualified/eval.yaml'),
      evalYaml('qualified', undefined, `agent_environment:\n  files:\n    - src: ../../../../.github/agents/qualified.agent.md\n      dest: .github/agents/qualified.agent.md\n`),
    );

    const result = await validateInventory(root);
    assert.deepEqual(result.errors, []);
    assert.deepEqual(result.components.map((component) => component.id), [
      'copilot-instructions', 'example', 'plain', 'qualified',
    ]);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory requires evals for agents and agentic workflows', async () => {
  const root = await makeRepo();
  try {
    await writeFile(join(root, '.github/agents/reviewer.agent.md'), '---\nname: reviewer\ndescription: Reviews changes\n---\n');
    await writeFile(join(root, '.github/workflows/issue-triage.md'), '---\non: issues\n---\nTriage the issue.\n');

    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /reviewer: eval does not exist: eng\/harness-evaluation\/agents\/reviewer\/eval\.yaml/);
    assert.match(result.errors.join('\n'), /issue-triage: eval does not exist: eng\/harness-evaluation\/workflows\/issue-triage\/eval\.yaml/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory rejects orphan agent and agentic-workflow evals', async () => {
  const root = await makeRepo();
  try {
    await mkdir(join(root, 'eng/harness-evaluation/agents/orphan-agent'), { recursive: true });
    await mkdir(join(root, 'eng/harness-evaluation/workflows/orphan-workflow'), { recursive: true });
    await writeFile(join(root, 'eng/harness-evaluation/agents/orphan-agent/eval.yaml'), 'name: orphan-agent\n');
    await writeFile(join(root, 'eng/harness-evaluation/workflows/orphan-workflow/eval.yaml'), 'name: orphan-workflow\n');

    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /Evaluation has no matching harness component: eng\/harness-evaluation\/agents\/orphan-agent\/eval\.yaml/);
    assert.match(result.errors.join('\n'), /Evaluation has no matching harness component: eng\/harness-evaluation\/workflows\/orphan-workflow\/eval\.yaml/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory validates agentic workflows without compiled lock files', async () => {
  const root = await makeRepo();
  try {
    await writeFile(join(root, '.github/workflows/issue-triage.md'), '---\non: issues\n---\nTriage the issue.\n');
    await writeFile(join(root, '.github/workflows/issue-triage.lock.yml'), 'name: compiled\n');
    await mkdir(join(root, 'eng/harness-evaluation/workflows/issue-triage'), { recursive: true });
    await writeFile(
      join(root, 'eng/harness-evaluation/workflows/issue-triage/eval.yaml'),
      evalYaml('issue-triage', undefined, `agent_environment:\n  files:\n    - src: ../../../../.github/workflows/issue-triage.md\n      dest: .github/workflows/issue-triage.md\n`),
    );

    const result = await validateInventory(root);
    assert.deepEqual(result.errors, []);
    assert.deepEqual(
      result.components.filter((component) => component.kind === 'agentic-workflow'),
      [{
        id: 'issue-triage',
        kind: 'agentic-workflow',
        source: '.github/workflows/issue-triage.md',
        eval: 'eng/harness-evaluation/workflows/issue-triage/eval.yaml',
      }],
    );

  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval validation delegates missing input files to Vally', async () => {
  const root = await makeRepo();
  try {
    const evalPath = join(root, 'eng/harness-evaluation/skills/example/eval.yaml');
    await writeFile(evalPath, `name: bad\ndefaults:\n  runs: \${RUNS=2}\nstimuli:\n  - name: missing-input\n    prompt: Do a generic task.\n    constraints:\n      max_turns: 10\n      max_tokens: 5000\n      max_duration: 1m\n    agent_environment:\n      files:\n        - src: src/EFCore/Missing.cs\n          dest: src/EFCore/Missing.cs\n    graders:\n      - type: token-budget\n        config:\n          max: 5000\nscoring:\n  weights:\n    token-budget: 0.1\n  threshold: 0.75\n`);
    const errors = await validateEval(evalPath);
    assert.deepEqual(errors, []);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('eval validation requires source-owned token grading', async () => {
  const root = await makeRepo();
  try {
    const evalPath = join(root, 'bad-token-eval.yaml');
    await writeFile(evalPath, evalYaml('example').replace(
      '      - type: token-budget\n        config:\n          max: 5000\n',
      '',
    ));
    const errors = await validateEval(evalPath);
    assert.match(errors.join('\n'), /token-budget grader must be defined/);

    await writeFile(evalPath, evalYaml('example').replace('token-budget: 0.1', 'token-budget: 0.2'));
    assert.deepEqual(await validateEval(evalPath), []);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('experiment discovery resolves one timestamped Vally run and rejects ambiguity', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-results-'));
  try {
    const first = join(root, '2026-09-15T21-25-40-253Z');
    await mkdir(first, { recursive: true });
    await writeFile(join(first, 'plan-snapshot.json'), '{}\n');
    assert.equal(await findExperimentRunDirectory(root), first);

    const second = join(root, '2026-09-15T21-30-00-000Z');
    await mkdir(second, { recursive: true });
    await writeFile(join(second, 'plan-snapshot.json'), '{}\n');
    await assert.rejects(findExperimentRunDirectory(root), /Expected one Vally experiment.*found 2/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('treatment verdict uses the eval scoring threshold', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-results-'));
  try {
    const evalPath = join(root, 'eval.yaml');
    const resultsPath = join(root, 'results.jsonl');
    const planPath = join(root, 'plan-snapshot.json');
    await writeFile(evalPath, `name: example\nstimuli:\n  - name: first\n    prompt: Test.\n    graders:\n      - type: output-contains\n        config: { substring: Test }\nscoring:\n  weights:\n    output-contains: 1\n  threshold: 0.75\n`);
    await writeFile(planPath, JSON.stringify({
      type: 'experiment-plan-snapshot',
      evals: [{
        variant: 'treatment',
        evalName: 'example',
        model: 'mock',
        runs: 1,
        plannedStimulusNames: ['first'],
      }],
    }));
    await writeFile(resultsPath, `${JSON.stringify({
      type: 'trial-result',
      evalName: 'example',
      model: 'mock',
      stimulus: 'first',
      status: 'success',
      gradeResult: { passed: true, score: 0.8 },
    })}\n`);
    assert.equal(await variantPassed(resultsPath, evalPath, planPath), true);

    await writeFile(resultsPath, `${JSON.stringify({
      type: 'trial-result',
      evalName: 'example',
      model: 'mock',
      stimulus: 'first',
      status: 'success',
      gradeResult: { passed: true, score: 0.7 },
    })}\n`);
    assert.equal(await variantPassed(resultsPath, evalPath, planPath), false);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('completed controls can be compared despite failing the scoring threshold', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-completion-'));
  try {
    const resultsPath = join(root, 'results.jsonl');
    const planPath = join(root, 'plan-snapshot.json');
    const evalPath = join(root, 'eval.yaml');
    await writeFile(evalPath, evalYaml('example'));
    await writeFile(planPath, JSON.stringify({
      type: 'experiment-plan-snapshot',
      evals: [{ variant: 'control', evalName: 'example', model: 'mock', runs: 1, plannedStimulusNames: ['inspect-anchor'] }],
    }));
    const trial = {
      type: 'trial-result', evalName: 'example', model: 'mock', stimulus: 'inspect-anchor',
      status: 'success', gradeResult: { passed: false, score: 0.1, details: [] },
    };
    await writeFile(resultsPath, `${JSON.stringify(trial)}\n`);
    assert.equal(await variantCompleted(resultsPath, planPath, 'control'), true);
    assert.equal(await variantPassed(resultsPath, evalPath, planPath, 'control'), false);

    await writeFile(planPath, JSON.stringify({
      type: 'experiment-plan-snapshot',
      evals: [
        { variant: 'control', evalName: 'example', model: 'mock', runs: 1, plannedStimulusNames: ['inspect-anchor'] },
        { variant: 'control', evalName: 'failed', model: 'mock', failure: 'Planning failed' },
      ],
    }));
    assert.equal(await variantCompleted(resultsPath, planPath, 'control'), false);
    await writeFile(planPath, JSON.stringify({
      type: 'experiment-plan-snapshot',
      evals: [{ variant: 'control', evalName: 'example', model: 'mock', runs: 1, plannedStimulusNames: ['inspect-anchor'] }],
    }));

    for (const incomplete of [
      { ...trial, status: 'error' },
      { ...trial, gradeResult: null },
      { ...trial, gradeResult: { score: 1, status: 'error' } },
      { ...trial, gradeResult: { score: 1, details: [{ status: 'error' }] } },
      { ...trial, gradeResult: { score: 1, details: [{ details: [{ status: 'error' }] }] } },
    ]) {
      await writeFile(resultsPath, `${JSON.stringify(incomplete)}\n`);
      assert.equal(await variantCompleted(resultsPath, planPath, 'control'), false);
    }
    await rm(resultsPath);
    assert.equal(await variantCompleted(resultsPath, planPath, 'control'), false);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('treatment activation gate accepts the exact target skill', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-activation-'));
  try {
    const resultsPath = await writeSkillInvocationTrial(root);
    assert.equal(await variantInvokedSkill(resultsPath, 'example'), true);

    await writeFile(
      join(root, 'treatment/example/inspect-anchor/mock/0/events.jsonl'),
      `${JSON.stringify({ type: 'skill_activation', data: { name: 'example' } })}\n`,
    );
    assert.equal(await variantInvokedSkill(resultsPath, 'example'), true);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('treatment activation gate fails closed for wrong or missing evidence', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-activation-'));
  try {
    const resultsPath = await writeSkillInvocationTrial(root, 'other');
    assert.equal(await variantInvokedSkill(resultsPath, 'example'), false);

    await rm(join(root, 'treatment/example/inspect-anchor/mock/0/events.jsonl'));
    assert.equal(await variantInvokedSkill(resultsPath, 'example'), false);

    await writeFile(join(root, 'treatment/example/inspect-anchor/mock/0/events.jsonl'), '{invalid json}\n');
    assert.equal(await variantInvokedSkill(resultsPath, 'example'), false);

    await rm(join(root, 'treatment/example/inspect-anchor/mock/0/metadata.json'));
    assert.equal(await variantInvokedSkill(resultsPath, 'example'), false);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('treatment verdict requires every planned trial index', async () => {
  const root = await mkdtemp(join(tmpdir(), 'efcore-agent-results-'));
  try {
    const evalPath = join(root, 'eval.yaml');
    const resultsPath = join(root, 'results.jsonl');
    const planPath = join(root, 'plan-snapshot.json');
    await writeFile(evalPath, `name: example\nstimuli:\n  - name: first\n    prompt: Test.\n    graders:\n      - type: output-contains\n        config: { substring: Test }\nscoring:\n  weights:\n    output-contains: 1\n  threshold: 0.75\n`);
    await writeFile(planPath, JSON.stringify({
      type: 'experiment-plan-snapshot',
      evals: [{
        variant: 'treatment',
        evalName: 'example',
        model: 'mock',
        runs: 2,
        plannedStimulusNames: ['first'],
      }],
    }));
    const passingTrial = {
      type: 'trial-result',
      evalName: 'example',
      model: 'mock',
      stimulus: 'first',
      trialIndex: 0,
      status: 'success',
      gradeResult: { passed: true, score: 1 },
    };
    await writeFile(resultsPath, `${JSON.stringify(passingTrial)}\n`);
    assert.equal(await variantPassed(resultsPath, evalPath, planPath), false);

    await writeFile(resultsPath, `${JSON.stringify(passingTrial)}\n${JSON.stringify(passingTrial)}\n`);
    assert.equal(await variantPassed(resultsPath, evalPath, planPath), false);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});
