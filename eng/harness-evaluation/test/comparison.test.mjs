import assert from 'node:assert/strict';
import { mkdir, mkdtemp, readFile, rm, symlink, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import test from 'node:test';
import { PromptGrader } from '@microsoft/vally';
import { comparisonOutput, MAX_COMPARISON_OUTPUT_CHARS, prepareComparison, requestedOutputPaths } from '../src/comparison.mjs';
import { loadJsonlFile } from '../node_modules/@microsoft/vally-cli/dist/commands/compare.js';
import { materializeComparisonWorkspace } from '../node_modules/@microsoft/vally/dist/graders/llm/judge-workspace.js';

function newFilePatch(path, content, mode = '100644') {
  const lines = content.split('\n');
  return `diff --git a/${path} b/${path}\nnew file mode ${mode}\n--- /dev/null\n+++ b/${path}\n@@ -0,0 +1,${lines.length} @@\n${lines.map((line) => `+${line}\n`).join('')}\\ No newline at end of file\n`;
}

async function experiment(prompt = 'Write report.md with your findings.', reports = ['# Final report\nFinal diagnosis.', '# Final report\nFinal diagnosis.']) {
  const root = await mkdtemp(join(tmpdir(), 'efcore-comparison-test-'));
  const source = join(root, 'experiment');
  for (const [index, variant] of ['control', 'treatment'].entries()) {
    const directory = join(source, variant, 'trial');
    await mkdir(directory, { recursive: true });
    const record = {
      type: 'trial-result', itemId: `${variant}-trial`, trialIndex: 0, status: 'success',
      gradeResult: { score: 0.8, passed: true },
      experiment: { baseline: 'control', variant, name: 'test', runId: 'run' },
      trajectory: {
        id: `${variant}-trajectory`, output: '[report](report.md)',
        workDir: join(root, 'candidate-must-not-be-read'),
        stimulus: { name: 'review', prompt, rubric: ['Find the defect.'] },
        events: [{ type: 'tool.execution_complete', data: { result: 'outdated draft' } }],
        metrics: { tokenUsage: { totalTokens: 123 }, turnCount: 1, toolCallCount: 1, wallTimeMs: 10, errorCount: 0 },
      },
    };
    await writeFile(join(source, variant, 'results.jsonl'), `${JSON.stringify(record)}\n`);
    await writeFile(join(directory, 'metadata.json'), JSON.stringify({
      trialId: record.itemId, trajectoryId: record.trajectory.id, status: 'success',
    }));
    if (reports[index] !== null) {
      await writeFile(join(directory, 'workspace.patch'), newFilePatch('report.md', reports[index]));
    }
  }
  return { root, source, output: join(root, 'comparison-input') };
}

async function copiedTrial(output, variant) {
  return JSON.parse((await readFile(join(output, variant, 'results.jsonl'), 'utf8')).trim());
}

test('Vally receives complete report output identically when the run order is swapped', async () => {
  const output = comparisonOutput('[report](report.md)', [{ path: 'report.md', content: '# Final report\nFinal diagnosis.' }]);
  const trajectory = { output, events: [], metrics: {} };
  for (const pair of [[trajectory, { ...trajectory }], [{ ...trajectory }, trajectory]]) {
    const workspace = await materializeComparisonWorkspace(...pair);
    try {
      for (const run of ['run-1', 'run-2']) {
        assert.equal(await readFile(join(workspace.workingDirectory, 'evidence', run, 'agent-output.txt'), 'utf8'), output);
      }
      assert.match(output, /Final diagnosis\./);
    } finally {
      await workspace.cleanup();
    }
  }
});

test('comparison output preserves reports beyond a single read and bounds excessive evidence', () => {
  assert.equal(comparisonOutput('final', []), 'final');
  const content = 'x'.repeat(MAX_COMPARISON_OUTPUT_CHARS * 2);
  assert.ok(comparisonOutput('', [{ path: 'report.md', content }]).includes(content));
  assert.throws(() => comparisonOutput('', [{ path: 'report.md', content: 'x'.repeat(1_000_000) }]), /safety budget/);
});

test('graded updated C# evidence includes the actual native diff, not a false complete-file claim, in either order', async () => {
  const context = await experiment('Add a functional test. Write report.md with validation.');
  const path = 'test/EFCore.Cosmos.FunctionalTests/CosmosTransactionalBatchTest.cs';
  const patch = `diff --git a/${path} b/${path}\nindex 1234567..abcdef0 100644\n--- a/${path}\n+++ b/${path}\n@@ -1,3 +1,4 @@\n public class CosmosTransactionalBatchTest\n {\n+    public Task Oversized_batch_is_atomic() => Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);\n }\n`;
  try {
    const originals = [];
    for (const variant of ['control', 'treatment']) {
      const record = await copiedTrial(context.source, variant);
      record.trajectory.stimulus.graders = [
        { type: 'file-matches', config: { path, pattern: 'Oversized_batch_is_atomic' } },
        { type: 'file-matches', config: { path: 'report.md', pattern: 'diagnosis' } },
        { type: 'file-matches', config: { path, pattern: 'ThrowsAsync' } },
      ];
      const original = `${JSON.stringify(record)}\n`;
      originals.push(original);
      await writeFile(join(context.source, variant, 'results.jsonl'), original);
      await writeFile(join(context.source, variant, 'trial/workspace.patch'), patch + newFilePatch('report.md', 'Final validation report.'));
    }
    await prepareComparison(context.source, context.output);
    const trajectories = [];
    for (const [index, variant] of ['control', 'treatment'].entries()) {
      const copy = await copiedTrial(context.output, variant);
      trajectories.push(copy.trajectory);
      assert.ok(copy.trajectory.output.includes(patch));
      assert.match(copy.trajectory.output, /Graded artifact diff \(baseline unavailable; NOT complete final content\)/);
      assert.doesNotMatch(copy.trajectory.output, /^Output artifact \(complete final content\): test\//m);
      assert.equal(copy.trajectory.output.match(/Output artifact .*: report\.md/g).length, 1);
      const original = JSON.parse(originals[index]);
      assert.deepEqual({ ...copy, trajectory: { ...copy.trajectory, output: original.trajectory.output } }, original);
      assert.equal(await readFile(join(context.source, variant, 'results.jsonl'), 'utf8'), originals[index]);
    }
    for (const pair of [trajectories, [...trajectories].reverse()]) {
      const workspace = await materializeComparisonWorkspace(...pair);
      try {
        for (const run of ['run-1', 'run-2']) {
          assert.equal(await readFile(join(workspace.workingDirectory, 'evidence', run, 'agent-output.txt'), 'utf8'), trajectories[0].output);
        }
      } finally {
        await workspace.cleanup();
      }
    }
    const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
    assert.ok(audit.trials.every((trial) => trial.artifacts.length === 2
      && trial.artifacts.find((artifact) => artifact.path === path).kind === 'diff'));
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('new graded C# files provide complete final content without inferring ungraded source outputs', async () => {
  const context = await experiment('Add the requested regression test.');
  const path = 'test/NewRegressionTest.cs';
  const content = 'public class NewRegressionTest { public Task Atomic_batch() => SaveChangesAsync(); }';
  try {
    for (const variant of ['control', 'treatment']) {
      const record = await copiedTrial(context.source, variant);
      record.trajectory.stimulus.graders = [{ type: 'file-matches', config: { path, pattern: 'Atomic_batch' } }];
      await writeFile(join(context.source, variant, 'results.jsonl'), `${JSON.stringify(record)}\n`);
      await writeFile(join(context.source, variant, 'trial/workspace.patch'),
        newFilePatch('ungraded.cs', 'Unrelated source sentinel') + newFilePatch(path, content));
    }
    await prepareComparison(context.source, context.output);
    const output = (await copiedTrial(context.output, 'control')).trajectory.output;
    assert.ok(output.includes(`Output artifact (complete final content): ${path}\n${content}`));
    assert.doesNotMatch(output, /Unrelated source sentinel|Graded artifact diff/);
    const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
    assert.ok(audit.trials.every((trial) => trial.artifacts[0].kind === 'final-content'));
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('zero-context insertions into existing graded files are never labelled complete final content', async () => {
  const context = await experiment('Add a regression test.');
  const path = 'test/RegressionTest.cs';
  const patch = `diff --git a/${path} b/${path}\nindex 1234567..abcdef0 100644\n--- a/${path}\n+++ b/${path}\n@@ -0,0 +1 @@\n+public Task Added_test() => SaveChangesAsync();\n`;
  try {
    for (const variant of ['control', 'treatment']) {
      const record = await copiedTrial(context.source, variant);
      record.trajectory.stimulus.graders = [{ type: 'file-matches', config: { path, pattern: 'Added_test' } }];
      await writeFile(join(context.source, variant, 'results.jsonl'), `${JSON.stringify(record)}\n`);
      await writeFile(join(context.source, variant, 'trial/workspace.patch'), patch);
    }
    await prepareComparison(context.source, context.output);
    const output = (await copiedTrial(context.output, 'control')).trajectory.output;
    assert.ok(output.includes(patch));
    assert.match(output, /NOT complete final content/);
    assert.doesNotMatch(output, /^Output artifact /m);
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('multiple reports beyond one read are retained and audited in either arm', async () => {
  const paths = Array.from({ length: 4 }, (_, index) => `reports/report-${index}.md`);
  for (const oversized of ['control', 'treatment']) {
    const context = await experiment(paths.map((path) => `Write ${path} with findings.`).join('\n'));
    try {
      for (const variant of ['control', 'treatment']) {
        const content = variant === oversized ? 'x'.repeat(4000) : 'short report';
        await writeFile(join(context.source, variant, 'trial/workspace.patch'), paths.map((path) => newFilePatch(path, content)).join(''));
      }
      await prepareComparison(context.source, context.output);
      for (const variant of ['control', 'treatment']) {
        const output = (await copiedTrial(context.output, variant)).trajectory.output;
        assert.ok(output.includes(variant === oversized ? 'x'.repeat(4000) : 'short report'));
      }
      const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
      assert.equal(audit.status, 'prepared');
      assert.ok(audit.trials.every(trial => trial.artifacts.length === paths.length));
      assert.equal(audit.trials.find(trial => trial.variant === oversized).requiresRangedReads, true);
    } finally {
      await rm(context.root, { recursive: true, force: true });
    }
  }
});

test('graded artifacts require exact safe paths and cannot fall back to candidate files', async () => {
  for (const path of ['test/*.cs', '../outside.cs', '.git/config']) {
    const context = await experiment('Add a regression test.');
    try {
      const record = await copiedTrial(context.source, 'control');
      record.trajectory.stimulus.graders = [{ type: 'file-matches', config: { path, pattern: 'Test' } }];
      await writeFile(join(context.source, 'control/results.jsonl'), `${JSON.stringify(record)}\n`);
      await mkdir(join(context.root, 'candidate-must-not-be-read/test'), { recursive: true });
      await writeFile(join(context.root, 'candidate-must-not-be-read/test/Missing.cs'), 'Unpreserved candidate test');
      await assert.rejects(prepareComparison(context.source, context.output), /Unsafe/);
      await assert.rejects(readFile(join(context.output, 'control/results.jsonl')), /ENOENT/);
    } finally {
      await rm(context.root, { recursive: true, force: true });
    }
  }
});

test('graded diff fallback rejects symlink edits, binary patches, and sections containing another file', async () => {
  const path = 'test/RegressionTest.cs';
  const update = `diff --git a/${path} b/${path}\nindex 1234567..abcdef0 100644\n--- a/${path}\n+++ b/${path}\n@@ -1 +1 @@\n-old\n+new\n`;
  for (const patch of [
    update.replace('100644', '120000'),
    `diff --git a/${path} b/${path}\nindex 1234567..abcdef0 100644\nBinary files a/${path} and b/${path} differ\n`,
    update + '--- a/ungraded.cs\n+++ b/ungraded.cs\n@@ -1 +1 @@\n-private source\n+unrelated source\n',
  ]) {
    const context = await experiment('Add a regression test.');
    try {
      const record = await copiedTrial(context.source, 'control');
      record.trajectory.stimulus.graders = [{ type: 'file-matches', config: { path, pattern: 'new' } }];
      await writeFile(join(context.source, 'control/results.jsonl'), `${JSON.stringify(record)}\n`);
      await writeFile(join(context.source, 'control/trial/workspace.patch'), patch);
      await assert.rejects(prepareComparison(context.source, context.output), /symbolic links|native text patch|Ambiguous|Missing or duplicate|reconstruct/);
      await assert.rejects(readFile(join(context.output, 'control/results.jsonl')), /ENOENT/);
    } finally {
      await rm(context.root, { recursive: true, force: true });
    }
  }
});

test('preparation reconstructs FINAL reports from native patches, preserving original evidence and grades', async () => {
  const context = await experiment();
  try {
    const originals = await Promise.all(['control', 'treatment'].map((variant) => readFile(join(context.source, variant, 'results.jsonl'))));
    const patches = await Promise.all(['control', 'treatment'].map((variant) => readFile(join(context.source, variant, 'trial', 'workspace.patch'))));
    await prepareComparison(context.source, context.output);
    const copies = [];
    for (const [index, variant] of ['control', 'treatment'].entries()) {
      const copy = await copiedTrial(context.output, variant);
      copies.push(copy.trajectory);
      assert.match(copy.trajectory.output, /Final diagnosis\./);
      assert.doesNotMatch(copy.trajectory.output, /outdated draft/);
      const original = JSON.parse(originals[index]);
      assert.deepEqual({ ...copy, trajectory: { ...copy.trajectory, output: original.trajectory.output } }, original);
      assert.deepEqual(await readFile(join(context.source, variant, 'results.jsonl')), originals[index]);
      assert.deepEqual(await readFile(join(context.source, variant, 'trial', 'workspace.patch')), patches[index]);
      const loaded = await loadJsonlFile(join(context.output, variant, 'results.jsonl'));
      assert.equal(loaded.trajectories.length, 1);
      assert.equal(loaded.trajectories[0].passed, true);
      assert.deepEqual(loaded.trajectories[0].trajectory, copy.trajectory);
    }
    for (const pair of [copies, [...copies].reverse()]) {
      const workspace = await materializeComparisonWorkspace(...pair);
      try {
        const first = await readFile(join(workspace.workingDirectory, 'evidence/run-1/agent-output.txt'), 'utf8');
        const second = await readFile(join(workspace.workingDirectory, 'evidence/run-2/agent-output.txt'), 'utf8');
        assert.equal(first, second);
      } finally {
        await workspace.cleanup();
      }
    }
    const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
    assert.equal(audit.status, 'prepared');
    assert.equal(audit.trials.length, 2);
    assert.ok(audit.trials.every((trial) => trial.status === 'included' && trial.patch.sha256.length === 64));
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('explicit output requests are deterministic and unsafe candidate paths are rejected', () => {
  assert.deepEqual(requestedOutputPaths({ prompt: 'Read input.md. Write analysis.md with findings. Save the report to `reports/final.txt`.' }), ['analysis.md', 'reports/final.txt']);
  for (const path of ['../report.md', '/report.md', 'C:/report.md', 'C:\\report.md', '.git/report.md', 'reports/*/report.md']) {
    assert.throws(() => requestedOutputPaths({ prompt: `Write ${path} with findings.` }), /Unsafe/);
  }
  assert.deepEqual(requestedOutputPaths({ prompt: 'Read input.md and explain the problem.' }), []);
});

test('missing reports fail closed without falling back to candidate source or producing comparison inputs', async () => {
  const context = await experiment(undefined, ['control report', null]);
  try {
    await mkdir(join(context.root, 'candidate-must-not-be-read'));
    await writeFile(join(context.root, 'candidate-must-not-be-read/report.md'), 'Must not be treated as evidence');
    await assert.rejects(prepareComparison(context.source, context.output), /ENOENT/);
    await assert.rejects(readFile(join(context.output, 'control/results.jsonl')), /ENOENT/);
    const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
    assert.equal(audit.status, 'failed');
    assert.equal(audit.trials.at(-1).status, 'pending');
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('both arms retain complete long reports and audit the need for ranged reads', async () => {
  for (const reports of [['short', 'x'.repeat(MAX_COMPARISON_OUTPUT_CHARS)], ['x'.repeat(MAX_COMPARISON_OUTPUT_CHARS), 'short']]) {
    const context = await experiment(undefined, reports);
    try {
      await prepareComparison(context.source, context.output);
      for (const [index, variant] of ['control', 'treatment'].entries()) {
        assert.ok((await copiedTrial(context.output, variant)).trajectory.output.includes(reports[index]));
      }
      const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
      assert.equal(audit.trials.filter(trial => trial.requiresRangedReads).length, 1);
    } finally {
      await rm(context.root, { recursive: true, force: true });
    }
  }
});

test('native patch selection excludes unrelated unsafe paths and rejects selected symlink artifacts', async () => {
  const context = await experiment();
  try {
    const patch = newFilePatch('../../outside.txt', 'escape') + newFilePatch('report.md', 'Final safe report');
    await writeFile(join(context.source, 'control/trial/workspace.patch'), patch);
    await prepareComparison(context.source, context.output);
    assert.match((await copiedTrial(context.output, 'control')).trajectory.output, /Final safe report/);
    await assert.rejects(readFile(join(context.root, 'outside.txt')), /ENOENT/);
    await writeFile(join(context.source, 'control/trial/workspace.patch'), newFilePatch('report.md', join(context.root, 'secret.txt'), '120000'));
    await writeFile(join(context.root, 'secret.txt'), 'outside secret');
    await assert.rejects(prepareComparison(context.source, join(context.root, 'symlink-input')), /symbolic links|regular file|reconstruct/);
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('symlinked trial artifacts and duplicate identities are not accepted as report evidence', async () => {
  const context = await experiment();
  try {
    await symlink(join(context.source, 'control/trial'), join(context.source, 'control/alias'), process.platform === 'win32' ? 'junction' : 'dir');
    await assert.rejects(prepareComparison(context.source, context.output), /symbolic links/);
    await rm(join(context.source, 'control/alias'));
    await mkdir(join(context.source, 'control/duplicate'));
    await writeFile(join(context.source, 'control/duplicate/metadata.json'), await readFile(join(context.source, 'control/trial/metadata.json')));
    await assert.rejects(prepareComparison(context.source, join(context.root, 'duplicate-input')), /duplicate trial/);
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('inline-only responses are budgeted and explicitly audited as having no requested artifacts', async () => {
  const context = await experiment('Explain the finding in your final response.');
  try {
    await prepareComparison(context.source, context.output);
    assert.equal((await copiedTrial(context.output, 'control')).trajectory.output, '[report](report.md)');
    const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
    assert.ok(audit.trials.every((trial) => trial.status === 'no-requested-artifacts' && !trial.artifacts));
    await assert.rejects(prepareComparison(context.source, context.source), /original experiment evidence/);
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('Vally keeps contradictory position-swapped verdicts conservative without rejudging', async () => {
  const context = await experiment(undefined, ['Final control report', 'Final treatment report']);
  try {
    await prepareComparison(context.source, context.output);
    const baseline = (await loadJsonlFile(join(context.output, 'control/results.jsonl'))).trajectories[0].trajectory;
    const treatment = (await loadJsonlFile(join(context.output, 'treatment/results.jsonl'))).trajectories[0].trajectory;
    const directions = [];
    const grader = new PromptGrader({
      supportsWorkspaceDelivery: true,
      async judge({ systemMessage, userMessage, workspace }) {
        assert.match(systemMessage, /UNTRUSTED agent-produced data/);
        assert.doesNotMatch(userMessage, /grader-only prompt sentinel/);
        directions.push(systemMessage.includes("Response A's evidence is under `evidence/run-1/`") ? 'forward' : 'reverse');
        assert.match(await readFile(join(workspace.workingDirectory, 'evidence/run-1/agent-output.txt'), 'utf8'), /Final control report/);
        assert.match(await readFile(join(workspace.workingDirectory, 'evidence/run-2/agent-output.txt'), 'utf8'), /Final treatment report/);
        return {
          latencyMs: 0,
          args: {
            rubric_results: [{ criterion: 'Find the defect.', winner: 'B', magnitude: 'much-better', reasoning: 'Test verdict.' }],
            overall_winner: 'B', overall_magnitude: 'much-better', overall_reasoning: 'Test verdict.',
          },
        };
      },
    });
    const result = await grader.compare({
      stimulus: { ...baseline.stimulus, graders: [{ type: 'prompt', config: { prompt: 'grader-only prompt sentinel' } }] },
      comparison: {
        topology: 'baseline-relative',
        baseline: { label: 'control', trajectory: baseline },
        treatment: { label: 'treatment', trajectory: treatment },
      },
    });
    assert.deepEqual(directions.sort(), ['forward', 'reverse']);
    assert.equal(result.score, 0);
    assert.match(JSON.stringify(result), /Position-swap inconsistent/);
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});

test('malformed patches and edits requiring an unavailable baseline fail closed', async () => {
  for (const patch of ['not a patch', 'diff --git a/report.md b/report.md\n--- a/report.md\n+++ b/report.md\n@@ -1 +1 @@\n-old\n+new\n']) {
    const context = await experiment();
    try {
      await writeFile(join(context.source, 'control/trial/workspace.patch'), patch);
      await assert.rejects(prepareComparison(context.source, context.output), /reconstruct/);
      const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
      assert.equal(audit.status, 'failed');
    } finally {
      await rm(context.root, { recursive: true, force: true });
    }
  }
});

test('unchanged or absent graded artifacts do not block fully graded low controls', async () => {
  const context = await experiment('Write report.md with findings.');
  try {
    const record = await copiedTrial(context.source, 'control');
    record.gradeResult = { score: 0, passed: false };
    record.trajectory.stimulus.graders = [{ type: 'file-matches', config: { path: 'test/Missing.cs', pattern: 'Test' } }];
    await writeFile(join(context.source, 'control/results.jsonl'), `${JSON.stringify(record)}\n`);
    await writeFile(join(context.source, 'control/trial/workspace.patch'), '');
    await mkdir(join(context.root, 'candidate-must-not-be-read/test'), { recursive: true });
    await writeFile(join(context.root, 'candidate-must-not-be-read/test/Missing.cs'), 'Unpreserved candidate test');
    await prepareComparison(context.source, context.output);
    const copy = await copiedTrial(context.output, 'control');
    assert.deepEqual(copy.gradeResult, record.gradeResult);
    assert.match(copy.trajectory.output, /Artifact evidence unavailable: report\.md/);
    assert.match(copy.trajectory.output, /Artifact evidence unavailable: test\/Missing\.cs/);
    assert.doesNotMatch(copy.trajectory.output, /Unpreserved candidate test/);
    const audit = JSON.parse(await readFile(join(context.output, 'preparation.json'), 'utf8'));
    assert.equal(audit.status, 'prepared');
    assert.equal(audit.trials[0].status, 'partially-available');
  } finally {
    await rm(context.root, { recursive: true, force: true });
  }
});