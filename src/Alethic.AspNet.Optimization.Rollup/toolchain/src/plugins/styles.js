import fs from 'node:fs';
import { fileURLToPath } from 'node:url';
import { transform } from 'lightningcss-wasm';
import * as sass from 'sass';

const entryId = '\0alethic:styles';
const sassExtensions = /\.s[ac]ss$/;

/**
 * Makes Rollup's entry a stylesheet: the entry stylesheet compiled by Sass, which follows its `@use` and `@import`
 * rules, then transformed by Lightning CSS and emitted as the bundle's one asset. The entry's own JavaScript is empty,
 * and discarded.
 *
 * Every file Sass reads is added to Rollup's watch files, so the build reports the partials a stylesheet depends on
 * as well as the stylesheet itself.
 *
 * @param {string} input absolute path of the entry stylesheet
 * @param {object} options
 * @param {string} options.fileName the asset's file name
 * @param {boolean} options.minify whether to minify the stylesheet
 * @param {boolean} options.sourceMap whether to emit the asset's source map beside it, as `fileName.map`
 * @param {string[]} options.warnings where to add warnings
 */
export function styles(input, { fileName, minify, sourceMap, warnings }) {
    return {
        name: 'alethic:styles',

        resolveId(source) {
            return source === entryId ? entryId : null;
        },

        load(id) {
            if (id !== entryId)
                return null;

            const compiled = compile(this, input, warnings);

            const result = transform({
                filename: fileName,
                code: new TextEncoder().encode(compiled.code),
                minify,
                sourceMap,
                inputSourceMap: compiled.map ? JSON.stringify(compiled.map) : undefined,
                // stylesheets written for old browsers carry hacks Lightning CSS cannot parse; keep them as written
                errorRecovery: true,
            });

            for (const warning of result.warnings ?? [])
                warnings.push(`${warning.loc?.filename ?? fileName}:${warning.loc?.line ?? 0}: ${warning.message}`);

            this.emitFile({ type: 'asset', fileName, source: new TextDecoder().decode(result.code) });
            if (sourceMap && result.map)
                this.emitFile({ type: 'asset', fileName: fileName + '.map', source: new TextDecoder().decode(result.map) });

            return { code: 'export {};', map: { mappings: '' } };
        },
    };
}

styles.entryId = entryId;

function compile(context, input, warnings) {
    if (!sassExtensions.test(input)) {
        context.addWatchFile(input);
        return { code: fs.readFileSync(input, 'utf8'), map: null };
    }

    const result = sass.compile(input, {
        style: 'expanded',
        sourceMap: true,
        sourceMapIncludeSources: true,
        logger: {
            warn: (message, { span }) => warnings.push(span ? `${span.url ? fileURLToPath(span.url) : input}:${span.start.line + 1}: ${message}` : message),
            debug: () => { },
        },
    });

    for (const url of result.loadedUrls)
        if (url.protocol === 'file:')
            context.addWatchFile(fileURLToPath(url));

    const map = { ...result.sourceMap, sources: result.sourceMap.sources.map(s => s.startsWith('file:') ? fileURLToPath(s) : s) };
    return { code: result.css, map };
}
