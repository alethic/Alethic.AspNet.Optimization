import { transformSync } from '@swc/wasm';

/**
 * Lowers each rendered chunk with SWC to what the target browsers run: newer syntax rewritten, with the helpers it needs
 * inlined at the top of the chunk. Polyfills for missing built-ins are not added.
 *
 * @param {object} options
 * @param {string} options.targets the browsers, as a browserslist query
 * @param {boolean} options.sourceMap whether to map the lowered code back to the chunk
 */
export function downlevel({ targets, sourceMap }) {
    return {
        name: 'alethic:downlevel',

        renderChunk(code, chunk) {
            const result = transformSync(code, {
                filename: chunk.fileName,
                // a chunk runs as a script: classic scripts joined, or modules bundled into one function
                isModule: false,
                sourceMaps: sourceMap,
                jsc: { parser: { syntax: 'ecmascript' } },
                env: { targets },
            });

            return { code: result.code, map: result.map ?? null };
        },
    };
}
