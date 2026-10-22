#!/usr/bin/env node

import { rm, writeFile } from 'node:fs/promises';
import { join, resolve } from 'node:path';
import { loadEvalWithParams } from '@microsoft/vally';
import { prepareComparison } from './comparison.mjs';
import { judgeTimeoutLimits } from './judge-timeout.mjs';
import {
  defaultRepoRoot,
  findExperimentRunDirectory,
  resolveComponent,
  runVally,
  timeoutMilliseconds,
  validateComponentId,
  validateInventory,
  validateOutputRoot,
  variantCompleted,
  variantInvokedSkill,
  variantPassed,
} from './harness.mjs';

function valueAfter(args, name, fallback) {
  const index = args.indexOf(name);
  if (index === -1) {
    return fallback;
  }

  const value = args[index + 1];
  if (value === undefined || value.startsWith('--')) {
    throw new Error(`${name} requires a value.`);
  }

  return value;
}

function printUsage() {
  console.error(`Usage:
  node src/cli.mjs lint
  node src/cli.mjs eval <component> [--repo-root <directory>] [--model <model>] [--judge-model <model>] [--judge-reasoning-effort <low|medium|high|xhigh>] [--judge-timeout <duration>] [--scoring-timeout <duration>] [--timeout <duration>] [--runs <n>] [--workers <n>] [--require-pass] [--output <directory>]`);
}

async function lint() {
  const validation = await validateInventory();
  if (!validation.valid) {
    throw new Error(`Harness inventory is invalid:\n${validation.errors.join('\n')}`);
  }

  const result = runVally([
    'lint', join(defaultRepoRoot, '.agents', 'skills'),
    '--eval-spec', join(defaultRepoRoot, 'eng', 'harness-evaluation'),
    '--strict',
  ]);
  if (result.status !== 0) {
    process.stderr.write(result.stdout ?? '');
    process.stderr.write(result.stderr ?? '');
    throw new Error('Vally lint failed.');
  }

  console.log(`Vally lint passed for ${validation.components.length} components.`);
}

