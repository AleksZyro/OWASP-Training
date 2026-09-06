document.querySelectorAll("[data-submit-form]").forEach((form) => {
    form.addEventListener("submit", () => {
        const button = form.querySelector("button[type='submit']");
        if (button instanceof HTMLButtonElement) {
            button.disabled = true;
            button.textContent = form.dataset.busyLabel ?? button.textContent;
        }
    });
});
