import path from 'node:path';

// tried in order for an import that names no extension, as a TypeScript or Node project would resolve it
const suffixes = ['', '.ts', '.tsx', '.mts', '.js', '.mjs', '/index.ts', '/index.js'];

/**
 * Resolves and loads every file through the host's files, so a bundle reads what the application serves: the files it
 * includes as System.Web.Optimization provides them, and what they import from the application's virtual path
 * provider. Nothing is read from disk directly.
 *
 * Only relative imports are resolved; a bare import, such as a package name, is left to the plugins after this one.
 *
 * @param {{ exists(path: string): boolean, read(path: string): string | null }} files the host's files, named by
 *     absolute path
 */
export function hostFiles(files) {
    return {
        name: 'alethic:host-files',

        resolveId(source, importer) {
            if (path.isAbsolute(source))
                return files.exists(source) ? source : null;

            if (importer && path.isAbsolute(importer) && (source.startsWith('./') || source.startsWith('../'))) {
                const base = path.resolve(path.dirname(importer), source);
                for (const suffix of suffixes)
                    if (files.exists(base + suffix))
                        return base + suffix;
            }

            return null;
        },

        load(id) {
            if (!path.isAbsolute(id))
                return null;

            const code = files.read(id);
            if (code == null)
                this.error(`'${id}' was not found.`);

            // Rollup records the files it reads itself, and these it does not
            this.addWatchFile(id);
            return code;
        },
    };
}
