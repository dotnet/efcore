import { spawnSync } from 'node:child_process';
import { existsSync, lstatSync, realpathSync } from 'node:fs';
import {
  readFile,
  readdir,
} from 'node:fs/promises';
import { dirname, isAbsolute, join, posix, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
  computeSkillScore,
  computeStimulusScore,
  resolveGradePass,
} from '@microsoft/vally';
import { parse } from 'yaml';

const moduleDirectory = dirname(fileURLToPath(import.meta.url));
export const defaultRepoRoot = resolve(moduleDirectory, '../../..');
export const packageRoot = resolve(moduleDirectory, '..');

const evaluationRoot = 'eng/harness-evaluation';
const componentIdPattern = /^[A-Za-z0-9][A-Za-z0-9._-]*$/;
const componentPatterns = [
  { directory: '.github/instructions', suffixes: ['.instructions.md'], kind: 'instruction', evalDirectory: 'instructions' },
  { directory: '.github/agents', suffixes: ['.agent.md', '.md'], kind: 'agent', evalDirectory: 'agents' },
  { directory: '.github/workflows', suffixes: ['.md'], kind: 'agentic-workflow', evalDirectory: 'workflows' },
  { directory: '.github/prompts', suffixes: ['.prompt.md'], kind: 'prompt', evalDirectory: 'prompts' },
];

export function validateComponentId(id) {
  if (!componentIdPattern.test(id) || id.includes('..')) {
    throw new Error(`Component id contains invalid characters: ${id}`);
  }
}

export function validateOutputRoot(outputRoot, repoRoot = defaultRepoRoot) {
  const artifactsRoot = resolve(repoRoot, 'artifacts');
  const resolvedOutput = resolve(outputRoot);
  const relativeOutput = relative(artifactsRoot, resolvedOutput);
  if (!relativeOutput || relativeOutput === '..' || relativeOutput.startsWith(`..${sep}`) || isAbsolute(relativeOutput)) {
    throw new Error(`Output path must be a descendant of '${artifactsRoot}': ${outputRoot}`);
  }

  let currentPath = resolve(repoRoot);
  for (const segment of relative(currentPath, resolvedOutput).split(sep)) {
    currentPath = join(currentPath, segment);
    try {
      if (lstatSync(currentPath).isSymbolicLink()) {
        throw new Error(`Output path must not contain symbolic links or junctions: ${currentPath}`);
      }
    } catch (error) {
      if (error?.code === 'ENOENT') {
        break;
      }
      throw error;
    }
  }

  if (existsSync(artifactsRoot)) {
    const canonicalRepoRoot = realpathSync(repoRoot);
    const canonicalArtifactsRoot = realpathSync(artifactsRoot);
    const artifactsRelativeToRepo = relative(canonicalRepoRoot, canonicalArtifactsRoot);
    if (!artifactsRelativeToRepo || artifactsRelativeToRepo === '..'
        || artifactsRelativeToRepo.startsWith(`..${sep}`) || isAbsolute(artifactsRelativeToRepo)) {
      throw new Error(`Repository artifacts path must not escape the checkout: ${artifactsRoot}`);
    }

    let existingAncestor = resolvedOutput;
    while (!existsSync(existingAncestor)) {
      existingAncestor = dirname(existingAncestor);
    }
    const canonicalAncestor = realpathSync(existingAncestor);
    const ancestorRelativeToArtifacts = relative(canonicalArtifactsRoot, canonicalAncestor);
    if (ancestorRelativeToArtifacts === '..' || ancestorRelativeToArtifacts.startsWith(`..${sep}`)
        || isAbsolute(ancestorRelativeToArtifacts)) {
      throw new Error(`Output path must not escape repository artifacts: ${outputRoot}`);
    }
  }
}

function toPosix(path) {
  return path.split(sep).join('/');
}

