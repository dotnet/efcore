#!/usr/bin/env node

import { readFile, rm } from 'node:fs/promises';
import { join, resolve } from 'node:path';
import { parse } from 'yaml';
import {
  defaultRepoRoot,
  findExperimentRunDirectory,
  resolveComponent,
  runVally,
  validateComponentId,
  validateInventory,
  validateOutputRoot,
  variantPassed,
} from './harness.mjs';

function valueAfter(args, name, fallback) {
  const index = args.indexOf(name);
  return index === -1 ? fallback : args[index + 1];
}

function printUsage() {
  console.error(`Usage:
  node src/cli.mjs lint
  node src/cli.mjs eval <component> [--repo-root <directory>] [--runs <n>] [--workers <n>] [--require-pass] [--output <directory>]`);
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
  await rm(outputRoot, { recursive: true, force: true });
  const experimentArguments = [
    'experiment', 'run', experimentPath,
    '--eval-filter', component.eval.slice('eng/harness-evaluation/'.length),
    '--output-dir', outputRoot,
    '--workers', String(workers),
    '--verbose',
  ];
  if (runs !== undefined) {
    experimentArguments.push('--param', `RUNS=${runs}`);
  }
  const experimentResult = runVally(experimentArguments, { cwd: repoRoot, inherit: true });
  if (experimentResult.status !== 0) {
    process.exitCode = 1;
    return;
  }

  const experimentDirectory = await findExperimentRunDirectory(outputRoot);
  const treatmentResults = join(experimentDirectory, 'treatment', 'results.jsonl');
  const experimentPlan = join(experimentDirectory, 'plan-snapshot.json');
  const treatmentSpec = parse(await readFile(evalPath, 'utf8'));
  const treatmentPass = !requirePass || await variantPassed(treatmentResults, evalPath, experimentPlan);
  if (!treatmentPass) {
    console.error(`Treatment '${componentId}' did not meet its committed scoring threshold.`);
  }

  const comparisonArguments = [
    'compare', experimentDirectory,
    '--output', join(outputRoot, 'comparison.jsonl'),
    '--verbose',
    '--fail-on-regression',
  ];
  if (treatmentSpec.defaults?.judge_model) {
    comparisonArguments.push('--judge-model', treatmentSpec.defaults.judge_model);
  }
  const comparisonResult = runVally(comparisonArguments, { cwd: repoRoot, inherit: true });
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
