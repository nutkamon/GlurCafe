/* ===== GLUR CAFE — Menu Board ===== */
(function () {
    'use strict';
    document.documentElement.classList.add('js');

    // ---- Autoplay videos when visible ----
    const videos = document.querySelectorAll('.menu-card video');
    if ('IntersectionObserver' in window) {
        const io = new IntersectionObserver((entries) => {
            entries.forEach((e) => {
                if (e.isIntersecting) e.target.play().catch(() => {});
                else e.target.pause();
            });
        }, { threshold: 0.25 });
        videos.forEach((v) => io.observe(v));
    } else {
        videos.forEach((v) => v.play().catch(() => {}));
    }

    // ---- Lightbox ----
    const lightbox = document.getElementById('menuLightbox');
    const lightboxContent = document.getElementById('lightboxContent');

  document.querySelectorAll('.menu-card').forEach((card) => {
        card.addEventListener('click', () => {
            const src = card.dataset.full;
            const type = card.dataset.type;
            if (!src) return;
            lightboxContent.innerHTML = type === 'video'
                ? `<video src="${src}" autoplay muted loop playsinline></video>`
                : `<img src="${src}" alt="" />`;
            lightbox.classList.add('open');
            document.body.style.overflow = 'hidden';
        });
    });

    function closeLightbox() {
        lightbox.classList.remove('open');
        lightboxContent.innerHTML = '';
        document.body.style.overflow = '';
    }
    lightbox.addEventListener('click', (e) => {
        if (e.target === lightbox || e.target.closest('#lightboxClose')) closeLightbox();
    });
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeLightbox();
    });

    // ---- Highlight active category chip on scroll ----
    const chips = document.querySelectorAll('.menu-chip');
    const sections = document.querySelectorAll('.menu-section');
    if (chips.length && 'IntersectionObserver' in window) {
        const chipObserver = new IntersectionObserver((entries) => {
            entries.forEach((e) => {
                if (e.isIntersecting) {
                    chips.forEach((c) => c.classList.toggle('active', c.getAttribute('href') === '#' + e.target.id));
                }
            });
        }, { rootMargin: '-30% 0px -60% 0px' });
        sections.forEach((s) => chipObserver.observe(s));
    }

    // ---- Fade-in sections on scroll ----
    if ('IntersectionObserver' in window) {
        const fadeObserver = new IntersectionObserver((entries) => {
            entries.forEach((e) => {
                if (e.isIntersecting) {
                    e.target.classList.add('in-view');
                    fadeObserver.unobserve(e.target);
                }
            });
        }, { threshold: 0.08 });
        sections.forEach((s) => fadeObserver.observe(s));
    } else {
        sections.forEach((s) => s.classList.add('in-view'));
    }

    // ---- TV kiosk mode: auto-scroll loop ----
    if (document.body.classList.contains('tv-mode')) {
        let dir = 1;
        setInterval(() => {
            const atBottom = window.innerHeight + window.scrollY >= document.body.scrollHeight - 4;
            const atTop = window.scrollY <= 4;
            if (atBottom) dir = -1;
            if (atTop) dir = 1;
            window.scrollBy({ top: dir * 2, behavior: 'auto' });
        }, 60);
    }
})();