function normalizeRepoPath(path) {
  const posixPath = toPosix(path).replace(/^\.\//, '');
  const normalized = posix.normalize(posixPath);
  if (!posixPath
    || normalized === '.'
    || posix.isAbsolute(normalized)
    || isAbsolute(path)
    || posixPath.split('/').includes('..')) {
    throw new Error(`Unsafe repository path '${path}'.`);
  }

  return normalized;
}

async function directoryNames(path) {
  if (!existsSync(path)) {
    return [];
  }

  return (await readdir(path, { withFileTypes: true }))
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name)
    .sort((left, right) => left.localeCompare(right));
}

async function filesRecursively(root, predicate) {
  if (!existsSync(root)) {
    return [];
  }

  const result = [];
  for (const entry of await readdir(root, { withFileTypes: true })) {
    const path = join(root, entry.name);
    if (entry.isDirectory()) {
      result.push(...await filesRecursively(path, predicate));
    } else if (entry.isFile() && predicate(path)) {
      result.push(path);
    }
  }

  return result;
}

function componentIdFromPath(path, directory, suffixes) {
  const suffix = suffixes.find((candidate) => path.endsWith(candidate));
  return path
    .slice(directory.length + 1, -suffix.length)
    .replaceAll('/', '--');
}

async function discoverNonMcpComponents(repoRoot) {
  const result = [{
    id: 'copilot-instructions',
    kind: 'instruction',
    source: '.github/copilot-instructions.md',
    eval: `${evaluationRoot}/instructions/copilot-instructions/eval.yaml`,
  }];

  for (const name of await directoryNames(join(repoRoot, '.agents/skills'))) {
    const source = `.agents/skills/${name}/SKILL.md`;
    if (existsSync(join(repoRoot, source))) {
      result.push({ id: name, kind: 'skill', source, eval: `${evaluationRoot}/skills/${name}/eval.yaml` });
    }
  }

  for (const componentPattern of componentPatterns) {
    for (const absolutePath of await filesRecursively(
      join(repoRoot, componentPattern.directory),
      (path) => componentPattern.suffixes.some((suffix) => path.endsWith(suffix)),
    )) {
      const source = toPosix(relative(repoRoot, absolutePath));
      const id = componentIdFromPath(source, componentPattern.directory, componentPattern.suffixes);
      result.push({
        id,
        kind: componentPattern.kind,
        source,
        eval: `${evaluationRoot}/${componentPattern.evalDirectory}/${id}/eval.yaml`,
      });
    }
  }

  return result.sort((left, right) => left.id.localeCompare(right.id));
}

export async function discoverComponents(repoRoot = defaultRepoRoot) {
  return discoverNonMcpComponents(repoRoot);
}

export async function validateEval(evalPath, componentId) {
  const spec = parse(await readFile(evalPath, 'utf8'));
  const errors = [];
  if (componentId !== undefined && spec?.name !== componentId) {
    errors.push(`eval name '${spec?.name}' must match component id '${componentId}'`);
  }
  if (spec?.scoring?.weights?.['token-budget'] === undefined) {
    errors.push('scoring.weights.token-budget must be defined');
  }

  for (const [index, stimulus] of (spec.stimuli ?? []).entries()) {
    const label = stimulus?.name || `stimuli[${index}]`;
    if (!(stimulus.graders ?? []).some((grader) => grader?.type === 'token-budget')) {
      errors.push(`${label}: token-budget grader must be defined`);
    }
  }

  return errors;
}

async function validateActivationGraders(component, evalPath) {
  if (component.kind !== 'skill') {
    return [];
  }

  const spec = parse(await readFile(evalPath, 'utf8'));
  const errors = [];
  for (const [index, stimulus] of (spec.stimuli ?? []).entries()) {
    const label = stimulus?.name || `stimuli[${index}]`;
    const graders = stimulus?.graders ?? [];
    const invokesTarget = graders.some(
      (grader) => grader?.type === 'skill-invocation'
        && Array.isArray(grader?.config?.required)
        && grader.config.required.length === 1
        && grader.config.required[0] === component.id,
    );
    if (!invokesTarget) {
      errors.push(`${label}: skill eval must require exact activation of '${component.id}'`);
    }
  }

  return errors;
}

