// wwwroot/js/fcm.js — Welcome push on first page open (once per browser)

// Firebase v10 (ES modules)
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js";
import { getMessaging, isSupported, getToken, deleteToken, onMessage } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging.js";

// ---- Your Firebase Web config + VAPID public key (must match the SAME project as service-account.json) ----
const firebaseConfig = {
    apiKey: "AIzaSyCPeR8WOTjlUXBSvvbKA3qElVGB33fJLwg",
    authDomain: "websitenotification-53e82.firebaseapp.com",
    projectId: "websitenotification-53e82",
    storageBucket: "websitenotification-53e82.appspot.com",
    messagingSenderId: "720780980996",
    appId: "1:720780980996:web:df5d3b0b35373c3b409651"
};
// From Firebase Console → Project Settings → Cloud Messaging → Web push certificates
const VAPID_PUBLIC_KEY = "BEzXbBuFuk0yN8K32dNHghxdQek0rw8tM-bMzvR5qc4JGDXDO92t1Eq--8RCwLTNSqW_1SoOUZeB8f6tDbXn0y0";
// ------------------------------------------------------------------------------------------------------------

const FLAG_KEY = "yaseer_welcome_sent_v1";

const app = initializeApp(firebaseConfig);

async function registerSW() {
    // Must be served from the site root: /firebase-messaging-sw.js
    return await navigator.serviceWorker.register("/firebase-messaging-sw.js");
}

