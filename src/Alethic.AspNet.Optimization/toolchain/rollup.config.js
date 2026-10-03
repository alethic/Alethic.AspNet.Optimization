// Builds the toolchain into one CommonJS file, the form Alethic.Node loads, with the WebAssembly binaries its tools
// read from beside it.

import fs from 'node:fs';
import path from 'node:path';
import { builtinModules } from 'node:module';
import commonjs from '@rollup/plugin-commonjs';
import json from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';

const wasm = [
    'node_modules/@rollup/wasm-node/dist/wasm-node/bindings_wasm_bg.wasm',
    'node_modules/lightningcss-wasm/lightningcss_node.wasm',
];

export default {
    input: 'src/index.js',
    output: {
        file: 'dist/toolchain.cjs',
        format: 'cjs',
        inlineDynamicImports: true,
        sourcemap: false,
    },
    external: [...builtinModules, ...builtinModules.map(m => `node:${m}`), '@parcel/watcher', 'fsevents'],
    plugins: [
        nodeResolve({ preferBuiltins: true, exportConditions: ['node'] }),
        commonjs({ ignoreDynamicRequires: true }),
        json(),
        {
            name: 'copy-wasm',
            writeBundle() {
                for (const file of wasm)
                    fs.copyFileSync(file, path.join('dist', path.basename(file)));
            },
        },
    ],
    onwarn(warning, warn) {
        if (warning.code === 'CIRCULAR_DEPENDENCY' || warning.code === 'THIS_IS_UNDEFINED' || warning.code === 'EVAL')
            return;

        warn(warning);
    },
};
