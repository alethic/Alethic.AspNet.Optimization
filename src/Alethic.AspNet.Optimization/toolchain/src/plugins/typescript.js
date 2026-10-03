import { transformSync } from '@swc/wasm-typescript';

const extensions = /\.[cm]?ts$/;

/**
 * Strips TypeScript's types, leaving the JavaScript as written. Syntax that would need code generated for it, such as
 * `enum` or `namespace`, is an error; type checking is the compiler's job, not the bundler's.
 */
export function typescript() {
    return {
        name: 'alethic:typescript',

        transform(code, id) {
            if (!extensions.test(id) || id.endsWith('.d.ts'))
                return null;

            const result = transformSync(code, { filename: id, mode: 'strip-only', sourceMap: true });
            return { code: result.code, map: result.map ?? null };
        },
    };
}
