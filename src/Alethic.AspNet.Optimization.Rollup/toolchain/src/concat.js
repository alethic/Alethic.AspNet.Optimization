import { decode, encode } from '@jridgewell/sourcemap-codec';

/**
 * Joins pieces of generated code in order, with a source map that carries each piece's own map across.
 *
 * @param {{ code: string, map?: object | null, source: string }[]} parts the pieces; `source` names a piece that
 *     has no map, which is then mapped line for line to itself
 * @param {string} separator placed after each piece; must end with a newline, so every piece starts a line
 * @returns {{ code: string, map: object }}
 */
export function concat(parts, separator) {
    if (!separator.endsWith('\n'))
        throw new Error('The separator must end with a newline.');

    let code = '';
    let line = 0;
    const sources = [];
    const sourcesContent = [];
    const names = [];
    const mappings = [];

    for (const part of parts) {
        const map = part.map ?? identity(part.code, part.source);
        const decoded = typeof map.mappings === 'string' ? decode(map.mappings) : map.mappings;
        const sourceOffset = sources.length;
        const nameOffset = names.length;

        sources.push(...map.sources);
        sourcesContent.push(...(map.sourcesContent ?? map.sources.map(() => null)));
        names.push(...(map.names ?? []));

        const lines = part.code.split('\n').length;
        for (let i = 0; i < lines; i++) {
            const segments = decoded[i] ?? [];
            mappings[line + i] = segments.map(s =>
                s.length === 1 ? [s[0]]
                : s.length === 4 ? [s[0], s[1] + sourceOffset, s[2], s[3]]
                : [s[0], s[1] + sourceOffset, s[2], s[3], s[4] + nameOffset]);
        }

        code += part.code + separator;
        line += lines - 1 + separator.split('\n').length - 1;
    }

    for (let i = 0; i < line; i++)
        mappings[i] ??= [];

    return { code, map: { version: 3, sources, sourcesContent, names, mappings: encode(mappings) } };
}

function identity(code, source) {
    const lines = code.split('\n').length;
    const mappings = [];
    for (let i = 0; i < lines; i++)
        mappings.push([[0, 0, i, 0]]);

    return { version: 3, sources: [source], sourcesContent: [code], names: [], mappings };
}
