// GLUR CAFE — admin.js

function toggleSidebar() {
    const sidebar = document.getElementById('adminSidebar');
    const main    = document.getElementById('adminMain');
    sidebar?.classList.toggle('open');
}

// Close sidebar when clicking outside on mobile
document.addEventListener('click', e => {
    const sidebar = document.getElementById('adminSidebar');
    const toggle  = document.getElementById('sidebarToggle');
    if (window.innerWidth < 768 &&
        sidebar?.classList.contains('open') &&
        !sidebar.contains(e.target) &&
        !toggle?.contains(e.target))
    {
        sidebar.classList.remove('open');
    }
});

// ─── CKEditor 5 auto-init ───────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    if (typeof ClassicEditor === 'undefined') return;

    document.querySelectorAll('textarea.ck-editor-target').forEach(el => {
        ClassicEditor
            .create(el, {
                language: 'th',
                toolbar: [
                    'heading', '|',
                    'bold', 'italic', 'underline', 'strikethrough', '|',
                    'bulletedList', 'numberedList', '|',
                    'blockQuote', 'link', '|',
                    'undo', 'redo'
                ],
                heading: {
                    options: [
                        { model: 'paragraph', title: 'Paragraph', class: 'ck-heading_paragraph' },
                        { model: 'heading2', view: 'h2', title: 'Heading 2', class: 'ck-heading_heading2' },
                        { model: 'heading3', view: 'h3', title: 'Heading 3', class: 'ck-heading_heading3' }
                    ]
                }
            })
            .then(editor => {
                // Store instance on element for manual sync
                el._ckEditor = editor;
            })
            .catch(err => console.error('CKEditor init error:', err));
    });
});

// ─── Client-side image compression (Canvas) ────────────────────────────────
async function compressImage(file, maxSizePx = 1200, quality = 0.85) {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onerror = () => resolve(file);
        reader.onload = (e) => {
            const img = new Image();
            img.onerror = () => resolve(file);
            img.onload = () => {
                let { width, height } = img;
                // Skip tiny files already under 1.5 MB and within size limit
                if (width <= maxSizePx && height <= maxSizePx && file.size < 1.5 * 1024 * 1024) {
                    resolve(file);
                    return;
                }
                if (width > maxSizePx || height > maxSizePx) {
                    if (width > height) {
                        height = Math.round(height * maxSizePx / width);
                        width  = maxSizePx;
                    } else {
                        width  = Math.round(width * maxSizePx / height);
                        height = maxSizePx;
                    }
                }
                const canvas = document.createElement('canvas');
                canvas.width  = width;
                canvas.height = height;
                canvas.getContext('2d').drawImage(img, 0, 0, width, height);
                canvas.toBlob((blob) => {
                    if (!blob) { resolve(file); return; }
                    const out = new File([blob],
                        file.name.replace(/\.[^.]+$/, '.jpg'),
                        { type: 'image/jpeg' }
                    );
                    resolve(out.size < file.size ? out : file);
                }, 'image/jpeg', quality);
            };
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    });
}

// ─── Upload overlay helpers ──────────────────────────────────────────────────
function showUploadOverlay(msg) {
    const el  = document.getElementById('uploadOverlay');
    const txt = document.getElementById('uploadOverlayText');
    if (el)  el.style.display  = 'flex';
    if (txt) txt.textContent   = msg || 'กำลังอัปโหลด...';
}
function hideUploadOverlay() {
    const el = document.getElementById('uploadOverlay');
    if (el) el.style.display = 'none';
}

// ─── Mobile-safe multipart form interceptor ──────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('form[enctype="multipart/form-data"]').forEach(form => {
        const fileInputs = [...form.querySelectorAll('input[type="file"]')];
        if (fileInputs.length === 0) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            // Sync any CKEditor instance before submit
            form.querySelectorAll('textarea.ck-editor-target').forEach(ta => {
                if (ta._ckEditor) ta.value = ta._ckEditor.getData();
            });

            showUploadOverlay('กำลังเตรียมรูปภาพ...');

            try {
                const totalFiles = fileInputs.reduce((s, inp) => s + inp.files.length, 0);
                let done = 0;

                for (const input of fileInputs) {
                    const files = [...input.files];
                    if (files.length === 0) continue;

                    const compressed = [];
                    for (const file of files) {
                        done++;
                        showUploadOverlay(
                            totalFiles > 1
                                ? `กำลังบีบอัดรูปที่ ${done} / ${totalFiles}...`
                                : 'กำลังบีบอัดรูปภาพ...'
                        );
                        compressed.push(
                            file.type.startsWith('image/')
                                ? await compressImage(file)
                                : file
                        );
                    }

                    try {
                        const dt = new DataTransfer();
                        compressed.forEach(f => dt.items.add(f));
                        input.files = dt.files;
                    } catch { /* DataTransfer unavailable — continue with original */ }
                }

                showUploadOverlay('กำลังอัปโหลด...');
                // Safety: hide overlay after 60 s in case navigation fails
                window._uploadSafetyTimer = setTimeout(hideUploadOverlay, 60000);
                form.submit();
            } catch (err) {
                hideUploadOverlay();
                console.error('Upload prep error:', err);
                form.submit(); // fallback
            }
        });
    });
});

// ─── Bulk select + delete ────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const selectAll = document.getElementById('selectAll');
    const bulkBar   = document.getElementById('bulkActionBar');
    const cancelBtn = document.getElementById('cancelBulk');
    if (!selectAll || !bulkBar) return;

    const getChecked = () => [...document.querySelectorAll('.row-check:checked')];

    function updateBulkBar() {
        const checked   = getChecked();
        const countEl   = document.getElementById('selectedCount');
        const container = document.getElementById('bulkIdsContainer');
        if (checked.length > 0) {
            bulkBar.classList.remove('d-none');
            if (countEl) countEl.textContent = `เลือก ${checked.length} รายการ`;
            if (container) {
                container.innerHTML = '';
                checked.forEach(cb => {
                    const inp = document.createElement('input');
                    inp.type = 'hidden'; inp.name = 'ids'; inp.value = cb.dataset.id;
                    container.appendChild(inp);
                });
            }
        } else {
            bulkBar.classList.add('d-none');
        }
    }

    selectAll.addEventListener('change', function () {
        document.querySelectorAll('.row-check').forEach(cb => cb.checked = this.checked);
        updateBulkBar();
    });

    document.querySelectorAll('.row-check').forEach(cb => {
        cb.addEventListener('change', () => {
            const all = document.querySelectorAll('.row-check');
            selectAll.checked = all.length > 0 && all.length === getChecked().length;
            updateBulkBar();
        });
    });

    if (cancelBtn) {
        cancelBtn.addEventListener('click', () => {
            selectAll.checked = false;
            document.querySelectorAll('.row-check').forEach(cb => cb.checked = false);
            bulkBar.classList.add('d-none');
        });
    }
});
