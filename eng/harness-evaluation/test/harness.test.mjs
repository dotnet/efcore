import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtemp, mkdir, readFile, rm, symlink, writeFile } from 'node:fs/promises';
import { dirname, join, parse as parsePath } from 'node:path';
import { tmpdir } from 'node:os';
import { fileURLToPath } from 'node:url';
import test from 'node:test';
import {
  findExperimentRunDirectory,
  validateComponentId,
  validateEval,
  validateInventory,
  validateOutputRoot,
  variantPassed,
} from '../src/harness.mjs';

const evalYaml = (
  name,
  grader = `      - type: output-contains\n        config:\n          substring: Anchor`,
  targetEnvironment = `agent_environment:\n  skills:\n    - ../../../../.agents/skills/example\n`,
) => `name: ${name}\ndefaults:\n  runs: \${RUNS=5}\n  executor: mock\n${targetEnvironment}stimuli:\n  - name: inspect-anchor\n    prompt: Inspect src/EFCore/Anchor.cs and explain it.\n    constraints:\n      max_turns: 10\n      max_tokens: 5000\n      max_duration: 1m\n    agent_environment:\n      files:\n        - src: src/EFCore/Anchor.cs\n          dest: src/EFCore/Anchor.cs\n    graders:\n${grader}\n      - type: token-budget\n        config:\n          max: 5000\nscoring:\n  weights:\n    token-budget: 0.1\n    output-contains: 0.9\n  threshold: 0.75\n`;

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

  await writeFile(join(root, 'eng/harness-evaluation/skills/example/eval.yaml'), evalYaml('example', `      - type: skill-invocation\n        config:\n          required: [example]`));
  await writeFile(
    join(root, 'eng/harness-evaluation/instructions/copilot-instructions/eval.yaml'),
    evalYaml('copilot-instructions', undefined, `agent_environment:\n  files:\n    - src: ../../../../.github/copilot-instructions.md\n      dest: .github/copilot-instructions.md\n`),
  );

  return root;
}

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
    await writeFile(evalPath, evalYaml('other', `      - type: skill-invocation\n        config:\n          required: [example]`));
    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /example: eval name 'other' must match component id 'example'/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory requires exact target activation graders', async () => {
  const root = await makeRepo();
  try {
    await writeFile(join(root, 'eng/harness-evaluation/skills/example/eval.yaml'), evalYaml('example'));
    const result = await validateInventory(root);
    assert.match(result.errors.join('\n'), /skill eval must require exact activation of 'example'/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test('inventory requires an isolated root treatment environment', async () => {
  const root = await makeRepo();
  try {
    const evalPath = join(root, 'eng/harness-evaluation/skills/example/eval.yaml');
    const grader = `      - type: skill-invocation\n        config:\n          required: [example]`;
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
