import { Counter } from './counter.ts';

const panel = document.getElementById('module')!;
new Counter(panel.querySelector<HTMLButtonElement>('.counter')!);

panel.classList.add('panel--ok');
panel.querySelector('.panel__result')!.textContent =
    `Imported Counter from counter.ts; as a global, Counter is ${typeof (window as any).Counter}.`;
