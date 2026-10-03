import { double } from './math.ts';

var local = double(21);
globalThis.answer = local;
