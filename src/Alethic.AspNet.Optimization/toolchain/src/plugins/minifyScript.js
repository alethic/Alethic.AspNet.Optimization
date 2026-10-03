import { minify } from 'terser';

/**
 * Minifies each rendered chunk with Terser.
 *
 * @param {object} options
 * @param {boolean} options.sourceMap whether to map the minified code back to the chunk
 * @param {boolean} options.classic whether the chunk runs as a classic script, whose top-level names are globals other
 *     scripts may use, so must be neither renamed nor dropped
 */
export function minifyScript({ sourceMap, classic }) {
    return {
        name: 'alethic:minify-script',

        async renderChunk(code) {
            const result = await minify(code, {
                module: false,
                toplevel: !classic,
                sourceMap: sourceMap ? { asObject: true } : false,
            });

            return { code: result.code, map: result.map ?? null };
        },
    };
}