function environmentReferencesComponent(environment, component, evalPath, repoRoot) {
  if (!environment || typeof environment !== 'object') {
    return false;
  }

  const componentPath = resolve(repoRoot, component.kind === 'skill' ? dirname(component.source) : component.source);
  const baseDirectory = dirname(evalPath);
  if (component.kind === 'skill') {
    return (environment.skills ?? []).some((skill) => resolve(baseDirectory, skill) === componentPath);
  }

  return (environment.files ?? []).some(
    (file) => resolve(baseDirectory, file.src) === componentPath || resolve(repoRoot, file.dest) === componentPath,
  );
}

async function validateComponentEnvironment(component, evalPath, repoRoot) {
  const spec = parse(await readFile(evalPath, 'utf8'));
  const errors = [];
  const rootEnvironment = spec.agent_environment;
  const rootReferencesComponent = component.kind === 'skill'
    ? environmentReferencesComponent(rootEnvironment, component, evalPath, repoRoot)
    : (rootEnvironment?.files ?? []).some(
      (file) => resolve(dirname(evalPath), file.src) === resolve(repoRoot, component.source)
        && resolve(repoRoot, file.dest) === resolve(repoRoot, component.source),
    );
  if (!rootReferencesComponent) {
    errors.push('eval root agent_environment must declare the evaluated customization');
  }
  const rootSkills = rootEnvironment?.skills ?? [];
  const rootFiles = rootEnvironment?.files ?? [];
  if ((component.kind === 'skill' && (rootSkills.length !== 1 || rootFiles.length !== 0))
      || (component.kind !== 'skill' && (rootSkills.length !== 0 || rootFiles.length !== 1))) {
    errors.push('eval root agent_environment skills/files must contain only the evaluated customization');
  }
  for (const [index, stimulus] of (spec.stimuli ?? []).entries()) {
    if (environmentReferencesComponent(stimulus.agent_environment, component, evalPath, repoRoot)) {
      errors.push(`${stimulus.name || `stimuli[${index}]`}: evaluated customization must not be declared per stimulus`);
    }
  }
  return errors;
}

export async function validateInventory(repoRoot = defaultRepoRoot) {
  const errors = [];
  const components = await discoverComponents(repoRoot);
  const componentIds = new Set();
  const componentEvals = new Set();

  for (const component of components) {
    try {
      validateComponentId(component.id);
    } catch (error) {
      errors.push(error.message);
      continue;
    }

    if (componentIds.has(component.id)) {
      errors.push(`Duplicate component id: ${component.id}`);
    }
    componentIds.add(component.id);

    const source = normalizeRepoPath(component.source);
    const evalPath = normalizeRepoPath(component.eval);
    componentEvals.add(evalPath);

    if (!existsSync(join(repoRoot, source))) {
      errors.push(`${component.id}: source does not exist: ${source}`);
    }
    if (!existsSync(join(repoRoot, evalPath))) {
      errors.push(`${component.id}: eval does not exist: ${evalPath}`);
    } else {
      const absoluteEvalPath = join(repoRoot, evalPath);
      errors.push(...(await validateEval(absoluteEvalPath, component.id)).map((error) => `${component.id}: ${error}`));
      errors.push(...(await validateActivationGraders(component, absoluteEvalPath)).map((error) => `${component.id}: ${error}`));
      errors.push(...(await validateComponentEnvironment(component, absoluteEvalPath, repoRoot)).map(
        (error) => `${component.id}: ${error}`,
      ));
    }
  }

  for (const directory of ['skills', 'instructions', 'agents', 'workflows', 'prompts']) {
    for (const absolutePath of await filesRecursively(
      join(repoRoot, evaluationRoot, directory),
      (path) => path.endsWith('eval.yaml'),
    )) {
      const evalPath = toPosix(relative(repoRoot, absolutePath));
      if (!componentEvals.has(evalPath)) {
        errors.push(`Evaluation has no matching harness component: ${evalPath}`);
      }
    }
  }

  return {
    valid: errors.length === 0,
    errors,
    components,
  };
}

