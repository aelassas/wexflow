document.addEventListener('DOMContentLoaded', () => {
    const targets = document.querySelectorAll('.github-button');
    if (!targets.length) return;

    const load = () => {
        // Prevent loading twice
        if (document.getElementById('github-buttons-js')) return;

        const script = document.createElement('script');
        script.id = 'github-buttons-js';
        script.src = 'https://buttons.github.io/buttons.js';
        script.async = true;
        script.onerror = () => {
            for (const target of targets) {
                target.style.position = 'absolute';
                target.style.opacity = '0';
            }
            console.warn('GitHub buttons.js failed to load (offline or blocked)');
        };
        script.onload = () => {
            for (const target of targets) {
                target.style.position = '';
                target.style.opacity = '';
            }
        };
        document.head.appendChild(script);
    };

    if ('IntersectionObserver' in window) {
        const io = new IntersectionObserver((entries) => {
            if (entries.some(e => e.isIntersecting)) {
                io.disconnect();
                load();
            }
        });
        targets.forEach(el => io.observe(el));
    } else {
        load();
    }
});
