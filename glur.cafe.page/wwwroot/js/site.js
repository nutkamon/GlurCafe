// GLUR CAFE — site.js

// ─── Portfolio Modal ──────────────────────────────────────────────────────────
var _portfolioGradients = {
    'port-1': 'linear-gradient(135deg,#00A59A,#007A72)',
    'port-2': 'linear-gradient(135deg,#007A72,#4A7C5F)',
    'port-3': 'linear-gradient(135deg,#4A7C5F,#6B4423)',
    'port-4': 'linear-gradient(135deg,#C8943A,#6B4423)',
    'port-5': 'linear-gradient(135deg,#00A59A,#4A7C5F)',
    'port-6': 'linear-gradient(135deg,#007A72,#C8943A)',
    'port-7': 'linear-gradient(135deg,#33C5BC,#00A59A)'
};

function openPortfolioModal(el) {
    var name     = el.getAttribute('data-name')        || '';
    var category = el.getAttribute('data-category')    || '';
    var desc     = el.getAttribute('data-description') || '';
    var client   = el.getAttribute('data-client')      || '';
    var image    = el.getAttribute('data-image')       || '';
    var icon     = el.getAttribute('data-icon')        || 'bi-image';
    var gradient = el.getAttribute('data-gradient')    || 'port-1';

    var modalImg   = document.getElementById('portfolioModalImg');
    var iconWrap   = document.getElementById('portfolioModalIconPlaceholder');
    var modalIcon  = document.getElementById('portfolioModalIcon');
    var imgWrap    = document.getElementById('portfolioModalImgWrap');

    if (image) {
        modalImg.src = image;
        modalImg.alt = name;
        modalImg.classList.remove('d-none');
        iconWrap.classList.add('d-none');
        imgWrap.style.background = '';
    } else {
        modalImg.classList.add('d-none');
        iconWrap.classList.remove('d-none');
        modalIcon.className = 'bi ' + icon;
        imgWrap.style.background = _portfolioGradients[gradient] || _portfolioGradients['port-1'];
    }

    document.getElementById('portfolioModalName').textContent = name;
    document.getElementById('portfolioModalCategory').textContent = category;

    var clientEl = document.getElementById('portfolioModalClient');
    clientEl.textContent = client || '';
    clientEl.style.display = client ? '' : 'none';

    var tmp = document.createElement('div');
    tmp.innerHTML = desc;
    document.getElementById('portfolioModalDesc').textContent = tmp.textContent || tmp.innerText || '';

    // ตั้งค่า form
    var itemInput = document.getElementById('portfolioContactItem');
    if (itemInput) itemInput.value = name;

    var msgInput = document.getElementById('portfolioContactMessage');
    if (msgInput) msgInput.placeholder = 'สนใจ "' + name + '" กรุณาระบุรายละเอียดเพิ่มเติม...';

    var pForm = document.getElementById('portfolioContactForm');
    if (pForm) {
        var itemVal = itemInput ? itemInput.value : '';
        pForm.reset();
        if (itemInput) itemInput.value = itemVal;
    }

    new bootstrap.Modal(document.getElementById('portfolioModal')).show();
}

// ─── AOS Init ───────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    if (typeof AOS !== 'undefined') {
        AOS.init({ duration: 800, once: true, offset: 60 });
    }

    // ─── Navbar scroll behaviour ─────────────────────────────────────────────
    const navbar = document.getElementById('mainNavbar');
    const backToTop = document.getElementById('backToTop');

    const onScroll = () => {
        if (window.scrollY > 50) {
            navbar?.classList.add('scrolled');
            backToTop?.classList.add('show');
        } else {
            navbar?.classList.remove('scrolled');
            backToTop?.classList.remove('show');
        }
    };

    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll(); // run once on load

    // ─── Back-to-top button ──────────────────────────────────────────────────
    backToTop?.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    // ─── Smooth-scroll for all anchor links ──────────────────────────────────
    document.querySelectorAll('a[href^="#"]').forEach(link => {
        link.addEventListener('click', e => {
            const target = document.querySelector(link.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                // close mobile navbar if open
                const navCollapse = document.getElementById('navbarNav');
                if (navCollapse?.classList.contains('show')) {
                    navCollapse.classList.remove('show');
                }
            }
        });
    });

    // ─── Contact form AJAX ───────────────────────────────────────────────────
    const form = document.getElementById('contactForm');
    if (form) {
        form.addEventListener('submit', async e => {
            e.preventDefault();

            const data = new FormData(form);

            const btn = form.querySelector('[type="submit"]');
            const originalHtml = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>กำลังส่ง...';

            try {
                const res = await fetch('/?handler=Contact', {
                    method: 'POST',
                    body: data
                });

                if (!res.ok) {
                    // Server returned 400/500 (e.g. expired antiforgery token)
                    if (typeof Swal !== 'undefined') {
                        await Swal.fire({
                            icon: 'error',
                            title: 'เกิดข้อผิดพลาด',
                            text: 'กรุณารีเฟรชหน้าเว็บแล้วลองใหม่อีกครั้ง',
                            confirmButtonColor: '#00A59A'
                        });
                    } else {
                        alert('กรุณารีเฟรชหน้าเว็บแล้วลองใหม่อีกครั้ง');
                    }
                    return;
                }

                const json = await res.json();

                if (json.success) {
                    if (typeof Swal !== 'undefined') {
                        await Swal.fire({
                            icon: 'success',
                            title: 'ส่งข้อความสำเร็จ! ☕',
                            text: json.message,
                            confirmButtonColor: '#00A59A',
                            confirmButtonText: 'ขอบคุณ'
                        });
                    } else {
                        alert(json.message);
                    }
                    form.reset();
                } else {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'warning',
                            title: 'กรุณาตรวจสอบข้อมูล',
                            text: json.message,
                            confirmButtonColor: '#00A59A'
                        });
                    } else {
                        alert(json.message);
                    }
                }
            } catch {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'เกิดข้อผิดพลาด',
                        text: 'ไม่สามารถส่งข้อความได้ กรุณาลองใหม่อีกครั้ง',
                        confirmButtonColor: '#00A59A'
                    });
                } else {
                    alert('เกิดข้อผิดพลาด กรุณาลองใหม่');
                }
            } finally {
                btn.disabled = false;
                btn.innerHTML = originalHtml;
            }
        });
    }
});

