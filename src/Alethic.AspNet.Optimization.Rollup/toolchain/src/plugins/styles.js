import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { transform } from 'lightningcss-wasm';
import * as sass from 'sass';

const entryId = '\0alethic:styles';
const sassExtensions = /\.s[ac]ss$/;

/**
 * Makes Rollup's entry a stylesheet: the entry stylesheet compiled by Sass, which follows its `@use` and `@import`
 * rules, then transformed by Lightning CSS and emitted as the bundle's one asset. The entry's own JavaScript is empty,
 * and discarded.
 *
 * Sass reads every file through the host's files, as Rollup does, and each is added to Rollup's watch files, so the
 * build reports the partials a stylesheet depends on as well as the stylesheet itself.
 *
 * @param {string} input absolute path of the entry stylesheet
 * @param {{ exists(path: string): boolean, read(path: string): string | null }} files the host's files
 * @param {object} options
 * @param {string} options.fileName the asset's file name
 * @param {boolean} options.minify whether to minify the stylesheet
 * @param {boolean} options.sourceMap whether to emit the asset's source map beside it, as `fileName.map`
 * @param {string[]} options.warnings where to add warnings
 */
export function styles(input, files, { fileName, minify, sourceMap, warnings }) {
    return {
        name: 'alethic:styles',

        resolveId(source) {
            return source === entryId ? entryId : null;
        },

        load(id) {
            if (id !== entryId)
                return null;

            const compiled = compile(this, input, files, warnings);

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

function compile(context, input, files, warnings) {
    const contents = files.read(input);
    if (contents == null)
        context.error(`'${input}' was not found.`);

    context.addWatchFile(input);
    if (!sassExtensions.test(input))
        return { code: contents, map: null };

    const result = sass.compileString(contents, {
        url: pathToFileURL(input),
        syntax: syntaxOf(input),
        importer: hostImporter(files),
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

/**
 * Returns a Sass importer that finds and reads stylesheets through the host's files, by Sass's own rules: a partial,
 * named with a leading underscore, or the file itself, with any of Sass's extensions, or a directory's index.
 *
 * @param {{ exists(path: string): boolean, read(path: string): string | null }} files the host's files
 */
function hostImporter(files) {
    return {
        canonicalize(url, context) {
            let target;
            if (url.startsWith('file:'))
                target = fileURLToPath(url);
            else if (context?.containingUrl?.protocol === 'file:')
                target = path.resolve(path.dirname(fileURLToPath(context.containingUrl)), url);
            else
                return null;

            const found = candidates(target).find(candidate => files.exists(candidate));
            return found ? pathToFileURL(found) : null;
        },

        load(canonicalUrl) {
            const file = fileURLToPath(canonicalUrl);
            const contents = files.read(file);
            return contents == null ? null : { contents, syntax: syntaxOf(file), sourceMapUrl: canonicalUrl };
        },
    };
}

function candidates(target) {
    const partial = (file) => path.join(path.dirname(file), '_' + path.basename(file));
    const extensions = ['.scss', '.sass', '.css'];

    if (extensions.includes(path.extname(target)))
        return [partial(target), target];

    return [
        ...extensions.flatMap(extension => [partial(target + extension), target + extension]),
        ...extensions.flatMap(extension => [path.join(target, '_index' + extension), path.join(target, 'index' + extension)]),
    ];
}

function syntaxOf(file) {
    return file.endsWith('.sass') ? 'indented' : file.endsWith('.css') ? 'css' : 'scss';
}
