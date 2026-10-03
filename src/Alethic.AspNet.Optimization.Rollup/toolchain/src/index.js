// The toolchain's single entry point. Every bundle is one Rollup build: Rollup resolves and loads the inputs, runs
// each through the transform plugins, and renders the output. A module bundle's entry is its entry module; classic
// scripts and stylesheets each have a plugin that makes Rollup's entry from them.

import path from 'node:path';
import { rollup } from '@rollup/wasm-node';
import { classicScripts } from './plugins/classicScripts.js';
import { hostFiles } from './plugins/hostFiles.js';
import { minifyScript } from './plugins/minifyScript.js';
import { styles } from './plugins/styles.js';
import { typescript } from './plugins/typescript.js';

/**
 * Builds one bundle.
 *
 * @param {object} request
 * @param {'script' | 'module' | 'style'} request.kind how the inputs combine: classic scripts sharing one global
 *     scope, ES modules bundled into an IIFE, or stylesheets
 * @param {string[]} request.inputs absolute paths of the inputs: the classic scripts in order, or the one entry module
 *     or stylesheet
 * @param {string} request.fileName the output's file name, which names the output in its source map
 * @param {boolean} request.minify whether to minify the output
 * @param {boolean} request.sourceMap whether to produce a source map
 * @param {string} [request.separator] what classic scripts are joined with, ending with a line break
 * @param {{ exists(path: string): boolean, read(path: string): string | null }} request.files the host's files, through
 *     which every file is found and read, named by absolute path
 * @returns {Promise<{ code: string, map: string | null, watchFiles: string[], warnings: string[] }>}
 */
export async function build(request) {
    const { kind, inputs, fileName, minify = false, sourceMap = false, files, separator = ';\n' } = request;
    const warnings = [];

    if ((kind === 'module' || kind === 'style') && inputs.length !== 1)
        throw new Error(`A ${kind} bundle is built from one entry, not ${inputs.length}.`);

    const plugins = [hostFiles(files), typescript()];
    if (kind === 'script')
        plugins.push(classicScripts(inputs, separator));
    else if (kind === 'style')
        plugins.push(styles(inputs[0], files, { fileName, minify, sourceMap, warnings }));
    else if (kind !== 'module')
        throw new Error(`Unknown bundle kind '${kind}'.`);

    if (minify && kind !== 'style')
        plugins.push(minifyScript({ sourceMap, classic: kind === 'script' }));

    const bundle = await rollup({
        input: kind === 'script' ? classicScripts.entryId : kind === 'style' ? styles.entryId : inputs[0],
        plugins,
        treeshake: kind === 'module',
        // classic scripts run as scripts, where top-level `this` is the global object, not module `undefined`
        context: kind === 'script' ? 'this' : undefined,
        onLog: (level, log) => { if (level === 'warn') warnings.push(log.message); },
    });

    try {
        const { output } = await bundle.generate({
            format: kind === 'module' ? 'iife' : 'es',
            sourcemap: sourceMap,
            // absolute, for the host to turn into URLs; Rollup would otherwise make them relative to the process
            sourcemapPathTransform: (relative, mapPath) => path.resolve(path.dirname(mapPath), relative),
            entryFileNames: fileName,
        });

        if (kind === 'style') {
            const asset = output.find(o => o.type === 'asset' && o.fileName === fileName);
            const map = output.find(o => o.type === 'asset' && o.fileName === fileName + '.map');
            return { code: String(asset.source), map: map ? String(map.source) : null, watchFiles: bundle.watchFiles, warnings };
        }

        const chunk = output[0];
        return { code: chunk.code, map: chunk.map ? chunk.map.toString() : null, watchFiles: bundle.watchFiles, warnings };
    } finally {
        await bundle.close();
    }
}
