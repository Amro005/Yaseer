document.addEventListener('DOMContentLoaded', function () {
    const chatBtn = document.querySelector('.chat-float');
    const modal = document.getElementById('chatModal');
    const closeBtn = modal ? modal.querySelector('.modal-close') : null;
    const messagesWrap = document.getElementById('chatMessages');
    const form = document.getElementById('chatForm');
    const input = document.getElementById('chatInput');
    if (!chatBtn || !window.YASEER_CHAT_API_BASE) return;

    const api = window.YASEER_CHAT_API_BASE.replace(/\/$/, '') + '/chat';
    let sessionId = null;
    let isWaiting = false;
    let typingEl = null;

    function openModal() {
        if (!modal) return;
        modal.classList.add('show');
        input && input.focus();
        if (messagesWrap && messagesWrap.children.length === 0) {
            addBot('مرحباً! كيف أساعدك في حجز موعد؟ اكتب اسمك للبدء.');
        }
    }

    function closeModal() {
        modal && modal.classList.remove('show');
    }

    function addMsg(html) {
        if (!messagesWrap) return;
        messagesWrap.insertAdjacentHTML('beforeend', html);
        messagesWrap.scrollTop = messagesWrap.scrollHeight;
    }

    function addMe(text) {
        addMsg('<div class="msg me"><div class="avatar"><i class="fas fa-user"></i></div><div class="bubble">' + escapeHtml(text) + '</div></div>');
    }

    function addBot(text) {
        addMsg('<div class="msg bot"><div class="avatar"><i class="fas fa-robot"></i></div><div class="bubble">' + escapeHtml(text) + '</div></div>');
    }

    function escapeHtml(s) {
        return (s || '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }

    function showTyping() {
        if (!messagesWrap) return;
        typingEl = document.createElement('div');
        typingEl.className = 'msg bot';
        typingEl.innerHTML = '<div class="avatar"><i class="fas fa-robot"></i></div><div class="bubble"><span class="typing"></span><span class="typing"></span><span class="typing"></span></div>';
        messagesWrap.appendChild(typingEl);
        messagesWrap.scrollTop = messagesWrap.scrollHeight;
    }

    function hideTyping() {
        if (typingEl && typingEl.parentNode) typingEl.parentNode.removeChild(typingEl);
        typingEl = null;
    }

    async function sendMessage(text) {
        showTyping();
        try {
            const res = await fetch(api, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: text, session_id: sessionId })
            });
            if (!res.ok) throw new Error('Bad response');
            const data = await res.json();
            sessionId = data.session_id || sessionId;
            hideTyping();
            addBot(data.reply || '...');
        } catch (err) {
            hideTyping();
            addBot('تعذر الاتصال بالخدمة.');
            console.error(err);
        }
    }

    chatBtn && chatBtn.addEventListener('click', function (e) { e.preventDefault(); openModal(); });
    closeBtn && closeBtn.addEventListener('click', function () { closeModal(); });

    form && form.addEventListener('submit', async function (e) {
        e.preventDefault();
        if (isWaiting) return;
        const text = (input && input.value || '').trim();
        if (!text) return;

        addMe(text);
        input.value = '';
        isWaiting = true;
        if (input) input.disabled = true;
        const sendBtn = form.querySelector('.chat-send');
        if (sendBtn) sendBtn.disabled = true;

        await sendMessage(text);

        isWaiting = false;
        if (input) input.disabled = false;
        if (sendBtn) sendBtn.disabled = false;
        input && input.focus();
    });
});