// ─── Pricing CTA ─────────────────────────────────────────────────────────────
function handlePricingClick(planName) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'info',
            title: `แพ็คเกจ: ${planName}`,
            text: 'ต้องการข้อมูลเพิ่มเติม? ทีมงานของเรายินดีให้คำปรึกษา',
            confirmButtonColor: '#00A59A',
            confirmButtonText: 'ไปยังแบบฟอร์มติดต่อ',
            showCancelButton: true,
            cancelButtonText: 'ภายหลัง'
        }).then(result => {
            if (result.isConfirmed) {
                const contact = document.getElementById('contact');
                if (contact) contact.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    } else {
        const contact = document.getElementById('contact');
        if (contact) contact.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

// ─── Service Modal ────────────────────────────────────────────────────────────
var _serviceGradients = {
    'grad-1':  'linear-gradient(135deg,#00A59A,#007A72)',
    'grad-2':  'linear-gradient(135deg,#007A72,#4A7C5F)',
    'grad-3':  'linear-gradient(135deg,#4A7C5F,#6B4423)',
    'grad-4':  'linear-gradient(135deg,#C8943A,#6B4423)',
    'grad-5':  'linear-gradient(135deg,#00A59A,#4A7C5F)',
    'grad-6':  'linear-gradient(135deg,#007A72,#C8943A)',
    'grad-7':  'linear-gradient(135deg,#33C5BC,#00A59A)',
    'grad-8':  'linear-gradient(135deg,#6B4423,#4A2F18)',
    'grad-9':  'linear-gradient(135deg,#4A7C5F,#007A72)',
    'grad-10': 'linear-gradient(135deg,#C8943A,#DEBA7A)'
};

function openServiceModal(el) {
    var title    = el.getAttribute('data-title')       || '';
    var desc     = el.getAttribute('data-description') || '';
    var image    = el.getAttribute('data-image')       || '';
    var icon     = el.getAttribute('data-icon')        || 'bi-cup-hot';
    var gradient = el.getAttribute('data-gradient')    || 'grad-1';

    var modalImg         = document.getElementById('serviceModalImg');
    var iconWrap         = document.getElementById('serviceModalIconPlaceholder');
    var modalIcon        = document.getElementById('serviceModalIcon');
    var imageWrap        = document.getElementById('serviceModalImageWrap');

    if (image) {
        modalImg.src = image;
        modalImg.alt = title;
        modalImg.classList.remove('d-none');
        iconWrap.classList.add('d-none');
        imageWrap.style.background = '';
    } else {
        modalImg.classList.add('d-none');
        iconWrap.classList.remove('d-none');
        modalIcon.className = 'bi ' + icon;
        imageWrap.style.background = _serviceGradients[gradient] || _serviceGradients['grad-1'];
    }

    document.getElementById('serviceModalName').textContent = title;

    // Strip HTML tags for the desc overlay
    var tmp = document.createElement('div');
    tmp.innerHTML = desc;
    document.getElementById('serviceModalDesc').textContent = tmp.textContent || tmp.innerText || '';

    var svcInput = document.getElementById('serviceContactService');
    if (svcInput) svcInput.value = title;

    var msgInput = document.getElementById('serviceContactMessage');
    if (msgInput) msgInput.placeholder = '\u0e2a\u0e19\u0e43\u0e08\u0e1a\u0e23\u0e34\u0e01\u0e32\u0e23 "' + title + '" \u0e01\u0e23\u0e38\u0e13\u0e32\u0e23\u0e30\u0e1a\u0e38\u0e23\u0e32\u0e22\u0e25\u0e30\u0e40\u0e2d\u0e35\u0e22\u0e14\u0e40\u0e1e\u0e34\u0e48\u0e21\u0e40\u0e15\u0e34\u0e21...';

    var pForm = document.getElementById('serviceContactForm');
    if (pForm) {
        var svcVal = svcInput ? svcInput.value : '';
        pForm.reset();
        if (svcInput) svcInput.value = svcVal;
    }

    new bootstrap.Modal(document.getElementById('serviceModal')).show();
}

document.addEventListener('DOMContentLoaded', function () {
    var sForm = document.getElementById('serviceContactForm');
    if (sForm) {
        sForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var name  = sForm.querySelector('[name="ContactInput.FullName"]');
            var phone = sForm.querySelector('[name="ContactInput.Phone"]');
            if (name && !name.value.trim()) {
                if (typeof Swal !== 'undefined') Swal.fire({ icon: 'warning', title: 'กรุณากรอกชื่อ', confirmButtonColor: '#00A59A' });
                return;
            }
            if (phone && !phone.value.trim()) {
                if (typeof Swal !== 'undefined') Swal.fire({ icon: 'warning', title: 'กรุณากรอกเบอร์โทรศัพท์', confirmButtonColor: '#00A59A' });
                return;
            }
            var formData = new FormData(sForm);
            fetch('/?handler=Contact', { method: 'POST', body: formData })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var svcVal = document.getElementById('serviceContactService') ? document.getElementById('serviceContactService').value : '';
                    if (data.success) {
                        if (typeof Swal !== 'undefined') {
                            Swal.fire({ icon: 'success', title: 'ส่งข้อความเรียบร้อย! ☕', text: 'เราจะติดต่อกลับโดยเร็วที่สุด', confirmButtonText: 'ขอบคุณ', confirmButtonColor: '#00A59A' });
                        }
                        sForm.reset();
                        if (document.getElementById('serviceContactService')) document.getElementById('serviceContactService').value = svcVal;
                        var modalEl = document.getElementById('serviceModal');
                        var bsModal = bootstrap.Modal.getInstance(modalEl);
                        if (bsModal) bsModal.hide();
                    } else {
                        if (typeof Swal !== 'undefined') Swal.fire({ icon: 'error', title: 'เกิดข้อผิดพลาด', text: data.message || 'กรุณาลองใหม่อีกครั้ง', confirmButtonColor: '#00A59A' });
                    }
                })
                .catch(function () {
                    if (typeof Swal !== 'undefined') Swal.fire({ icon: 'error', title: 'ไม่สามารถส่งข้อความได้', text: 'กรุณาลองใหม่อีกครั้ง', confirmButtonColor: '#00A59A' });
                });
        });
    }

    // ─── Portfolio Contact Form ───────────────────────────────────────────────
    var pForm = document.getElementById('portfolioContactForm');
    if (pForm) {
        pForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var name  = pForm.querySelector('[name="ContactInput.FullName"]');
            var phone = pForm.querySelector('[name="ContactInput.Phone"]');
            if (name && !name.value.trim()) {
                if (typeof Swal !== 'undefined') Swal.fire({ icon: 'warning', title: 'กรุณากรอกชื่อ', confirmButtonColor: '#00A59A' });
                return;
            }
            if (phone && !phone.value.trim()) {
                if (typeof Swal !== 'undefined') Swal.fire({ icon: 'warning', title: 'กรุณากรอกเบอร์โทรศัพท์', confirmButtonColor: '#00A59A' });
                return;
            }
            var formData = new FormData(pForm);
            fetch('/?handler=Contact', { method: 'POST', body: formData })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var itemVal = document.getElementById('portfolioContactItem') ? document.getElementById('portfolioContactItem').value : '';
                    if (data.success) {
                        if (typeof Swal !== 'undefined') {
                            Swal.fire({ icon: 'success', title: 'ส่งข้อความเรียบร้อย! ☕', text: 'เราจะติดต่อกลับโดยเร็วที่สุด', confirmButtonText: 'ขอบคุณ', confirmButtonColor: '#00A59A' });
                        }
                        pForm.reset();
                        if (document.getElementById('portfolioContactItem')) document.getElementById('portfolioContactItem').value = itemVal;
                        var modalEl = document.getElementById('portfolioModal');
                        var bsModal = bootstrap.Modal.getInstance(modalEl);
                        if (bsModal) bsModal.hide();
                    } else {
                        if (typeof Swal !== 'undefined') Swal.fire({ icon: 'error', title: 'เกิดข้อผิดพลาด', text: data.message || 'กรุณาลองใหม่อีกครั้ง', confirmButtonColor: '#00A59A' });
                    }
                })
                .catch(function () {
                    if (typeof Swal !== 'undefined') Swal.fire({ icon: 'error', title: 'ไม่สามารถส่งข้อความได้', text: 'กรุณาลองใหม่อีกครั้ง', confirmButtonColor: '#00A59A' });
                });
        });
    }
});

