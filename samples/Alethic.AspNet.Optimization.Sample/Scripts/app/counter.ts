/**
 * Counts clicks on a button, and says so on it.
 */
export class Counter {

    readonly button: HTMLButtonElement;
    #count = 0;

    constructor(button: HTMLButtonElement) {
        this.button = button;
        button.disabled = false;
        button.addEventListener('click', () => this.increment());
    }

    increment(): void {
        this.#count++;
        this.button.textContent = `Clicked ${this.#count} time${this.#count === 1 ? '' : 's'}`;
    }

}

/**
 * Never imported, so tree-shaking leaves it out of the bundle.
 */
export function neverUsed(): string {
    return 'tree-shaken';
}
