// The toolchain's single entry point. Every bundle is one Rollup build: Rollup resolves and loads the inputs, runs
// each through the transform plugins, and renders the output; scripts, modules and stylesheets differ only in which
// plugin turns the ordered inputs into Rollup's entry.

import path from 'node:path';
import { rollup } from '@rollup/wasm-node';
import { classicScripts } from './plugins/classicScripts.js';
import { moduleEntry } from './plugins/moduleEntry.js';
import { minifyScript } from './plugins/minifyScript.js';
import { styles } from './plugins/styles.js';
import { typescript } from './plugins/typescript.js';

/**
 * Builds one bundle.
 *
 * @param {object} request
 * @param {'script' | 'module' | 'style'} request.kind how the inputs combine: classic scripts sharing one global
 *     scope, ES modules bundled into an IIFE, or stylesheets
 * @param {string[]} request.inputs absolute paths of the inputs, in bundle order
 * @param {string} request.fileName the output's file name, which names the output in its source map
 * @param {boolean} request.minify whether to minify the output
 * @param {boolean} request.sourceMap whether to produce a source map
 * @returns {Promise<{ code: string, map: string | null, watchFiles: string[], warnings: string[] }>}
 */
export async function build(request) {
    const { kind, inputs, fileName, minify = false, sourceMap = false } = request;
    const warnings = [];

    const plugins = [typescript()];
    if (kind === 'script')
        plugins.push(classicScripts(inputs));
    else if (kind === 'module')
        plugins.push(moduleEntry(inputs));
    else if (kind === 'style')
        plugins.push(styles(inputs, { fileName, minify, sourceMap, warnings }));
    else
        throw new Error(`Unknown bundle kind '${kind}'.`);

    if (minify && kind !== 'style')
        plugins.push(minifyScript({ sourceMap, classic: kind === 'script' }));

    const bundle = await rollup({
        input: kind === 'script' ? classicScripts.entryId : kind === 'module' ? moduleEntry.entryId : styles.entryId,
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
