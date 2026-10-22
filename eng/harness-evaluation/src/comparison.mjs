import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { lstat, mkdir, mkdtemp, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, isAbsolute, join, relative, resolve, sep } from 'node:path';

export const MAX_COMPARISON_OUTPUT_CHARS = 12_000;
const MAX_COMPARISON_EVIDENCE_CHARS = 1_000_000;
const MAX_PATCH_BYTES = 8 * 1024 * 1024;
const MAX_RESULTS_BYTES = 128 * 1024 * 1024;
const variants = ['control', 'treatment'];

function digest(content) {
  return createHash('sha256').update(content).digest('hex');
}

function artifactPath(path) {
  if (typeof path !== 'string' || !/^[a-zA-Z0-9_./-]+$/.test(path)
    || path.split('/').some((segment) => !segment || ['.', '..', '.git'].includes(segment.toLowerCase()))) {
    throw new Error(`Unsafe requested output artifact path: ${path}`);
  }
  return path;
}

export function requestedOutputPaths(stimulus) {
  const prompt = stimulus.prompt;
  if (typeof prompt !== 'string') {
    throw new Error('Comparison requires a string stimulus prompt.');
  }
  const requests = prompt.matchAll(/\b(?:write|create|save|produce|update)\s+(?:[^.\n]{0,160}?\s(?:to|in|at)\s+)?[`'"]?([^\s`'"]+\.(?:md|txt|json|patch|diff))[`'"]?(?=[\s,.;:]|$)/gi);
  return [...new Set([...requests].map((match) => artifactPath(match[1])))].sort();
}

function gradedOutputPaths(stimulus) {
  return [...new Set((stimulus.graders ?? [])
    .filter((grader) => grader.type === 'file-matches')
    .map((grader) => artifactPath(grader.config?.path)))].sort();
}

async function regularFile(root, path, maxBytes) {
  const absolute = resolve(root, path);
  const child = relative(resolve(root), absolute);
  if (isAbsolute(child) || child === '..' || child.startsWith(`..${sep}`)) {
    throw new Error('Evidence file must be inside its artifact directory.');
  }
  let current = resolve(root);
  for (const segment of ['', ...child.split(sep)]) {
    current = join(current, segment);
    if ((await lstat(current)).isSymbolicLink()) {
      throw new Error(`Evidence path must not contain symbolic links: ${current}`);
    }
  }
  const info = await lstat(absolute);
  if (!info.isFile() || info.size > maxBytes) {
    throw new Error(`Evidence must be a regular file of at most ${maxBytes} bytes: ${absolute}`);
  }
  const bytes = await readFile(absolute);
  if (bytes.length > maxBytes) {
    throw new Error(`Evidence exceeded its byte budget: ${absolute}`);
  }
  return bytes;
}

async function trialDirectories(root) {
  const byId = new Map();
  async function visit(directory) {
    const entries = await readdir(directory, { withFileTypes: true });
    for (const entry of entries) {
      if (entry.isSymbolicLink()) {
        throw new Error(`Trial artifacts must not contain symbolic links: ${join(directory, entry.name)}`);
      }
      if (entry.isDirectory()) {
        await visit(join(directory, entry.name));
      } else if (entry.name === 'metadata.json') {
        const metadata = JSON.parse(await regularFile(root, relative(root, join(directory, entry.name)), 64_000));
        if (typeof metadata.trialId !== 'string' || byId.has(metadata.trialId)) {
          throw new Error('Missing or duplicate trial artifact identity.');
        }
        byId.set(metadata.trialId, { directory, metadata });
      }
    }
  }
  await visit(root);
  return byId;
}

async function artifactsFromPatch(patch, paths, requestedPaths) {
  const directory = await mkdtemp(join(tmpdir(), 'efcore-comparison-'));
  try {
    const env = Object.fromEntries(Object.entries(process.env).filter(([key]) => !/^GIT_/i.test(key)));
    Object.assign(env, {
      GIT_CONFIG_NOSYSTEM: '1',
      GIT_CONFIG_GLOBAL: process.platform === 'win32' ? 'NUL' : '/dev/null',
      GIT_CEILING_DIRECTORIES: dirname(directory),
      HOME: directory,
      XDG_CONFIG_HOME: directory,
    });
    function git(args, input) {
      return spawnSync('git', [
        '-c', 'core.autocrlf=false', '-c', 'core.symlinks=true', 'apply', ...args,
      ], { cwd: directory, env, input, encoding: 'utf8', timeout: 30_000, maxBuffer: MAX_PATCH_BYTES });
    }
    function stats(input, includes) {
      const result = git(['--numstat', '-z', ...includes.map((path) => `--include=${path}`)], input);
      if (result.status !== 0) {
        throw new Error(`Cannot reconstruct artifact evidence from the final workspace patch: ${result.error?.message ?? result.stderr}`);
      }
      return result.stdout.split('\0').filter(Boolean).map((entry) => {
        const fields = entry.split('\t');
        if (fields.length !== 3 || !fields.slice(0, 2).every((field) => /^\d+$/.test(field))) {
          throw new Error('Artifact evidence requires a native text patch with an exact path.');
        }
        return { added: Number(fields[0]), removed: Number(fields[1]), path: artifactPath(fields[2]) };
      });
    }
    const selected = patch.length ? stats(patch, paths) : [];
    if (paths.some((path) => selected.filter((entry) => entry.path === path).length > 1)) {
      throw new Error('Duplicate graded/requested artifacts in the final workspace patch.');
    }
    const text = new TextDecoder('utf-8', { fatal: true }).decode(patch);
    const boundaries = [...text.matchAll(/^diff --git /gm)].map((match) => match.index);
    if (patch.length && (!boundaries.length || boundaries[0] !== 0 || boundaries.length > 256)) {
      throw new Error('Artifact evidence requires a native Git patch with at most 256 file sections.');
    }
    const sections = new Map();
    for (const [index, start] of boundaries.entries()) {
      const section = text.slice(start, boundaries[index + 1] ?? text.length);
      if (!stats(section, paths).length) {
        continue;
      }
      const entries = stats(section, []);
      if (entries.length !== 1 || sections.has(entries[0].path)
        || JSON.stringify(entries[0]) !== JSON.stringify(selected.find((entry) => entry.path === entries[0].path))) {
        throw new Error('Ambiguous native patch section for artifact evidence.');
      }
      const header = section.slice(0, section.indexOf('\n@@'));
      if (/^(?:index [^\n]+ |(?:new|deleted) file mode |(?:old|new) mode )(?:120000|160000)\r?$/m.test(header)) {
        throw new Error('Artifact evidence must not contain symbolic links or submodules.');
      }
      sections.set(entries[0].path, section);
    }
    const artifacts = [];
    for (const path of paths) {
      const section = sections.get(path);
      if (!section) {
        artifacts.push({
          path,
          kind: 'unavailable',
          content: 'No change for this requested or graded path was recorded in the final workspace patch. The artifact may be absent or unchanged; its final contents cannot be reconstructed from the saved patch. Do not infer successful artifact creation from this notice.',
        });
        continue;
      }
      const summary = git(['--summary', `--include=${path}`], section);
      if (summary.status !== 0) {
        throw new Error(`Cannot reconstruct artifact summary: ${summary.error?.message ?? summary.stderr}`);
      }
      const newFile = ['100644', '100755'].some((mode) => summary.stdout === ` create mode ${mode} ${path}\n`);
      if (!newFile) {
        if (!requestedPaths.includes(path) && !summary.stdout) {
          artifacts.push({ path, kind: 'diff', content: section });
          continue;
        }
        throw new Error(`Cannot reconstruct requested artifacts from the final workspace patch: ${path} is not a new regular file.`);
      }
      const result = git(['--whitespace=nowarn', `--include=${path}`], section);
      if (result.status !== 0) {
        throw new Error(`Cannot reconstruct requested artifacts from the final workspace patch: ${result.error?.message ?? result.stderr}`);
      }
      const bytes = await regularFile(directory, path, MAX_PATCH_BYTES);
      artifacts.push({ path, kind: 'final-content', content: new TextDecoder('utf-8', { fatal: true }).decode(bytes) });
    }
    return artifacts;
  } finally {
    await rm(directory, { recursive: true, force: true });
  }
}

export function comparisonOutput(output, artifacts) {
  if (typeof output !== 'string') {
    throw new Error('Comparison requires a string final output.');
  }
  const evidence = artifacts.map(({ path, content, kind = 'final-content' }) =>
    `${kind === 'unavailable' ? 'Artifact evidence unavailable' : kind === 'diff' ? 'Graded artifact diff (baseline unavailable; NOT complete final content)' : 'Output artifact (complete final content)'}: ${path}\n${content}`).join('\n\n');
  const prepared = evidence ? `${evidence}\n\nOriginal final response:\n${output}` : output;
  if (prepared.length > MAX_COMPARISON_EVIDENCE_CHARS) {
    throw new Error(`Comparison evidence exceeds the ${MAX_COMPARISON_EVIDENCE_CHARS}-character safety budget.`);
  }
  return prepared;
}

export async function prepareComparison(experimentDirectory, outputDirectory) {
  const source = resolve(experimentDirectory);
  const output = resolve(outputDirectory);
  const sourceFromOutput = relative(output, source);
  if (!sourceFromOutput || (!isAbsolute(sourceFromOutput)
    && sourceFromOutput !== '..' && !sourceFromOutput.startsWith(`..${sep}`))) {
    throw new Error('Comparison preparation must not overwrite original experiment evidence.');
  }
  await mkdir(output);
  const manifest = {
    type: 'comparison-evidence-preparation',
    status: 'preparing',
    singleReadTargetChars: MAX_COMPARISON_OUTPUT_CHARS,
    maxOutputCharsPerRun: MAX_COMPARISON_EVIDENCE_CHARS,
    sources: [],
    trials: [],
  };
  try {
    const preparedVariants = [];
    for (const variant of variants) {
      const root = join(source, variant);
      const bytes = await regularFile(source, `${variant}/results.jsonl`, MAX_RESULTS_BYTES);
      manifest.sources.push({ path: join(root, 'results.jsonl'), sha256: digest(bytes) });
      const records = bytes.toString('utf8').split(/\r?\n/).filter(Boolean).map(JSON.parse);
      const directories = await trialDirectories(root);
      const seen = new Set();
      for (const record of records.filter((record) => record.type === 'trial-result')) {
        if (!record.trajectory || typeof record.itemId !== 'string' || seen.has(record.itemId)) {
          throw new Error('Missing trajectory or duplicate trial result identity.');
        }
        seen.add(record.itemId);
        const requestedPaths = requestedOutputPaths(record.trajectory.stimulus);
        const gradedPaths = gradedOutputPaths(record.trajectory.stimulus);
        const paths = [...new Set([...requestedPaths, ...gradedPaths])].sort();
        let artifacts = [];
        const audit = {
          variant,
          itemId: record.itemId,
          status: paths.length ? 'pending' : 'no-requested-artifacts',
          requestedPaths,
          gradedPaths,
          originalOutputSha256: digest(record.trajectory.output),
        };
        manifest.trials.push(audit);
        if (paths.length) {
          const trial = directories.get(record.itemId);
          if (!trial || trial.metadata.trajectoryId !== record.trajectory.id || trial.metadata.status !== 'success') {
            throw new Error(`Missing or mismatched artifacts for trial ${record.itemId}.`);
          }
          const patchPath = relative(root, join(trial.directory, 'workspace.patch'));
          const patch = await regularFile(root, patchPath, MAX_PATCH_BYTES);
          audit.patch = { path: join(root, patchPath), sha256: digest(patch) };
          artifacts = await artifactsFromPatch(patch, paths, requestedPaths);
          audit.artifacts = artifacts.map(({ path, kind, content }) => ({ path, kind, sha256: digest(content), chars: content.length }));
        }
        record.trajectory.output = comparisonOutput(record.trajectory.output, artifacts);
        audit.preparedOutputSha256 = digest(record.trajectory.output);
        audit.preparedOutputChars = record.trajectory.output.length;
        audit.requiresRangedReads = record.trajectory.output.length > MAX_COMPARISON_OUTPUT_CHARS;
        if (paths.length) {
          audit.status = artifacts.some((artifact) => artifact.kind === 'unavailable') ? 'partially-available' : 'included';
        }
      }
      if (seen.size === 0) {
        throw new Error(`No trial results for ${variant}.`);
      }
      preparedVariants.push({ variant, content: `${records.map((record) => JSON.stringify(record)).join('\n')}\n` });
    }
    for (const { variant, content } of preparedVariants) {
      const directory = join(output, variant);
      await mkdir(directory);
      await writeFile(join(directory, 'results.jsonl'), content, { flag: 'wx' });
    }
    manifest.status = 'prepared';
  } catch (error) {
    manifest.status = 'failed';
    manifest.error = error.message;
    throw error;
  } finally {
    await writeFile(join(output, 'preparation.json'), `${JSON.stringify(manifest, null, 2)}\n`, { flag: 'wx' });
  }
  return output;
}