export async function resolveComponent(id, repoRoot = defaultRepoRoot) {
  validateComponentId(id);
  const components = await discoverComponents(repoRoot);
  const component = components.find((candidate) => candidate.id === id);
  if (!component) {
    throw new Error(`Unknown component '${id}'.`);
  }

  return component;
}

export async function findExperimentRunDirectory(directory) {
  const matches = await filesRecursively(directory, (path) => path.endsWith(`${sep}plan-snapshot.json`));
  if (matches.length !== 1) {
    throw new Error(`Expected one Vally experiment below '${directory}', found ${matches.length}.`);
  }
  return dirname(matches[0]);
}

export async function variantPassed(resultsFile, evalFile, planFile, variant = 'treatment') {
  const content = await readFile(resultsFile, 'utf8');
  const records = content.split(/\r?\n/).filter(Boolean).map(JSON.parse);
  const trials = records.filter((record) => record.type === 'trial-result');
  const plan = JSON.parse(await readFile(planFile, 'utf8'));
  if (plan.type !== 'experiment-plan-snapshot' || !Array.isArray(plan.evals)) {
    return false;
  }

  const variantPlans = plan.evals.filter((evalPlan) => evalPlan.variant === variant && !evalPlan.failure);
  if (variantPlans.length === 0 || variantPlans.some((evalPlan) =>
    !Number.isSafeInteger(evalPlan.runs) || evalPlan.runs <= 0
      || !Array.isArray(evalPlan.plannedStimulusNames))) {
    return false;
  }

  const trialKeys = trials
    .map((trial) => `${trial.evalName}\0${trial.model ?? ''}\0${trial.stimulus}\0${trial.trialIndex ?? 0}`)
    .sort();
  const plannedTrialKeys = variantPlans
    .flatMap((evalPlan) => evalPlan.plannedStimulusNames.flatMap((stimulus) =>
      Array.from(
        { length: evalPlan.runs },
        (_, trialIndex) => `${evalPlan.evalName}\0${evalPlan.model ?? ''}\0${stimulus}\0${trialIndex}`,
      )))
    .sort();
  if (trials.length === 0 || JSON.stringify(trialKeys) !== JSON.stringify(plannedTrialKeys)) {
    return false;
  }

  const spec = parse(await readFile(evalFile, 'utf8'));
  const threshold = spec?.scoring?.threshold;
  const stimulusScores = [];

  for (const stimulus of spec.stimuli ?? []) {
    const stimulusTrials = trials.filter((trial) => trial.stimulus === stimulus.name);
    if (stimulusTrials.some((trial) => trial.status !== 'success' || !trial.gradeResult)) {
      return false;
    }

    stimulusScores.push(computeStimulusScore(
      stimulus.name,
      stimulusTrials.map((trial) => ({
        grade: trial.gradeResult,
        passed: resolveGradePass(trial.gradeResult, threshold),
      })),
      (stimulus.graders?.length ?? 0) > 0,
    ));
  }

  return typeof threshold === 'number'
    ? computeSkillScore(spec.name, stimulusScores, threshold).passed
    : stimulusScores.every((stimulus) => stimulus.multiTrial.passToTheK === 1);
}

export function runVally(argumentsList, options = {}) {
  const executable = join(packageRoot, 'node_modules', '.bin', process.platform === 'win32' ? 'vally.cmd' : 'vally');
  const result = spawnSync(executable, argumentsList, {
    cwd: options.cwd ?? defaultRepoRoot,
    env: { ...process.env, ...options.env },
    encoding: 'utf8',
    stdio: options.inherit ? 'inherit' : 'pipe',
    shell: process.platform === 'win32',
    timeout: options.timeout,
  });

  return result;
}
