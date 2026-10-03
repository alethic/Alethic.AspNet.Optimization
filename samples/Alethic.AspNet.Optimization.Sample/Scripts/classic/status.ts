// A classic script in TypeScript: no imports or exports, so its names are globals too, and it uses greeting.js's.

declare function formatGreeting(source: string): string;

interface PanelResult {
    panel: string;
    message: string;
}

function markPanel(result: PanelResult): void {
    const panel = document.getElementById(result.panel);
    if (!panel)
        return;

    panel.classList.add('panel--ok');
    panel.querySelector('.panel__result')!.textContent = result.message;
}

markPanel({
    panel: 'classic',
    message: formatGreeting('status.ts') + ', which found formatGreeting as a global: ' + (typeof (window as any).formatGreeting),
});
