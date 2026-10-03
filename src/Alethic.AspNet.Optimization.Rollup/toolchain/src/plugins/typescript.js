import { transformSync } from '@swc/wasm';

const extensions = /\.[cm]?tsx?$/;

/**
 * Compiles TypeScript to JavaScript with SWC, leaving the JavaScript at the language level it was written in; lowering
 * it for older browsers is the downlevel plugin's. Type checking is the compiler's job, not the bundler's.
 */
export function typescript() {
    return {
        name: 'alethic:typescript',

        transform(code, id) {
            if (!extensions.test(id) || id.endsWith('.d.ts'))
                return null;

            const result = transformSync(code, {
                filename: id,
                isModule: 'unknown',
                sourceMaps: true,
                jsc: {
                    parser: { syntax: 'typescript', tsx: id.endsWith('x') },
                    target: 'esnext',
                },
            });

            return { code: result.code, map: result.map ?? null };
        },
    };
}
