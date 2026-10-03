import { concat } from '../concat.js';

// not a \0 virtual id: Rollup leaves those out of source maps, and this module's code is the inputs' own
const entryId = 'alethic:classic-scripts';

/**
 * Makes Rollup's entry the given classic scripts, joined in order into one module.
 *
 * Each script is loaded through Rollup on its own, so every transform plugin sees it as the file it is. They are then
 * joined into the entry rather than imported by it: one module has one top-level scope, as scripts sharing a page
 * do, where modules importing each other would each get their own and have clashing names renamed apart.
 *
 * @param {string[]} inputs absolute paths of the scripts, in order
 * @param {string} separator what the scripts are joined with, ending with a line break
 */
export function classicScripts(inputs, separator) {
    const inputSet = new Set(inputs);
    const maps = new Map();

    return {
        name: 'alethic:classic-scripts',

        resolveId(source) {
            return source === entryId ? entryId : null;
        },

        async load(id) {
            if (id !== entryId)
                return null;

            const parts = [];
            for (const input of inputs) {
                const info = await this.load({ id: input });
                parts.push({ code: info.code, map: maps.get(input), source: input });
            }

            return concat(parts, separator);
        },

        transform: {
            order: 'post',
            handler(_code, id) {
                if (inputSet.has(id))
                    maps.set(id, this.getCombinedSourcemap());

                return null;
            },
        },
    };
}

classicScripts.entryId = entryId;
