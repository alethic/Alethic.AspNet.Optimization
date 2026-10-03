const entryId = '\0alethic:module-entry';

/**
 * Makes Rollup's entry a module importing each of the given modules in order, for their effects.
 *
 * @param {string[]} inputs absolute paths of the modules, in order
 */
export function moduleEntry(inputs) {
    return {
        name: 'alethic:module-entry',

        resolveId(source) {
            return source === entryId ? entryId : null;
        },

        load(id) {
            if (id !== entryId)
                return null;

            return inputs.map(input => `import ${JSON.stringify(input)};`).join('\n');
        },
    };
}

moduleEntry.entryId = entryId;