async function evaluate(args) {
  const componentId = args[0];
  if (!componentId) {
    throw new Error('eval requires a component id.');
  }

  validateComponentId(componentId);
  const repoRoot = resolve(valueAfter(args, '--repo-root', defaultRepoRoot));
  const component = await resolveComponent(componentId, repoRoot);
  const model = valueAfter(args, '--model');
  if (model !== undefined && (!model.trim() || model.includes('::'))) {
    throw new Error(`--model must be a non-empty model name without '::': ${model}`);
  }
  const judgeModel = valueAfter(args, '--judge-model');
  if (judgeModel !== undefined && (!judgeModel.trim() || judgeModel.includes('::'))) {
    throw new Error(`--judge-model must be a non-empty model name without '::': ${judgeModel}`);
  }
  const judgeReasoningEffort = valueAfter(args, '--judge-reasoning-effort');
  if (judgeReasoningEffort !== undefined && !['low', 'medium', 'high', 'xhigh'].includes(judgeReasoningEffort)) {
    throw new Error(`--judge-reasoning-effort must be low, medium, high, or xhigh: ${judgeReasoningEffort}`);
  }
  const judgeTimeout = valueAfter(args, '--judge-timeout');
  if (judgeTimeout !== undefined) {
    judgeTimeoutLimits(timeoutMilliseconds(judgeTimeout, '--judge-timeout'));
  }
  const scoringTimeout = valueAfter(args, '--scoring-timeout');
  if (scoringTimeout !== undefined) {
    timeoutMilliseconds(scoringTimeout, '--scoring-timeout');
  }
  const evalTimeout = valueAfter(args, '--timeout');
  if (evalTimeout !== undefined) {
    timeoutMilliseconds(evalTimeout, '--timeout');
  }
  const runsValue = valueAfter(args, '--runs');
  const runs = runsValue === undefined ? undefined : Number(runsValue);
  if (runs !== undefined && (!Number.isSafeInteger(runs) || runs <= 0)) {
    throw new Error(`--runs must be a positive integer: ${runsValue}`);
  }
  const workersValue = valueAfter(args, '--workers', '1');
  const workers = Number(workersValue);
  if (!Number.isSafeInteger(workers) || workers <= 0) {
    throw new Error(`--workers must be a positive integer: ${workersValue}`);
  }
  const requirePass = args.includes('--require-pass');
  const outputRoot = resolve(valueAfter(args, '--output', join(repoRoot, 'artifacts', 'TestResults', 'harness-evaluation', componentId)));
  validateOutputRoot(outputRoot, repoRoot);
  const evalPath = join(repoRoot, component.eval);
  const experimentPath = join(repoRoot, 'eng', 'harness-evaluation', 'harness.experiment.yaml');
  const cliParams = Object.fromEntries(Object.entries({
    RUNS: runs === undefined ? undefined : String(runs),
    MODEL: model,
    JUDGE_MODEL: judgeModel,
    JUDGE_REASONING_EFFORT: judgeReasoningEffort,
    JUDGE_TIMEOUT: judgeTimeout,
    SCORING_TIMEOUT: scoringTimeout,
    EVAL_TIMEOUT: evalTimeout,
  }).filter(([, value]) => value !== undefined));
  const { spec: treatmentSpec } = await loadEvalWithParams(evalPath, { cliParams });
  const resolvedJudgeTimeout = treatmentSpec.defaults?.judge_timeout;
  judgeTimeoutLimits(timeoutMilliseconds(resolvedJudgeTimeout, 'defaults.judge_timeout'));
  if (judgeTimeout !== undefined && resolvedJudgeTimeout !== judgeTimeout) {
    throw new Error('defaults.judge_timeout must use the JUDGE_TIMEOUT parameter to support --judge-timeout.');
  }
  timeoutMilliseconds(treatmentSpec.scoring?.timeout, 'scoring.timeout');
  if (scoringTimeout !== undefined && treatmentSpec.scoring.timeout !== scoringTimeout) {
    throw new Error('scoring.timeout must use the SCORING_TIMEOUT parameter to support --scoring-timeout.');
  }
  if (evalTimeout !== undefined
    && treatmentSpec.stimuli.some((stimulus) => stimulus.constraints?.max_duration !== evalTimeout)) {
    throw new Error('All stimulus duration limits must use the EVAL_TIMEOUT parameter to support --timeout.');
  }
  await rm(outputRoot, { recursive: true, force: true });
  const experimentArguments = [
    'experiment', 'run', experimentPath,
    '--eval-filter', component.eval.slice('eng/harness-evaluation/'.length),
    '--output-dir', outputRoot,
    '--workers', String(workers),
    '--verbose',
  ];
  for (const [name, value] of Object.entries(cliParams)) {
    experimentArguments.push('--param', `${name}=${value}`);
  }
  const experimentResult = runVally(experimentArguments, {
    cwd: repoRoot,
    inherit: true,
    judgeTimeout: resolvedJudgeTimeout,
    env: judgeReasoningEffort ? { EVAL_JUDGE_REASONING_EFFORT: judgeReasoningEffort } : {},
  });
  const experimentDirectory = await findExperimentRunDirectory(outputRoot);
  const treatmentResults = join(experimentDirectory, 'treatment', 'results.jsonl');
  const experimentPlan = join(experimentDirectory, 'plan-snapshot.json');
  if (experimentResult.status !== 0
    && !(await variantCompleted(join(experimentDirectory, 'control', 'results.jsonl'), experimentPlan, 'control')
      && await variantCompleted(treatmentResults, experimentPlan))) {
    process.exitCode = 1;
    return;
  }
  const qualityPass = await variantPassed(treatmentResults, evalPath, experimentPlan);
  const activationPass = component.kind !== 'skill'
    || await variantInvokedSkill(treatmentResults, component.id);
  await writeFile(join(outputRoot, 'validation.json'), `${JSON.stringify({
    type: 'harness-validation',
    component: component.id,
    quality: { passed: qualityPass },
    activation: {
      required: component.kind === 'skill',
      skill: component.kind === 'skill' ? component.id : null,
      passed: activationPass,
    },
  }, null, 2)}\n`);
  const treatmentPass = !requirePass || (qualityPass && activationPass);
  if (component.kind === 'skill' && !activationPass) {
    console.error(`Treatment '${componentId}' did not invoke the target skill in every trial.`);
  }
  if (requirePass && !qualityPass) {
    console.error(`Treatment '${componentId}' did not meet its committed scoring threshold.`);
  }

  const comparisonDirectory = await prepareComparison(experimentDirectory, join(outputRoot, 'comparison-input'));
  const comparisonArguments = [
    'compare', comparisonDirectory,
    '--output', join(outputRoot, 'comparison.jsonl'),
    '--verbose',
    '--fail-on-regression',
  ];
  const comparisonJudgeModel = judgeModel ?? treatmentSpec.defaults?.judge_model;
  if (comparisonJudgeModel) {
    comparisonArguments.push('--judge-model', comparisonJudgeModel);
  }
  const comparisonJudgeEffort = judgeReasoningEffort
    ?? treatmentSpec.defaults?.judge_reasoning_effort
    ?? process.env.EVAL_JUDGE_REASONING_EFFORT;
  if (comparisonJudgeEffort) {
    comparisonArguments.push('--judge-reasoning-effort', comparisonJudgeEffort);
  }
  const comparisonResult = runVally(comparisonArguments, {
    cwd: repoRoot, inherit: true, judgeTimeout: resolvedJudgeTimeout,
  });
  if (!treatmentPass || comparisonResult.status !== 0) {
    process.exitCode = 1;
  }
}

async function main() {
  const [command, ...args] = process.argv.slice(2);
  switch (command) {
    case 'lint':
      await lint();
      break;
    case 'eval':
      await evaluate(args);
      break;
    default:
      printUsage();
      process.exitCode = 1;
      break;
  }
}

try {
  await main();
} catch (error) {
  console.error(error instanceof Error ? error.stack ?? error.message : String(error));
  process.exitCode = 1;
}