(async () => {
    // If browser doesn’t support FCM for web, bail out
    if (!(await isSupported())) return;

    // Ask permission if not decided yet
    if (Notification.permission === "default") {
        try { await Notification.requestPermission(); } catch { /* ignore */ }
    }
    if (Notification.permission !== "granted") return;

    const swReg = await registerSW();
    const messaging = getMessaging(app);

    // In dev, clear stale tokens to avoid UNREGISTERED errors
    try { await deleteToken(messaging); } catch { /* ignore */ }

    let token = "";
    try {
        token = await getToken(messaging, {
            vapidKey: VAPID_PUBLIC_KEY,
            serviceWorkerRegistration: swReg
        });
    } catch {
        return;
    }
    if (!token) return;
    window._fcmToken = token;

    // Call server exactly once per browser (localStorage guard)
    if (!localStorage.getItem(FLAG_KEY)) {
        try {
            const res = await fetch("/api/token", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ token })
            });
            if (res.ok) {
                // Even if server already welcomed, we stop future calls in this browser
                localStorage.setItem(FLAG_KEY, "1");
            }
        } catch {
            // If it fails, we don't set the flag so it can retry next open
        }
    }

    // Optional: foreground messages while the tab is open
    onMessage(messaging, (payload) => {
        const title = payload?.notification?.title ?? "New message";
        const body = payload?.notification?.body ?? "";
        try { new Notification(title, { body }); } catch { /* ignore */ }
    });

    // Optional: global helper for a manual test button / console
    window.sendDebugWelcome = async function () {
        const res = await fetch("/api/debug/send-welcome", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ token: window._fcmToken })
        });
        const json = await res.json();
        alert(JSON.stringify(json, null, 2));
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    // Mobile nav toggle
    const navToggle = document.querySelector('.nav-toggle');
    const nav = document.querySelector('.nav');
    if (navToggle && nav) {
        navToggle.addEventListener('click', function () {
            nav.classList.toggle('open');
        });
    }

    const navbar = document.querySelector('.navbar');
    if (navbar) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 50) {
                navbar.classList.add('navbar-scrolled');
                const header = document.querySelector('.header');
                if (header) header.classList.add('scrolled-light');
            } else {
                navbar.classList.remove('navbar-scrolled');
                const header = document.querySelector('.header');
                if (header) header.classList.remove('scrolled-light');
            }
        });
    }

    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.opacity = '0.9';
        });

        card.addEventListener('mouseleave', function () {
            this.style.opacity = '1';
        });
    });

    const slider = document.querySelector('.slider .slides');
    if (slider && slider.children.length > 0) {
        let index = 0;
        const total = slider.children.length;
        const sliderViewport = document.querySelector('.slider');
        const prevBtn = document.querySelector('.slider .prev');
        const nextBtn = document.querySelector('.slider .next');

        function viewportWidth() {
            return sliderViewport ? sliderViewport.clientWidth : 0;
        }

        function setSlideWidths() {
            const w = viewportWidth();
            Array.from(slider.children).forEach((el) => {
                el.style.flex = `0 0 ${w}px`;
                el.style.width = `${w}px`;
            });
        }

        function goTo(i) {
            index = ((i % total) + total) % total;
            const distance = index * viewportWidth();
            slider.style.transform = `translateX(-${distance}px)`;
            if (dotsContainer) {
                const dots = dotsContainer.querySelectorAll('span');
                dots.forEach((d, j) => { d.style.background = j === index ? 'var(--primary)' : 'rgba(255,255,255,0.25)'; });
            }
        }

        const dotsContainer = document.querySelector('.slider .dots');
        if (dotsContainer) {
            dotsContainer.innerHTML = '';
            for (let i = 0; i < total; i++) {
                const dot = document.createElement('span');
                if (i === 0) dot.style.background = 'var(--primary)';
                dotsContainer.appendChild(dot);
            }
        }

        if (total === 1 && dotsContainer) {
            dotsContainer.style.display = 'none';
        }

        if (dotsContainer && total > 1) {
            const dots = dotsContainer.querySelectorAll('span');
            dots.forEach((dot, i) => {
                dot.addEventListener('click', () => goTo(i));
            });
        }

        if (total > 1) {
            setInterval(() => { goTo(index + 1); }, 5000);
        }

        setSlideWidths();
        goTo(0);
        let resizeTimer;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => { setSlideWidths(); goTo(index); }, 100);
        });

        if (prevBtn) prevBtn.addEventListener('click', () => goTo(index - 1));
        if (nextBtn) nextBtn.addEventListener('click', () => goTo(index + 1));
    }

    // Enhanced file upload with preview and drag & drop
    const fileInput = document.getElementById('imageFileInput');
    const fileUploadLabel = document.getElementById('fileUploadLabel');
    const imagePreview = document.getElementById('imagePreview');
    const previewImg = document.getElementById('previewImg');
    const uploadHint = document.getElementById('uploadHint');
    const removeImageBtn = document.getElementById('removeImageBtn');
    const changeImageBtn = document.getElementById('changeImageBtn');
    const fileContainer = document.querySelector('.file-upload-container');

    if (fileInput && fileUploadLabel) {
        if (fileContainer) {
            const existing = fileContainer.getAttribute('data-existing-image');
            if (existing) {
                previewImg.src = existing;
                imagePreview.style.display = 'block';
                fileUploadLabel.classList.add('has-file');
                uploadHint.textContent = '';
            }
        }

        fileInput.addEventListener('change', function () {
            handleFileSelect(this.files[0]);
        });

        fileUploadLabel.addEventListener('dragover', function (e) {
            e.preventDefault();
            this.classList.add('drag-over');
        });

        fileUploadLabel.addEventListener('dragleave', function (e) {
            e.preventDefault();
            this.classList.remove('drag-over');
        });

        fileUploadLabel.addEventListener('drop', function (e) {
            e.preventDefault();
            this.classList.remove('drag-over');
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                fileInput.files = files;
                handleFileSelect(files[0]);
            }
        });

        if (removeImageBtn) {
            removeImageBtn.addEventListener('click', function (e) {
                e.preventDefault();
                removeImage();
            });
        }

        if (changeImageBtn) {
            changeImageBtn.addEventListener('click', function (e) {
                e.preventDefault();
                fileInput.click();
            });
        }
    }

    function handleFileSelect(file) {
        if (file && file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function (e) {
                previewImg.src = e.target.result;
                imagePreview.style.display = 'block';
                fileUploadLabel.classList.add('has-file');
                uploadHint.textContent = `تم اختيار: ${file.name}`;
                uploadHint.style.color = 'var(--success)';
            };
            reader.readAsDataURL(file);
        } else {
            Swal.fire({
                title: 'خطأ!',
                text: 'يرجى اختيار ملف صورة صالح',
                icon: 'error',
                confirmButtonText: 'حسناً'
            });
        }
    }

    function removeImage() {
        fileInput.value = '';
        imagePreview.style.display = 'none';
        fileUploadLabel.classList.remove('has-file');
        uploadHint.textContent = 'اختر صورة للعيادة';
        uploadHint.style.color = 'var(--muted)';
    }

    // chatbot logic moved to chat.js
});
